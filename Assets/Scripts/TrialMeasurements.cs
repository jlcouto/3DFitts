using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialMeasurements {
    public int trialId;

    public int initialTargetId;
    public int finalTargetId;

    public Vector3 initialTargetPosition;
    public Vector3 finalTargetPosition;

    public float initialTime;
    public float finalTime;
    public float trialDuration { get { return finalTime - initialTime; } }

    public SimpleVector3 initialPosition;
    public SimpleVector3 finalPosition;

    public float finalPositionProjectedCoordinate;

    public bool missedTarget = false;

    public TrialMeasurements(int trialId, TargetBehaviour initialTarget, TargetBehaviour finalTarget) {
        this.trialId = trialId;
        this.initialTargetId = initialTarget.targetId;
        this.finalTargetId = finalTarget.targetId;
        this.initialTargetPosition = initialTarget.position;
        this.finalTargetPosition = finalTarget.position;
    }

    public Dictionary<string, object> SerializeToDictionary()
    {
        Dictionary<string, object> output = new Dictionary<string, object>(8);
        output["trialId"] = trialId;
        output["initialTargetId"] = initialTargetId;
        output["finalTargetId"] = finalTargetId;
        output["initialTime"] = initialTime;
        output["finalTime"] = finalTime;
        output["trialDuration"] = trialDuration;
        output["initialPosition"] = initialPosition;
        output["finalPosition"] = finalPosition;
        output["missedTarget"] = missedTarget;

        finalPositionProjectedCoordinate = (float) ResultsMath.Projected3DPointCoordinate(initialTargetPosition, finalTargetPosition, SimpleVector3.ToVector3(finalPosition));
        output["finalProjectedCoordinate"] = finalPositionProjectedCoordinate;

        return output;
    }
}
