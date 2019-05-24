using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITrialListener {
    void OnTrialEnded(TrialMeasurements measurements);
}

public abstract class TrialController : ICursorListener {

    public TargetBehaviour initialTarget;
    public TargetBehaviour finalTarget;
    public CursorInteractorBehaviour cursor;

    public ITrialListener listener;

    public int trialId;

    public TrialMeasurements trialData;

    public TrialController(int theTrialId, TargetBehaviour initialTarget, TargetBehaviour finalTarget, ITrialListener theListener, CursorInteractorBehaviour theCursor) {
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
        if (cursor.cursorPositionController is Mouse2DInputBehaviour)
        {
            Mouse2DInputBehaviour mouse = (Mouse2DInputBehaviour) cursor.cursorPositionController;
            trialData.initialMousePositionOnScreen = SimpleVector2.FromVector2(mouse.GetCurrentCursorPositionOnScreen());
        }
    }

    public virtual void FinishTrial() {
        cursor.RemoveListener(this);
        trialData.finalTime = cursor.lastCursorSelectionTime;
        trialData.finalPosition = SimpleVector3.FromVector3(cursor.lastCursorSelectionPosition);
        if (cursor.cursorPositionController is Mouse2DInputBehaviour)
        {
            Mouse2DInputBehaviour mouse = (Mouse2DInputBehaviour)cursor.cursorPositionController;
            trialData.finalMousePositionOnScreen = SimpleVector2.FromVector2(mouse.GetCurrentCursorPositionOnScreen());
        }
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
