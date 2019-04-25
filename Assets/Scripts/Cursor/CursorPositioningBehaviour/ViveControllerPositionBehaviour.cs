using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEditor;

public class ViveControllerPositionBehaviour : CursorPositioningController
{
    public Vector3 offset = Vector3.zero;
    public Vector3 positionOffset = Vector3.zero;
    public Vector3 scale = Vector3.one;
    public Vector3 rotation;

    public GameObject XAxisFirstObject;
    public GameObject XAxisSecondObject;
    public GameObject YAxisFirstObject;
    public GameObject YAxisSecondObject;
    public GameObject ZAxisFirstObject;
    public GameObject ZAxisSecondObject;

    public GameObject finalFirstCube;
    public GameObject finalSecondCube;
    public GameObject finalOffsetCube;

    Vector3 finalFirst;
    Vector3 finalSecond;

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
    
    Vector3 lastCursorPosition;
    Quaternion lastCursorRotation;
    
    Matrix4x4 translationMatrix;
    Matrix4x4 scalingMatrix;
    Matrix4x4 rotationMatrix;

    /* UDP communication to read controller data */
    Thread receiveThread;
    UdpClient client;
    private int port = 8051;
    private bool _lastProcessedTriggerState;
    private bool _isTriggerDown;
    private Double[] float_array; 

    public override Vector3 GetCurrentCursorPosition()
    {
        return lastCursorPosition;
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
    }
    
    void Update()
    {
        Vector3 rawPosition = new Vector3((float)float_array[0], (float)float_array[1], (float)float_array[2]);
        Quaternion rawRotation = new Quaternion((float)float_array[3], (float)float_array[4], (float)float_array[5], (float)float_array[6]);
        ConvertVIVEToUnity(ref rawPosition, ref rawRotation);
        
        if (isCalibrating && _isTriggerDown)
        {
            if (gotFirstPoint)
            {
                secondPoint[currentCalibrationAxis] = rawPosition;                    
                FinishAxisCalibration();
                gotFirstPoint = false;                    
            }
            else
            {
                firstPoint[currentCalibrationAxis] = rawPosition;
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

        //translationMatrix = Matrix4x4.Translate(-positionOffset);
        //scalingMatrix = Matrix4x4.Scale(scale);
        //rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(rotation.x, rotation.y, rotation.z));

        lastCursorPosition = offset + scalingMatrix.MultiplyPoint(rotationMatrix.MultiplyPoint(translationMatrix.MultiplyPoint(rawPosition)));
        lastCursorRotation = rawRotation;

        _isTriggerDown = false;
    }

    public static void ConvertVIVEToUnity(ref Vector3 pos, ref Quaternion rot)
    {
        pos = new Vector3(pos[0], pos[1], -pos[2]);

        rot = new Quaternion(-rot[2], -rot[1], rot[0], rot[3]);
        Quaternion rot180 = Quaternion.AngleAxis(180.0f, Vector3.up);
        Quaternion rot90 = Quaternion.AngleAxis(90.0f, Vector3.forward);

        rot = rot * rot90 * rot180;
    }

    void OnApplicationQuit()
    {
        if (receiveThread != null)
            receiveThread.Abort();
        client.Close();
    }

    private void ReceiveData()
    {
        port = 8051;
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

        // scale
        scaleByAxis[currentCalibrationAxis] = sizeMeta.magnitude / sizeVive.magnitude;

        // offset
        Vector3 centerVive = firstPoint[currentCalibrationAxis] + sizeVive / 2;
        Vector3 centerMeta = desiredFirstPoint[currentCalibrationAxis] + sizeMeta / 2;

        offsetByAxis[currentCalibrationAxis] = centerVive;
        metaOffsetByAxis[currentCalibrationAxis] = centerMeta;

        // rotation
        Vector3 realDataAxis = secondPoint[currentCalibrationAxis] - firstPoint[currentCalibrationAxis];
        
        // Find next rotation
        rotationByAxis[currentCalibrationAxis] = Quaternion.FromToRotation(realDataAxis, desiredSecondPoint[currentCalibrationAxis] - desiredFirstPoint[currentCalibrationAxis]);

        positionOffset = offsetByAxis[currentCalibrationAxis];

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

        translationMatrix = Matrix4x4.Translate(-positionOffset);
        scalingMatrix = Matrix4x4.Scale(scale);

        Vector3 axis = Vector3.zero;        
        for (int i = 0; i <= currentCalibrationAxis; i++)
        {
            axis += (secondPoint[i] - firstPoint[i]).normalized;            
        }

        Vector3 reference = Vector3.right;
        if (currentCalibrationAxis >= 1)
        {
            reference += Vector3.up;
        }
        if (currentCalibrationAxis >= 2);
        {
            reference += Vector3.forward;
        }
        //rotation = Quaternion.FromToRotation(axis, reference);
        rotation = rotationByAxis[currentCalibrationAxis].eulerAngles;
        rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(rotation.x, rotation.y, rotation.z));
        
        finalOffsetCube.transform.position = metaOffsetByAxis[currentCalibrationAxis] + translationMatrix.MultiplyPoint(centerVive);
        finalFirstCube.transform.position = metaOffsetByAxis[currentCalibrationAxis] + rotationMatrix.MultiplyPoint(scalingMatrix.MultiplyPoint(translationMatrix.MultiplyPoint(firstPoint[currentCalibrationAxis])));
        finalSecondCube.transform.position = metaOffsetByAxis[currentCalibrationAxis] + rotationMatrix.MultiplyPoint(scalingMatrix.MultiplyPoint(translationMatrix.MultiplyPoint(secondPoint[currentCalibrationAxis])));
        
        Debug.Log("Calibration result: First = " + firstPoint[currentCalibrationAxis] + " | Second = " + secondPoint[currentCalibrationAxis] + " | Offset = " + offsetByAxis[currentCalibrationAxis] + " | scaling = " + scaleByAxis[currentCalibrationAxis] + " | rot = " + rotationByAxis[currentCalibrationAxis].eulerAngles + " | Meta offset = " + metaOffsetByAxis[currentCalibrationAxis]);

        currentCalibrationAxis++;

        if (isZAxisCalibrated)
        {
            FinishCalibration();
        }
    }

    void FinishCalibration()
    {
        //Vector3 measuredXAxis = (secondPoint[0] - firstPoint[0]).normalized;
        //Vector3 measuredYAxis = (secondPoint[1] - firstPoint[1]).normalized;
        //Vector3 measuredZAxis = (secondPoint[2] - firstPoint[2]).normalized;
        //Vector3 measuredSystemCoordinatesOrientation = (measuredXAxis + measuredYAxis + measuredZAxis).normalized;

        //Vector3 systemCoordinatesOrientation = Vector3.one;

        //rotation = Quaternion.FromToRotation(measuredSystemCoordinatesOrientation, systemCoordinatesOrientation).eulerAngles;
        rotation = rotationByAxis[2].eulerAngles;
        rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(rotation.x, rotation.y, rotation.z));

        // Computes final offset
        positionOffset = Vector3.zero;
        for (int i = 0; i < 3; i++)
        {
            positionOffset += offsetByAxis[i];
        }
        positionOffset /= 3;
        translationMatrix = Matrix4x4.Translate(-positionOffset);

        scale.x = scaleByAxis[0];
        scale.y = scaleByAxis[1];
        scale.z = scaleByAxis[2];
        scalingMatrix = Matrix4x4.Scale(scale);

        Debug.Log("Calibration final results: Offset = " + positionOffset + " | Scale = " + scale + " | Rotation = " + rotation);
        
        isCalibrating = false;
        SetCalibrationObjectsActive(false);
    }

