using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using Leap.Unity;
using Leap.Unity.Interaction;

public class CursorSelectionTechniqueLeapMotionGrab : CursorSelectionTechnique
{
    public LeapServiceProvider leapService;
    bool wasGrabbing = false;

    public CursorSelectionTechniqueLeapMotionGrab(LeapServiceProvider leapService)
    {
        this.leapService = leapService;
    }

    bool IsGrabbing()
    {
        if (leapService != null)
        {
            Leap.Frame frame = leapService.CurrentFrame;
            if (frame.Hands.Count > 0)
            {
                return frame.Hands[0].GrabStrength > 0.9;
            }
        }
        return false;
    }

    public override bool SelectionInteractionStarted()
    {
        if (!wasGrabbing && IsGrabbing())
        {
            wasGrabbing = true;
            return true;
        }
        return false;
    }

    public override bool SelectionInteractionMantained()
    {
        return wasGrabbing && IsGrabbing();
    }

    public override bool SelectionInteractionEnded()
    {
        if (wasGrabbing && !IsGrabbing())
        {
            wasGrabbing = false;
            return true;
        }
        return false;
    }
}