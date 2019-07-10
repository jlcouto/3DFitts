using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.Mouse;

public class Mouse2DInputBehaviour : CursorPositioningController
{
    Vector3 mousePosition;

    public Transform targetPlane;
    public Transform camera;
    public PlaneOrientation plane = PlaneOrientation.PlaneXY;
    public float spaceSize = 1;

    private void Update()
    {
        Vector3 screenPos = Input.mousePosition;

        float minScreenSize = Mathf.Min(Screen.width, Screen.height);
        //float xCoord = Mathf.Clamp((screenPos.x - 0.5f*Screen.width) / minScreenSize, -1, 1) * spaceSize.x;
        //float yCoord = Mathf.Clamp((screenPos.y - 0.5f*Screen.height) / minScreenSize, -1, 1) * spaceSize.y;
        float xCoord = (screenPos.x - 0.5f * Screen.width) * spaceSize / minScreenSize;
        float yCoord = (screenPos.y - 0.5f * Screen.height) * spaceSize / minScreenSize;

        Vector3 offset = targetPlane.position;

        switch (plane)
        {
            case PlaneOrientation.PlaneZX:
                mousePosition.x = xCoord + offset.x;
                mousePosition.y = offset.y;
                mousePosition.z = yCoord + offset.z;
                break;
            case PlaneOrientation.PlaneXY:
                xCoord += camera.localPosition.x;
                yCoord += camera.localPosition.y;
                mousePosition.x = Mathf.Cos(-Mathf.PI * targetPlane.rotation.eulerAngles.y / 180) * xCoord + offset.x;
                mousePosition.y = yCoord + offset.y;
                mousePosition.z = Mathf.Sin(-Mathf.PI * targetPlane.rotation.eulerAngles.y / 180) * xCoord + offset.z;
                break;
            case PlaneOrientation.PlaneYZ:
                mousePosition.x = offset.x;
                mousePosition.y = xCoord + offset.y;
                mousePosition.z = yCoord + offset.z;
                break;
        }

        //ManualCameraAdjustment();
    }

    void ManualCameraAdjustment() {
        /*
        * This code can be used to manually offset the camera using the keyboard arrows.
        * I used this to move the targets plane so it would overlap the targets in the GoFitts software.
        * This way, I could test both softwares with an autoclicker (that generated the same mouse inputs),
        * allowing me to validate the algorithm implemented in this software.
        */   
        float c = 0.001f/SharedData.currentConfiguration.screenPixelsPerMillimeter; // 1 pixel
        Vector3 p = camera.transform.position;
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            p.y += c;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            p.y -= c;
        }   
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            p.x += c;
        }   
        else if (Input.GetKeyDown(KeyCode.RightArrow)) {
            p.x -= c;
        }
        camera.transform.position = p;
    }

    public override string GetDeviceName()
    {
        return "Mouse";
    }

    public override Vector3 GetCurrentCursorPosition()
    {
        return mousePosition;
    }

    public override int GetTrackedHandId()
    {
        return 0;
    }
}