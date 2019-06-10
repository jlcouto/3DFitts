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

    public float effectiveIndexOfDifficulty { get; set; }
    public float averageMovementTime { get; set; }
    public float throughput { get; set; }
    public float errorRate { get; set; }

    public string task { get; set; }
    public string cursorPositioningMethod { get; set; }
    public string cursorSelectionMethod { get; set; }

    public int numberOfTargets { get; set; }
    public float amplitude { get; set; }
    public float width { get; set; }
    public float indexOfDifficulty { get; set; }
    public float cursorWidth { get; set; }
    public string planeOrientation { get; set; }

    public float xPlanePosition { get; set; }
    public float yPlanePosition { get; set; }
    public float zPlanePosition { get; set; }
    public float xPlaneRotation { get; set; }
    public float yPlaneRotation { get; set; }
    public float zPlaneRotation { get; set; }

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

    public int missedTarget { get; set; }

    public float projectionOnMovementAxis { get; set; }
    public float effectiveWidth { get; set; }

    public string observations { get; set; }

    public static List<ExperimentResultRecord> GetRecordsFromTestMeasurements(TestMeasurements test)
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

                //r.effectiveIndexOfDifficulty = (float)computed["effectiveIndexOfDifficulty"];
                //r.errorRate = (float)computed["errorRate"];
                //r.averageMovementTime = (float)computed["averageMovementTime"];
                //r.throughput = (float)computed["throughput"];

                r.task = Enum2String.GetTaskString(test.configuration.experimentTask);
                r.cursorPositioningMethod = Enum2String.GetCursorPositioningMethodString(test.configuration.cursorPositioningMethod);
                r.cursorSelectionMethod = Enum2String.GetCursorSelectionMethodString(test.configuration.cursorSelectionMethod);

                r.numberOfTargets = test.configuration.numberOfTargets;
                r.amplitude = test.sequenceInfo.targetsDistance;
                r.width = test.sequenceInfo.targetWidth;
                r.indexOfDifficulty = test.sequenceInfo.indexOfDifficulty;
                r.cursorWidth = test.configuration.cursorWidth;
                r.planeOrientation = Enum2String.GetPlaneOrientationString(test.configuration.planeOrientation);

                r.timestamp = test.timestamp;
                r.totalDuration = test.testDuration;

                r.blockId = b.blockId;
                r.blockDuration = b.blockDuration;

                r.trialId = t.trialId;
                r.trialDuration = t.trialDuration;

                r.fromTargetId = t.initialTargetId;
                r.xFrom = t.initialPosition.x;
                r.yFrom = t.initialPosition.y;
                r.zFrom = t.initialPosition.z;


                r.toTargetId = t.finalTargetId;
                r.xTo = t.finalPosition.x;
                r.yTo = t.finalPosition.y;
                r.zTo = t.finalPosition.z;

                r.xFromMeasured = t.initialPosition.x;
                r.yFromMeasured = t.initialPosition.y;
                r.zFromMeasured = t.initialPosition.z;

                r.xToMeasured = t.finalPosition.x;
                r.yToMeasured = t.finalPosition.y;
                r.zToMeasured = t.finalPosition.z;

                r.missedTarget = t.missedTarget ? 1 : 0;

                //r.projectionOnMovementAxis = t.finalPositionProjectedCoordinate;
                //r.effectiveWidth = (float)computed["effectiveWidth"];

                r.observations = test.configuration.observations;

                results.Add(r);
            }
        }

        return results;
    }
}
