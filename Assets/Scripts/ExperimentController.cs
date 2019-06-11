using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.SceneManagement;
using CsvHelper;

public class ExperimentController : MonoBehaviour, ITestListener
{
    enum ExperimentStatus
    {
        Initializing,
        CalibrationRunning,
        Stopped,
        Running,
        Paused
    }    

    public Text statusText;

    public CursorInteractorBehaviour cursor;
    public GameObject baseTarget;
    public Transform targetPlane;

    public GameObject centerOfTestPlanesObject;    
    public Meta2CursorBehaviour calibrationPositioningCursor;

    public GameObject inputDevices;
    public GameObject metaCameraObject;

    public Camera computerCamera;

    ExperimentConfiguration experimentConfig;
    ExperimentStatus status = ExperimentStatus.Initializing;
    TestController currentTestController;
    int currentSequence = 0;
    ulong frameNumber = 0;

    bool isCalibratingCenterOfPLanes = false;
    CursorPositioningController.CursorHandPosition originalCursorPosition;    

    struct FrameData
    {
        public ulong frameNumber;
        public float time;
        public Vector3 cursorPosition;
        public int trackingHandId;
    }
    List<FrameData> frameData;

    void Start()
    {
        status = ExperimentStatus.Initializing;

        frameData = new List<FrameData>(60 * 60 * 120); // Enough capacity to record up to 2 min of data at 60 fps     

        LoadConfigurationsFromCanvasValues();        
    }

    void LoadConfigurationsFromCanvasValues()
    {
        experimentConfig = SharedData.currentConfiguration;
    }

    void ActivateMetaCamera()
    {        
        if (!metaCameraObject.activeInHierarchy)
        {
            metaCameraObject.SetActive(true);
        }
    }

