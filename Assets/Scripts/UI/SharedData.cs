using System.Collections.Generic;
using Newtonsoft.Json;

public static class SharedData
{
    public static ExperimentConfiguration currentConfiguration = new ExperimentConfiguration();
    public static CalibrationData calibrationData = new CalibrationData();
}