    void UpdateTriggerState(bool isTriggerActive)
    {
        if (_lastProcessedTriggerState && isTriggerActive)
        {
            OnTriggerEvent();
        }
        else if (!_lastProcessedTriggerState && isTriggerActive)
        {
            _isTriggerDown = true;
            OnTriggerDownEvent();
        }
        else if (_lastProcessedTriggerState && !isTriggerActive)
        {
            OnTriggerUpEvent();
        }
        _lastProcessedTriggerState = isTriggerActive;
    }

    void OnTriggerDownEvent()
    {

    }

    void OnTriggerEvent()
    {

    }

    void OnTriggerUpEvent()
    {

    }

    public void StartVIVEControllerCalibration()
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

        SetCalibrationObjectsActive(false);
        XAxisFirstObject.SetActive(true);        
    }

    void SetCalibrationObjectsActive(bool isActive)
    {        
        XAxisFirstObject.SetActive(isActive);
        XAxisSecondObject.SetActive(isActive);
        YAxisFirstObject.SetActive(isActive);
        YAxisSecondObject.SetActive(isActive);
        ZAxisFirstObject.SetActive(isActive);
        ZAxisSecondObject.SetActive(isActive);
        finalFirstCube.SetActive(isActive);
        finalSecondCube.SetActive(isActive);
        finalOffsetCube.SetActive(isActive);
    }
}


[CustomEditor(typeof(ViveControllerPositionBehaviour))]
public class VIVEEditorBuilder : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ViveControllerPositionBehaviour myScript = (ViveControllerPositionBehaviour)target;

        if (GUILayout.Button("Start Calibration of Controller"))
        {
            myScript.StartVIVEControllerCalibration();
        }
    }
}
