using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMousePositionBehaviour : MonoBehaviour {
    Vector2 GameWindowSize;

    // Use this for initialization
    void Start()
    {
        GameWindowSize = GetMainGameViewSize();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out hit, 5.0f))
            {
                Debug.Log("You selected the " + hit.transform.name); // ensure you picked right object
            }
        }

        //transform.position = new Vector3(TargetsPlane.targetDistance * (mousePos.x - GameWindowSize.x/2)/GameWindowSize.x, 0, TargetsPlane.targetDistance * (mousePos.y - GameWindowSize.y/2)/GameWindowSize.y);
    }

    public static Vector2 GetMainGameViewSize()
    {
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        System.Object Res = GetSizeOfMainGameView.Invoke(null, null);
        return (Vector2)Res;
    }
}