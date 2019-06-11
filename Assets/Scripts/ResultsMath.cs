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

    public static double EffectiveWidthForStdevValue(double stdev)
    {
        return Math.Sqrt(2*Math.PI*Math.Exp(1)) * stdev;
    }

    public static double Projected3DPointCoordinate(Vector3 initialTargetPosition, Vector3 finalTargetPosition, Vector3 realInteractionPoint)
    {
        // Returns the realInteractionPoint projected into the line defined by initial and final target positions, considering that the origin
        // of the coordinates is at the finalTargetPosition and that the coordinates are negative if they are in the same side of the initialTargetPosition point

        // This formula was obtained trigonometrically.
        // A positive value indicates an overshoot occurred, whereas a negative value indicates an undershoot occurred.
        double distanceRealToFinal = Vector3.Distance(realInteractionPoint, finalTargetPosition);
        double distanceRealToInitial = Vector3.Distance(realInteractionPoint, initialTargetPosition);
        double distanceInitialToFinal = Vector3.Distance(initialTargetPosition, finalTargetPosition);
        return (Math.Pow(distanceRealToInitial, 2) - (Math.Pow(distanceRealToFinal, 2) + Math.Pow(distanceInitialToFinal, 2))) / (2 * distanceInitialToFinal);
    }

    public static double Projected3DPointCoordinateV2(Vector3 initialTargetPosition, Vector3 finalTargetPosition, Vector3 realInteractionPoint)
    {
        // This method is the same as the Projected3DPointCoordinate, but using vectors dot product to find the projection of realInteractionPoint
        // in the line defined by initialTargetPosition and finalTargetPosition

        // Computing distance between 'realInteractionPoint' and the line determined by 'initialTargetPosition' and 'finalTargetPosition'
        Vector3 x1x0 = initialTargetPosition - realInteractionPoint;
        Vector3 x2x1 = finalTargetPosition - initialTargetPosition;
        double x1x0_2 = Vector3.Dot(x1x0, x1x0);
        double x2x1_2 = Vector3.Dot(x2x1, x2x1);

        // We must round the value because when the real point is very near or over the line, floating errors may result in negative numbers
        // inside the square-root (when they should be zero), generating NaN values
        double Dreal_projection = Math.Sqrt((Math.Round(x1x0_2 * x2x1_2, 6) - Math.Round(Math.Pow((Vector3.Dot(x1x0, x2x1)), 2), 6)) / x2x1_2);

        // Finding the distance between 'initialTargetPosition' and the point of the projection of 'realInteractionPoint' onto the line
        double Dinitial_real = Vector3.Distance(initialTargetPosition, realInteractionPoint);
        double Dinitial_final = Vector3.Distance(initialTargetPosition, finalTargetPosition);
        double Theta_real_initial_final = Math.Asin(Dreal_projection / Dinitial_real);
        double Dinitial_projection = Dinitial_real * Math.Cos(Theta_real_initial_final);

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
