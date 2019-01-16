using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseCursorBehaviour : CursorBehaviour
{
    void Update()
    {
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

    public override Vector3 GetCursorPosition() { return Input.mousePosition; }

    public override void EnterTarget(TargetBehaviour theTarget)
    {
        theTarget.HighlightTarget();
        if (listener != null)
        {
            listener.CursorEnteredTarget(theTarget);
        }
    }

    public override void ExitTarget(TargetBehaviour theTarget)
    {
        theTarget.UnhighlightTarget();
        if (listener != null)
        {
            listener.CursorExitedTarget(theTarget);
        }
    }

    public override void AcquireTarget(TargetBehaviour theTarget)
    {
        if (theTarget != null)
        {
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
}
