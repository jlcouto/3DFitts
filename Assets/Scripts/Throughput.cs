using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throughput
{
    public readonly int totalTrials;
    public readonly int missedTrials;

    public readonly double errorRate;
    public readonly double throughput;
    public readonly double averageMovementTime;

    public readonly double effectiveAmplitude;
    public readonly double effectiveWidth;
    public readonly double effectiveIndexOfDifficulty;

    public Throughput(TestMeasurements test)
    {
        totalTrials = 0;
        missedTrials = 0;

        double totalMovementTime = 0;
        double totalEffectiveAmplitude = 0;
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
                    totalEffectiveAmplitude += t.effectiveAmplitudeOfMovement;
                    projectedCoordinates.Add(t.finalPositionProjectedOnMovementAxis);
                }                
            }
        }
        double stdev = ResultsMath.ComputeStandardDeviation(projectedCoordinates);

        errorRate = (float)missedTrials / (float)totalTrials;
        averageMovementTime = totalMovementTime / totalTrials;
        effectiveAmplitude = totalEffectiveAmplitude / totalTrials;
        effectiveWidth = ResultsMath.EffectiveWidthForStdevValue(stdev);
        effectiveIndexOfDifficulty = ResultsMath.IndexOfDifficulty(effectiveWidth, effectiveAmplitude);
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
