using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Interaction;

public class LeapMotionControllerCursorBehaviour : CursorBehaviour
{
    public LeapServiceProvider leapService;
    public InteractionManager manager;

    public override Vector3 GetCursorPosition() { return Vector3.one; }

    public void AddInteractionBehaviourToObject(GameObject obj)
    {
        var interaction = obj.AddComponent<InteractionBehaviour>();
        interaction.manager = manager;
        interaction.ignoreHoverMode = IgnoreHoverMode.Both;
        interaction.ignoreGrasping = true;
        interaction.OnPerControllerContactBegin += new System.Action<InteractionController>(OnContactBegin);
        interaction.OnContactEnd += new System.Action(OnContactEnd);
    }

    public void OnContactBegin(InteractionController controller)
    {
        
    }

    public void OnContactEnd()
    {

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
