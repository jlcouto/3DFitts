using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CursorSelectionMethod
{
    AUTOMATIC_BYCONTACT = 0,
    KEYBOARD_SPACEBAR,
    MOUSE_LEFTCLICK
}

public class CursorInteractorBehaviour : CursorBehaviour
{
    public CursorSelectionMethod selectionMethod;

    TargetBehaviour currentHighlightedTarget;
  
    void Update()
    {
        switch (selectionMethod)
        {
            case CursorSelectionMethod.KEYBOARD_SPACEBAR:
                CheckSpaceBarSelection();
                break;
            case CursorSelectionMethod.MOUSE_LEFTCLICK:
                break;
            case CursorSelectionMethod.AUTOMATIC_BYCONTACT:
            default:
                break;
        }

        
    }

    void CheckSpaceBarSelection()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AcquireTarget(currentHighlightedTarget);
        }
    }

    void CheckMouseLeftClickSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            AcquireTarget(currentHighlightedTarget);
        }
    }

    void CheckAutomaticByContactSelection()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        var targetBehaviour = other.GetComponent<TargetBehaviour>();
        if (targetBehaviour != null)
        {
            EnterTarget(targetBehaviour);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var targetBehaviour = other.GetComponent<TargetBehaviour>();
        if (targetBehaviour != null)
        {
            ExitTarget(targetBehaviour);
        }
    }

    public override void EnterTarget(TargetBehaviour target)
    {
        target.HighlightTarget();
        currentHighlightedTarget = target;
        if (listener != null)
        {
            listener.CursorEnteredTarget(target);
        }
    }

    public override void ExitTarget(TargetBehaviour target)
    {
        target.UnhighlightTarget();
        currentHighlightedTarget = null;
        if (listener != null)
        {
            listener.CursorExitedTarget(target);
        }
    }

    public override void AcquireTarget(TargetBehaviour target)
    {
        if (listener != null)
        {
            listener.CursorAcquiredTarget(target);
        }

#if DEBUG
        if (target != null)
        {
            Debug.Log("Acquired target: " + target.name);
        }
        else
        {
            Debug.Log("Acquired target: none");
        }
#endif
    }

    public override Vector3 GetCursorPosition()
    {
        return this.transform.position;
    }
}
