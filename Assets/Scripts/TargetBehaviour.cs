using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargetType
{
    StartingTestTarget,
    NormalTarget,
    NextTarget,
    DraggableTarget
}

public class TargetBehaviour : MonoBehaviour
{
    public int targetId;

    public Material startingTestTargetMaterial;
    public Material normalTargetMaterial;
    public Material highlightedTargetMaterial;
    public Material nextTargetMaterial;
    public Material draggableTargetMaterial;

    public TargetType type = TargetType.NormalTarget;

    public Vector3 position { get { return transform.position; } }
    public Vector3 localScale { get { return transform.localScale; } }

    public bool isHighlighted = false;

    void UpdateTargetMaterial()
    {
        Material material = normalTargetMaterial;
        if (isHighlighted)
        {
            material = highlightedTargetMaterial;
        }
        else
        {
            switch (type)
            {
                case TargetType.StartingTestTarget: { material = startingTestTargetMaterial; break; }
                case TargetType.NextTarget: { material = nextTargetMaterial; break; }
                case TargetType.DraggableTarget: { material = draggableTargetMaterial; break; }
                default:
                case TargetType.NormalTarget: { material = normalTargetMaterial; break; }
            }
        }

        GetComponent<MeshRenderer>().material = material;
    }

    public void SetAsStartingTestTarget()
    {
        type = TargetType.StartingTestTarget;
        UpdateTargetMaterial();
    }

    public void SetAsNextTarget()
    {
        type = TargetType.NextTarget;
        UpdateTargetMaterial();
    }

    public void SetAsNormalTarget()
    {
        type = TargetType.NormalTarget;
        UpdateTargetMaterial();
    }

    public void SetAsDraggableTarget()
    {
        type = TargetType.DraggableTarget;
        UpdateTargetMaterial();
    }

    public void HighlightTarget()
    {
        isHighlighted = true;
        UpdateTargetMaterial();
    }

    public void UnhighlightTarget()
    { 
        isHighlighted = false;
        UpdateTargetMaterial();
    } 
}
