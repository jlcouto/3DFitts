using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEditor;
using UnityEngine.Events;

public class ViveControllerPositionBehaviour : CursorPositioningController
{
    public Transform offset;
    public Vector3 positionOffset = Vector3.zero;
    public Vector3 scale = Vector3.one;
    public Vector3 rotation;

    public GameObject calibrationObjects;
    public GameObject XAxisFirstObject;
    public GameObject XAxisSecondObject;
    public GameObject YAxisFirstObject;
    public GameObject YAxisSecondObject;
    public GameObject ZAxisFirstObject;
    public GameObject ZAxisSecondObject;

    public UnityAction OnTriggerDown;
    public UnityAction OnTrigger;
    public UnityAction OnTriggerUp;

    bool isCalibrating = false;
    bool gotFirstPoint = false;
    bool isXAxisCalibrated = false;
    bool isYAxisCalibrated = false;
    bool isZAxisCalibrated = false;

    Vector3[] firstPoint;
    Vector3[] secondPoint;
    Vector3[] desiredFirstPoint;
    Vector3[] desiredSecondPoint;
    Vector3[] offsetByAxis;
    Vector3[] metaOffsetByAxis;
    float[] scaleByAxis;
    Quaternion[] rotationByAxis;
    int currentCalibrationAxis = 0; // 0 = x, 1 = y, 2 = z

    Vector3 lastRawCursorPosition;    
    Vector3 lastCursorPosition;

    Quaternion lastRawCursorRotation;
    Quaternion lastCursorRotation;
    
    Matrix4x4 translationMatrix;
    Matrix4x4 scalingMatrix;
    Matrix4x4 rotationMatrix;

    Action finishCalibrationCallback;

    /* UDP communication to read controller data */
    Thread receiveThread;
    private static Mutex updateTriggerMutex = new Mutex();
    UdpClient client;
    private int port = 8051;
    private bool _lastProcessedTriggerState;
    private int frameTriggerDown = -1;
    private int frameTriggerUp = -1;
    private int currentFrame = 0;
    private Double[] float_array;

    private const uint numFramesToWaitBeforeNewTrigger = 5;

    public override string GetDeviceName()
    {
        return "VIVEController";
    }

    public override Vector3 GetCurrentCursorPosition()
    {
        return lastCursorPosition;
    }

    public override int GetTrackedHandId()
    {
        return 0;
    }

