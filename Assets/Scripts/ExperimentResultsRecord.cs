using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This class will be used easily to export the results (represented by its properties) to a .csv file with CSVHelper.
/// </summary>
public class ExperimentResultRecord
{
    public string participantCode { get; set; }
    public string conditionCode { get; set; }
    public string sessionCode { get; set; }
    public string groupCode { get; set; }

    public string task { get; set; }
    public string cursorPositioningMethod { get; set; }
    public string cursorSelectionMethod { get; set; }

    public int numberOfTargets { get; set; }
    public float amplitude { get; set; }
    public float width { get; set; }
    public double indexOfDifficulty { get; set; }
    public float cursorWidth { get; set; }
    public string planeOrientation { get; set; }

    public double effectiveIndexOfDifficulty { get; set; }
    public double averageMovementTime { get; set; }
    public double throughput { get; set; }
    public double errorRate { get; set; }

    public double effectiveWidth { get; set; }
    public double effectiveAmplitude { get; set; }
    public double distanceError { get; set; } // Distance between the desired 'to' point and the 'toMeasured' point

    public double projectionOnMovementAxis { get; set; }
    public double amplitudeOnMovementAxis { get; set; }

    public int missedTarget { get; set; }
    public int markedAsOutlier { get; set; }

    public string timestamp { get; set; }
    public float totalDuration { get; set; }

    public int blockId { get; set; }
    public float blockDuration { get; set; }

    public int trialId { get; set; }
    public float trialDuration { get; set; }

    public int fromTargetId { get; set; }
    public float xFrom { get; set; }
    public float yFrom { get; set; }
    public float zFrom { get; set; }

    public int toTargetId { get; set; }
    public float xTo { get; set; }
    public float yTo { get; set; }
    public float zTo { get; set; }

    public float xFromMeasured { get; set; }
    public float yFromMeasured { get; set; }
    public float zFromMeasured { get; set; }

    public float xToMeasured { get; set; }
    public float yToMeasured { get; set; }
    public float zToMeasured { get; set; }

    public float xPlanePosition { get; set; }
    public float yPlanePosition { get; set; }
    public float zPlanePosition { get; set; }
    public float xPlaneRotation { get; set; }
    public float yPlaneRotation { get; set; }
    public float zPlaneRotation { get; set; }
    public float screenPixelsPerMillimeter { get; set; }

    public string observations { get; set; }

    public static List<ExperimentResultRecord> GetRecordsFromTestMeasurements(TestMeasurements test, Vector3 experimentPosition, Vector3 experimentRotation)
    {
        List<ExperimentResultRecord> results = new List<ExperimentResultRecord>();
        foreach (BlockMeasurements b in test.blocksData)
        {
            foreach (TrialMeasurements t in b.trialsData)
            {
                ExperimentResultRecord r = new ExperimentResultRecord();
                r.participantCode = test.configuration.participantCode;
                r.conditionCode = test.configuration.conditionCode;
                r.sessionCode = test.configuration.sessionCode;
                r.groupCode = test.configuration.groupCode;

                r.task = Enum2String.GetTaskString(test.configuration.experimentTask);
                r.cursorPositioningMethod = Enum2String.GetCursorPositioningMethodString(test.configuration.cursorPositioningMethod);
                r.cursorSelectionMethod = Enum2String.GetCursorSelectionMethodString(test.configuration.cursorSelectionMethod);

                r.numberOfTargets = test.configuration.numberOfTargets;
                r.amplitude = test.sequenceInfo.targetsDistance;
                r.width = test.sequenceInfo.targetWidth;
                r.indexOfDifficulty = test.sequenceInfo.indexOfDifficulty;
                r.cursorWidth = test.configuration.cursorWidth;
                r.planeOrientation = Enum2String.GetPlaneOrientationString(test.configuration.planeOrientation);

                r.errorRate = test.computedResults.errorRate;
                r.averageMovementTime = test.computedResults.averageMovementTime;
                r.throughput = test.computedResults.throughput;

                r.effectiveIndexOfDifficulty = test.computedResults.effectiveIndexOfDifficulty;
                r.effectiveWidth = test.computedResults.effectiveWidth;
                r.effectiveAmplitude = test.computedResults.effectiveAmplitude;

                r.projectionOnMovementAxis = t.finalPositionProjectedOnMovementAxis;
                r.amplitudeOnMovementAxis = t.effectiveAmplitudeOfMovement;
                r.distanceError = t.distanceErrorFromTarget;

                r.timestamp = test.timestamp;
                r.totalDuration = test.testDuration;

                r.blockId = b.blockId;
                r.blockDuration = b.blockDuration;

                r.trialId = t.trialId;
                r.trialDuration = t.trialDuration;

                r.fromTargetId = t.initialTargetId;
                r.xFrom = t.initialTargetPosition.x;
                r.yFrom = t.initialTargetPosition.y;
                r.zFrom = t.initialTargetPosition.z;

                r.toTargetId = t.finalTargetId;
                r.xTo = t.finalTargetPosition.x;
                r.yTo = t.finalTargetPosition.y;
                r.zTo = t.finalTargetPosition.z;

                r.xFromMeasured = t.initialPosition.x;
                r.yFromMeasured = t.initialPosition.y;
                r.zFromMeasured = t.initialPosition.z;

                r.xToMeasured = t.finalPosition.x;
                r.yToMeasured = t.finalPosition.y;
                r.zToMeasured = t.finalPosition.z;

                r.missedTarget = t.missedTarget ? 1 : 0;
                r.markedAsOutlier = t.isMarkedAsOutlier ? 1 : 0;

                r.xPlanePosition = experimentPosition.x;
                r.yPlanePosition = experimentPosition.y;
                r.zPlanePosition = experimentPosition.z;
                
                r.xPlaneRotation = experimentRotation.x;
                r.yPlaneRotation = experimentRotation.y;
                r.zPlaneRotation = experimentRotation.z;

                r.screenPixelsPerMillimeter = test.configuration.screenPixelsPerMillimeter;
                r.observations = test.configuration.observations;

                results.Add(r);
            }
        }

        return results;
    }
}
