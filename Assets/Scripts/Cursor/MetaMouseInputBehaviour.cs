using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.Mouse;

public class MetaMouseInputBehaviour : CursorPositioningController
{
    Vector3 mousePosition;

    public Transform targetPlane;
    public PlaneOrientation plane = PlaneOrientation.PlaneXY;
    public Vector2 screenSize = new Vector2(1024, 768);
    public Vector3 spaceSize = new Vector3(1, 1, 1);
    public Vector3 offset = new Vector3(0, 0, 0);

    private void Update()
    {
        Vector3 screenPos = Input.mousePosition;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            mousePosition = new Vector3(0, 0, 0);
        }
        else
        {
            switch (plane)
            {
                case PlaneOrientation.PlaneXY:
                    mousePosition.x = screenPos.x / screenSize.x * spaceSize.x + offset.x;
                    mousePosition.y = offset.y;
                    mousePosition.z = screenPos.y / screenSize.y * spaceSize.y + offset.z;
                    break;
                case PlaneOrientation.PlaneYZ:
                    mousePosition.x = screenPos.x / screenSize.x * spaceSize.x + offset.x;
                    mousePosition.y = screenPos.y / screenSize.y * spaceSize.y + offset.y;
                    mousePosition.z = offset.z;
                    break;
                case PlaneOrientation.PlaneZX:
                    mousePosition.x = offset.x;
                    mousePosition.y = screenPos.x / screenSize.x * spaceSize.x + offset.y;
                    mousePosition.z = screenPos.y / screenSize.y * spaceSize.y + offset.z;
                    break;
            }
        }
    }

    public override Vector3 GetCurrentCursorPosition()
    {
        return mousePosition;
    }
}