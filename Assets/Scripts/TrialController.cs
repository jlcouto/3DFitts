using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITrialListener {
    void OnTrialEnded(TrialMeasurements measurements);
}

public abstract class TrialController : ICursorListener {

    private TrialMeasurements _lastTrial;

    public TargetBehaviour initialTarget;
    public TargetBehaviour finalTarget;
    public CursorBehaviour cursor;

    public ITrialListener listener;

    public int trialId;

    public TrialMeasurements trialData;

    public TrialController(int theTrialId, TargetBehaviour initialTarget, TargetBehaviour finalTarget, ITrialListener theListener, CursorBehaviour theCursor, TrialMeasurements lastTrial) {
        this.trialId = theTrialId;
        this.initialTarget = initialTarget;
        this.finalTarget = finalTarget;
        this.listener = theListener;
        this.cursor = theCursor;
        _lastTrial = lastTrial;
        InitializeTrial();
    }

    public virtual void InitializeTrial()
    {
        trialData = new TrialMeasurements(trialId, initialTarget, finalTarget, _lastTrial);      
        initialTarget.SetAsNormalTarget();
        finalTarget.SetAsNextTarget();
    }
	
    public virtual void StartTrial()
    {
        cursor.RegisterNewListener(this);
        trialData.StartTrial(Time.realtimeSinceStartup, cursor.GetCursorPosition());  
    }

    public virtual void FinishTrial(bool missedTrial) {
        finalTarget.SetAsNormalTarget();
        cursor.RemoveListener(this);
        trialData.FinishTrial(cursor.lastCursorSelectionTime, cursor.lastCursorSelectionPosition, missedTrial);
        if (listener != null) {
            listener.OnTrialEnded(trialData);
        }
    }

    public virtual void AbortTrial()
    {
        cursor.RemoveListener(this);
        listener = null;
    }

    public abstract void CursorEnteredTarget(TargetBehaviour target);
    public abstract void CursorExitedTarget(TargetBehaviour target);
    public abstract void CursorAcquiredTarget(TargetBehaviour target);
    public abstract void CursorDragTargetStarted(TargetBehaviour target);
    public abstract void CursorDragTargetEnded(TargetBehaviour draggedTarget, TargetBehaviour receivingTarget);
}
