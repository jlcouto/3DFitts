using System.Collections.Generic;
using System.IO;
using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Some code that are not used anymore but may be useful sometimes when dealing with old stuff.
/// </summary>
public class LegacyCode
{
    private void ConvertJSONFilesToCSV(string directory, string csvFilename)
    {
        var filenames = FileManager.GetFilenamesOnDirectory(directory, ".json");

        StreamWriter writer = new StreamWriter(directory + csvFilename);
        CsvWriter csvWriter = new CsvWriter(writer);

        foreach (string filename in filenames)
        {
            string jsonData = FileManager.LoadFile(directory, filename);
            var dictData = JsonConvert.DeserializeObject(jsonData);
            csvWriter.WriteRecords(GenerateCSVFileFromDictionary((JObject)dictData));
        }
        writer.Flush();
        writer.Close();
    }

    /// <summary>
    /// Prior to the use of csv, the software used to write the results in a JSON file.
    /// The code below can decode those files.
    /// </summary>
    private List<ExperimentResultRecord> GenerateCSVFileFromDictionary(JObject data)
    {
        var test = (JObject)data["test"];
        var config = (JObject)test["configuration"];
        var targets = (JArray)config["targetsPositions"];
        var testData = (JObject)test["data"];
        var blocksData = (JArray)testData["blocksData"];
        var trialsData = (JArray)((JObject)blocksData[0])["trialsData"];
        var computed = (JObject)data["computedResults"];

        int trialCount = 0;
        List<ExperimentResultRecord> results = new List<ExperimentResultRecord>();
        foreach (JObject t in trialsData)
        {
            ExperimentResultRecord r = new ExperimentResultRecord();
            r.participantCode = (string)data["ParticipantCode"];
            r.conditionCode = (string)data["ConditionCode"];
            r.sessionCode = (string)data["SessionCode"];
            r.groupCode = (string)data["GroupCode"];

            r.task = (string)data["ExperimentTask"];
            r.cursorPositioningMethod = (string)data["PositioningMethod"];
            r.cursorSelectionMethod = (string)data["SelectionMethod"];

            r.numberOfTargets = (int)config["numberOfTargets"];
            r.amplitude = (float)config["targetDistance"];
            r.width = (float)config["targetWidth"];
            r.indexOfDifficulty = (float)config["indexOfDifficulty"];
            r.cursorWidth = data.GetValue("CursorWidth").Value<float>();
            r.planeOrientation = (string)data["PlaneOrientation"];

            r.timestamp = (string)testData["timestamp"];
            r.totalDuration = (float)testData["testDuration"];

            JObject block = (JObject)blocksData[0];
            r.blockId = (int)block["blockId"];
            r.blockDuration = (float)block["blockDuration"];

            r.trialId = (int)t["trialId"];
            r.trialDuration = (float)t["trialDuration"];

            r.fromTargetId = (int)t["initialTargetId"];
            JObject fromPos = (JObject)targets[trialCount];
            r.xFrom = (float)fromPos["x"];
            r.yFrom = (float)fromPos["y"];
            r.zFrom = (float)fromPos["z"];

            trialCount = (trialCount + 1) % r.numberOfTargets;

            r.toTargetId = (int)t["finalTargetId"];
            JObject toPos = (JObject)targets[trialCount];
            r.xTo = (float)toPos["x"];
            r.yTo = (float)toPos["y"];
            r.zTo = (float)toPos["z"];

            JObject fromMeasured = (JObject)t["initialPosition"];
            r.xTrialStarted = (float)fromMeasured["x"];
            r.yTrialStarted = (float)fromMeasured["y"];
            r.zTrialStarted = (float)fromMeasured["z"];

            JObject toMeasured = (JObject)t["finalPosition"];
            r.xActionEnded = (float)toMeasured["x"];
            r.yActionEnded = (float)toMeasured["y"];
            r.zActionEnded = (float)toMeasured["z"];

            r.missedTarget = (int)t["missedTarget"];

            r.projectionOnMovementAxis = (float)t["finalProjectedCoordinate"];
            r.effectiveWidth = (float)computed["effectiveWidth"];
            r.effectiveIndexOfDifficulty = (float)computed["effectiveIndexOfDifficulty"];

            r.errorRate = (float)computed["errorRate"];
            r.averageMovementTime = (float)computed["averageMovementTime"];
            r.throughput = (float)computed["throughput"];

            r.observations = (string)data["Observations"];

            results.Add(r);
        }

        return results;
    }
}
