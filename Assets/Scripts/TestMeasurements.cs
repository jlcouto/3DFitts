using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMeasurements {
    public readonly ExperimentConfiguration configuration;
    public readonly IndexOfDifficulty sequenceInfo;
    public readonly List<Vector3> targetsPositions;

    public string timestamp;
    public float initialTime;
    public float finalTime;
    public float testDuration { get { return finalTime - initialTime; }}

    public List<BlockMeasurements> blocksData;
    public Throughput computedResults;

    public TestMeasurements(ExperimentConfiguration configuration, IndexOfDifficulty sequenceInfo, TargetBehaviour[] targets)
    {
        this.configuration = configuration;
        this.sequenceInfo = sequenceInfo;

        this.targetsPositions = new List<Vector3>(targets.Length);
        for (int i = 0; i < targets.Length; i++)
        {
            this.targetsPositions.Add(targets[i].position);
        }

        blocksData = new List<BlockMeasurements>(configuration.numberOfBlocks);
    }

    public void ComputeResults()
    {
        this.computedResults = new Throughput(this);
    }
}
