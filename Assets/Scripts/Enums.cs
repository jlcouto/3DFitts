public enum ExperimentMode
{
    Experiment2D,
    Experiment3DOnMeta2
}

public enum ExperimentTask
{
    ReciprocalTapping,
    ReciprocalDragging
}

public enum CursorPositioningMethod
{
    Mouse,
    Meta2Interaction,
    LeapMotionController,
    VIVE
}

public enum CursorSelectionMethod
{
    MouseLeftButton,
    DwellTime,
    SelectionOnContact,
    KeyboardSpaceBar,
    Meta2GrabInteraction,
    LeapMotionGrabInteraction,
    VIVETriggerButton
}

public enum PlaneOrientation
{
    PlaneXY = 0,
    PlaneZX,
    PlaneYZ        
}

public static class Enum2String
{
    public static string GetTaskString(ExperimentTask task)
    {
        switch (task)
        {
            case ExperimentTask.ReciprocalDragging:
                return "ReciprocalDragging";
            case ExperimentTask.ReciprocalTapping:
                return "ReciprocalTapping";
        }
        return task.ToString();
    }

    public static string GetCursorPositioningMethodString(CursorPositioningMethod method)
    {
        switch (method)
        {                       
            case CursorPositioningMethod.Mouse:
                return "Mouse";
            case CursorPositioningMethod.Meta2Interaction:
                return "Meta2Interaction";
            case CursorPositioningMethod.LeapMotionController:
                return "LeapMotionController";
            case CursorPositioningMethod.VIVE:
                return "VIVEController";
        }
        return method.ToString();
    }

    public static string GetCursorSelectionMethodString(CursorSelectionMethod method)
    {
        switch (method)
        {
            case CursorSelectionMethod.MouseLeftButton:
                return "MouseLeftButton";
            case CursorSelectionMethod.DwellTime:
                return "DwellTime";
            case CursorSelectionMethod.SelectionOnContact:
                return "OnContact";
            case CursorSelectionMethod.KeyboardSpaceBar:
                return "KeyboardSpacebar";
            case CursorSelectionMethod.Meta2GrabInteraction:
                return "Meta2GrabInteraction";
            case CursorSelectionMethod.LeapMotionGrabInteraction:
                return "LeapMotionGrabInteraction";
            case CursorSelectionMethod.VIVETriggerButton:
                return "VIVETrigger";
        }
        return method.ToString();
    }

    public static string GetPlaneOrientationString(PlaneOrientation orientation)
    {
        switch (orientation)
        {
            case PlaneOrientation.PlaneXY: return "XY";
            case PlaneOrientation.PlaneYZ: return "YZ";
            case PlaneOrientation.PlaneZX: return "ZX";
            default: return orientation.ToString();
        }
    }
}