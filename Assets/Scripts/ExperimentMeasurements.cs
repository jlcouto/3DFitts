using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentMeasurements {
    /* Experiment Configuration */
    public ExperimentConfiguration experimentConfiguration;

    /* Experiment Data */
    public string timestamp;

    public float initialTime;
    public float finalTime;
    public float experimentDuration { get { return finalTime - initialTime; }}

    public List<BlockMeasurements> blocksData;

    public ExperimentMeasurements(ExperimentConfiguration configuration) {
        this.experimentConfiguration = configuration;
        blocksData = new List<BlockMeasurements>(configuration.numOfBlocksPerExperiment);
    }

    public Dictionary<string, object> SerializeToDictionary()
    {
        Dictionary<string, object> data = new Dictionary<string, object>();
        data["timestamp"] = timestamp;
        data["experimentId"] = experimentConfiguration.experimentId;
        data["initialTime"] = initialTime;
        data["finalTime"] = finalTime;
        data["experimentDuration"] = experimentDuration;

        List<Dictionary<string, object>> blocks = new List<Dictionary<string, object>>(blocksData.Count);
        foreach (BlockMeasurements b in blocksData)
        {
            blocks.Add(b.SerializeToDictionary());
        }
        data["blocksData"] = blocks;

        var configuration = experimentConfiguration.SerializeToDictionary();

        Dictionary<string, object> theExperiment = new Dictionary<string, object>();
        theExperiment["configuration"] = configuration;
        theExperiment["data"] = data;

        Dictionary<string, object> output = new Dictionary<string, object>();
        output["experiment"] = theExperiment;
        return output;
    }
}
