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

    public void SetAsStartingTestTarget()
    {
        type = TargetType.StartingTestTarget;
        GetComponent<MeshRenderer>().material = startingTestTargetMaterial;
    }

    public void SetAsNextTarget()
    {
        type = TargetType.NextTarget;
        GetComponent<MeshRenderer>().material = nextTargetMaterial;
    }

    public void SetAsNormalTarget()
    {
        type = TargetType.NormalTarget;
        GetComponent<MeshRenderer>().material = normalTargetMaterial;
    }

    public void SetAsDraggableTarget()
    {
        type = TargetType.DraggableTarget;
        GetComponent<MeshRenderer>().material = draggableTargetMaterial;
    }

    public void HighlightTarget()
    {
        GetComponent<MeshRenderer>().material = highlightedTargetMaterial;
    }

    public void UnhighlightTarget()
    {
        switch (type)
        {
            case TargetType.NextTarget: SetAsNextTarget(); return;   
            case TargetType.DraggableTarget: SetAsDraggableTarget(); return;
            default:
            case TargetType.NormalTarget: SetAsNormalTarget(); return;
        }
    }

}
