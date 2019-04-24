using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEditor;

public class ViveControllerPositionBehaviour : CursorPositioningController
{
    public Vector3 positionOffset = Vector3.zero;
    public Vector3 scale = Vector3.one;
    public Quaternion rotation;

    public GameObject XAxisFirstObject;
    public GameObject XAxisSecondObject;
    public GameObject YAxisFirstObject;
    public GameObject YAxisSecondObject;
    public GameObject ZAxisFirstObject;
    public GameObject ZAxisSecondObject;

    public GameObject finalFirstCube;
    public GameObject finalSecondCube;

    Vector3 finalFirst;
    Vector3 finalSecond;

    bool isCalibrating = false;
    bool gotFirstPoint = false;
    bool isXAxisCalibrated = false;
    bool isYAxisCalibrated = false;
    bool isZAxisCalibrated = false;

    Vector3[] firstPoint;
    Vector3[] secondPoint;
    Vector3[] offsetByAxis;
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
        offsetByAxis = new Vector3[3];
        scaleByAxis = new float[3];
        rotationByAxis = new Quaternion[3];

        translationMatrix = Matrix4x4.identity;
        scalingMatrix = Matrix4x4.identity;
        rotationMatrix = Matrix4x4.identity;
    }
    
    void Update()
    {
        Vector3 pos = new Vector3((float)float_array[0], (float)float_array[1], (float)float_array[2]);
        Quaternion rot = new Quaternion((float)float_array[3], (float)float_array[4], (float)float_array[5], (float)float_array[6]);
        ConvertVIVEToUnity(ref pos, ref rot);

        translationMatrix = Matrix4x4.Translate(-positionOffset);
        scalingMatrix = Matrix4x4.Scale(scale);
        rotationMatrix = Matrix4x4.Rotate(rotation);

        lastCursorPosition = rotationMatrix.MultiplyPoint(scalingMatrix.MultiplyPoint(translationMatrix.MultiplyPoint(pos)));
        lastCursorRotation = rot;
        
        if (isCalibrating && _isTriggerDown)
        {
            if (gotFirstPoint)
            {
                secondPoint[currentCalibrationAxis] = lastCursorPosition;                    
                FinishAxisCalibration();
                gotFirstPoint = false;                    
            }
            else
            {
                firstPoint[currentCalibrationAxis] = lastCursorPosition;
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
        Vector3 desiredFirstPoint;
        Vector3 desiredSecondPoint;
        if (isXAxisCalibrated)
        {
            if (isYAxisCalibrated)
            {
                isZAxisCalibrated = true;
                ZAxisFirstObject.SetActive(false);
                ZAxisSecondObject.SetActive(false);                

                desiredFirstPoint = ZAxisFirstObject.transform.position;
                desiredSecondPoint = ZAxisSecondObject.transform.position;

                FinishCalibration();
            }
            else
            {
                isYAxisCalibrated = true;
                YAxisFirstObject.SetActive(false);
                YAxisSecondObject.SetActive(false);
                ZAxisFirstObject.SetActive(true);
                ZAxisSecondObject.SetActive(false);

                desiredFirstPoint = YAxisFirstObject.transform.position;
                desiredSecondPoint = YAxisSecondObject.transform.position;
            }
        }
        else
        {
            isXAxisCalibrated = true;
            XAxisFirstObject.SetActive(false);
            XAxisSecondObject.SetActive(false);
            YAxisFirstObject.SetActive(true);
            YAxisSecondObject.SetActive(false);

            desiredFirstPoint = XAxisFirstObject.transform.position;
            desiredSecondPoint = XAxisSecondObject.transform.position;
        }

        Vector3 sizeVive = (secondPoint[currentCalibrationAxis] - firstPoint[currentCalibrationAxis]);
        Vector3 sizeMeta = (desiredSecondPoint - desiredFirstPoint);

        // scale
        scaleByAxis[currentCalibrationAxis] = sizeMeta.magnitude / sizeVive.magnitude;

        // offset
        Vector3 centerVive = firstPoint[currentCalibrationAxis] + sizeVive / 2;
        Vector3 centerMeta = desiredFirstPoint + sizeMeta / 2;
        offsetByAxis[currentCalibrationAxis] = centerVive - centerMeta;

        // rotation
        Vector3 realDataAxis = secondPoint[currentCalibrationAxis] - firstPoint[currentCalibrationAxis];
        
        // Apply rotations so far
        //for (int i = 0; i < currentCalibrationAxis; i++)
        //{
        //    realDataAxis = rotationByAxis[i] * realDataAxis;
        //}

        // Find next rotation
        rotationByAxis[currentCalibrationAxis] = Quaternion.FromToRotation(realDataAxis, desiredSecondPoint - desiredFirstPoint);

        //positionOffset = Vector3.zero;
        //for (int i = 0; i <= currentCalibrationAxis; i++)
        //{
        //    positionOffset += offsetByAxis[i];
        //    if (i == 0)
        //    {
        //        scale.x = scaleByAxis[0];
        //    }
        //    else if (i == 1)
        //    {
        //        scale.y = scaleByAxis[1];
        //    }
        //    else
        //    {
        //        scale.z = scaleByAxis[2];
        //    }
        //    rotation *= rotationByAxis[i];
        //}
        //positionOffset /= (currentCalibrationAxis + 1);
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
        rotationMatrix = Matrix4x4.Rotate(rotation);

        finalFirstCube.transform.position = rotationMatrix.MultiplyPoint(scalingMatrix.MultiplyPoint(translationMatrix.MultiplyPoint(firstPoint[currentCalibrationAxis])));
        finalSecondCube.transform.position = rotationMatrix.MultiplyPoint(scalingMatrix.MultiplyPoint(translationMatrix.MultiplyPoint(secondPoint[currentCalibrationAxis])));
        
        Debug.Log("Calibration result: First = " + firstPoint + " | Second = " + secondPoint + " | Offset = " + offsetByAxis[currentCalibrationAxis] + " | scaling = " + scaleByAxis[currentCalibrationAxis] + " | rot = " + rotationByAxis[currentCalibrationAxis]);

        currentCalibrationAxis++;
    }

    void FinishCalibration()
    {
        // Computes final offset
        positionOffset = Vector3.zero;
        for (int i = 0; i < 3; i++)
        {
            positionOffset += offsetByAxis[i];
        }
        positionOffset /= 3;

        scale.x = scaleByAxis[0];
        scale.y = scaleByAxis[1];
        scale.z = scaleByAxis[2];

        rotation = rotationByAxis[0];

        translationMatrix = Matrix4x4.Translate(-positionOffset);
        scalingMatrix = Matrix4x4.Scale(scale);
        rotationMatrix = Matrix4x4.Rotate(rotation);

        isCalibrating = false;
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
        
        isCalibrating = true;
        isXAxisCalibrated = false;
        isYAxisCalibrated = false;
        isZAxisCalibrated = false;
        gotFirstPoint = false;

        currentCalibrationAxis = 0;

        XAxisFirstObject.SetActive(true);
        XAxisSecondObject.SetActive(false);
        YAxisFirstObject.SetActive(false);
        YAxisSecondObject.SetActive(false);
        ZAxisFirstObject.SetActive(false);
        ZAxisSecondObject.SetActive(false);
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
