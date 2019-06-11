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
                UpdateOutlierState(t);
                if (!t.isMarkedAsOutlier)
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
        }
        double stdev = ResultsMath.ComputeStandardDeviation(projectedCoordinates);

        errorRate = (float)missedTrials / (float)totalTrials;
        averageMovementTime = totalMovementTime / totalTrials;
        effectiveWidth = (float)ResultsMath.EffectiveWidthForStdevValue(stdev);
        effectiveIndexOfDifficulty = ResultsMath.IndexOfDifficulty(effectiveWidth, test.sequenceInfo.targetsDistance);
        throughput = effectiveIndexOfDifficulty / averageMovementTime;
    }

    public void UpdateOutlierState(TrialMeasurements theTrial)
    {        
        Vector3 initialTargetPos = theTrial.initialTargetPosition;
        Vector3 finalTargetPos = theTrial.finalTargetPosition;
        float expectedAmplitude = (initialTargetPos - finalTargetPos).magnitude;

        Vector3 initialPos = theTrial.initialPosition;
        Vector3 finalPos = theTrial.finalPosition;
        float realAmplitude = (initialPos - finalPos).magnitude;

        theTrial.MarkAsOutlier(realAmplitude < (expectedAmplitude / 2));
    }
}
