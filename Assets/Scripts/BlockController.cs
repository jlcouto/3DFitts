using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBlockListener {
    void OnBlockEnded(BlockMeasurements measurements);
}

public class BlockController : ITrialListener {
    BlockMeasurements blockMeasurements;
    TrialController currentTrial;
    TargetBehaviour[] targets;
    CursorBehaviour cursor;
    int blockId;

    int currentInitialTargetId = 0;
    IBlockListener listener;

    public BlockController(int blockId, TargetBehaviour[] targets, IBlockListener listener, CursorBehaviour theCursor) {
        this.blockId = blockId;
        this.targets = targets;
        this.listener = listener;
        this.cursor = theCursor;
    }
		
    public void StartBlockOfTrials() {
        blockMeasurements = new BlockMeasurements(blockId, targets.Length - 1);
        blockMeasurements.initialTime = Time.realtimeSinceStartup;
        blockMeasurements.initialTargetId = targets[currentInitialTargetId].targetId;
        RunNextTrial();
    }

    void RunNextTrial()
    {
        int currentFinalIndex = (currentInitialTargetId + 1) % targets.Length;
        if (currentInitialTargetId < targets.Length)
        {
            currentTrial = new TrialController(currentInitialTargetId, targets[currentInitialTargetId], targets[currentFinalIndex], this, cursor);
            currentTrial.StartTrial();
        }
        else
        {
            FinishBlockOfTrials();
        }
    }

    void FinishBlockOfTrials() {
        blockMeasurements.finalTime = Time.realtimeSinceStartup;
        listener.OnBlockEnded(blockMeasurements);
    }      

    public void OnTrialEnded(TrialMeasurements measurements) {
        blockMeasurements.trialsData.Add(measurements);
        currentInitialTargetId++;
        RunNextTrial();
    }
}
