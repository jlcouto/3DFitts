using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TappingTrialController : TrialController
{
    public TappingTrialController(int theTrialId, TargetBehaviour initialTarget, TargetBehaviour finalTarget, ITrialListener theListener, CursorBehaviour theCursor, TrialMeasurements lastTrial)
        : base(theTrialId, initialTarget, finalTarget, theListener, theCursor, lastTrial)
    {
    }

    public override void InitializeTrial()
    {
        base.InitializeTrial();
        initialTarget.SetAsNormalTarget();
        finalTarget.SetAsNextTarget();
    }

    public override void CursorEnteredTarget(TargetBehaviour target)
    {
        //Debug.Log("Tapping CursorEnteredTarget");
    }

    public override void CursorExitedTarget(TargetBehaviour target)
    {
        //Debug.Log("Tapping CursorExitedTarget");
    }

    public override void CursorAcquiredTarget(TargetBehaviour target)
    {
        //Debug.Log("Tapping CursorAcquiredTarget");
        float distanceFinalTargetToSelectionPosition = Vector3.Distance(finalTarget.position, cursor.lastCursorSelectionPosition);
        float maxDistanceToHitTarget = 0.5f*(finalTarget.localScale.x + cursor.transform.localScale.x); // this will only work for target and cursor represented as spheres
        bool missedTarget = (target == null) || (distanceFinalTargetToSelectionPosition > maxDistanceToHitTarget);
        if (missedTarget)
        {
            cursor.PlayErrorAudio();            
        }
        else
        {
            cursor.PlayCorrectAudio();
        }
        FinishTrial(missedTarget);
    }

    public override void CursorDragTargetStarted(TargetBehaviour target)
    {
        //Debug.Log("Tapping CursorDragTargetStarted");
    }

    public override void CursorDragTargetEnded(TargetBehaviour draggedTarget, TargetBehaviour receivingTarget)
    {
        //Debug.Log("Tapping CursorDragTargetEnded");
    }
}
