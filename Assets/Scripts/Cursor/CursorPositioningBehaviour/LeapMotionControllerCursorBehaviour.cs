using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Interaction;
using UnityEngine.Events;

public class LeapMotionControllerCursorBehaviour : CursorPositioningController
{
    public LeapServiceProvider leapService;
    public CursorHandPosition handPosition;

    Vector3 lastCursorPosition;

    private void Update()
    {
        Leap.Frame frame = leapService.CurrentFrame;
        if (frame.Hands.Count > 0)
        {
            if (handPosition == CursorHandPosition.HandTop)
            {
                lastCursorPosition = frame.Hands[0].GetIndex().TipPosition.ToVector3();
            }
            else
            {
                lastCursorPosition = frame.Hands[0].PalmPosition.ToVector3();
            }
        }
    }

    public override Vector3 GetCurrentCursorPosition()
    {
        return lastCursorPosition;
    }
}
