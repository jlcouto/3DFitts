using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;

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
    public GameObject baseTarget2D;
    public GameObject baseTarget3D;
    public Transform targetPlane;

    public GameObject centerOfTestPlanesObject;    
    public Meta2CursorBehaviour calibrationPositioningCursor;

    public GameObject inputDevices;
    public GameObject metaCameraObject;

    public Camera computerCamera;
    public ResultsPanelBehaviour panelResults;

    ExperimentConfiguration configuration;
    ExperimentStatus status = ExperimentStatus.Initializing;
    TestController currentTestController;
    int currentSequence = 0;
    int frameNumber = 0;

    float worldSpaceRatio = 1f;

    bool isCalibratingCenterOfPLanes = false;
    CursorPositioningController.CursorHandPosition originalCursorPosition;

    Action OnCalibrationFinished; 

    class FrameData
    {
        public int frameNumber { get; set; }
        public float time { get; set; }
        public float xCursorPosition { get; set; }
        public float yCursorPosition { get; set; }
        public float zCursorPosition { get; set; }
        public int trackingHandId { get; set; }
    }
    List<FrameData> frameData;

    void Start()
    {
        status = ExperimentStatus.Initializing;

        frameData = new List<FrameData>(60 * 60 * 120); // Enough capacity to record up to 2 min of data at 60 fps     

        LoadConfigurationsFromCanvasValues();

        SharedData.calibrationData.LoadFromFile();
    }

    void LoadConfigurationsFromCanvasValues()
    {
        configuration = SharedData.currentConfiguration;
    }

    void ActivateMetaCamera()
    {        
        if (!metaCameraObject.activeInHierarchy)
        {
            computerCamera.GetComponent<AudioListener>().enabled = false;
            metaCameraObject.SetActive(true);
        }
    }

    public void SetCursorActive(bool active)
    {
        cursor.cursorPositionController.gameObject.SetActive(active);
        cursor.gameObject.SetActive(active);
    }

    private void Update()
    {
        if (status == ExperimentStatus.CalibrationRunning && isCalibratingCenterOfPLanes)
        {
            transform.position = cursor.GetCursorPosition();

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
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
            KeepContentsSizeConstantOn2DScreen();
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
        Vector3 pos = cursor.GetCursorPosition();
        newData.xCursorPosition = pos.x;
        newData.yCursorPosition = pos.y;
        newData.zCursorPosition = pos.z;
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

            centerOfTestPlanesObject.SetActive(false);

            cursor.cursorPositionController = GetControllerForPositioningMethod(configuration.cursorPositioningMethod, configuration.planeOrientation);
            cursor.selectionMethod = configuration.cursorSelectionMethod;            
            cursor.SetDwellTime(configuration.dwellTime);            
            
            cursor.transform.localScale = configuration.cursorWidth * Vector3.one;
            cursor.cursorPositionController.gameObject.SetActive(true);
            cursor.gameObject.SetActive(true);                        

            if (configuration.experimentMode == ExperimentMode.Experiment2D)
            {
                // Do not show the cursor when running the 2D mode
                //cursor.GetComponent<MeshRenderer>().enabled = false;
                computerCamera.backgroundColor = Color.white;
                worldSpaceRatio = 1f;
            }
            else if (configuration.experimentMode == ExperimentMode.Experiment3DOnMeta2)
            {
                worldSpaceRatio = configuration.screenTo3DWorldDimensionRatio;
                computerCamera.backgroundColor = Color.grey;
                ActivateMetaCamera();
            }

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

    CursorPositioningController GetControllerForPositioningMethod(CursorPositioningMethod cursorPositioningMethod, PlaneOrientation plane)
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
            {
                Mouse2DInputBehaviour mouseController = inputDevices.GetComponentInChildren<Mouse2DInputBehaviour>(true);
                mouseController.plane = plane;
                return mouseController;
            }                
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
                if (currentSequence < configuration.GetSequences().Count)
                {
                    CleanTargetPlane();
                    frameData.Clear();

                    GameObject baseTarget = (configuration.experimentMode == ExperimentMode.Experiment2D) ? baseTarget2D : baseTarget3D;
                    currentTestController = new TestController(this, statusText, cursor, baseTarget,
                                                               targetPlane, configuration, currentSequence);                        

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

            if (configuration.experimentMode == ExperimentMode.Experiment3DOnMeta2)
            {
                centerOfTestPlanesObject.SetActive(true);
            }

            cursor.cursorPositionController.gameObject.SetActive(false);
            cursor.gameObject.SetActive(false);

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
            Destroy(child.gameObject);
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
        panelResults.ShowPanel(testMeasurements, () => { RunNextTestConfiguration(); });        
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
        string filename = FileManager.GetResultsFilenameForTest(results) + ".csv";
        var records = ExperimentResultRecord.GetRecordsFromTestMeasurements(results, transform.position, transform.rotation.eulerAngles);
        FileManager.WriteToCsvFile(directory, filename, records);

        ExportFrameDataToFile(results);
    }

    void ExportFrameDataToFile(TestMeasurements results)
    {    
        string filename = GetFrameDataResultsFilenameForTest(results);
        string directory = FileManager.GetFrameDataFolder(configuration.participantCode);
        FileManager.WriteToCsvFile(directory, filename, frameData);
    }

    string GetFrameDataResultsFilenameForTest(TestMeasurements test)
    {
        return "FrameData_" + FileManager.GetResultsFilenameForTest(test) + ".csv";
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

    public void StartCalibrationOfExperimentPosition(Action onFinished = null)
    {
        if (CanStartCalibrationProcess())
        {
            OnCalibrationFinished = onFinished;

            status = ExperimentStatus.CalibrationRunning;
            isCalibratingCenterOfPLanes = true;

            originalCursorPosition = calibrationPositioningCursor.cursorPosition;
            calibrationPositioningCursor.cursorPosition = CursorPositioningController.CursorHandPosition.HandTop;

            cursor.cursorPositionController = calibrationPositioningCursor;
            SetCursorActive(true);

            cursor.selectionMethod = CursorSelectionMethod.KeyboardSpaceBar;
            cursor.transform.localScale = 0.01f * Vector3.one;

            ActivateMetaCamera();
        }
    }

    public void StartCalibrationOfVIVEController(Action onFinished = null)
    {
        if (CanStartCalibrationProcess())
        {
            OnCalibrationFinished = onFinished;

            ViveControllerPositionBehaviour controller = inputDevices.GetComponentInChildren<ViveControllerPositionBehaviour>(true);
            if (controller != null)
            {
                cursor.cursorPositionController = controller;
                status = ExperimentStatus.CalibrationRunning;
                SetCursorActive(true);
                controller.StartVIVEControllerCalibration(() => {
                    FinishCalibration();
                });
            }
            else
            {
                Debug.LogWarning("Could not find the ViveControllerPositionBehaviour component in the children of inputMethods!");
            }
        }
    }

    public void StartCalibrationOfLeapMotionController(Action onFinished = null)
    {
        if (CanStartCalibrationProcess())
        {
            OnCalibrationFinished = onFinished;

            LeapMotionControllerCursorBehaviour controller = inputDevices.GetComponentInChildren<LeapMotionControllerCursorBehaviour>(true);
            if (controller != null)
            {
                cursor.cursorPositionController = controller;
                controller.gameObject.SetActive(true);
                status = ExperimentStatus.CalibrationRunning;
                controller.StartLeapMotionCalibration( () => {
                    FinishCalibration();
                });
            }
            else
            {
                Debug.LogWarning("Could not find the LeapMotionControllerCursorBehaviour component in the children of inputMethods!");
            }
        }
    }

    void FinishCalibration()
    {
        if (status == ExperimentStatus.CalibrationRunning)
        {
            SetCursorActive(false);
            status = ExperimentStatus.Stopped;
            OnCalibrationFinished?.Invoke();
            OnCalibrationFinished = null;
        }
    }

    public void FinishCalibrationOfExperimentPosition()
    {
        if (status == ExperimentStatus.CalibrationRunning)
        {
            isCalibratingCenterOfPLanes = false;
            calibrationPositioningCursor.cursorPosition = originalCursorPosition;
            FinishCalibration();
        }
    }

    void KeepContentsSizeConstantOn2DScreen()
    {
        if (computerCamera != null)
        {
            float minScreenSizePixels = Mathf.Min(Screen.height, Screen.width);
            computerCamera.orthographicSize = worldSpaceRatio * 0.5f * minScreenSizePixels / (configuration.screenPixelsPerMillimeter * 1000);

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
