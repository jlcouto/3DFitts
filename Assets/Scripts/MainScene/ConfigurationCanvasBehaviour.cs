using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

public class ConfigurationCanvasBehaviour : MonoBehaviour
{
    public Dropdown participantCode;
    public Dropdown conditionCode;
    public Dropdown sessionCode;
    public Dropdown groupCode;
    public InputField observations;

    public Dropdown experimentMode;    
    public Dropdown experimentTask;
    
    public Dropdown cursorPositioningMethod;    
    public Dropdown cursorSelectionMethod;
    
    public Dropdown planeOrientation;    
    public Dropdown numberOfTargets;
        
    public InputField dwellTime;    
    public InputField cursorWidth;    
    public InputField amplitudes;    
    public InputField widths;

    public SelectFilePanelBehaviour panelSelectFile;

    const float TIME_SCALE = 0.001f;
    const float DIMENSIONS_SCALE = 0.001f;

    void Start()
    {
        InitializeValues();
    }

    void InitializeValues()
    {
        ResetDropdown(participantCode);
        ResetDropdown(conditionCode);
        ResetDropdown(sessionCode);
        ResetDropdown(groupCode);

        observations.SetTextWithoutNotify("");
        OnInputFieldTextChanged(observations);

        ResetConfigurationValuesToDefault();
    }

    void ResetDropdown(Dropdown dropdown)
    {
        dropdown.SetValueWithoutNotify(0);
        OnDropdownValueChanged(dropdown);
    }

    public void ResetConfigurationValuesToDefault()
    {
        LoadValuesFromFile(FileManager.GetInternalConfigurationFolder(), FileManager.GetDefaultConfigurationFilename());
    }

    void UpdateCanvasFromCurrentValues()
    {
        UpdateCanvasElementWithInternalValue(experimentMode);
        UpdateCanvasElementWithInternalValue(experimentTask);
        UpdateCanvasElementWithInternalValue(cursorPositioningMethod);
        UpdateCanvasElementWithInternalValue(cursorSelectionMethod);
        UpdateCanvasElementWithInternalValue(planeOrientation);
        UpdateCanvasElementWithInternalValue(numberOfTargets);
        UpdateCanvasElementWithInternalValue(dwellTime);
        UpdateCanvasElementWithInternalValue(cursorWidth);
        UpdateCanvasElementWithInternalValue(amplitudes);
        UpdateCanvasElementWithInternalValue(widths);
        UpdateCanvasState();
    }

    void UpdateCanvasState()
    {
        switch (SharedData.currentConfiguration.experimentMode)
        {
            case ExperimentMode.Experiment2D:
                // For now, only mouse can be used for 2D experiment
                SharedData.currentConfiguration.cursorPositioningMethod = CursorPositioningMethod.Mouse;
                UpdateCanvasElementWithInternalValue(cursorPositioningMethod);
                cursorPositioningMethod.interactable = false;

                // For now, you can only test the plane of the screen
                SharedData.currentConfiguration.planeOrientation = PlaneOrientation.PlaneXY;
                UpdateCanvasElementWithInternalValue(planeOrientation);
                planeOrientation.interactable = false;
                break;
            case ExperimentMode.Experiment3DOnMeta2:
                cursorPositioningMethod.interactable = true;
                planeOrientation.interactable = true;
                break;
        }

        bool isDraggingTask = SharedData.currentConfiguration.experimentTask == ExperimentTask.ReciprocalDragging;

        // Drag task do not work with dwell and automatic selection for now
        cursorSelectionMethod.GetComponent<DropDownController>().EnableOption((int)CursorSelectionMethod.DwellTime, !isDraggingTask);
        cursorSelectionMethod.GetComponent<DropDownController>().EnableOption((int)CursorSelectionMethod.SelectionOnContact, !isDraggingTask);

        if (isDraggingTask &&
            (SharedData.currentConfiguration.cursorSelectionMethod == CursorSelectionMethod.DwellTime ||
             SharedData.currentConfiguration.cursorSelectionMethod == CursorSelectionMethod.SelectionOnContact))
        {
            SharedData.currentConfiguration.cursorSelectionMethod = CursorSelectionMethod.MouseLeftButton;
            UpdateCanvasElementWithInternalValue(cursorSelectionMethod);
        }

        dwellTime.interactable = SharedData.currentConfiguration.cursorSelectionMethod == CursorSelectionMethod.DwellTime;
    }

