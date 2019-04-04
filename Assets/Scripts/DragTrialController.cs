using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragTestController : TrialController
{
    public DragTestController(int theTrialId, TargetBehaviour initialTarget, TargetBehaviour finalTarget, ITrialListener theListener, CursorBehaviour theCursor)
        : base(theTrialId, initialTarget, finalTarget, theListener, theCursor)
    {
    }

    public new void StartTrial()
    {
        base.StartTrial();
        initialTarget.SetAsNextTarget();
        finalTarget.SetAsNormalTarget();
        trialData.initialTime = -1;
    }

    public override void CursorEnteredTarget(TargetBehaviour target)
    {

    }

    public override void CursorExitedTarget(TargetBehaviour target)
    {

    }

    public override void CursorAcquiredTarget(TargetBehaviour target)
    {
        
    }

    public override void CursorDragTargetStarted(TargetBehaviour target)
    {
        if (target != null && target.targetId == initialTarget.targetId)
        {
            initialTarget.SetAsNormalTarget();
            finalTarget.SetAsNextTarget();
            trialData.initialTime = Time.realtimeSinceStartup;
            cursor.PlayCorrectAudio();
        }
        else
        {
            cursor.PlayErrorAudio();
        }
    }

    public override void CursorDragTargetEnded(TargetBehaviour draggedTarget, TargetBehaviour receivingTarget)
    {
        if (draggedTarget != null && draggedTarget.targetId == initialTarget.targetId &&
            finalTarget != null && receivingTarget.targetId == finalTarget.targetId)
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
}
