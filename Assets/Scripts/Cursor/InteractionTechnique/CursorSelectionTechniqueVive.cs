using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorSelectionTechniqueVive : CursorSelectionTechnique
{
    public ViveControllerPositionBehaviour viveController;

    public CursorSelectionTechniqueVive(ViveControllerPositionBehaviour viveController)
    {
        this.viveController = viveController;
    }

    public override bool SelectionInteractionStarted()
    {
        return viveController.GetTriggerDown();
    }

    public override bool SelectionInteractionMantained()
    {
        return viveController.GetTrigger();
    }

    public override bool SelectionInteractionEnded()
    {
        return viveController.GetTriggerUp();
    }

    public override string GetInteractionName()
    {
        return "VIVEController_TriggerButton";
    }
}
