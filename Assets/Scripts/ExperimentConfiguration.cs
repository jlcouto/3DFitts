using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentConfiguration
{
    public string experimentId;
    public float targetWidth;
    public float targetDistance;
    public int numberOfTargets;
    public List<Vector3> targetsPositions;
    public int numOfBlocksPerExperiment;

    public ExperimentConfiguration(TargetBehaviour[] targets, float targetWidth, float targetDistance, int numOfBlocksPerExperiment) {
        this.targetWidth = targetWidth;
        this.targetDistance = targetDistance;
        this.numberOfTargets = targets.Length;

        this.targetsPositions = new List<Vector3>(this.numberOfTargets);
        for (int i = 0; i < targets.Length; i++)
        {
            this.targetsPositions.Add(targets[i].position);
        }

        this.experimentId = GetExperimentId();
        this.numOfBlocksPerExperiment = numOfBlocksPerExperiment;
    }

    string GetExperimentId()
    {
        return "W" + Mathf.RoundToInt(targetWidth * 1000) + "D" + Mathf.RoundToInt(targetDistance * 1000) + "T" + numberOfTargets;
    }

    public Dictionary<string, object> SerializeToDictionary()
    {
        Dictionary<string, object> output = new Dictionary<string, object>(9);
        output["experimentId"] = experimentId;
        output["numberOfTargets"] = numberOfTargets;
        output["targetWidth"] = targetWidth;
        output["targetDistance"] = targetDistance;
        output["numOfBlocksPerExperiment"] = numOfBlocksPerExperiment;
        output["targetsPositions"] = targetsPositions;

        return output;
    }
}
