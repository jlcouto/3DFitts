using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

[JsonObject(MemberSerialization.OptIn)]
public class ExperimentConfiguration
{
    /// <summary>
    /// The code to designate a participant, which will be included in the results of the test.
    /// Participants' personal information should be tracked separetely from the tests results.
    /// </summary>
    public string participantCode;

    /// <summary>
    /// Use this field to specify different conditions between tests that have the same configuration.
    /// For instance, you may wish to test the same configuration in a bright and in a dark environment
    /// to see how it affects users' performance.
    /// </summary>
    public string conditionCode;

    /// <summary>
    /// A session denotes an execution of a test configuration.
    /// Everytime a participant repeats the same configuration, the session code should go up.
    /// </summary>
    public string sessionCode;

    /// <summary>
    /// The code of the test group the participant is in
    /// </summary>
    public string groupCode;

    /// <summary>
    /// Any relevant observations to be saved together with the results of the test.
    /// </summary>
    public string observations;

    /// <summary>
    /// The desired environment to run the test (2D or 3D).
    /// </summary>
    [JsonProperty]
    public ExperimentMode experimentMode;

    /// <summary>
    /// The type of task to be performed during the tests.
    /// </summary>
    [JsonProperty]
    public ExperimentTask experimentTask;

    /// <summary>
    /// The method to use to drive the cursor position.
    /// </summary>
    [JsonProperty]
    public CursorPositioningMethod cursorPositioningMethod;

    /// <summary>
    /// The method to use for selecting the targets.
    /// </summary>
    [JsonProperty]
    public CursorSelectionMethod cursorSelectionMethod;

    /// <summary>
    /// The desired orientation of the plane of targets to test.
    /// </summary>
    [JsonProperty]
    public PlaneOrientation planeOrientation;

    /// <summary>
    /// Dwell time in seconds.
    /// </summary>
    [JsonProperty]
    public float dwellTime;

    /// <summary>
    /// The diameter of the cursor in meters.
    /// </summary>
    [JsonProperty]
    public float cursorWidth;

    /// <summary>
    /// Number of targets in the multidirectional task.
    /// </summary>
    [JsonProperty]
    public int numberOfTargets;

    /// <summary>
    /// Amplitudes of movements to test, in meters.
    /// These values will be fully crossed with the ones provided in the 'widths' array to determine the sequences to test.
    /// </summary>
    [JsonProperty]
    public float[] amplitudes
    {
        get { return _amplitudes; }
        set
        {
            _amplitudes = value;
            _sequecesDirtyBit = true;
        }
    }
    private float[] _amplitudes;

    /// <summary>
    /// Widths of targets to test, in meters.
    /// These values will be fully crossed with the ones provided in the 'amplitudes' array to determine the sequences to test.
    /// </summary>
    [JsonProperty]
    public float[] widths
    { 
        get { return _widths; }
        set {
            _widths = value;
            _sequecesDirtyBit = true;
            }
    }
    private float[] _widths;

    private bool _sequecesDirtyBit = false;
    public List<IndexOfDifficulty> sequences
    {
        get
        {
            if (_sequecesDirtyBit)
            {
                _sequecesDirtyBit = false;
                ComputeIndexOfDifficultySequences(amplitudes, widths, _sequences);
            }
            return _sequences;
        }
    }
    private List<IndexOfDifficulty> _sequences = new List<IndexOfDifficulty>();

    public void ComputeIndexOfDifficultySequences(float[] A, float[] W, List<IndexOfDifficulty> result)
    {
        result.Clear();     
        if (A != null && W != null)
        {
            foreach (float a in A)
            {
                foreach (float w in W)
                {
                    result.Add(new IndexOfDifficulty(w, a));
                }
            }
        }
    }
}
