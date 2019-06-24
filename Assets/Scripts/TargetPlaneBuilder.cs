using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPlaneBuilder {
	public static void Build (GameObject baseTarget, Transform targetPlane, IndexOfDifficulty sequence, int numberOfTargets) {
        if (numberOfTargets % 2 == 0) // If even number
        { 
            numberOfTargets++; // number of targets must be an odd number
        }

        float thetaStep = -2 * Mathf.PI / numberOfTargets;
        int startTargetIndex = 0;
        int finalTargetIndex = (int) numberOfTargets / 2 + 1;
        for (int i = 0; i < numberOfTargets; i++) {
            int targetPositionIndex;
            if (i % 2 == 0) {
                targetPositionIndex = startTargetIndex;
                startTargetIndex++;
            }
            else {
                targetPositionIndex = finalTargetIndex;
                finalTargetIndex++;
            }
            GameObject newTarget = Object.Instantiate(baseTarget, targetPlane);
            newTarget.name = "Target " + i;

            var targetBehaviour = newTarget.GetComponent<TargetBehaviour>();
            targetBehaviour.targetId = targetPositionIndex;
            targetBehaviour.SetAsNormalTarget();

            newTarget.transform.localPosition = sequence.targetsDistance / 2 * (new Vector3(Mathf.Cos(targetPositionIndex * thetaStep), 0, Mathf.Sin(targetPositionIndex * thetaStep)));
            newTarget.transform.localScale = Vector3.one * sequence.targetWidth;
            newTarget.SetActive(true);
        }
	}
}