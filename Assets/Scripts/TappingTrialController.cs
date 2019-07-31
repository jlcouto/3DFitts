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

    public override void CursorTargetSelectionStarted(TargetBehaviour target)
    {
        //Debug.Log("Tapping CursorTargetSelectionStarted");
        ActionStarted();
    }

    public override void CursorTargetSelectionEnded(TargetBehaviour target)
    {
        //Debug.Log("Tapping CursorTargetSelectionEnded");
                
        bool missedTarget = !cursor.IsOverTarget(finalTarget);
        if (missedTarget)
        {
            cursor.PlayErrorAudio();
        }
        else
        {
            cursor.PlayCorrectAudio();
        }
        ActionEnded(missedTarget);
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
