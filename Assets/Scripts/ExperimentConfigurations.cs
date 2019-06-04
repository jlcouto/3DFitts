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

public abstract class ExperimentConfiguration
{
    public abstract float GetCursorDiameter();
    public abstract ExperimentTask GetExperimentTask();
    public abstract int GetNumBlocksPerTest();
    public abstract int GetNumTargetsPerTest();
    public abstract PlaneOrientation[] GetPlaneOrientationsToTest();
    public abstract IndexOfDifficulty[] GetTargetConfigurationsToTest();
}

public static class CurrentExperimentConfiguration
{
    public static ExperimentMode experimentMode;
    public static ExperimentTask experimentTask;
    public static CursorPositioningMethod positioningMethod;
    public static CursorSelectionMethod selectionMethod;
    public static PlaneOrientation planeOrientation;
    public static float cursorDiameter;
    public static int numberOfTargets;
    public static float[] amplitudes;
    public static float[] widths;    
    public static List<IndexOfDifficulty> sequences = new List<IndexOfDifficulty>();

    public static void SetAmplitudes(string stringAmplitudes)
    {
        ParseFloatsOnString(stringAmplitudes, out amplitudes); 
    }

    public static void SetWidths(string stringWidths)
    {
        ParseFloatsOnString(stringWidths, out widths);
    }

    static void ComputeIndexOfDifficultySequences()
    {
        sequences.Clear();
        if (amplitudes != null && widths != null)
        {
            foreach (float a in amplitudes)
            {
                foreach (float w in widths)
                {
                    sequences.Add(new IndexOfDifficulty(w, a));
                }
            }
        }
    }

    static bool ParseFloatsOnString(string stringWithValues, out float[] values)
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
            if (!float.TryParse(stringValues[i], out values[i]))
            {
                return false;
            }
        }

        return true;
    }
}

public class Tapping3DMouseExperimentConfiguration : ExperimentConfiguration
{
    protected static readonly PlaneOrientation[] planeOrientations = { PlaneOrientation.PlaneXY, PlaneOrientation.PlaneZX, PlaneOrientation.PlaneYZ };
    static readonly IndexOfDifficulty[] configurations = {
        new IndexOfDifficulty(0.013f, 0.074f),
        new IndexOfDifficulty(0.013f, 0.103f),
        new IndexOfDifficulty(0.013f, 0.133f),
        new IndexOfDifficulty(0.0055f, 0.074f),
        new IndexOfDifficulty(0.0055f, 0.103f),
        new IndexOfDifficulty(0.0055f, 0.133f)
    };
    /*
    static readonly IndexOfDifficulty[] configurations = {
        new IndexOfDifficulty(0.035f, 0.2f),
        new IndexOfDifficulty(0.035f, 0.3f),
        new IndexOfDifficulty(0.035f, 0.4f),
        new IndexOfDifficulty(0.015f, 0.2f),
        new IndexOfDifficulty(0.015f, 0.3f),
        new IndexOfDifficulty(0.015f, 0.4f)
    };
    */
    /*static readonly IndexOfDifficulty[] configurations = {
        new IndexOfDifficulty(0.04f, 0.2f),
        new IndexOfDifficulty(0.04f, 0.3f),
        new IndexOfDifficulty(0.04f, 0.4f),
        new IndexOfDifficulty(0.02f, 0.2f),
        new IndexOfDifficulty(0.02f, 0.3f),
        new IndexOfDifficulty(0.02f, 0.4f),
        new IndexOfDifficulty(0.01f, 0.2f),
        new IndexOfDifficulty(0.01f, 0.3f),
        new IndexOfDifficulty(0.01f, 0.4f)
    };*/

    public override float GetCursorDiameter()
    {
        return 0.005f;
    }

    public override ExperimentTask GetExperimentTask()
    {
        return ExperimentTask.ReciprocalTapping;
    }

    public override int GetNumBlocksPerTest()
    {
        return 1;
    }

    public override int GetNumTargetsPerTest()
    {
        return 9;
    }

    public override PlaneOrientation[] GetPlaneOrientationsToTest()
    {
        return planeOrientations;
    }

    public override IndexOfDifficulty[] GetTargetConfigurationsToTest()
    {
        return configurations;
    }
}

public class Tapping2DMouseExperimentConfiguration : Tapping3DMouseExperimentConfiguration
{
    new protected static readonly PlaneOrientation[] planeOrientations = { PlaneOrientation.PlaneXY };

    public override PlaneOrientation[] GetPlaneOrientationsToTest()
    {
        return planeOrientations;
    }
}

public class Drag3DMouseExperimentConfiguration : ExperimentConfiguration
{
    protected static readonly PlaneOrientation[] planeOrientations = { PlaneOrientation.PlaneXY, PlaneOrientation.PlaneZX, PlaneOrientation.PlaneYZ };
    static readonly IndexOfDifficulty[] configurations = {
        new IndexOfDifficulty(0.035f, 0.2f),
        new IndexOfDifficulty(0.035f, 0.3f),
        new IndexOfDifficulty(0.035f, 0.4f),
        new IndexOfDifficulty(0.015f, 0.2f),
        new IndexOfDifficulty(0.015f, 0.3f),
        new IndexOfDifficulty(0.015f, 0.4f)
    };
    /*static readonly IndexOfDifficulty[] configurations = {
        new IndexOfDifficulty(0.04f, 0.2f),
        new IndexOfDifficulty(0.04f, 0.3f),
        new IndexOfDifficulty(0.02f, 0.2f),
        new IndexOfDifficulty(0.02f, 0.4f),
        new IndexOfDifficulty(0.01f, 0.3f),
        new IndexOfDifficulty(0.01f, 0.4f)
    };*/

    public override float GetCursorDiameter()
    {
        return 0.01f;
    }

    public override ExperimentTask GetExperimentTask()
    {
        return ExperimentTask.ReciprocalDragging;
    }

    public override int GetNumBlocksPerTest()
    {
        return 1;
    }

    public override int GetNumTargetsPerTest()
    {
        return 9;
    }

    public override PlaneOrientation[] GetPlaneOrientationsToTest()
    {
        return planeOrientations;
    }

    public override IndexOfDifficulty[] GetTargetConfigurationsToTest()
    {
        return configurations;
    }
}

public class Drag2DMouseExperimentConfiguration : Drag3DMouseExperimentConfiguration
{
    new protected static readonly PlaneOrientation[] planeOrientations = { PlaneOrientation.PlaneXY };

    public override PlaneOrientation[] GetPlaneOrientationsToTest()
    {
        return planeOrientations;
    }
}
