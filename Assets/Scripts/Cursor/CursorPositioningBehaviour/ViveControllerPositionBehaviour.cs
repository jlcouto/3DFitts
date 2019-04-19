using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEditor;

public class ViveControllerPositionBehaviour : CursorPositioningController
{
    public Vector3 positionOffset = Vector3.zero;

    public GameObject XAxisFirstObject;
    public GameObject XAxisSecondObject;
    public GameObject YAxisFirstObject;
    public GameObject YAxisSecondObject;
    public GameObject ZAxisFirstObject;
    public GameObject ZAxisSecondObject;

    public GameObject finalFirstCube;
    public GameObject finalSecondCube;

    public Vector3 rotation;

    Vector3 finalFirst;
    Vector3 finalSecond;

    bool isCalibrating = false;
    bool isGettingCalibrationData = false;
    bool isXAxisCalibrated = false;
    bool isYAxisCalibrated = false;
    bool isZAxisCalibrated = false;

    bool firstCalibrationTrigger = false;
    Vector3[] positionHistory;
    Vector3 firstPoint;
    Vector3 secondPoint;
    int currentCalibratingIndex = 0;

    Vector3 lastCursorPosition;
    Quaternion lastCursorRotation;

    Thread receiveThread;
    UdpClient client;
    private bool _lastProcessedTriggerState;
    private bool _isTriggerDown;

    private Double[] float_array;
    private int port = 8051;

    public override Vector3 GetCurrentCursorPosition()
    {
        return lastCursorPosition;
    }

    // Use this for initialization
    void Start()
    {
        float_array = new Double[7];
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        lastCursorPosition = new Vector3((float)float_array[2] - positionOffset.x, (float)float_array[1] - positionOffset.y, (float)float_array[0] - positionOffset.z);
        lastCursorRotation = new Quaternion((float)float_array[3], (float)float_array[4], (float)float_array[5], (float)float_array[6]);

        if (Input.GetKeyDown(KeyCode.R))
        {
            Quaternion rot = Quaternion.Euler(rotation);
            finalFirstCube.transform.position = rot * (finalFirst - positionOffset) + positionOffset;
            finalSecondCube.transform.position = rot * (finalSecond - positionOffset) + positionOffset;
        }

        if (isCalibrating)
        {
            if (_isTriggerDown)
            {
                if (isGettingCalibrationData)
                {
                    secondPoint = lastCursorPosition;
                    isGettingCalibrationData = false;
                    FinishAxisCalibration();
                }
                else {
                    firstPoint = lastCursorPosition;
                    isGettingCalibrationData = true;
                }                
            }

            //if (isGettingCalibrationData)
            //{
                //positionHistory[currentCalibratingIndex] = lastCursorPosition;
                //currentCalibratingIndex++;
            //}            
        }

        _isTriggerDown = false;
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

    public void StartVIVEControllerCalibration()
    {
        positionHistory = new Vector3[100];
        currentCalibratingIndex = 0;

        isCalibrating = true;
        isXAxisCalibrated = false;
        isYAxisCalibrated = false;
        isZAxisCalibrated = false;

        XAxisFirstObject.SetActive(true);
    }

    public void FinishAxisCalibration()
    {

        //Vector3 sumAll = Vector3.zero;
        //for (int i = 0; i < currentCalibratingIndex; i++)
        //{
        //    sumAll += positionHistory[i];
        //}
        //positionOffset = sumAll / 100;       

        Vector3 desiredFirstPoint;
        Vector3 desiredSecondPoint;
        if (isXAxisCalibrated)
        {
            if (isYAxisCalibrated)
            {
                isZAxisCalibrated = true;
                ZAxisFirstObject.SetActive(false);
                ZAxisSecondObject.SetActive(false);
                isCalibrating = false;

                desiredFirstPoint = ZAxisFirstObject.transform.position;
                desiredSecondPoint = ZAxisSecondObject.transform.position;
            }
            else
            {
                isYAxisCalibrated = true;
                YAxisFirstObject.SetActive(false);
                YAxisSecondObject.SetActive(false);
                ZAxisFirstObject.SetActive(true);
                ZAxisSecondObject.SetActive(true);

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
            YAxisSecondObject.SetActive(true);

            desiredFirstPoint = XAxisFirstObject.transform.position;
            desiredSecondPoint = XAxisSecondObject.transform.position;        
        }

        Vector3 sizeVive = (secondPoint - firstPoint);
        Vector3 sizeMeta = (desiredSecondPoint - desiredFirstPoint);

        // scale
        float scaling = sizeMeta.magnitude / sizeVive.magnitude;
        Vector3 scaledFirstVive = firstPoint * scaling;
        Vector3 scaledSecondVive = secondPoint * scaling;

        // offset
        Vector3 centerVive = scaling * (firstPoint + sizeVive / 2);
        Vector3 centerMeta = desiredFirstPoint + sizeMeta / 2;
        Vector3 offset = centerVive - centerMeta;

        // rotation
        Vector3 centerV = firstPoint + sizeVive / 2;
        Vector3 centerM = desiredFirstPoint + sizeMeta / 2;

        Quaternion rot = Quaternion.FromToRotation(firstPoint - centerV, desiredFirstPoint - centerM);
        Quaternion rot1 = Quaternion.FromToRotation(scaledFirstVive - offset, desiredFirstPoint);
        Quaternion rot2 = Quaternion.FromToRotation(scaledSecondVive - offset, desiredSecondPoint);
        Quaternion rot3 = Quaternion.FromToRotation(scaledFirstVive, desiredFirstPoint);
        Quaternion rot4 = Quaternion.FromToRotation(scaledSecondVive, desiredSecondPoint);
        Quaternion rot5 = Quaternion.FromToRotation(scaledFirstVive - offset - centerMeta, desiredFirstPoint - centerMeta);
        Quaternion rot6 = Quaternion.FromToRotation(scaledSecondVive - offset - centerMeta, desiredSecondPoint - centerMeta);

        if (isXAxisCalibrated)
        {            
            finalFirstCube.transform.position = (rot * (firstPoint - centerV)) * scaling + centerV;
            finalSecondCube.transform.position = (rot * (secondPoint - centerV)) * scaling + centerV;
        }
        else if (isYAxisCalibrated)
        {
            finalFirstCube.transform.position = (rot * (firstPoint - centerV)) * scaling + centerV + offset;
            finalSecondCube.transform.position = (rot * (secondPoint - centerV)) * scaling + centerV + offset;
        }
        else
        {
            finalFirstCube.transform.position = (rot * (firstPoint - centerV)) * scaling;
            finalSecondCube.transform.position = (rot * (secondPoint - centerV)) * scaling;
        }

        finalFirst = scaledFirstVive - offset;
        finalSecond = scaledSecondVive - offset;

        
        
        Debug.Log("Calibration result: First = " + firstPoint + " | Second = " + secondPoint + " | Offset = " + offset + " | scaling = " + scaling + " | rot1 = " + rot1 + " | rot2 = " + rot2 + " | rot3 = " + rot3 + " | rot4 = " + rot4 + " | rot5 = " + rot5 + " | rot6 = " + rot6);        
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