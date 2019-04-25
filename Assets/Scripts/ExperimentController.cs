using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Newtonsoft.Json;
using System.IO;

public class IndexOfDifficultyConfiguration
{
    public float targetWidth;
    public float targetsDistance;

    public IndexOfDifficultyConfiguration(float targetWidth, float targetsDistance)
    {
        this.targetWidth = targetWidth;
        this.targetsDistance = targetsDistance;
    }

    float getIndexOfDifficulty()
    {
        return Mathf.Log((targetsDistance / targetWidth + 1), 2);
    }
}


public class ExperimentController : MonoBehaviour, ITestListener
{
    enum ExperimentStatus
    {
        CalibrationRunning,
        Stopped,
        Running,
        Paused
    }

    public string participantName;
    public int participantAge;
    public string testDescription;

    public ExperimentTask task;

    public CursorPositioningController cursorPositionController;
    public CursorSelectionMethod cursorSelectionMethod;

    public ExperimentConfiguration experimentConfig;

    public Text statusText;
    public Text testText;
    public Text repetitionText;
    public Text noteText;

    public AudioSource correctTargetAudio;
    public AudioSource wrongTargetAudio;

    public CursorInteractorBehaviour cursor;
    public GameObject baseTarget;
    public Transform targetPlane;

    public GameObject centerOfTestPlanesObject;
    public Transform centerOfTestPlanes;
    public Meta.HandsProvider metaHandsProvider;
    public Meta2CursorBehaviour calibrationPositioningCursor;

    bool isMetaHandsActive;
    CursorPositioningController.CursorHandPosition originalCursorPosition;

    ExperimentStatus status = ExperimentStatus.Stopped;

    int currentTestConfiguration = 0;
    int currentPlaneOrientation = 0;
    TestController currentTestController;

    void Start()
    {
        UISetNoteText("");
    }

    public void StartCalibrationOfExperimentPosition()
    {
        if (status == ExperimentStatus.Stopped)
        {
            status = ExperimentStatus.CalibrationRunning;
               
            isMetaHandsActive = metaHandsProvider.gameObject.activeInHierarchy;
            metaHandsProvider.gameObject.SetActive(true);

            originalCursorPosition = calibrationPositioningCursor.cursorPosition;
            calibrationPositioningCursor.cursorPosition = CursorPositioningController.CursorHandPosition.HandTop;

            cursor.cursorPositionController = calibrationPositioningCursor;
            calibrationPositioningCursor.gameObject.SetActive(true);

            cursor.selectionMethod = CursorSelectionMethod.KEYBOARD_SPACEBAR;
            cursor.transform.localScale = 0.01f * Vector3.one;
        }
        else if (status == ExperimentStatus.CalibrationRunning)
        {
            Debug.Log("Calibration is already running!");
        }
        else
        {
            Debug.Log("Calibration can only run when experiment is stopped!");
        }
    }

    public void FinishCalibrationOfExperimentPosition()
    {
        if (status == ExperimentStatus.CalibrationRunning)
        {
            status = ExperimentStatus.Stopped;
            calibrationPositioningCursor.cursorPosition = originalCursorPosition;
            metaHandsProvider.gameObject.SetActive(isMetaHandsActive);
            calibrationPositioningCursor.gameObject.SetActive(isMetaHandsActive);
        }
    }

