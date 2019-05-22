using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorSelectionTechniqueDwell : CursorSelectionTechnique, ICursorListener
{
    public float dwellTimeInSeconds;

    private TargetBehaviour lastTarget;
    private Vector3 cursorPositionWhenEnteredLastTarget;
    private float timeEnteredLastTarget = -1;
    private bool interactionStarted = false;    

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
        lastTarget = target;
        cursorPositionWhenEnteredLastTarget = this.cursor.GetCursorPosition();
        timeEnteredLastTarget = Time.realtimeSinceStartup;
        interactionStarted = false;
    }

    public void CursorExitedTarget(TargetBehaviour target)
    {
        if (target == lastTarget)
        {
            lastTarget = null;
            FinishSelection();
        }       
    }

    public void CursorAcquiredTarget(TargetBehaviour target) { }
    public void CursorDragTargetStarted(TargetBehaviour target) { }
    public void CursorDragTargetEnded(TargetBehaviour draggedTarget, TargetBehaviour receivingTarget) { }

    public override bool SelectionInteractionStarted()
    {        
        if (!interactionStarted && (timeEnteredLastTarget > 0) && (Time.realtimeSinceStartup - timeEnteredLastTarget > dwellTimeInSeconds))
        {
            interactionStarted = true;
            this.cursor.RegisterSelectionInformation(timeEnteredLastTarget, cursorPositionWhenEnteredLastTarget);
            return true;
        }
        else
        {
            return false;
        }
    }

    public override bool SelectionInteractionMantained()
    {
        return interactionStarted && (timeEnteredLastTarget > 0);        
    }

    public override bool SelectionInteractionEnded()
    {
        if (interactionStarted)
        {
            FinishSelection();
            return true;
        }
        return false;
    }

    void FinishSelection()
    {
        timeEnteredLastTarget = -1;
        interactionStarted = false;
    }

    public override string GetInteractionName()
    {
        return "DwellTime_" + dwellTimeInSeconds.ToString() + "s";
    }
}
