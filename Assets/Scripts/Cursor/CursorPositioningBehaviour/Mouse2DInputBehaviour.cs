using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.Mouse;

public class Mouse2DInputBehaviour : CursorPositioningController
{
    Vector3 mousePosition;

    public Transform targetPlane;
    public PlaneOrientation plane = PlaneOrientation.PlaneXY;
    public Vector2 screenSize = new Vector2(1920, 1080);
    public Vector3 spaceSize = new Vector3(1, 1, 1);
    public bool manualOffset = false;
    public Vector3 offset = new Vector3(0, 0, 0);

    private void Update()
    {
        Vector3 screenPos = Input.mousePosition;

        if (!manualOffset)
        {
            offset = targetPlane.transform.position;
        }

        if (Input.GetKey(KeyCode.F7))
        {
            mousePosition = offset;
        }
        else
        {
            float xCoord = (Mathf.Clamp01(screenPos.x / Screen.width) - 0.5f) * spaceSize.x;
            float yCoord = (Mathf.Clamp01(screenPos.y / Screen.height) - 0.5f) * spaceSize.y;

            switch (plane)
            {
                case PlaneOrientation.PlaneXY:
                    mousePosition.x = xCoord + offset.x;
                    mousePosition.y = offset.y;
                    mousePosition.z = yCoord + offset.z;
                    break;
                case PlaneOrientation.PlaneYZ:
                    mousePosition.x = Mathf.Cos(-Mathf.PI*targetPlane.rotation.eulerAngles.y/180) * xCoord + offset.x;
                    mousePosition.y = yCoord + offset.y;
                    mousePosition.z = Mathf.Sin(-Mathf.PI*targetPlane.rotation.eulerAngles.y/180) * xCoord + offset.z;
                    break;
                case PlaneOrientation.PlaneZX:
                    mousePosition.x = offset.x;
                    mousePosition.y = xCoord + offset.y;
                    mousePosition.z = yCoord + offset.z;
                    break;
            }
        }
    }

    public override Vector3 GetCurrentCursorPosition()
    {
        return mousePosition;
    }
}