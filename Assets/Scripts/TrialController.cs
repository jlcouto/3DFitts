using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITrialListener {
    void OnTrialEnded(TrialMeasurements measurements);
}

public abstract class TrialController : MonoBehaviour, ICursorListener {

    public TargetBehaviour initialTarget;
    public TargetBehaviour finalTarget;
    public CursorBehaviour cursor;

    public ITrialListener listener;

    public int trialId;

    public TrialMeasurements trialData;

    public TrialController(int theTrialId, TargetBehaviour initialTarget, TargetBehaviour finalTarget, ITrialListener theListener, CursorBehaviour theCursor) {
        this.trialId = theTrialId;
        this.initialTarget = initialTarget;
        this.finalTarget = finalTarget;
        this.listener = theListener;
        this.cursor = theCursor;
        InitializeTrial();
    }

    public virtual void InitializeTrial()
    {
        trialData = new TrialMeasurements(trialId, initialTarget, finalTarget);      
        initialTarget.SetAsNormalTarget();
        finalTarget.SetAsNextTarget();
    }
	
    public virtual void StartTrial()
    {
        cursor.RegisterNewListener(this);
        trialData.initialTime = Time.realtimeSinceStartup;
        trialData.initialPosition = SimpleVector3.FromVector3(cursor.GetCursorPosition());  
    }

    public virtual void FinishTrial() {
        cursor.RemoveCurrentListener();
        trialData.finalTime = Time.realtimeSinceStartup;
        trialData.finalPosition = SimpleVector3.FromVector3(cursor.GetCursorPosition());
        if (listener != null) {
            listener.OnTrialEnded(trialData);
        }
    }

    public abstract void CursorEnteredTarget(TargetBehaviour target);
    public abstract void CursorExitedTarget(TargetBehaviour target);
    public abstract void CursorAcquiredTarget(TargetBehaviour target);
    public abstract void CursorDragTargetStarted(TargetBehaviour target);
    public abstract void CursorDragTargetEnded(TargetBehaviour draggedTarget, TargetBehaviour receivingTarget);
}
