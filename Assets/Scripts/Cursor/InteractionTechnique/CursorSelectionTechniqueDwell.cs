using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorSelectionTechniqueDwell : CursorSelectionTechnique, ICursorListener
{
    /* This Selection Technique will select only the starting or next target types, if the cursor is kept inside them
     * for more than dwellTimeInSeconds. The user won't be able to select other types of targets. */
    public float dwellTimeInSeconds;

    private TargetBehaviour lastTarget;    
    private float timeEnteredLastTarget = -1;
    private bool selectionStarted = false;
    private bool selectionEnded = false;

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
                selectionStarted = false;
                selectionEnded = false;                      
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

    public void CursorTargetSelectionStarted(TargetBehaviour target) { }
    public void CursorTargetSelectionEnded(TargetBehaviour target) { }
    public void CursorDragTargetStarted(TargetBehaviour target) { }
    public void CursorDragTargetEnded(TargetBehaviour draggedTarget, TargetBehaviour receivingTarget) { }
    
    public override bool SelectionInteractionStarted()
    {
        /* We need to check if the cursor is over the target here because Unity Engine seems to process
           the ExitedTarget step only after passing through here. That would trigger a 'missed target' when
           moving the cursor out of the target in the exact time that the dwell time would be completed. */
        if (!selectionStarted && (timeEnteredLastTarget > 0) && cursor.IsOverTarget(lastTarget))
        {            
            selectionStarted = true;                        
            return true;
        }
        else
        {
            return false;
        }
    }

    public override bool SelectionInteractionMantained()
    {        
        return selectionStarted && !selectionEnded;
    }

    public override bool SelectionInteractionEnded()
    {        
        if (selectionStarted && !selectionEnded && (Time.realtimeSinceStartup - timeEnteredLastTarget > dwellTimeInSeconds) && cursor.IsOverTarget(lastTarget))
        {  
            selectionEnded = true;
            return true;
        }
        else
        {
            return false;
        }        
    }

    void FinishSelection()
    {
        timeEnteredLastTarget = -1;
        selectionStarted = false;
        selectionEnded = false;
    }

    public override string GetInteractionName()
    {
        return string.Format("DwellTime_{0:0}ms", dwellTimeInSeconds * 1000);
    }
}
