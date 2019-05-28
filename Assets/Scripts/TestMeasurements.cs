using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMeasurements {
    /* Test Configuration */
    public TestConfiguration testConfiguration;

    /* Test Data */
    public string timestamp;

    public float initialTime;
    public float finalTime;
    public float testDuration { get { return finalTime - initialTime; }}

    public List<BlockMeasurements> blocksData;

    public TestMeasurements(TestConfiguration configuration) {
        this.testConfiguration = configuration;
        blocksData = new List<BlockMeasurements>(configuration.numOfBlocksPerTest);
    }

    public Dictionary<string, object> SerializeToDictionary()
    {
        Dictionary<string, object> data = new Dictionary<string, object>();
        data["timestamp"] = timestamp;
        data["testId"] = testConfiguration.testId;
        data["planeOrientation"] = Enum2String.GetPlaneOrientationString(testConfiguration.planeOrientation);
        data["initialTime"] = initialTime;
        data["finalTime"] = finalTime;
        data["testDuration"] = testDuration;

        List<Dictionary<string, object>> blocks = new List<Dictionary<string, object>>(blocksData.Count);
        foreach (BlockMeasurements b in blocksData)
        {
            blocks.Add(b.SerializeToDictionary());            
        }
        data["blocksData"] = blocks;
        
        var configuration = testConfiguration.SerializeToDictionary();

        Dictionary<string, object> theTest = new Dictionary<string, object>();
        theTest["configuration"] = configuration;
        theTest["data"] = data;

        Dictionary<string, object> output = new Dictionary<string, object>();
        output["test"] = theTest;
        output["computedResults"] = ComputeResults();

        return output;
    }

    public Dictionary<string, object> ComputeResults()
    {
        int totalTrials = 0;
        int totalMissedTargets = 0;
        float totalMovementTime = 0;
        List<double> projectedCoordinates = new List<double>();        

        foreach (BlockMeasurements b in blocksData)
        {
            foreach (TrialMeasurements t in b.trialsData)
            {
                totalTrials++;
                if (t.missedTarget)
                {
                    totalMissedTargets++;
                }
                totalMovementTime += t.trialDuration;
                projectedCoordinates.Add(t.finalPositionProjectedCoordinate);
            }
        }

        Dictionary<string, object> results = new Dictionary<string, object>();
        results["totalTrials"] = totalTrials;
        results["totalMissedTargets"] = totalMissedTargets;
        results["totalMovementTime"] = totalMovementTime;
        results["errorRate"] = (float)totalMissedTargets / (float)totalTrials;

        float averageMovementTime = totalMovementTime / totalTrials;
        results["averageMovementTime"] = averageMovementTime;

        float effectiveWidth = (float) (4.133f * ResultsMath.ComputeStandardDeviation(projectedCoordinates));
        results["effectiveWidth"] = effectiveWidth;

        float effectiveIndexOfDifficulty = ResultsMath.IndexOfDifficulty(effectiveWidth, testConfiguration.targetDistance);
        results["effectiveIndexOfDifficulty"] = effectiveIndexOfDifficulty;

        results["throughput"] = effectiveIndexOfDifficulty / averageMovementTime;
        
        return results;
    }

}
