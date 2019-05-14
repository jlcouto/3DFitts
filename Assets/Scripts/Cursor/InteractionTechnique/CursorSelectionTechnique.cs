using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using Meta;

public abstract class CursorSelectionTechnique
{
    public abstract bool SelectionInteractionStarted();
    public abstract bool SelectionInteractionMantained();
    public abstract bool SelectionInteractionEnded();
}