    void UpdateCanvasElementWithInternalValue(object element)
    {
        if (element == (object)experimentMode)
        {
            experimentMode.SetValueWithoutNotify((int)SharedData.currentConfiguration.experimentMode);
        }
        else if (element == (object)experimentTask)
        {
            experimentTask.SetValueWithoutNotify((int)SharedData.currentConfiguration.experimentTask);
        }
        else if (element == (object)cursorPositioningMethod)
        {
            cursorPositioningMethod.SetValueWithoutNotify((int)SharedData.currentConfiguration.cursorPositioningMethod);
        }
        else if (element == (object)cursorSelectionMethod)
        {
            cursorSelectionMethod.SetValueWithoutNotify((int)SharedData.currentConfiguration.cursorSelectionMethod);
        }
        else if (element == (object)planeOrientation)
        {
            planeOrientation.SetValueWithoutNotify((int)SharedData.currentConfiguration.planeOrientation);
        }
        else if (element == (object)numberOfTargets)
        {
            numberOfTargets.SetValueWithoutNotify((SharedData.currentConfiguration.numberOfTargets - 3) / 2);
        }
        else if (element == (object)dwellTime)
        {
            string text = FloatToCanvasString(SharedData.currentConfiguration.dwellTime, 1 / TIME_SCALE);
            dwellTime.SetTextWithoutNotify(text);
        }
        else if (element == (object)cursorWidth)
        {
            string text = FloatToCanvasString(SharedData.currentConfiguration.cursorWidth, 1 / DIMENSIONS_SCALE);
            cursorWidth.SetTextWithoutNotify(text);
        }
        else if (element == (object)amplitudes)
        {
            string text = FloatArrayToCanvasString(SharedData.currentConfiguration.amplitudes, 1 / DIMENSIONS_SCALE);
            amplitudes.SetTextWithoutNotify(text);
        }
        else if (element == (object)widths)
        {
            string text = FloatArrayToCanvasString(SharedData.currentConfiguration.widths, 1 / DIMENSIONS_SCALE);
            widths.SetTextWithoutNotify(text);
        }
    }

    public void OnDropdownValueChanged(Dropdown dropdown)
    {
        // TODO: one day, refactor the canvas so the dropdown items order will automatically
        // follow the order in the equivalent enums
        if (dropdown == experimentMode)
        {
            SharedData.currentConfiguration.experimentMode = (ExperimentMode)experimentMode.value; ;
        }
        else if (dropdown == experimentTask)
        {
            SharedData.currentConfiguration.experimentTask = (ExperimentTask)dropdown.value;
        }
        else if (dropdown == cursorPositioningMethod)
        {
            SharedData.currentConfiguration.cursorPositioningMethod = (CursorPositioningMethod)dropdown.value;
        }
        else if (dropdown == cursorSelectionMethod)
        {
            SharedData.currentConfiguration.cursorSelectionMethod = (CursorSelectionMethod)dropdown.value;
        }
        else if (dropdown == planeOrientation)
        {
            SharedData.currentConfiguration.planeOrientation = (PlaneOrientation)dropdown.value;
        }
        else if (dropdown == numberOfTargets)
        {
            SharedData.currentConfiguration.numberOfTargets = int.Parse(dropdown.options[dropdown.value].text);
        }
        else if (dropdown == participantCode)
        {
            SharedData.currentConfiguration.participantCode = dropdown.options[dropdown.value].text;
        }
        else if (dropdown == conditionCode)
        {
            SharedData.currentConfiguration.conditionCode = dropdown.options[dropdown.value].text;
        }
        else if (dropdown == sessionCode)
        {
            SharedData.currentConfiguration.sessionCode = dropdown.options[dropdown.value].text;
        }
        else if (dropdown == groupCode)
        {
            SharedData.currentConfiguration.groupCode = dropdown.options[dropdown.value].text;
        }
        UpdateCanvasState();
    }

