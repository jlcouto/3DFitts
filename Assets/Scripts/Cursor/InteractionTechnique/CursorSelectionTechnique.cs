using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using Meta;

public abstract class CursorSelectionTechnique : MonoBehaviour
{
    public abstract bool SelectionInteractionStarted();
    public abstract bool SelectionInteractionMantained();
    public abstract bool SelectionInteractionEnded();
}