    private void Update()
    {
        if (status == ExperimentStatus.CalibrationRunning)
        {
            centerOfTestPlanes.position = cursor.GetCursorPosition();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                FinishCalibrationOfExperimentPosition();
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                Vector3 currentRotation = centerOfTestPlanes.rotation.eulerAngles;
                centerOfTestPlanes.rotation =  Quaternion.Euler(new Vector3(currentRotation.x, currentRotation.y - 0.5f, currentRotation.z));
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                Vector3 currentRotation = centerOfTestPlanes.rotation.eulerAngles;
                centerOfTestPlanes.rotation = Quaternion.Euler(new Vector3(currentRotation.x, currentRotation.y + 0.5f, currentRotation.z));
            }
        }
    }

    public void RunExperiment()
    {
        if (status == ExperimentStatus.CalibrationRunning)
        {
            Debug.Log("Calibration is running. Finish Calibration process first.");
            return;
        }
        
        if (status == ExperimentStatus.Stopped)
        {
            Debug.Log("Starting experiment...");

            centerOfTestPlanesObject.SetActive(false);

            if (task == ExperimentTask.Dragging)
            {
                if (cursorPositionController.GetType() == typeof(MetaMouseInputBehaviour))
                {
                    experimentConfig = new Drag2DMouseExperimentConfiguration();
                }
                else
                {
                    experimentConfig = new Drag3DMouseExperimentConfiguration();
                }
            }
            else
            {
                if (cursorPositionController.GetType() == typeof(MetaMouseInputBehaviour))
                {
                    experimentConfig = new Tapping2DMouseExperimentConfiguration();
                }
                else
                {
                    experimentConfig = new Tapping3DMouseExperimentConfiguration();
                }
            }

            cursor.cursorPositionController = cursorPositionController;
            cursor.selectionMethod = cursorSelectionMethod;
            cursor.transform.localScale = experimentConfig.GetCursorDiameter() * Vector3.one;

            currentTestConfiguration = 0;
            currentPlaneOrientation = 0;
            status = ExperimentStatus.Running;
            RunNextTestConfiguration();
        }
        else if (status == ExperimentStatus.Paused)
        {
            Debug.Log("Resuming experiment...");
            status = ExperimentStatus.Running;
            RunNextTestConfiguration();
        }
        else
        {
            Debug.Log("Experiment already running!");
        }
    }

    void RunNextTestConfiguration()
    {
        if (currentTestController != null)
        {
            Debug.Log("Error: tried to execute new test but the previous one still exists!");
        }
        else
        {
            if (status == ExperimentStatus.Running)
            {
                if (currentPlaneOrientation < experimentConfig.GetPlaneOrientationsToTest().Length)
                {
                    if (currentTestConfiguration < experimentConfig.GetTargetConfigurationsToTest().Length)
                    {
                        CleanTargetPlane();

                        currentTestController = new TestController(this, statusText, testText, repetitionText, correctTargetAudio, wrongTargetAudio,
                            cursor, baseTarget, targetPlane,
                            experimentConfig.GetExperimentTask(),
                            experimentConfig.GetPlaneOrientationsToTest()[currentPlaneOrientation],
                            experimentConfig.GetNumTargetsPerTest(),
                            experimentConfig.GetTargetConfigurationsToTest()[currentTestConfiguration].targetWidth,
                            experimentConfig.GetTargetConfigurationsToTest()[currentTestConfiguration].targetsDistance,
                            experimentConfig.GetNumBlocksPerTest());

                        Debug.Log("Starting test: (P" + currentPlaneOrientation + ", C" + currentTestConfiguration + ")");
                        currentTestController.InitializeTest();
                        currentTestConfiguration++;
                    }
                    else
                    {
                        currentPlaneOrientation++;
                        currentTestConfiguration = 0;
                        RunNextTestConfiguration();
                        return;
                    }
                }
                else
                {
                    Debug.Log("Experiment finished with success!");
                    StopExperiment();
                }
            }
            else
            {
                Debug.Log("Experimented is currently paused/stopped.");
            }
        }       
    }

    public void PauseExperiment()
    {
        if (status == ExperimentStatus.Running)
        {
            Debug.Log("Pausing experiment...");
            status = ExperimentStatus.Paused;
        }
        else
        {
            Debug.Log("No experiment running!");
        }
    }

    public void StopExperiment()
    {
        if (status == ExperimentStatus.Running || status == ExperimentStatus.Paused)
        {
            status = ExperimentStatus.Stopped;
            centerOfTestPlanesObject.SetActive(true);
            CleanTargetPlane();
            Debug.Log("Stopping experiment...");
        }
        else
        {
            Debug.Log("No experiment running!");
        }
    }

    void CleanTargetPlane()
    {
        foreach (Transform child in targetPlane)
        {
            GameObject.Destroy(child.gameObject);
        }
        targetPlane.DetachChildren();
    }

    public void OnTestStarted()
    {
        UISetNoteText("");
    }

    public void OnTestEnded(TestMeasurements testMeasurements)
    {
        Debug.Log("Current test finished.");
        ExportResultsToFile(testMeasurements);
        currentTestController = null;
        RunNextTestConfiguration();
    }

    void ExportResultsToFile(TestMeasurements results)
    {
        var output = results.SerializeToDictionary();
        output["participantName"] = participantName;
        output["participantAge"] = participantAge;
        output["testDescription"] = testDescription;
        output["testTask"] = Enum2String.GetTaskString(results.testConfiguration.task);
        output["cursorDiameter"] = experimentConfig.GetCursorDiameter();
        output["planeOrientation"] = Enum2String.GetPlaneOrientationString(experimentConfig.GetPlaneOrientationsToTest()[currentPlaneOrientation]);

        string jsonData = JsonConvert.SerializeObject(output, new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented
        });
        string filename = GetFilenameForTest(results);
        string path = GetResultsFolder() + filename;
        Debug.Log("Saving results on file: " + path);
        try
        {
            Directory.CreateDirectory(GetResultsFolder());
            File.WriteAllText(path, jsonData);
        }
        catch
        {
            Debug.Log("Error writing results file for experiment: " + filename);
        }
        UISetNoteText("File saved: " + filename);
    }

    string GetFilenameForTest(TestMeasurements test)
    {
        var timestamp = test.timestamp.Replace(":", "_");
        return Enum2String.GetTaskString(test.testConfiguration.task) + test.testConfiguration.testId + "_" + timestamp + ".json";
    }

    string GetResultsFolder()
    {
        return "./Experiments/" + participantName +"/";
    }

    void UISetNoteText(string text)
    {
        noteText.text = text;
    }
}




[CustomEditor(typeof(ExperimentController))]
public class ObjectBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ExperimentController myScript = (ExperimentController)target;

        if (GUILayout.Button("Start Calibration"))
        {
            myScript.StartCalibrationOfExperimentPosition();
        }

        if (GUILayout.Button("Run Experiment"))
        {
            myScript.RunExperiment();
        }

        if (GUILayout.Button("Pause Experiment"))
        {
            myScript.PauseExperiment();
        }
        
        if (GUILayout.Button("Stop Experiment"))
        {
            myScript.StopExperiment();
        }
    }
}
