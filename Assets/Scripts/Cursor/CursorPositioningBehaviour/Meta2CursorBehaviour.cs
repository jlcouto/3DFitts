using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.HandInput;

public class Meta2CursorBehaviour : CursorPositioningController
{
    public Meta.HandsProvider provider;
    public CursorHandPosition cursorPosition;

    Vector3 lastCursorPosition;

    bool isTrackingHand;
    int detectedHandID;

    private void Update()
    {
        isTrackingHand = provider.ActiveHands.Count > 0;
        if (isTrackingHand)
        {
            if (cursorPosition == CursorHandPosition.HandPalm)
            {
                lastCursorPosition = provider.ActiveHands[0].Data.Palm;
            }
            else
            {
                lastCursorPosition = provider.ActiveHands[0].Data.Top;
            }
            detectedHandID = provider.ActiveHands[0].Data.UniqueId;
        }
        else
        {
            detectedHandID = -1;
        }
    }

    public override Vector3 GetCurrentCursorPosition()
    {
        return lastCursorPosition;
    }

    public override int GetTrackedHandId()
    {
        return detectedHandID;
    }

    private void OnEnable()
    {
        // Enable Meta2 interaction related services
        provider.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        // Disabel Meta2 interaction related services
        provider.gameObject.SetActive(false);
    }
}
