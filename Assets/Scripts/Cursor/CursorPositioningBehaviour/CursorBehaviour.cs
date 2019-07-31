using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICursorListener {
    void CursorEnteredTarget(TargetBehaviour target);
    void CursorExitedTarget(TargetBehaviour target);
    void CursorTargetSelectionStarted(TargetBehaviour target);
    void CursorTargetSelectionEnded(TargetBehaviour target);
    void CursorDragTargetStarted(TargetBehaviour target);
    void CursorDragTargetEnded(TargetBehaviour draggedTarget, TargetBehaviour receivingTarget);
}

public abstract class CursorBehaviour : MonoBehaviour {
    protected HashSet<ICursorListener> listeners;

    public AudioSource correctAudio;
    public AudioSource errorAudio;

    protected void Start()
    {
        listeners = new HashSet<ICursorListener>();        
    }

    public void RegisterNewListener(ICursorListener newListener)
    {        
        listeners.Add(newListener);        
    }

    public void RemoveListener(ICursorListener listener) {
        listeners.Remove(listener);
    }

    public bool IsOverTarget(TargetBehaviour target)
    {
        float distanceFinalTargetToSelectionPosition = Vector3.Distance(target.position, GetCursorPosition());
        float maxDistanceToHitTarget = 0.5f * (target.localScale.x + this.transform.localScale.x); // this will only work for target and cursor represented as spheres
        return (distanceFinalTargetToSelectionPosition <= maxDistanceToHitTarget);
    }

    protected void ExecuteForEachCursorListener(System.Action<ICursorListener> action)
    {
        ICursorListener[] listenersCopy = new ICursorListener[listeners.Count];
        listeners.CopyTo(listenersCopy);
        foreach (var listener in listenersCopy)
        {
            action?.Invoke(listener);
        }
    }

    public abstract void EnterTarget(TargetBehaviour theTarget);
    public abstract void ExitTarget(TargetBehaviour theTarget);
    public abstract void CursorTargetSelectionStarted(TargetBehaviour target);
    public abstract void CursorTargetSelectionEnded(TargetBehaviour target);
    public abstract void CursorDragTargetStarted(TargetBehaviour target);
    public abstract void CursorDragTargetEnded(TargetBehaviour draggedTarget, TargetBehaviour receivingTarget);

    public abstract Vector3 GetCursorPosition();
    public abstract int GetTrackedHandId();

    public abstract string GetDeviceName();
    public abstract string GetInteractionTechniqueName();

    public void PlayCorrectAudio() {
        correctAudio.Play();
    }

    public void PlayErrorAudio() {
        errorAudio.Play();
    }
}
