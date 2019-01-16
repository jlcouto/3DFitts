using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockMeasurements {
    public int initialTargetId;

    public List<TrialMeasurements> trialsData;

    public int blockId;
    public float initialTime;
    public float finalTime;
    public float blockDuration { get { return finalTime - initialTime; }}

    public BlockMeasurements(int blockId, int numOfTrials) {
        this.blockId = blockId;
        trialsData = new List<TrialMeasurements>(numOfTrials);
    }

    public Dictionary<string, object> SerializeToDictionary()
    {
        Dictionary<string, object> output = new Dictionary<string, object>(6);
        output["blockId"] = blockId;
        output["initialTargetId"] = initialTargetId;
        output["initialTime"] = initialTime;
        output["finalTime"] = finalTime;
        output["blockDuration"] = blockDuration;

        List<Dictionary<string, object>> trials = new List<Dictionary<string, object>>(trialsData.Count);
        foreach (TrialMeasurements t in trialsData) {
            trials.Add(t.SerializeToDictionary());
        }
        output["trialsData"] = trials;

        return output;
    }
}
