using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.HandInput;

public class Meta2CursorBehaviour : CursorPositioningController
{
    public Meta.HandsProvider provider;
    public CursorHandPosition cursorPosition;

    Vector3 lastCursorPosition;

    private void Update()
    {
        if (provider.ActiveHands.Count > 0)
        {
            if (cursorPosition == CursorHandPosition.HandPalm)
            {
                lastCursorPosition = provider.ActiveHands[0].Data.Palm;
            }
            else
            {
                lastCursorPosition = provider.ActiveHands[0].Data.Top;
            }
        }
    }

    public override Vector3 GetCurrentCursorPosition()
    {
        return lastCursorPosition;
    }
}
