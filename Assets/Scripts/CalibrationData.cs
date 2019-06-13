using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class CalibrationData
{
    [JsonIgnore]
    public Vector3 leapMotionOffset
    {
        get
        {
            return VectorFromArray(_leapMotionOffset);
        }
        set
        {
            _leapMotionOffset[0] = value.x;
            _leapMotionOffset[1] = value.y;
            _leapMotionOffset[2] = value.z;
        }
    }

    [JsonIgnore]
    public Quaternion leapMotionRotation
    {
        get
        {
            return QuaternionFromArray(_leapMotionRotation);
        }
        set
        {
            _leapMotionRotation[0] = value.eulerAngles.x;
            _leapMotionRotation[1] = value.eulerAngles.y;
            _leapMotionRotation[2] = value.eulerAngles.z;
        }
    }

    [JsonProperty]
    private float[] _leapMotionOffset { get; set; } = { -0.015f, 0.06f, 0.1f };
    [JsonProperty]
    private float[] _leapMotionRotation { get; set; } = { 30, 0, 0 };

    public void LoadFromFile()
    {
        string json = FileManager.LoadFile(FileManager.GetCalibrationDataFolder(), FileManager.GetCalibrationDataFilename());
        if (json != null)
        {
            JsonConvert.PopulateObject(json, this);
        }
    }

    public void SaveToFile()
    {
        JObject obj = (JObject)JToken.FromObject(this);
        if (obj != null)
        {
            FileManager.SaveFile(FileManager.GetCalibrationDataFolder(), FileManager.GetCalibrationDataFilename(), obj.ToString());
        }
    }

    private Vector3 VectorFromArray(float[] array)
    {
        if (array.Length == 3) {
            return new Vector3(array[0], array[1], array[2]);
        }
        return Vector3.zero;
    }

    private Quaternion QuaternionFromArray(float[] array)
    {
        if (array.Length == 3)
        {
            return Quaternion.Euler(array[0], array[1], array[2]);
        }
        return Quaternion.identity;
    }
}