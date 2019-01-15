using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICursorListener {
    void CursorEnteredTarget(TargetBehaviour target);
    void CursorExitedTarget(TargetBehaviour target);
    void CursorAcquiredTarget(TargetBehaviour target);
}

public class CursorBehaviour : MonoBehaviour {
    ICursorListener listener;

    public AudioSource correctAudio;
    public AudioSource errorAudio;

    public Vector3 position { get { return Input.mousePosition; } }

	void Start () {
		
	}
	
	void Update () {
        Vector3 mousePos = Input.mousePosition;
        if (Input.GetMouseButtonDown(0))
        {
            TargetBehaviour aTarget = null;
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out hit, 5.0f))
            {
                aTarget = hit.transform.GetComponent<TargetBehaviour>();
            }
            AcquireTarget(aTarget);
        }
    }

    public void RegisterNewListener(ICursorListener newListener)
    {
        listener = newListener;
    }

    public void RemoveCurrentListener() {
        listener = null;
    }

    public void EnterTarget(TargetBehaviour theTarget) {
        theTarget.HighlightTarget();
        if (listener != null)
        {
            listener.CursorEnteredTarget(theTarget);
        }
    }

    public void ExitTarget(TargetBehaviour theTarget) {
        theTarget.UnhighlightTarget();
        if (listener != null)
        {
            listener.CursorExitedTarget(theTarget);
        }
    }

    public void AcquireTarget(TargetBehaviour theTarget) {
        if (theTarget != null) {
            EnterTarget(theTarget);
            StartCoroutine(ExitTargetAfterTime(0.2f, theTarget));
        }

        if (listener != null)
        {
            listener.CursorAcquiredTarget(theTarget);
        }
    }

    IEnumerator ExitTargetAfterTime(float time, TargetBehaviour theTarget)
    {
        yield return new WaitForSeconds(time);
        ExitTarget(theTarget);
    }

    public void PlayCorrectAudio() {
        correctAudio.Play();
    }

    public void PlayErrorAudio() {
        errorAudio.Play();
    }
}
