using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITrialListener {
    void OnTrialEnded(TrialMeasurements measurements);
}

public class TrialController : ICursorListener {
    /*
     * This class is responsible for controlling one trial, which consists of the following tasks:
     * 1. Capture the time the trials begins and the cursor position at that time
     * 2. Register the trajectory of the cursor while the trial is active
     * 3. Register itself in the controller that checks if a trial ended and responds to the end of the trial
     * 4. Identify if user missed the final target
     * 5. Capture the time the trials ends and the cursor position at that time
     */

    public TargetBehaviour initialTarget;
    public TargetBehaviour finalTarget;
    public CursorBehaviour cursor;

    public ITrialListener listener;

    int trialId;

    TrialMeasurements trialData;

    public TrialController(int trialId, TargetBehaviour initialTarget, TargetBehaviour finalTarget, ITrialListener theListener, CursorBehaviour theCursor) {
        this.trialId = trialId;
        this.initialTarget = initialTarget;
        this.finalTarget = finalTarget;
        this.listener = theListener;
        this.cursor = theCursor;
    }
	
    public void StartTrial() {
        initialTarget.SetAsNormalTarget();
        finalTarget.SetAsNextTarget();
        trialData = new TrialMeasurements(trialId, initialTarget, finalTarget);
        trialData.initialTime = Time.realtimeSinceStartup;
        trialData.initialPosition = cursor.position;
        cursor.RegisterNewListener(this);
    }

    void FinishTrial() {
        cursor.RemoveCurrentListener();
        trialData.finalTime = Time.realtimeSinceStartup;
        trialData.finalPosition = cursor.position;
        if (listener != null) {
            listener.OnTrialEnded(trialData);
        }
    }

    public void CursorEnteredTarget(TargetBehaviour target) {

    }

    public void CursorExitedTarget(TargetBehaviour target) {

    }

    public void CursorAcquiredTarget(TargetBehaviour target) {
        if (target != null && target.targetId == finalTarget.targetId) {
            trialData.missedTarget = false;
            cursor.PlayCorrectAudio();
        }
        else {
            trialData.missedTarget = true;
            cursor.PlayErrorAudio();
        }
        FinishTrial();
    }
}
