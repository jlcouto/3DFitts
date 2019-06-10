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
}