    void Start()
    {
        float_array = new Double[7];
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

        firstPoint = new Vector3[3];
        secondPoint = new Vector3[3];
        desiredFirstPoint = new Vector3[3];
        desiredSecondPoint = new Vector3[3];
        offsetByAxis = new Vector3[3];
        scaleByAxis = new float[3];
        rotationByAxis = new Quaternion[3];
        metaOffsetByAxis = new Vector3[3];

        translationMatrix = Matrix4x4.identity;
        scalingMatrix = Matrix4x4.identity;
        rotationMatrix = Matrix4x4.identity;

        OnTriggerDown += OnTriggerDownEvent;        
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            FinishCalibration();
        }
        else
        {
            updateTriggerMutex.WaitOne();

            lastRawCursorPosition = new Vector3((float)float_array[0], (float)float_array[1], (float)float_array[2]);
            lastRawCursorRotation = new Quaternion((float)float_array[3], (float)float_array[4], (float)float_array[5], (float)float_array[6]);
            ConvertVIVEToUnity(ref lastRawCursorPosition, ref lastRawCursorRotation);

            //translationMatrix = Matrix4x4.Translate(-positionOffset);
            //scalingMatrix = Matrix4x4.Scale(scale);
            //rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(rotation.x, rotation.y, rotation.z));

            lastCursorPosition = offset.position + scalingMatrix.MultiplyPoint(rotationMatrix.MultiplyPoint(translationMatrix.MultiplyPoint(lastRawCursorPosition)));
            lastCursorRotation = lastRawCursorRotation;

            currentFrame++;
            if (currentFrame < 0)
            {
                currentFrame = 0;
            }

            updateTriggerMutex.ReleaseMutex();
        }
    }

    public static void ConvertVIVEToUnity(ref Vector3 pos, ref Quaternion rot)
    {
        pos = new Vector3(pos[0], pos[1], -pos[2]);

        rot = new Quaternion(-rot[2], -rot[1], rot[0], rot[3]);
        Quaternion rot180 = Quaternion.AngleAxis(180.0f, Vector3.up);
        Quaternion rot90 = Quaternion.AngleAxis(90.0f, Vector3.forward);

        rot = rot * rot90 * rot180;
    }

    public void StartVIVEControllerCalibration(Action callback)
    {
        Debug.Log("Calibrating...");

        translationMatrix = Matrix4x4.identity;
        scalingMatrix = Matrix4x4.identity;
        rotationMatrix = Matrix4x4.identity;

        isCalibrating = true;
        isXAxisCalibrated = false;
        isYAxisCalibrated = false;
        isZAxisCalibrated = false;
        gotFirstPoint = false;

        currentCalibrationAxis = 0;

        calibrationObjects.SetActive(true);
        SetCalibrationObjectsActive(false);
        XAxisFirstObject.SetActive(true);

        finishCalibrationCallback = callback;
    }

    void OnTriggerDownEvent()
    {
        MainThreadDispatcher.RunOnMainThread(() =>
        {
            if (isCalibrating)
            {
                if (gotFirstPoint)
                {
                    secondPoint[currentCalibrationAxis] = lastRawCursorPosition;
                    FinishAxisCalibration();
                    gotFirstPoint = false;
                }
                else
                {
                    firstPoint[currentCalibrationAxis] = lastRawCursorPosition;
                    gotFirstPoint = true;

                    if (currentCalibrationAxis == 0)
                    {
                        XAxisFirstObject.SetActive(false);
                        XAxisSecondObject.SetActive(true);
                    }
                    else if (currentCalibrationAxis == 1)
                    {
                        YAxisFirstObject.SetActive(false);
                        YAxisSecondObject.SetActive(true);
                    }
                    else
                    {
                        ZAxisFirstObject.SetActive(false);
                        ZAxisSecondObject.SetActive(true);
                    }
                }
            }
        });
    }
    
    public void FinishAxisCalibration()
    {
        if (isXAxisCalibrated)
        {
            if (isYAxisCalibrated)
            {
                isZAxisCalibrated = true;
                ZAxisFirstObject.SetActive(false);
                ZAxisSecondObject.SetActive(false);

                desiredFirstPoint[2] = ZAxisFirstObject.transform.position;
                desiredSecondPoint[2] = ZAxisSecondObject.transform.position;                
            }
            else
            {
                isYAxisCalibrated = true;
                YAxisFirstObject.SetActive(false);
                YAxisSecondObject.SetActive(false);
                ZAxisFirstObject.SetActive(true);
                ZAxisSecondObject.SetActive(false);

                desiredFirstPoint[1] = YAxisFirstObject.transform.position;
                desiredSecondPoint[1] = YAxisSecondObject.transform.position;
            }
        }
        else
        {
            isXAxisCalibrated = true;
            XAxisFirstObject.SetActive(false);
            XAxisSecondObject.SetActive(false);
            YAxisFirstObject.SetActive(true);
            YAxisSecondObject.SetActive(false);

            desiredFirstPoint[0] = XAxisFirstObject.transform.position;
            desiredSecondPoint[0] = XAxisSecondObject.transform.position;
        }

        Vector3 sizeVive = (secondPoint[currentCalibrationAxis] - firstPoint[currentCalibrationAxis]);
        Vector3 sizeMeta = (desiredSecondPoint[currentCalibrationAxis] - desiredFirstPoint[currentCalibrationAxis]);
        
        // Offset
        Vector3 centerVive = firstPoint[currentCalibrationAxis] + sizeVive / 2;
        Vector3 centerMeta = desiredFirstPoint[currentCalibrationAxis] + sizeMeta / 2;

        offsetByAxis[currentCalibrationAxis] = centerVive;
        metaOffsetByAxis[currentCalibrationAxis] = centerMeta;
        
        // Scale
        scaleByAxis[currentCalibrationAxis] = sizeMeta.magnitude / sizeVive.magnitude;

        // Rotation
        Vector3 realDataAxis = secondPoint[currentCalibrationAxis] - firstPoint[currentCalibrationAxis];        
        rotationByAxis[currentCalibrationAxis] = Quaternion.FromToRotation(realDataAxis, desiredSecondPoint[currentCalibrationAxis] - desiredFirstPoint[currentCalibrationAxis]);



        // Apply transformations to measured axis (just for visual feedback while calibrating)
        // offset
        positionOffset = offsetByAxis[currentCalibrationAxis];
        translationMatrix = Matrix4x4.Translate(-positionOffset);

        // scale
        scale.x = 1;
        scale.y = 1;
        scale.z = 1;
        if (currentCalibrationAxis == 0)
        {
            scale.x = scaleByAxis[0];
        }
        else if (currentCalibrationAxis == 1)
        {
            scale.y = scaleByAxis[1];
        }
        else
        {            
            scale.z = scaleByAxis[2];
        }        
        scalingMatrix = Matrix4x4.Scale(scale);

        // rotation
        rotation = rotationByAxis[currentCalibrationAxis].eulerAngles;
        rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(rotation.x, rotation.y, rotation.z));
        
        Debug.Log("Calibration result: First = " + firstPoint[currentCalibrationAxis] + " | Second = " + secondPoint[currentCalibrationAxis] + " | Offset = " + offsetByAxis[currentCalibrationAxis] + " | scaling = " + scaleByAxis[currentCalibrationAxis] + " | rot = " + rotationByAxis[currentCalibrationAxis].eulerAngles + " | Meta offset = " + metaOffsetByAxis[currentCalibrationAxis]);

        currentCalibrationAxis++;

        if (isZAxisCalibrated)
        {
            FinishCalibration();
        }
    }

    void FinishCalibration()
    {
        // Computes final offset by averaging the offsets obtained for each axis measurement
        positionOffset = Vector3.zero;
        for (int i = 0; i < 3; i++)
        {
            positionOffset += offsetByAxis[i];
        }
        positionOffset /= 3;
        translationMatrix = Matrix4x4.Translate(-positionOffset);

        // Get the scaling values for each axis
        scale.x = scaleByAxis[0];
        scale.y = scaleByAxis[1];
        scale.z = scaleByAxis[2];
        scalingMatrix = Matrix4x4.Scale(scale);


        // Get the rotation needed to align both system coordinates

        /*************
        // This is not working properly. I guess it is because the three measured axis usually won't be orthogonal.

        //Vector3 measuredXAxis = (secondPoint[0] - firstPoint[0]).normalized;
        //Vector3 measuredYAxis = (secondPoint[1] - firstPoint[1]).normalized;
        //Vector3 measuredZAxis = (secondPoint[2] - firstPoint[2]).normalized;
        //Vector3 measuredSystemCoordinatesOrientation = (measuredXAxis + measuredYAxis + measuredZAxis).normalized;

        //Vector3 systemCoordinatesOrientation = Vector3.one;

        //rotation = Quaternion.FromToRotation(measuredSystemCoordinatesOrientation, systemCoordinatesOrientation).eulerAngles;
        **************/

        // Empirically, the Y axis of both coordinate systems are always facing up.
        // So we just need to rotate around it so the Z axis are aligned to align all three axis. 
        rotation = rotationByAxis[2].eulerAngles;
        rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(rotation.x, rotation.y, rotation.z));
        
        isCalibrating = false;
        SetCalibrationObjectsActive(false);
        calibrationObjects.SetActive(false);

        Debug.Log("Calibration final results: Offset = " + positionOffset + " | Scale = " + scale + " | Rotation = " + rotation);

        finishCalibrationCallback?.Invoke();
        finishCalibrationCallback = null;
    }

    void SetCalibrationObjectsActive(bool isActive)
    {        
        XAxisFirstObject.SetActive(isActive);
        XAxisSecondObject.SetActive(isActive);
        YAxisFirstObject.SetActive(isActive);
        YAxisSecondObject.SetActive(isActive);
        ZAxisFirstObject.SetActive(isActive);
        ZAxisSecondObject.SetActive(isActive);
    }
    
    private void ReceiveData()
    {
        client = new UdpClient(port);
        print("Starting UDP Server to get the HTC VIVE information");
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);
                for (int i = 0; i < float_array.Length; i++)
                {
                    float_array[i] = BitConverter.ToDouble(data, i * sizeof(double));
                }
                UpdateTriggerState(BitConverter.ToDouble(data, float_array.Length * sizeof(double)) != 0);
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    void UpdateTriggerState(bool isTriggerActive)
    {
        updateTriggerMutex.WaitOne();

        if (_lastProcessedTriggerState && isTriggerActive)
        {            
            OnTrigger?.Invoke();
        }
        else if (!_lastProcessedTriggerState && isTriggerActive)
        {
            if (currentFrame > frameTriggerDown + numFramesToWaitBeforeNewTrigger)
            {
                // Filter sequential trigger actions, to avoid multiple triggers in a short period of time
                frameTriggerDown = currentFrame;
                OnTriggerDown?.Invoke();
            }
        }
        else if (_lastProcessedTriggerState && !isTriggerActive)
        {            
            frameTriggerUp = currentFrame;
            OnTriggerUp?.Invoke();
        }
        _lastProcessedTriggerState = isTriggerActive;

        updateTriggerMutex.ReleaseMutex();
    }

    public bool GetTriggerDown()
    {   
        updateTriggerMutex.WaitOne();
        bool result = frameTriggerDown == currentFrame || frameTriggerDown == currentFrame - 1;
        updateTriggerMutex.ReleaseMutex();
        return result;
    }

    public bool GetTrigger()
    {
        updateTriggerMutex.WaitOne();
        bool result = _lastProcessedTriggerState;
        updateTriggerMutex.ReleaseMutex();
        return result;
    }

    public bool GetTriggerUp()
    {
        updateTriggerMutex.WaitOne();
        bool result = frameTriggerUp == currentFrame || frameTriggerUp == currentFrame - 1;
        updateTriggerMutex.ReleaseMutex();
        return result;
    }

    void OnApplicationQuit()
    {
        if (receiveThread != null)
        {
            receiveThread.Abort();
        }
        client.Close();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ViveControllerPositionBehaviour))]
public class VIVEEditorBuilder : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ViveControllerPositionBehaviour myScript = (ViveControllerPositionBehaviour)target;

        if (GUILayout.Button("Start Calibration of Controller"))
        {
            myScript.StartVIVEControllerCalibration(null);
        }
    }
}
#endif
