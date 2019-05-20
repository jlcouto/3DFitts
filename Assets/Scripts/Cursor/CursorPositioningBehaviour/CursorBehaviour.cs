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
    protected ICursorListener listener;

    public AudioSource correctAudio;
    public AudioSource errorAudio;

    public void RegisterNewListener(ICursorListener newListener)
    {
        listener = newListener;
    }

    public void RemoveCurrentListener() {
        listener = null;
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
