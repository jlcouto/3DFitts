using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragTestController : TrialController
{
    public DragTestController(int theTrialId, TargetBehaviour initialTarget, TargetBehaviour finalTarget, ITrialListener theListener, CursorBehaviour theCursor)
        : base(theTrialId, initialTarget, finalTarget, theListener, theCursor)
    {
    }

    public override void StartTrial()
    {
        base.StartTrial();
        initialTarget.SetAsNextTarget();
        finalTarget.SetAsNormalTarget();
        trialData.initialTime = -1;
    }

    public override void CursorEnteredTarget(TargetBehaviour target)
    {
        Debug.Log("Drag CursorEnteredTarget");
    }

    public override void CursorExitedTarget(TargetBehaviour target)
    {
        Debug.Log("Drag CursorExitedTarget");
    }

    public override void CursorAcquiredTarget(TargetBehaviour target)
    {
        Debug.Log("Drag CursorAcquiredTarget");
    }

    public override void CursorDragTargetStarted(TargetBehaviour target)
    {
        Debug.Log("Drag CursorDragTargetStarted");
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
        Debug.Log("Drag CursorDragTargetExited");
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
