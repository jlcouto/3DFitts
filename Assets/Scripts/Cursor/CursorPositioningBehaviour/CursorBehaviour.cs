using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICursorListener {
    void CursorEnteredTarget(TargetBehaviour target);
    void CursorExitedTarget(TargetBehaviour target);
    void CursorAcquiredTarget(TargetBehaviour target);
    void CursorDragTargetStarted(TargetBehaviour target);
    void CursorDragTargetEnded(TargetBehaviour draggedTarget, TargetBehaviour receivingTarget);
}

public abstract class CursorBehaviour : MonoBehaviour {
    protected HashSet<ICursorListener> listeners;

    public AudioSource correctAudio;
    public AudioSource errorAudio;

    public Vector3 lastCursorSelectionPosition;
    public float lastCursorSelectionTime;

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

    public void RegisterSelectionInformation(float timeOfSelection, Vector3 positionOfCursor)
    {
        this.lastCursorSelectionTime = timeOfSelection;
        this.lastCursorSelectionPosition = positionOfCursor;
    }

    public abstract void EnterTarget(TargetBehaviour theTarget);
    public abstract void ExitTarget(TargetBehaviour theTarget);
    public abstract void AcquireTarget(TargetBehaviour theTarget);
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
