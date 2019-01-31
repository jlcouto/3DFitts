using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Interaction;
using UnityEngine.Events;

public class LeapMotionControllerCursorBehaviour : CursorBehaviour
{
    public LeapServiceProvider leapService;
    public InteractionManager manager;
    public GameObject cursorInteractor;

    public override Vector3 GetCursorPosition() { return Vector3.one; }

    public void AddInteractionBehaviourToObject(GameObject obj)
    {
        //var mesh = obj.GetComponent<MeshRenderer>();
        //var interaction = obj.GetComponent<InteractionBehaviour>();
        //interaction.manager = manager;
        //interaction.ignoreHoverMode = IgnoreHoverMode.Both;
        //interaction.ignoreGrasping = true;
    }

    private void Update()
    {
        Leap.Frame frame = leapService.CurrentFrame;
        if (frame.Hands.Count > 0)
        {
            cursorInteractor.transform.position = frame.Hands[0].GetIndex().TipPosition.ToVector3();
        }
        //if (input.getkeydown(keycode.space))
        //{

        //    acquiretarget(atarget);
        //}
    }

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
        if (listener != null)
        {
            listener.CursorAcquiredTarget(theTarget);
        }
    }
}
