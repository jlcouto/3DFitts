using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TappingTrialController : TrialController
{
    public TappingTrialController(int theTrialId, TargetBehaviour initialTarget, TargetBehaviour finalTarget, ITrialListener theListener, CursorBehaviour theCursor)
        : base(theTrialId, initialTarget, finalTarget, theListener, theCursor)
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
        bool missedTarget;
        if (target != null && target.targetId == finalTarget.targetId)
        {
            missedTarget = false;
            cursor.PlayCorrectAudio();
        }
        else
        {
            missedTarget = true;
            cursor.PlayErrorAudio();
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