    private void Update()
    {
        KeepContentsSizeConstantOn2DScreen();

        if (status == ExperimentStatus.CalibrationRunning && isCalibratingCenterOfPLanes)
        {
            transform.position = cursor.GetCursorPosition();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                FinishCalibrationOfExperimentPosition();
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                Vector3 currentRotation = transform.rotation.eulerAngles;
                transform.rotation =  Quaternion.Euler(new Vector3(currentRotation.x, currentRotation.y - 0.5f, currentRotation.z));
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                Vector3 currentRotation = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(new Vector3(currentRotation.x, currentRotation.y + 0.5f, currentRotation.z));
            }
        }
        else if (status == ExperimentStatus.Running)
        {            
            if (currentTestController != null && currentTestController.isRunning())
            {
                LogFrameData();
            }            
        }
        else if (status == ExperimentStatus.Initializing)
        {
            status = ExperimentStatus.Stopped;            
        }
    }

    void LogFrameData()
    {
        frameNumber++;
        FrameData newData = new FrameData();
        newData.frameNumber = frameNumber;
        newData.time = Time.realtimeSinceStartup;
        newData.cursorPosition = cursor.GetCursorPosition();
        newData.trackingHandId = cursor.GetTrackedHandId();
        frameData.Add(newData);
    }

    public void RunExperiment()
    {
        if (status == ExperimentStatus.CalibrationRunning)
        {
            Debug.Log("[ExperimentController] Calibration is running. Finish Calibration process first.");
            return;
        }
        
        if (status == ExperimentStatus.Initializing || status == ExperimentStatus.Stopped)
        {
            Debug.Log("[ExperimentController] Starting experiment...");

            if (experimentConfig.experimentMode == ExperimentMode.Experiment2D)
            {
                // Do not show the cursor when running the 2D mode
                //cursor.GetComponent<MeshRenderer>().enabled = false;
            }
            else if (experimentConfig.experimentMode == ExperimentMode.Experiment3DOnMeta2)
            {
                ActivateMetaCamera();
            }

            centerOfTestPlanesObject.SetActive(false);

            cursor.cursorPositionController = GetControllerForPositioningMethod(experimentConfig.cursorPositioningMethod);
            cursor.selectionMethod = experimentConfig.cursorSelectionMethod;            
            cursor.SetDwellTime(experimentConfig.dwellTime);            
            
            cursor.transform.localScale = experimentConfig.cursorWidth * Vector3.one;
            cursor.cursorPositionController.gameObject.SetActive(true);
            
            currentSequence = 0;
            status = ExperimentStatus.Running;            

            RunNextTestConfiguration();
        }
        else if (status == ExperimentStatus.Paused)
        {
            Debug.Log("[ExperimentController] Resuming experiment...");
            cursor.cursorPositionController.gameObject.SetActive(true);
            status = ExperimentStatus.Running;
            RunNextTestConfiguration();
        }
        else
        {
            Debug.Log("[ExperimentController] Experiment already running!");
        }
    }

    CursorPositioningController GetControllerForPositioningMethod(CursorPositioningMethod cursorPositioningMethod)
    {     
        switch (cursorPositioningMethod)
        {            
            case CursorPositioningMethod.Meta2Interaction:
                return inputDevices.GetComponentInChildren<Meta2CursorBehaviour>(true);                
            case CursorPositioningMethod.LeapMotionController:
                return inputDevices.GetComponentInChildren<LeapMotionControllerCursorBehaviour>(true);
            case CursorPositioningMethod.VIVE:
                return inputDevices.GetComponentInChildren<ViveControllerPositionBehaviour>(true);
            case CursorPositioningMethod.Mouse:
            default:
                return inputDevices.GetComponentInChildren<Mouse2DInputBehaviour>(true);
        }        
    }

    void RunNextTestConfiguration()
    {
        if (currentTestController != null)
        {
            Debug.LogWarning("[ExperimentController] Tried to execute new test but the previous one still exists!");
        }
        else
        {
            if (status == ExperimentStatus.Running)
            {                
                if (currentSequence < experimentConfig.sequences.Count)
                {
                    CleanTargetPlane();
                    frameData.Clear();

                    currentTestController = new TestController(this, statusText, cursor, baseTarget,
                                                               targetPlane, experimentConfig, currentSequence);                        

                    Debug.Log("[ExperimentController] Starting sequence " + currentSequence + ")");
                    currentTestController.InitializeTest();                        
                    currentSequence++;
                }
                else
                {
                    currentSequence = 0;
                    Debug.Log("[ExperimentController] Experiment finished with success!");
                    StopExperiment();
                    return;
                }
            }
            else
            {
                Debug.Log("[ExperimentController] Experimented is currently paused/stopped.");
            }
        }       
    }

    public void PauseExperiment()
    {
        if (status == ExperimentStatus.Running)
        {
            Debug.Log("[ExperimentController] Pausing experiment...");
            status = ExperimentStatus.Paused;
            cursor.cursorPositionController.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("[ExperimentController] No experiment running!");
        }
    }

    public void StopExperiment()
    {
        if (status == ExperimentStatus.Running || status == ExperimentStatus.Paused)
        {
            status = ExperimentStatus.Stopped;
            AbortCurrentTest();
            centerOfTestPlanesObject.SetActive(true);
            cursor.cursorPositionController.gameObject.SetActive(false);
            CleanTargetPlane();
            Debug.Log("[ExperimentController] Stopped current experiment...");
        }
        else
        {
            Debug.Log("[ExperimentController] No experiment running!");
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

    }

    public void OnTestEnded(TestMeasurements testMeasurements)
    {
        Debug.Log("[ExperimentController] Current test finished.");
        ExportResultsToFile(testMeasurements);
        AbortCurrentTest();
        RunNextTestConfiguration();
    }

    bool AbortCurrentTest()
    {
        if (currentTestController != null)
        {            
            currentTestController.AbortTest();
            currentTestController = null;
            return true;
        }
        return false;
    }

    public void AbortCurrentConfigurationAndRunNext()
    {
        if (AbortCurrentTest())
        {
            Debug.Log("[ExperimentController] Current test aborted.");
            RunNextTestConfiguration();
        }
    }

    void ExportResultsToFile(TestMeasurements results)
    {
        string directory = FileManager.GetResultsFolder(results.configuration.participantCode);
        string filename = GetTestResultsFilenameForTest(results) + ".csv";
        var records = ExperimentResultRecord.GetRecordsFromTestMeasurements(results);
        FileManager.SaveRecordsToCSVFile(directory, filename, records);

        ExportFrameDataToFile(results);
    }

    void ExportFrameDataToFile(TestMeasurements results)
    {
        Dictionary<string, object> frameDataDictionary = new Dictionary<string, object>();
        List<Dictionary<string, object>> frames = new List<Dictionary<string, object>>();
        foreach (FrameData data in frameData)
        {
            Dictionary<string, object> aFrame = new Dictionary<string, object>
            {
                ["Frame"] = data.frameNumber,
                ["Time"] = data.time,
                ["CursorPosition"] = data.cursorPosition,
                ["TrackedHandID"] = data.trackingHandId
            };
            frames.Add(aFrame);
        }
        frameDataDictionary["frameData"] = frames;

        string jsonData = JsonConvert.SerializeObject(frameDataDictionary, new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented
        });
        string filename = GetFrameDataResultsFilenameForTest(results);
        string directory = FileManager.GetFrameDataFolder(experimentConfig.participantCode);
        FileManager.SaveFile(directory, filename, jsonData);
    }

    string GetTestResultsFilenameForTest(TestMeasurements test)
    {
        var timestamp = test.timestamp.Replace(":", "_");
        return System.String.Format("{0}-{1}-{2}-{3}-{4}",
                experimentConfig.participantCode, experimentConfig.conditionCode,
                experimentConfig.sessionCode, experimentConfig.groupCode,
                timestamp);
    }

    string GetFrameDataResultsFilenameForTest(TestMeasurements test)
    {
        return "FrameData_" + GetTestResultsFilenameForTest(test) + ".json";
    }

    bool CanStartCalibrationProcess()
    {
        if (status == ExperimentStatus.Stopped)
        {
            return true;
        }
        else if (status == ExperimentStatus.CalibrationRunning)
        {
            Debug.Log("[ExperimentController] Calibration is already running!");
        }
        else
        {
            Debug.Log("[ExperimentController] Calibration can only run when experiment is stopped!");
        }
        return false;
    }

    public void StartCalibrationOfExperimentPosition()
    {
        if (CanStartCalibrationProcess())
        {
            status = ExperimentStatus.CalibrationRunning;
            isCalibratingCenterOfPLanes = true;

            originalCursorPosition = calibrationPositioningCursor.cursorPosition;
            calibrationPositioningCursor.cursorPosition = CursorPositioningController.CursorHandPosition.HandTop;

            cursor.cursorPositionController = calibrationPositioningCursor;
            calibrationPositioningCursor.gameObject.SetActive(true);

            cursor.selectionMethod = CursorSelectionMethod.KeyboardSpaceBar;
            cursor.transform.localScale = 0.01f * Vector3.one;

            ActivateMetaCamera();
        }
    }

    public void StartCalibrationOfVIVEController()
    {
        if (CanStartCalibrationProcess())
        {
            ViveControllerPositionBehaviour controller = inputDevices.GetComponentInChildren<ViveControllerPositionBehaviour>(true);
            if (controller != null)
            {
                cursor.cursorPositionController = controller;
                status = ExperimentStatus.CalibrationRunning;
                controller.gameObject.SetActive(true);
                controller.StartVIVEControllerCalibration(() => {
                    FinishCalibration();
                    controller.gameObject.SetActive(false);
                });
            }
            else
            {
                Debug.LogWarning("Could not find the ViveControllerPositionBehaviour component in the children of inputMethods!");
            }
        }
    }

    public void StartCalibrationOfLeapMotionController()
    {
        if (CanStartCalibrationProcess())
        {
            LeapMotionControllerCursorBehaviour controller = inputDevices.GetComponentInChildren<LeapMotionControllerCursorBehaviour>(true);
            if (controller != null)
            {
                cursor.cursorPositionController = controller;
                controller.gameObject.SetActive(true);
                status = ExperimentStatus.CalibrationRunning;
                controller.StartLeapMotionCalibration(() => {
                    FinishCalibration();
                    ExecuteAfterTime(() => { controller.gameObject.SetActive(false); }, 1); // avoid bug when trying to disable hands group in leap motion                 
                });
            }
            else
            {
                Debug.LogWarning("Could not find the LeapMotionControllerCursorBehaviour component in the children of inputMethods!");
            }
        }
    }

    IEnumerator ExecuteAfterTime(System.Action action, float timeInSeconds)
    {
        yield return new WaitForSeconds(timeInSeconds);
        action?.Invoke();
    }

    void FinishCalibration()
    {
        if (status == ExperimentStatus.CalibrationRunning)
        {
            status = ExperimentStatus.Stopped;
        }
    }

    public void FinishCalibrationOfExperimentPosition()
    {
        if (status == ExperimentStatus.CalibrationRunning)
        {
            isCalibratingCenterOfPLanes = false;
            calibrationPositioningCursor.cursorPosition = originalCursorPosition;            
            calibrationPositioningCursor.gameObject.SetActive(false);
            FinishCalibration();
        }
    }

    void KeepContentsSizeConstantOn2DScreen()
    {
        if (computerCamera != null)
        {
            float minScreenSizePixels = Mathf.Min(Screen.height, Screen.width);

            float pixelsPerInch = Screen.dpi;
            const float inchesPerMeter = 0.0254f;
            
            computerCamera.orthographicSize = 0.5f * inchesPerMeter * minScreenSizePixels / pixelsPerInch;

            if (cursor.cursorPositionController.GetType() == typeof(Mouse2DInputBehaviour))
            {
                ((Mouse2DInputBehaviour)cursor.cursorPositionController).spaceSize = computerCamera.orthographicSize * 2;
            }
        }
    }
}



#if UNITY_EDITOR
[CustomEditor(typeof(ExperimentController))]
public class ObjectBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ExperimentController myScript = (ExperimentController)target;
        
        GUILayout.Space(20);
        if (GUILayout.Button("Run Experiment"))
        {
            myScript.RunExperiment();
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Abort Current & Run Next Configuration"))
        {
            myScript.AbortCurrentConfigurationAndRunNext();
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Pause Experiment"))
        {
            myScript.PauseExperiment();
        }
        
        if (GUILayout.Button("Stop Experiment"))
        {
            myScript.StopExperiment();
        }

        GUILayout.Space(20);
        if (GUILayout.Button("Start Meta2 Origin Calibration"))
        {
            myScript.StartCalibrationOfExperimentPosition();
        }
        GUILayout.Space(20);
        if (GUILayout.Button("Calibrate VIVE Controller Cursor Offset"))
        {
            myScript.StartCalibrationOfVIVEController();
        }

        if (GUILayout.Button("Calibrate Leap Motion Controller Cursor Offset"))
        {
            myScript.StartCalibrationOfLeapMotionController();
        }
    }
}
#endif
