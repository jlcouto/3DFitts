using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragTestController : TrialController
{
    GameObject draggableObject;
    bool isDraggingTarget = false;

    public DragTestController(int theTrialId, TargetBehaviour initialTarget, TargetBehaviour finalTarget, ITrialListener theListener, CursorBehaviour theCursor)
        : base(theTrialId, initialTarget, finalTarget, theListener, theCursor)
    {
        draggableObject = Object.Instantiate(initialTarget.gameObject);

        var targetBehaviour = draggableObject.GetComponent<TargetBehaviour>();
        targetBehaviour.UnhighlightTarget();
        targetBehaviour.SetAsDraggableTarget();
        Object.Destroy(targetBehaviour);

        draggableObject.transform.SetParent(cursor.transform);
        draggableObject.transform.localPosition = Vector3.zero;
        draggableObject.SetActive(false);
    }

    public override void InitializeTrial()
    {
        base.InitializeTrial();
        initialTarget.SetAsDraggableTarget();
        finalTarget.SetAsNormalTarget();
    }

    public override void StartTrial()
    {
        base.StartTrial();
        trialData.ForceInitialTime(-1);
    }

    public override void FinishTrial(bool missedTrial)
    {
        finalTarget.SetAsNormalTarget();
        base.FinishTrial(missedTrial);
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
        if (!isDraggingTarget &&
            target != null && target.targetId == initialTarget.targetId && target.type == TargetType.DraggableTarget)
        {
            isDraggingTarget = true;
            draggableObject.SetActive(true);
            initialTarget.SetAsNormalTarget();
            finalTarget.SetAsNextTarget();
            trialData.ForceInitialTime(Time.realtimeSinceStartup);
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
        if (isDraggingTarget)
        {
            bool missedTarget;
            if (draggedTarget != null && draggedTarget.targetId == initialTarget.targetId &&
                (receivingTarget != null && receivingTarget.targetId == finalTarget.targetId))// || IsDraggableObjectTouchingFinalTarget()))
            {
                missedTarget = false;
                cursor.PlayCorrectAudio();
            }
            else
            {
                missedTarget = true;
                cursor.PlayErrorAudio();
            }
            Object.Destroy(draggableObject);
            isDraggingTarget = false;
            FinishTrial(missedTarget);
        }
    }

    bool IsDraggableObjectTouchingFinalTarget()
    {
        float centersDistance = Vector3.Distance(finalTarget.gameObject.transform.position, draggableObject.gameObject.transform.position);
        float maxDistanceToIntersect = finalTarget.gameObject.transform.localScale.x;
        return centersDistance <= maxDistanceToIntersect;
    }
}
