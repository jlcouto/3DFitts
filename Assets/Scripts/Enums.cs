
public enum ExperimentTask
{
    ReciprocalTapping,
    Dragging
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
            case ExperimentTask.Dragging:
                return "DraggingTask";
            case ExperimentTask.ReciprocalTapping:
                return "ReciprocalTappingTest";
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