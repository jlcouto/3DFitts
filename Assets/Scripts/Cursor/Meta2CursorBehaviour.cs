using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.HandInput;

public class Meta2CursorBehaviour : CursorPositioningController
{
    public Meta.HandsProvider provider;

    Vector3 lastCursorPosition;

    private void Update()
    {
        if (provider.ActiveHands.Count > 0)
        {         
            lastCursorPosition = provider.ActiveHands[0].Data.Top;
        }
    }

    public override Vector3 GetCurrentCursorPosition()
    {
        return lastCursorPosition;
    }
}