    public void OnInputFieldTextChanged(InputField inputField)
    {
        string newValue = inputField.text;

        if (inputField == observations)
        {
            SharedData.currentConfiguration.observations = newValue;
        }
        else if (inputField == dwellTime)
        {
            float value;
            if (ParseFloatOnCanvasString(newValue, out value, TIME_SCALE))
            {
                SharedData.currentConfiguration.dwellTime = value;
                UpdateCanvasElementWithInternalValue(inputField);
            }
            else
            {
                SharedData.currentConfiguration.dwellTime = -1;
                inputField.SetTextWithoutNotify("-");
            }
        }
        else if (inputField == cursorWidth)
        {
            float value;
            if (ParseFloatOnCanvasString(newValue, out value, DIMENSIONS_SCALE))            
            {
                SharedData.currentConfiguration.cursorWidth = value;
                UpdateCanvasElementWithInternalValue(inputField);
            }
            else
            {
                SharedData.currentConfiguration.cursorWidth = -1;
                inputField.SetTextWithoutNotify("-");
            }
        }
        else if (inputField == amplitudes)
        {
            float[] values = ParseFloatArrayOnCanvasString(newValue, DIMENSIONS_SCALE);
            if (values != null)
            {
                SharedData.currentConfiguration.amplitudes = values;
                UpdateCanvasElementWithInternalValue(inputField);
            }
            else
            {
                SharedData.currentConfiguration.amplitudes = null;
                inputField.SetTextWithoutNotify("-");
            }
        }
        else if (inputField == widths)
        {
            float[] values = ParseFloatArrayOnCanvasString(newValue, DIMENSIONS_SCALE);
            if (values != null)
            {
                SharedData.currentConfiguration.widths = values;
                UpdateCanvasElementWithInternalValue(inputField);
            }
            else
            {
                SharedData.currentConfiguration.widths = null;
                inputField.SetTextWithoutNotify("-");
            }
        }
        UpdateCanvasState();
    }

    public void RunExperiment()
    {
        switch (SharedData.currentConfiguration.experimentMode)
        {
            case ExperimentMode.Experiment2D:
                SceneManager.LoadScene("2DMouseExperimentOnScreen");
                break;
            case ExperimentMode.Experiment3DOnMeta2:
                SceneManager.LoadScene("3DExperimentMeta2");
                break;
        }
    }

    public void SaveCurrentValuesOnFile()
    {
        string directory = FileManager.GetUserConfigurationsFolder();
        string fileFormat = FileManager.GetConfigurationFileFormat();
        var availableFiles = FileManager.GetFilenamesOnDirectory(directory, fileFormat);

        panelSelectFile.ShowSelectFilePanel("Choose an existing file to overwrite...", availableFiles,
            fileFormat, "Save file",
            (string filename) => {
                Dictionary<string, object> values = new Dictionary<string, object>();
                var data = JsonConvert.SerializeObject(SharedData.currentConfiguration, Formatting.Indented, new StringEnumConverter());
                FileManager.SaveFile(directory, filename, data);
            }, null, true, "...or create a new configuration file with name:");        
    }

    public void LoadValuesFromFile()
    {
        string directory = FileManager.GetUserConfigurationsFolder();
        string fileFormat = FileManager.GetConfigurationFileFormat();
        var availableFiles = FileManager.GetFilenamesOnDirectory(directory, fileFormat);

        panelSelectFile.ShowSelectFilePanel("Choose a file to load:", availableFiles,
            fileFormat, "Load file", (string filename) => { LoadValuesFromFile(directory, filename); }, null);
    }

    public void LoadValuesFromFile(string directory, string filename)
    {
        string configData = FileManager.LoadFile(directory, filename);        

        if (configData != null)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new StringEnumConverter());
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new StringEnumConverter());
            JsonConvert.PopulateObject(configData, SharedData.currentConfiguration, settings);
            UpdateCanvasFromCurrentValues();
        }
    }

    bool ParseFloatOnCanvasString(string canvasString, out float value, float scale = 1)
    {
        value = 0;
        float temp;
        if (float.TryParse(canvasString, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out temp))
        {
            value = temp * scale;
            return true;
        }
        return false;
    }

    float[] ParseFloatArrayOnCanvasString(string stringWithValues, float scale = 1)
    {
        float[] values = null;
        char[] delimiters = { ' ', ',' };
        string[] stringValues = stringWithValues.Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries);

        if (stringValues.Length > 0)
        {
            values = new float[stringValues.Length];
        }

        for (int i = 0; i < stringValues.Length; i++)
        {
            if (!ParseFloatOnCanvasString(stringValues[i], out values[i], scale))
            {
                return null;
            }
        }
            
        return values;
    }

    string FloatArrayToCanvasString(float[] values, float scale = 1)
    {
        string s = "";
        for (int i = 0; i < values.Length; i++)
        {
            s += FloatToCanvasString(values[i], scale); 

            if (i < values.Length - 1)
            {
                s += ", ";
            }
        }
        return s;
    }

    string FloatToCanvasString(float value, float scale = 1)
    {
        return String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:###.##}", value * scale);
    }
}