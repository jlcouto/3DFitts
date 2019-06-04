public static class CanvasExperimentConfigurationValues
{
    public static string participantCode;
    public static string conditionCode;
    public static string sessionCode;
    public static string groupCode;
    public static string observations;

    public static ExperimentMode experimentMode;
    public static ExperimentTask experimentTask;
    public static CursorPositioningMethod cursorPositioningMethod;
    public static CursorSelectionMethod cursorSelectionMethod;

    /// <summary>
    /// The desired orientation of the plane of targets to test.
    /// </summary>
    public static PlaneOrientation planeOrientation;

    /// <summary>
    /// Dwell time in seconds.
    /// </summary>
    public static float dwellTime;

    /// <summary>
    /// The diameter of the cursor in meters.
    /// </summary>
    public static float cursorWidth;

    /// <summary>
    /// Number of targets in the multidirectional task.
    /// </summary>
    public static int numberOfTargets;

    /// <summary>
    /// Amplitudes of movements to test, in meters.
    /// These values will be fully crossed with the ones provided in the 'widths' array to determine the sequences to test.
    /// </summary>
    public static float[] amplitudes;

    /// <summary>
    /// Widths of targets to test, in metters.
    /// These values will be fully crossed with the ones provided in the 'amplitudes' array to determine the sequences to test.
    /// </summary>
    public static float[] widths;

    public static bool TrySetDwellTimeFromString(string dwellTimeString)
    {
        float[] temp;
        if (ParseFloatsOnString(dwellTimeString, out temp, 0.001f))
        {
            dwellTime = temp[0];
            return true;
        }
        else
        {
            dwellTime = -1;
            return false;
        }
    }

    public static bool TrySetCursorWidthFromString(string stringCursorWidth)
    {
        float[] temp;
        if (ParseFloatsOnString(stringCursorWidth, out temp, 0.001f))
        {
            cursorWidth = temp[0];
            return true;
        }
        else
        {
            cursorWidth = -1;
            return false;
        }
    }

    public static bool TrySetAmplitudesFromString(string stringAmplitudes)
    {
        return ParseFloatsOnString(stringAmplitudes, out amplitudes, 0.001f);
    }

    public static bool TrySetWidthsFromString(string stringWidths)
    {
        return ParseFloatsOnString(stringWidths, out widths, 0.001f);
    }

    static bool ParseFloatsOnString(string stringWithValues, out float[] values, float scale = 1)
    {
        values = null;
        char[] delimiters = { ' ', ',' };
        string[] stringValues = stringWithValues.Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries);

        if (stringValues.Length > 0)
        {
            values = new float[stringValues.Length];
        }

        for (int i = 0; i < stringValues.Length; i++)
        {
            if (!float.TryParse(stringValues[i], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out values[i]))
            {
                return false;
            }
        }

        if (scale != 1)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] *= scale;
            }
        }

        return true;
    }
}
