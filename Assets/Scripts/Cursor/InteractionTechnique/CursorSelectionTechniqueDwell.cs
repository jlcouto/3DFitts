using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorSelectionTechniqueDwell : CursorSelectionTechnique, ICursorListener
{
    /* This Selection Technique will select only the starting or next target types, if the cursor is kept inside them
     * for more than dwellTimeInSeconds. The user won't be able to select other types of targets. */
    public float dwellTimeInSeconds;

    private TargetBehaviour lastTarget;
    private Vector3 cursorPositionWhenEnteredLastTarget;
    private float timeEnteredLastTarget = -1;
    private bool targetWasSelected = false;

    private CursorBehaviour cursor;

    public CursorSelectionTechniqueDwell(float dwellTimeInSeconds, CursorBehaviour cursor)
    {     
        this.dwellTimeInSeconds = dwellTimeInSeconds;
        this.cursor = cursor;
        this.cursor.RegisterNewListener(this);
    }
    
    ~CursorSelectionTechniqueDwell()
    {
        this.cursor.RemoveListener(this);
    }

    public void CursorEnteredTarget(TargetBehaviour target)
    {
        if (target.type == TargetType.NextTarget || target.type == TargetType.StartingTestTarget)
        {
            if (lastTarget == null || target.targetId != lastTarget.targetId)
            {
                lastTarget = target;
                targetWasSelected = false;
                cursorPositionWhenEnteredLastTarget = this.cursor.GetCursorPosition();
                timeEnteredLastTarget = Time.realtimeSinceStartup;
            }
        }
    }

    public void CursorExitedTarget(TargetBehaviour target)
    {
        if (lastTarget != null && target.targetId == lastTarget.targetId)
        {
            lastTarget = null;
            FinishSelection();
        }       
    }

    bool ShouldTriggerSelectionEvent()
    {
        if (!targetWasSelected && (timeEnteredLastTarget > 0) && (Time.realtimeSinceStartup - timeEnteredLastTarget > dwellTimeInSeconds))
        {                        
            this.cursor.RegisterSelectionInformation(timeEnteredLastTarget, cursorPositionWhenEnteredLastTarget);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void CursorAcquiredTarget(TargetBehaviour target) { }
    public void CursorDragTargetStarted(TargetBehaviour target) { }
    public void CursorDragTargetEnded(TargetBehaviour draggedTarget, TargetBehaviour receivingTarget) { }

    public override bool SelectionInteractionStarted()
    {
        // We are assuming the Dwell to be a instantaneous selection event.
        // Thus, it will trigger both the Start of Selection and the End of Selection events simultaneously.
        return ShouldTriggerSelectionEvent();        
    }

    public override bool SelectionInteractionMantained()
    {
        // The Dwell method will never be able to "mantain" a selection because it will release the target at the same frame it was acquired.
        return false;
    }

    public override bool SelectionInteractionEnded()
    {
        // We are assuming the Dwell to be a instantaneous selection event.
        // Thus, it will trigger both the Start of Selection and the End of Selection events simultaneously.
        if (ShouldTriggerSelectionEvent())
        {
            targetWasSelected = true;
            return true;
        }
        return false;
    }

    void FinishSelection()
    {
        timeEnteredLastTarget = -1;
        targetWasSelected = false;
    }

    public override string GetInteractionName()
    {
        return string.Format("DwellTime_{0:0}ms", dwellTimeInSeconds * 1000);
    }
}
