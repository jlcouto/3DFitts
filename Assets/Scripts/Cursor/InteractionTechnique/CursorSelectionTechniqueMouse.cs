using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using Meta;

public class CursorSelectionTechniqueMouse : CursorSelectionTechnique
{
    int button = 0;

    public override bool SelectionInteractionStarted()
    {
        return Input.GetMouseButtonDown(button);
    }

    public override bool SelectionInteractionMantained()
    {
        return Input.GetMouseButton(button);
    }

    public override bool SelectionInteractionEnded()
    {
        return Input.GetMouseButtonUp(button);
    }
}