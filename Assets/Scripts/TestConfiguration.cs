using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestConfiguration
{
    public string testId;
    public ExperimentTask task;
    public float targetWidth;
    public float targetDistance;
    public PlaneOrientation planeOrientation;
    public int numberOfTargets;
    public List<SimpleVector3> targetsPositions;
    public int numOfBlocksPerTest;

    public TestConfiguration(TargetBehaviour[] targets, ExperimentTask task, PlaneOrientation orientation, float targetWidth, float targetDistance, int numOfBlocksPerTest) {
        this.task = task;
        this.planeOrientation = orientation;
        this.targetWidth = targetWidth;
        this.targetDistance = targetDistance;
        this.numberOfTargets = targets.Length;

        this.targetsPositions = new List<SimpleVector3>(this.numberOfTargets);
        for (int i = 0; i < targets.Length; i++)
        {
            this.targetsPositions.Add(SimpleVector3.FromVector3(targets[i].position));
        }

        this.testId = GetTestId();
        this.numOfBlocksPerTest = numOfBlocksPerTest;
    }

    string GetTestId()
    {
        return "P" + GetPlaneName(planeOrientation) + "W" + Mathf.RoundToInt(targetWidth * 1000) + "D" + Mathf.RoundToInt(targetDistance * 1000) + "T" + numberOfTargets + "R" + numOfBlocksPerTest;
    }

    string GetPlaneName(PlaneOrientation orientation)
    {
        switch (orientation)
        {
            case PlaneOrientation.PlaneXY: return "XY";
            case PlaneOrientation.PlaneYZ: return "YZ";
            case PlaneOrientation.PlaneZX: return "ZX";
            default: return "?";
        }
    }

    string GetTaskName(ExperimentTask task)
    {
        switch (task)
        {
            case ExperimentTask.ReciprocalTapping: return "ReciprocalTapping";
            case ExperimentTask.Dragging: return "Dragging";
            default: return "undefined";
        }
    }

    public Dictionary<string, object> SerializeToDictionary()
    {
        Dictionary<string, object> output = new Dictionary<string, object>(9);
        output["testId"] = testId;
        output["task"] = GetTaskName(task);
        output["numberOfTargets"] = numberOfTargets;
        output["targetWidth"] = targetWidth;
        output["targetDistance"] = targetDistance;
        output["numOfBlocksPerTest"] = numOfBlocksPerTest;
        output["targetsPositions"] = targetsPositions;

        return output;
    }
}
