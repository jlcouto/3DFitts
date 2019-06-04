using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndexOfDifficulty
{
    public float targetWidth;
    public float targetsDistance;

    public IndexOfDifficulty(float targetWidth, float targetsDistance)
    {
        this.targetWidth = targetWidth;
        this.targetsDistance = targetsDistance;
    }

    float getIndexOfDifficulty()
    {
        return Mathf.Log((targetsDistance / targetWidth + 1), 2);
    }
}

public class ExperimentConfiguration
{
    public readonly string participantCode;
    public readonly string conditionCode;
    public readonly string sessionCode;
    public readonly string groupCode;
    public readonly string observations;

    public readonly ExperimentMode experimentMode;
    public readonly ExperimentTask experimentTask;
    public readonly CursorPositioningMethod cursorPositioningMethod;
    public readonly CursorSelectionMethod cursorSelectionMethod;
    public readonly PlaneOrientation planeOrientation;
    public readonly float dwellTime;
    public readonly float cursorWidth;
    public readonly int numberOfTargets;
    public readonly List<IndexOfDifficulty> sequences = new List<IndexOfDifficulty>();

    public ExperimentConfiguration(
        string participantCode, string conditionCode, string sessionCode, string groupCode, string observations,
        ExperimentMode experimentMode, ExperimentTask experimentTask, CursorPositioningMethod cursorPositioningMethod,
        CursorSelectionMethod cursorSelectionMethod, PlaneOrientation planeOrientation, float dwellTime, float cursorWidth,
        int numberOfTargets, float[] amplitudes, float[] widths)
    {
        this.participantCode = participantCode;
        this.conditionCode = conditionCode;
        this.sessionCode = sessionCode;
        this.groupCode = groupCode;
        this.observations = observations;

        this.experimentMode = experimentMode;
        this.experimentTask = experimentTask;
        this.cursorPositioningMethod = cursorPositioningMethod;
        this.cursorSelectionMethod = cursorSelectionMethod;
        this.planeOrientation = planeOrientation;
        this.dwellTime = dwellTime;
        this.cursorWidth = cursorWidth;
        this.numberOfTargets = numberOfTargets;

        sequences = ComputeIndexOfDifficultySequences(amplitudes, widths);
    }

    List<IndexOfDifficulty> ComputeIndexOfDifficultySequences(float[] amplitudes, float[] widths)
    {                
        if (amplitudes != null && widths != null)
        {
            List<IndexOfDifficulty> sequences = new List<IndexOfDifficulty>();
            foreach (float a in amplitudes)
            {
                foreach (float w in widths)
                {
                    sequences.Add(new IndexOfDifficulty(w, a));
                }
            }
            return sequences;
        }
        return null;
    }
}
