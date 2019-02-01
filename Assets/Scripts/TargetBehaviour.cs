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

    MeshRenderer meshRenderer;

	// Use this for initialization
	void Start () {
        meshRenderer = GetComponent<MeshRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetAsNextTarget() {
        type = TargetType.NextTarget;
        meshRenderer.material = nextTargetMaterial;
    }

    public void SetAsNormalTarget() {
        type = TargetType.NormalTarget;
        meshRenderer.material = normalTargetMaterial;
    }

    public void HighlightTarget() {
        meshRenderer.material = highlightedTargetMaterial;
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
