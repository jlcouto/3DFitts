using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using Meta;

public class CursorSelectionTechniqueMeta2Grab : CursorSelectionTechnique
{
    public HandsProvider handsProvider;
    bool wasGrabbing = false;

    public CursorSelectionTechniqueMeta2Grab(HandsProvider handsProvider)
    {
        this.handsProvider = handsProvider;
    }

    bool IsGrabbing()
    {
        if (handsProvider != null)
        {
            if (handsProvider.ActiveHands.Count > 0)
            {
                return handsProvider.ActiveHands[0].IsGrabbing;
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

    public override string GetInteractionName()
    {
        return "Meta2_GrabGesture";
    }
}