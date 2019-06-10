using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throughput
{
    public readonly int totalTrials;
    public readonly int missedTrials;

    public readonly float errorRate;
    public readonly float throughput;
    public readonly float averageMovementTime;

    public readonly float effectiveWidth;
    public readonly float effectiveIndexOfDifficulty;

    public Throughput(TestMeasurements test)
    {
        totalTrials = 0;
        missedTrials = 0;

        float totalMovementTime = 0;
        List<double> projectedCoordinates = new List<double>();

        foreach (BlockMeasurements b in test.blocksData)
        {
            foreach (TrialMeasurements t in b.trialsData)
            {
                totalTrials++;
                if (t.missedTarget)
                {
                    missedTrials++;
                }
                totalMovementTime += t.trialDuration;
                projectedCoordinates.Add(t.finalPositionProjectedOnMovementAxis);
            }
        }

        errorRate = (float)missedTrials / (float)totalTrials;
        averageMovementTime = totalMovementTime / totalTrials;
        effectiveWidth = (float)(4.133f * ResultsMath.ComputeStandardDeviation(projectedCoordinates));
        effectiveIndexOfDifficulty = ResultsMath.IndexOfDifficulty(effectiveWidth, test.sequenceInfo.targetsDistance);
        throughput = effectiveIndexOfDifficulty / averageMovementTime;
    }
}
