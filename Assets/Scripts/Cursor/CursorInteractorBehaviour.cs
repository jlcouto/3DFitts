//#define DEBUG_CURSOR
#define DRAG_START_WITH_CURSOR_MOVEMENT
//#define DRAG_START_WITH_CONTINUOUS_SELECTION

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CursorSelectionMethod
{
    DWELL_TIME,
    KEYBOARD_SPACEBAR,
    MOUSE_LEFTCLICK,
    AUTOMATIC_BYCONTACT,
    META2_GRAB,
    LMC_GRAB,
    VIVE_TRIGGER
}

public abstract class CursorPositioningController : MonoBehaviour
{
    public enum CursorHandPosition
    {
        HandTop,
        HandPalm
    }

    public abstract string GetDeviceName();
    public abstract Vector3 GetCurrentCursorPosition();    
    public abstract int GetTrackedHandId();
}

public class CursorInteractorBehaviour : CursorBehaviour
{
    public CursorPositioningController cursorPositionController;

    CursorSelectionMethod _selectionMethod;
    public CursorSelectionMethod selectionMethod
    {
        set {
            _selectionMethod = value;
            switch (_selectionMethod)
            {
                case CursorSelectionMethod.DWELL_TIME:
                    selectionTechnique = new CursorSelectionTechniqueDwell(1f, this);
                    break;
                case CursorSelectionMethod.KEYBOARD_SPACEBAR:
                    selectionTechnique = new CursorSelectionTechniqueKeyboard();
                    break;
                case CursorSelectionMethod.MOUSE_LEFTCLICK:
                    selectionTechnique = new CursorSelectionTechniqueMouse();
                    break;
                case CursorSelectionMethod.META2_GRAB:
                    selectionTechnique = new CursorSelectionTechniqueMeta2Grab(metaHandsProvider);
                    break;
                case CursorSelectionMethod.LMC_GRAB:
                    selectionTechnique = new CursorSelectionTechniqueLeapMotionGrab(leapMotionServiceProvider);
                    break;
                case CursorSelectionMethod.VIVE_TRIGGER:
                    selectionTechnique = new CursorSelectionTechniqueVive(viveController);
                    break;
                default:
                    break;
            }
        }
        get { return _selectionMethod; }
    }

    public Meta.HandsProvider metaHandsProvider;
    public Leap.Unity.LeapServiceProvider leapMotionServiceProvider;
    public ViveControllerPositionBehaviour viveController;

    CursorSelectionTechnique selectionTechnique;

    HashSet<TargetBehaviour> currentTargetsCollidingWithCursor;
    TargetBehaviour currentHighlightedTarget;
    TargetBehaviour currentDraggedTarget;

    TargetBehaviour currentAcquiredTarget;

    bool isDragging = false;

#if DRAG_START_WITH_CURSOR_MOVEMENT
    Vector3 acquiredPosition;
    float distanceToInitiateDrag = 0.005f;
#elif DRAG_START_WITH_CONTINUOUS_SELECTION
    int numFramesSelectionIsActive = 0;
    const int numFramesToStartDragInteraction = 5;
#endif

    private new void Start()
    {
        base.Start();
        currentTargetsCollidingWithCursor = new HashSet<TargetBehaviour>();
        selectionMethod = CursorSelectionMethod.KEYBOARD_SPACEBAR;        
    }

    void Update()
    {
        this.transform.position = cursorPositionController.GetCurrentCursorPosition();

        if (selectionMethod == CursorSelectionMethod.AUTOMATIC_BYCONTACT)
        {
            CheckAutomaticByContactSelection();
        }
        else if (selectionTechnique != null)
        {
            ManageSelectionInteraction(selectionTechnique);
        }
    }

    void ManageSelectionInteraction(CursorSelectionTechnique selectionInteraction)
    {
        if (selectionInteraction.SelectionInteractionStarted())
        {
#if DRAG_START_WITH_CONTINUOUS_SELECTION
            numFramesSelectionIsActive = 0;
#endif
            if (selectionMethod != CursorSelectionMethod.DWELL_TIME)
            {
                RegisterSelectionInformation(Time.realtimeSinceStartup, GetCursorPosition());
            }
            AcquireTarget(currentHighlightedTarget);
            acquiredPosition = GetCursorPosition();
        }

        if (isDragging && selectionInteraction.SelectionInteractionEnded())
        {
            CursorDragTargetEnded(currentDraggedTarget, currentHighlightedTarget);
#if DRAG_START_WITH_CONTINUOUS_SELECTION
            numFramesSelectionIsActive = 0;
#endif
            currentDraggedTarget = null;
            isDragging = false;
        }
        else if (selectionInteraction.SelectionInteractionMantained())
        {
#if DRAG_START_WITH_CONTINUOUS_SELECTION
            numFramesSelectionIsActive++;
            if (!isDragging && numFramesSelectionIsActive > numFramesToStartDragInteraction)
            {
#elif DRAG_START_WITH_CURSOR_MOVEMENT
            float distanceMoved = Vector3.Distance(acquiredPosition, GetCursorPosition());
            if (!isDragging && distanceMoved > distanceToInitiateDrag)
            {
#endif
                isDragging = true;
                currentDraggedTarget = currentHighlightedTarget;
                CursorDragTargetStarted(currentDraggedTarget);
            }
        }
        else
        {
            isDragging = false;
#if DRAG_START_WITH_CONTINUOUS_SELECTION
            numFramesSelectionIsActive = 0;
#endif
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
#if DEBUG_CURSOR
                Debug.Log("AUTOMATIC SELECTION Acquired");
#endif
            }
            if (currentHighlightedTarget != currentAcquiredTarget)
            {            
                AcquireTarget(currentHighlightedTarget);
                currentAcquiredTarget = currentHighlightedTarget;
#if DEBUG_CURSOR
                Debug.Log("AUTOMATIC SELECTION Acquired");
#endif
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
#if DEBUG_CURSOR
                    Debug.Log("AUTOMATIC SELECTION Released");
#endif
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

        ExecuteForEachCursorListener((ICursorListener listener) => { listener.CursorEnteredTarget(target); });        
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

        ExecuteForEachCursorListener((ICursorListener listener) => { listener.CursorExitedTarget(target); });        
    }

    void ExecuteForEachCursorListener(System.Action<ICursorListener> action)
    {
        ICursorListener[] listenersCopy = new ICursorListener[listeners.Count];
        listeners.CopyTo(listenersCopy);
        foreach (var listener in listenersCopy)
        {
            action?.Invoke(listener);
        }
    }

    public override void AcquireTarget(TargetBehaviour target)
    {        
        ExecuteForEachCursorListener((ICursorListener listener) => { listener.CursorAcquiredTarget(target); });        

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
        ExecuteForEachCursorListener((ICursorListener listener) => { listener.CursorDragTargetStarted(target); });        
    }

    public override void CursorDragTargetEnded(TargetBehaviour draggedTarget, TargetBehaviour receivingTarget)
    {
        ExecuteForEachCursorListener((ICursorListener listener) => { listener.CursorDragTargetEnded(draggedTarget, receivingTarget); });        
    }

    public override Vector3 GetCursorPosition()
    {
        return this.transform.position;
    }

    public override int GetTrackedHandId()
    {
        return cursorPositionController.GetTrackedHandId();
    }

    public override string GetDeviceName()
    {
        return cursorPositionController.GetDeviceName();
    }

    public override string GetInteractionTechniqueName()
    {
        return selectionTechnique.GetInteractionName();
    }
}
