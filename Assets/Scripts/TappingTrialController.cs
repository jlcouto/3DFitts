using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TappingTrialController : TrialController
{
    public TappingTrialController(int theTrialId, TargetBehaviour initialTarget, TargetBehaviour finalTarget, ITrialListener theListener, CursorBehaviour theCursor)
        : base(theTrialId, initialTarget, finalTarget, theListener, theCursor)
    {
    }

    public override void CursorEnteredTarget(TargetBehaviour target)
    {

    }

    public override void CursorExitedTarget(TargetBehaviour target)
    {

    }

    public override void CursorAcquiredTarget(TargetBehaviour target)
    {
        if (target != null && target.targetId == finalTarget.targetId)
        {
            trialData.missedTarget = false;
            cursor.PlayCorrectAudio();
        }
        else
        {
            trialData.missedTarget = true;
            cursor.PlayErrorAudio();
        }
        FinishTrial();
    }

    public override void CursorDragTargetStarted(TargetBehaviour target)
    {

    }

    public override void CursorDragTargetEnded(TargetBehaviour draggedTarget, TargetBehaviour receivingTarget)
    {

    }
}
