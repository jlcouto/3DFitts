using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using Meta;

public class CursorSelectionTechniqueKeyboard : CursorSelectionTechnique
{
    KeyCode key = KeyCode.Space;

    public override bool SelectionInteractionStarted()
    {
        return Input.GetKeyDown(key);
    }

    public override bool SelectionInteractionMantained()
    {
        return Input.GetKey(key);
    }

    public override bool SelectionInteractionEnded()
    {
        return Input.GetKeyUp(key);
    }

    public override string GetInteractionName()
    {
        if (key == KeyCode.Space)
        {
            return "Keyboard_SpaceBar";
        }
        else
        {
            return "Keyboard";
        }        
    }
}