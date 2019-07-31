//#define DEBUG_CURSOR
#define DRAG_START_WITH_CURSOR_MOVEMENT
//#define DRAG_START_WITH_CONTINUOUS_SELECTION

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                case CursorSelectionMethod.DwellTime:
                    selectionTechnique = new CursorSelectionTechniqueDwell(this.dwellTime, this);
                    break;
                case CursorSelectionMethod.KeyboardSpaceBar:
                    selectionTechnique = new CursorSelectionTechniqueKeyboard();
                    break;
                case CursorSelectionMethod.MouseLeftButton:
                    selectionTechnique = new CursorSelectionTechniqueMouse();
                    break;
                case CursorSelectionMethod.Meta2GrabInteraction:
                    selectionTechnique = new CursorSelectionTechniqueMeta2Grab(metaHandsProvider);
                    break;
                case CursorSelectionMethod.LeapMotionGrabInteraction:
                    selectionTechnique = new CursorSelectionTechniqueLeapMotionGrab(leapMotionServiceProvider);
                    break;
                case CursorSelectionMethod.VIVETriggerButton:
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
    private float dwellTime = 0.5f;

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
        selectionMethod = CursorSelectionMethod.KeyboardSpaceBar;        
    }

    void Update()
    {
        this.transform.position = cursorPositionController.GetCurrentCursorPosition();

        if (selectionTechnique != null)
        {
            ManageSelectionInteraction(selectionTechnique);
        }
    }

    public void SetDwellTime(float dwellTime)
    {
        this.dwellTime = dwellTime;
        if (selectionTechnique.GetType() == typeof(CursorSelectionTechniqueDwell))
        {
            ((CursorSelectionTechniqueDwell)selectionTechnique).dwellTimeInSeconds = this.dwellTime;
        }      
    }

    void ManageSelectionInteraction(CursorSelectionTechnique selectionInteraction)
    {
        if (selectionInteraction.SelectionInteractionStarted())
        {
#if DRAG_START_WITH_CONTINUOUS_SELECTION
            numFramesSelectionIsActive = 0;
#endif
            acquiredPosition = GetCursorPosition();

            CursorTargetSelectionStarted(currentHighlightedTarget);                                   
        }

        if (selectionInteraction.SelectionInteractionEnded())
        {
            if (isDragging)
            {
                CursorDragTargetEnded(currentDraggedTarget, currentHighlightedTarget);
#if DRAG_START_WITH_CONTINUOUS_SELECTION
            numFramesSelectionIsActive = 0;
#endif
                currentDraggedTarget = null;
                isDragging = false;
            }

            CursorTargetSelectionEnded(currentHighlightedTarget);
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

        // When a TargetBehaviour is destroyed while inside the HashSet, it will become a 'null' entry that must be removed
        currentTargetsCollidingWithCursor.RemoveWhere((TargetBehaviour t) => { return t == null; });

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

    public override void CursorTargetSelectionStarted(TargetBehaviour target)
    {
        ExecuteForEachCursorListener((ICursorListener listener) => { listener.CursorTargetSelectionStarted(target); });        

#if DEBUG_CURSOR
        if (target != null)
        {
            Debug.Log("Target selection started: " + target.name);
        }
        else
        {
            Debug.Log("Target selection started: none, cursor pos = " + cursorPositionController.GetCurrentCursorPosition());
        }
#endif
    }
    public override void CursorTargetSelectionEnded(TargetBehaviour target)
    {
        ExecuteForEachCursorListener((ICursorListener listener) => { listener.CursorTargetSelectionEnded(target); });        

#if DEBUG_CURSOR
        if (target != null)
        {
            Debug.Log("Target selection ended: " + target.name);
        }
        else
        {
            Debug.Log("Target selection ended: none, cursor pos = " + cursorPositionController.GetCurrentCursorPosition());
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
