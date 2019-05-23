using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public static class ResultsMath
{
    public static float IndexOfDifficulty(float targetWidth, float targetsDistance)
    {
        return Mathf.Log((targetsDistance / targetWidth + 1), 2);
    }

    public static float Projected3DPointCoordinate(Vector3 initialTargetPosition, Vector3 finalTargetPosition, Vector3 realInteractionPoint)
    {
        // Returns the realInteractionPoint projected into the line defined by initial and final target positions, considering that the origin
        // of the coordinates is at the finalTargetPosition and that the coordinates are negative if they are in the same side of the initialTargetPosition point
        // Computing distance between 'realInteractionPoint' and the line determined by 'initialTargetPosition' and 'finalTargetPosition'
        Vector3 x1x0 = initialTargetPosition - realInteractionPoint;
        Vector3 x2x1 = finalTargetPosition - initialTargetPosition;
        float x1x0_2 = Vector3.Dot(x1x0, x1x0);
        float x2x1_2 = Vector3.Dot(x2x1, x2x1);
        float Dreal_projection = Mathf.Sqrt((x1x0_2 * x2x1_2 - Mathf.Pow((Vector3.Dot(x1x0, x2x1)), 2)) / x2x1_2);

        // Finding the distance between 'initialTargetPosition' and the point of the projection of 'realInteractionPoint' onto the line
        float Dinitial_real = Vector3.Distance(initialTargetPosition, realInteractionPoint);
        float Dinitial_final = Vector3.Distance(initialTargetPosition, finalTargetPosition);
        float Theta_real_initial_final = Mathf.Asin(Dreal_projection / Dinitial_real);
        float Dinitial_projection = Dinitial_real * Mathf.Cos(Theta_real_initial_final);

        // Return the desired coordinate of the projected point into the line, relative to the final point
        // A positive value indicates an overshoot occurred, whereas a negative value indicates an undershoot occurred.
        return Dinitial_projection - Dinitial_final;
    }

    public static double ComputeStandardDeviation(this IEnumerable<double> values)
    {
        double avg = values.Average();
        int numSamples = values.Count();
        return Math.Sqrt(values.Sum(v => Math.Pow(v - avg, 2))/(numSamples - 1));
    }

    public static double ComputeStandardDeviationPopulation(this IEnumerable<double> values)
    {
        double avg = values.Average();
        return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
    }
}
