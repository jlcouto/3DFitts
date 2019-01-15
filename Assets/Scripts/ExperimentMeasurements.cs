using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentMeasurements {
    /* Experiment Configuration */
    public string experimentId;
    public uint numberOfTargets;
    public float targetWidth;
    public float targetDistance;
    public uint numOfBlocksPerExperiment;

    /* Experiment Data */
    public string timestamp;

    public float initialTime;
    public float finalTime;
    public float experimentDuration { get { return finalTime - initialTime; }}

    public List<BlockMeasurements> blocksData;

    public ExperimentMeasurements(uint numberOfTargets, float targetWidth, float targetDistance, uint numOfBlocksPerExperiment) {
        this.numberOfTargets = numberOfTargets;
        this.targetWidth = targetWidth;
        this.targetDistance = targetDistance;
        this.experimentId = GetExperimentId();
        blocksData = new List<BlockMeasurements>((int)numOfBlocksPerExperiment);
    }

    string GetExperimentId()
    {
        return "W" + (uint)(targetWidth * 1000) + "D" + (uint)(targetDistance * 1000) + "T" + numberOfTargets;
    }

    public Dictionary<string, object> SerializeToDictionary()
    {
        Dictionary<string, object> output = new Dictionary<string, object>(9);
        output["timestamp"] = timestamp;
        output["experimentId"] = experimentId;
        output["numberOfTargets"] = numberOfTargets;
        output["targetWidth"] = targetWidth;
        output["targetDistance"] = targetDistance;
        output["numOfBlocksPerExperiment"] = numOfBlocksPerExperiment;

        output["initialTime"] = initialTime;
        output["finalTime"] = finalTime;
        output["experimentDuration"] = experimentDuration;

        List<Dictionary<string, object>> blocks = new List<Dictionary<string, object>>(blocksData.Count);
        foreach (BlockMeasurements b in blocksData)
        {
            blocks.Add(b.SerializeToDictionary());
        }
        output["blocksData"] = blocks;

        return output;
    }
}
