using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargetType
{
    NormalTarget,
    NextTarget
}

public class TargetBehaviour : MonoBehaviour
{
    public int targetId;

    public Material normalTargetMaterial;
    public Material highlightedTargetMaterial;
    public Material nextTargetMaterial;

    public TargetType type = TargetType.NormalTarget;

    public Vector3 position { get { return transform.position; } }
    public Vector3 localScale { get { return transform.localScale; } }

    public void SetAsNextTarget() {
        type = TargetType.NextTarget;
        GetComponent<MeshRenderer>().material = nextTargetMaterial;
    }

    public void SetAsNormalTarget() {
        type = TargetType.NormalTarget;
        GetComponent<MeshRenderer>().material = normalTargetMaterial;
    }

    public void HighlightTarget() {
        GetComponent<MeshRenderer>().material = highlightedTargetMaterial;
    }

    public void UnhighlightTarget() {
        if (type == TargetType.NextTarget)
        {
            SetAsNextTarget();
        }
        else
        {
            SetAsNormalTarget();
        }
    }

}
