//#define DEBUG_CURSOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CursorSelectionMethod
{
    AUTOMATIC_BYCONTACT = 0,
    KEYBOARD_SPACEBAR,
    MOUSE_LEFTCLICK
}

public abstract class CursorPositioningController : MonoBehaviour
{
    public abstract Vector3 GetCurrentCursorPosition();
}

public class CursorInteractorBehaviour : CursorBehaviour
{
    public CursorPositioningController cursorPositionController;
    public CursorSelectionMethod selectionMethod = CursorSelectionMethod.AUTOMATIC_BYCONTACT;

    HashSet<TargetBehaviour> currentTargetsCollidingWithCursor;
    TargetBehaviour currentHighlightedTarget;
    TargetBehaviour currentDraggedTarget;

    TargetBehaviour currentAcquiredTarget;

    int numFramesSelectionIsActive = 0;
    const int numFramesToStartDragInteraction = 5;
    Vector3 acquiredPosition;
    float distanceToInitiateDrag = 0.005f;
    bool isDragging = false;
     
    private void Start()
    {
        currentTargetsCollidingWithCursor = new HashSet<TargetBehaviour>();
    }

    void Update()
    {
        this.transform.position = cursorPositionController.GetCurrentCursorPosition();

        switch (selectionMethod)
        {
            case CursorSelectionMethod.KEYBOARD_SPACEBAR:
                ManageSelectionInteraction(
                    input => { return Input.GetKeyDown(KeyCode.Space); },
                    input => { return Input.GetKey(KeyCode.Space); },
                    input => { return Input.GetKeyUp(KeyCode.Space); }
                );
                break;
            case CursorSelectionMethod.MOUSE_LEFTCLICK:
                ManageSelectionInteraction(
                    input => { return Input.GetMouseButtonDown(0); },
                    input => { return Input.GetMouseButton(0); },
                    input => { return Input.GetMouseButtonUp(0); }
                );
                break;
            case CursorSelectionMethod.AUTOMATIC_BYCONTACT:
            default:
                CheckAutomaticByContactSelection();
                break;
        }  
    }

    void ManageSelectionInteraction(
        System.Predicate<bool> SelectionInteractionStared,
        System.Predicate<bool> SelectionInteractionMantained,
        System.Predicate<bool> SelectionInteractionEnded)
    {
        if (SelectionInteractionStared(true))
        {
            numFramesSelectionIsActive = 0;
            AcquireTarget(currentHighlightedTarget);
            acquiredPosition = GetCursorPosition();
        }

        if (isDragging && SelectionInteractionEnded(true))
        {
            CursorDragTargetEnded(currentDraggedTarget, currentHighlightedTarget);
            numFramesSelectionIsActive = 0;
            currentDraggedTarget = null;
            isDragging = false;
        }
        else if (SelectionInteractionMantained(true))
        {
            numFramesSelectionIsActive++;
            float distanceMoved = Vector3.Distance(acquiredPosition, GetCursorPosition());
            //if (!isDragging && numFramesSelectionIsActive > numFramesToStartDragInteraction)
            if (!isDragging && distanceMoved > distanceToInitiateDrag)
            {
                isDragging = true;
                currentDraggedTarget = currentHighlightedTarget;
                CursorDragTargetStarted(currentDraggedTarget);
            }
        }
        else
        {
            isDragging = false;
            numFramesSelectionIsActive = 0;
        }
    }

    void CheckAutomaticByContactSelection()
    {
        if (currentHighlightedTarget != null)
        {
            if (currentAcquiredTarget == null)
            {
                AcquireTarget(currentHighlightedTarget);
                currentAcquiredTarget = currentHighlightedTarget;
                Debug.Log("AUTOMATIC SELECTION Acquired");
            }
            if (currentHighlightedTarget != currentAcquiredTarget)
            {            
                AcquireTarget(currentHighlightedTarget);
                currentAcquiredTarget = currentHighlightedTarget;
                Debug.Log("AUTOMATIC SELECTION Acquired");
            }  
        }
        else
        {
            if (currentAcquiredTarget != null)
            {
                Vector3 cursorTargetDistance = currentAcquiredTarget.position - GetCursorPosition();
                if (cursorTargetDistance.magnitude > currentAcquiredTarget.localScale.magnitude)
                {
                    currentAcquiredTarget = null;
                    Debug.Log("AUTOMATIC SELECTION Released");
                }
            }
        }
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

        currentTargetsCollidingWithCursor.Add(target);
        currentHighlightedTarget = target;

#if DEBUG_CURSOR
        Debug.Log("TargetEnter: " + target.name + " pos= " + target.transform.position);
#endif

        if (listener != null)
        {
            listener.CursorEnteredTarget(target);         
        }
    }

    public override void ExitTarget(TargetBehaviour target)
    {
        target.UnhighlightTarget();

        currentTargetsCollidingWithCursor.Remove(target);

        if (currentTargetsCollidingWithCursor.Count > 0)
        {
            foreach (TargetBehaviour t in currentTargetsCollidingWithCursor)
            {
                currentHighlightedTarget = t;
                break;
            }
        }
        else
        {
            currentHighlightedTarget = null;
        }
       
#if DEBUG_CURSOR
        Debug.Log("TargetExit: " + target.name + " pos= " + target.transform.position);
#endif

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

#if DEBUG_CURSOR
        if (target != null)
        {
            Debug.Log("Acquired target: " + target.name);
        }
        else
        {
            Debug.Log("Acquired target: none, cursor pos = " + cursorPositionController.GetCurrentCursorPosition());
        }
#endif
    }

    public override void CursorDragTargetStarted(TargetBehaviour target)
    {
        if (listener != null)
        {
            listener.CursorDragTargetStarted(target);
        }  
    }

    public override void CursorDragTargetEnded(TargetBehaviour draggedTarget, TargetBehaviour receivingTarget)
    {
        if (listener != null)
        {
            listener.CursorDragTargetEnded(draggedTarget, receivingTarget);
        }
    }

    public override Vector3 GetCursorPosition()
    {
        return this.transform.position;
    }
}
