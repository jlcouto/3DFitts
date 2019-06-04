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
    DwellTime,
    KeyboardSpaceBar,
    MouseLeftButton,
    FirstEntrySelection,
    Meta2GrabInteraction,
    LeapMotionGrabInteraction,
    VIVETriggerButton
}

public enum PlaneOrientation
{
    PlaneXY = 0,
    PlaneYZ,
    PlaneZX    
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
        return "?";
    }

    public static string GetPlaneOrientationString(PlaneOrientation orientation)
    {
        switch (orientation)
        {
            case PlaneOrientation.PlaneXY: return "XY";
            case PlaneOrientation.PlaneYZ: return "YZ";
            case PlaneOrientation.PlaneZX: return "ZX";
            default: return "?";
        }
    }
}