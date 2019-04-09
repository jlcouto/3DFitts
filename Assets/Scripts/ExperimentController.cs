using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Newtonsoft.Json;
using System.IO;


public enum ExperimentTask
{
    ReciprocalTapping,
    Dragging
}

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
        Stopped,
        Running,
        Paused
    }

    public string participantName;
    public int participantAge;
    public string testDescription;

    public ExperimentTask task;

    public ExperimentConfiguration experimentConfig;

    public Text statusText;
    public Text testText;
    public Text repetitionText;
    public Text noteText;

    public AudioSource correctTargetAudio;
    public AudioSource wrongTargetAudio;

    public CursorBehaviour cursor;
    public GameObject baseTarget;
    public Transform targetPlane;

    ExperimentStatus status = ExperimentStatus.Stopped;

    int currentTestConfiguration = 0;
    int currentPlaneOrientation = 0;
    TestController currentTestController;

    void Start()
    {
        UISetNoteText("");
    }

    public void RunExperiment()
    {
        if (task == ExperimentTask.Dragging)
        {
            experimentConfig = new DragMouseExperimentConfiguration();
        }
        else
        {
            experimentConfig = new TappingMouseExperimentConfiguration();
        }

        cursor.transform.localScale = experimentConfig.GetCursorDiameter() * Vector3.one;

        if (status == ExperimentStatus.Stopped)
        {
            Debug.Log("Starting experiment...");

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
                        foreach (Transform child in targetPlane)
                        {
                            GameObject.Destroy(child.gameObject);
                        }
                        targetPlane.DetachChildren();

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
            Debug.Log("Stopping experiment...");
        }
        else
        {
            Debug.Log("No experiment running!");
        }
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
        output["cursorDiameter"] = experimentConfig.GetCursorDiameter();

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
        return test.testConfiguration.testId + "_" + timestamp + ".json";
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
