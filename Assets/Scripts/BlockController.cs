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
    ExperimentTask task;

    int currentInitialTargetId = 0;
    IBlockListener listener;

    public BlockController(ExperimentTask task, int blockId, TargetBehaviour[] targets, IBlockListener listener, CursorBehaviour theCursor) {
        this.task = task;
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
            TrialMeasurements lastTrial = null;
            if (blockMeasurements.trialsData.Count > 0)
            {
                lastTrial = blockMeasurements.trialsData[blockMeasurements.trialsData.Count - 1];
            }
            if (task == ExperimentTask.ReciprocalDragging)
            {
                currentTrial = new DragTestController(currentInitialTargetId, targets[currentInitialTargetId], targets[currentFinalIndex], this, cursor, lastTrial);
            }
            else
            {
                currentTrial = new TappingTrialController(currentInitialTargetId, targets[currentInitialTargetId], targets[currentFinalIndex], this, cursor, lastTrial);
            }            
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

    public void AbortBlock()
    {
        if (currentTrial != null)
        {
            currentTrial.AbortTrial();
        }
        currentTrial = null;
        listener = null;
    }
}
