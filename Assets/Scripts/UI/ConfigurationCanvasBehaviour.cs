using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SimpleFileBrowser;
using System.Runtime.InteropServices;

public class ConfigurationCanvasBehaviour : MonoBehaviour
{
    public ExperimentController experimentController;    

    public GameObject panelConfigurationMenu;
    public GameObject panelBottomMenu;
    public GameObject panelBackground;
    public CalibrationPanelBehaviour panelCalibration;

    public SelectFilePanelBehaviour panelSelectFile;
    public MessageBoxPanelBehaviour panelMessageBox;

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

    public Toggle unitsUsePixels;
    public Toggle unitsUseMillimeters;

    public Button buttonMetaPositionCalibration;
    public Button buttonDeviceCalibration;

    const float TIME_SCALE = 0.001f;
    const float DIMENSIONS_SCALE = 0.001f;

    private bool _isInitialized;

    private IntPtr _appWindow;

    void Start()
    {
        _appWindow = (IntPtr)GetActiveWindow();
        panelBottomMenu.SetActive(false);
        panelSelectFile.HidePanel();
        InitializeValues();
        _isInitialized = true;
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

        // Drag task do not work with dwell yet
        cursorSelectionMethod.GetComponent<DropDownController>().EnableOption((int)CursorSelectionMethod.DwellTime, !isDraggingTask);

        if (isDraggingTask && SharedData.currentConfiguration.cursorSelectionMethod == CursorSelectionMethod.DwellTime)
        {
            SharedData.currentConfiguration.cursorSelectionMethod = CursorSelectionMethod.MouseLeftButton;
            UpdateCanvasElementWithInternalValue(cursorSelectionMethod);
        }

        dwellTime.interactable = SharedData.currentConfiguration.cursorSelectionMethod == CursorSelectionMethod.DwellTime;

        UpdateButtonsStates();
    }

    void UpdateButtonsStates()
    {
        bool is3DModeSelected = SharedData.currentConfiguration.experimentMode == ExperimentMode.Experiment3DOnMeta2;
        buttonMetaPositionCalibration.gameObject.SetActive(is3DModeSelected);

        bool isLeapMotionSelected = SharedData.currentConfiguration.cursorPositioningMethod == CursorPositioningMethod.LeapMotionController;
        bool isVIVESelected = SharedData.currentConfiguration.cursorPositioningMethod == CursorPositioningMethod.VIVE;
        bool isScreenCalibrationAvailable = is3DModeSelected && SharedData.currentConfiguration.cursorPositioningMethod == CursorPositioningMethod.Mouse;
        buttonDeviceCalibration.interactable = isLeapMotionSelected || isVIVESelected || isScreenCalibrationAvailable;
        buttonDeviceCalibration.gameObject.SetActive(buttonDeviceCalibration.interactable);

        Text calibrateText = buttonDeviceCalibration.GetComponentInChildren<Text>();
        if (isLeapMotionSelected)
        {
            calibrateText.text = "Calibrate Leap Motion";
        }
        else if (isVIVESelected)
        {
            calibrateText.text = "Calibrate VIVE";            
        }
        else if (isScreenCalibrationAvailable) {
            calibrateText.text = "Configure Screen Size";
        }
    }

    public void StartMetaSpaceCalibration()
    {
        panelCalibration.ShowPanel("Calibrating the center of the tests' planes...",
            "Using the Meta 2 headset, you should be able to see, when moving your hands in front of you, the cube that represents " +
            "the center of the tests' planes. It will follow your hands in space.\n\n" +
            "Moving your hands in space, position the cube where you would like the tests to take place.\n\n" +
            "Use the left and right arrow key on the keyboard to rotate the cube over the z axis, making sure the sphere " +
            "is facing you.\n\n" +
            "PRESS ENTER to finish the process...");
        experimentController.StartCalibrationOfExperimentPosition(() => { panelCalibration.HidePanel(); });
        SetAsActiveWindow(0.5f);                        
    }

    private void SetAsActiveWindow(float timeInSeconds) {
        StartCoroutine(ExecuteAfterTime(() => {
            if (_appWindow != (IntPtr)GetActiveWindow()) {
                SetForegroundWindow(_appWindow);
            }
        }, timeInSeconds));        
    }

    public void StartCalibrationOfCurrentCursorPositioning()
    {        
        if (SharedData.currentConfiguration.cursorPositioningMethod == CursorPositioningMethod.LeapMotionController)
        {
            panelCalibration.ShowPanel("Calibrating Leap Motion Controller...",
            "Make sure the Leap Motion Controller is connected to the computer and running properly.\n\n" +
            "Using the Meta 2 headset, you should see virtual hands tracking your own when moving them in front of the device.\n\n" +
            "Using the keyboard arrows and letters 'w' and 's', you can control the offset of the virtual hands in the three dimensions. " +
            "Using the letters 'o' and 'l' you can control the rotation of the virtual hands over the x axis.\n\n" +
            "Adjust the position of the virtual hands over your own hand and PRESS ENTER to finish the process.");
            experimentController.StartCalibrationOfLeapMotionController(() => { panelCalibration.HidePanel(); });
        }
        else if (SharedData.currentConfiguration.cursorPositioningMethod == CursorPositioningMethod.VIVE)
        {
            panelCalibration.ShowPanel("Calibrating HTC VIVE Controller...",
            "Make sure the 'udp_emitter.py' script and the SteamVR application are running and that the VIVE Controller is connected to them.\n\n" +
            "Using the Meta 2 headset, you will see a sphere in space. Position the VIVE Controller where it is and press the trigger button. " +
            "The sphere should disappear and another one should appear near the first one. Repeat the process until you select six spheres.\n\n" +
            "You may also cancel the calibration process pressing ESC.");
            experimentController.StartCalibrationOfVIVEController(() => { panelCalibration.HidePanel(); });
        }
        else if (SharedData.currentConfiguration.cursorPositioningMethod == CursorPositioningMethod.Mouse)
        {

        }
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
            string text = FloatToCanvasString(SharedData.currentConfiguration.cursorWidth, 1 / ScreenUnitScale(), unitsUsePixels.isOn);
            cursorWidth.SetTextWithoutNotify(text);
        }
        else if (element == (object)amplitudes)
        {
            string text = FloatArrayToCanvasString(SharedData.currentConfiguration.amplitudes, 1 / ScreenUnitScale(), unitsUsePixels.isOn);
            amplitudes.SetTextWithoutNotify(text);
        }
        else if (element == (object)widths)
        {
            string text = FloatArrayToCanvasString(SharedData.currentConfiguration.widths, 1 / ScreenUnitScale(), unitsUsePixels.isOn);
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
            if (ParseFloatOnCanvasString(newValue, out value, ScreenUnitScale()))            
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
            float[] values = ParseFloatArrayOnCanvasString(newValue, ScreenUnitScale());
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
            float[] values = ParseFloatArrayOnCanvasString(newValue, ScreenUnitScale());
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

    private float ScreenUnitScale()
    {
        if (unitsUsePixels.isOn)
        {
            return ScreenDimension.ToMillimeters(1) * DIMENSIONS_SCALE;
        }
        else
        {
            return DIMENSIONS_SCALE;
        }
    }

    public void OnToggleValueChanged()
    {
        if (_isInitialized)
        {
            UpdateCanvasFromCurrentValues();
        }
    }

    public void RunExperiment()
    {
        string prefixOfResultFile = FileManager.GetResultsFilenamePrefix(SharedData.currentConfiguration);
        string directory = FileManager.GetResultsFolder(SharedData.currentConfiguration.participantCode);
        if (FileManager.CheckExistenceOfFilesWithPrefix(prefixOfResultFile, directory,".csv"))
        {
            panelMessageBox.ShowPanel(
                "A result file already exists for the current configuration:\n\n" +
                prefixOfResultFile +
                "\n\nCollect data anyway?" +
                "\n\n(the previous files won't be overwritten)",
                () => { DoRunExperiment(); },
                null, "Continue", "Cancel",
                "Use next session code", () => {
                    UseNextAvailableSessionCode();
                    DoRunExperiment();                
                });
        }
        else
        {
            DoRunExperiment();
        }
    }

    public void UseNextAvailableSessionCode()
    {
        string prefixOfResultFile = FileManager.GetResultsFilenamePrefix(SharedData.currentConfiguration);
        string directory = FileManager.GetResultsFolder(SharedData.currentConfiguration.participantCode);

        while (FileManager.CheckExistenceOfFilesWithPrefix(prefixOfResultFile, directory, ".csv"))
        {
            IncrementSessionCode();
            prefixOfResultFile = FileManager.GetResultsFilenamePrefix(SharedData.currentConfiguration);
        }
    }

    public void IncrementSessionCode()
    {
        int newSessionCode = (sessionCode.value + 1) % sessionCode.options.Count;
        sessionCode.SetValueWithoutNotify(newSessionCode);
        OnDropdownValueChanged(sessionCode);
    }

    private void DoRunExperiment()
    {
        panelConfigurationMenu.SetActive(false);
        panelBackground.SetActive(false);
        panelBottomMenu.SetActive(true);
        experimentController.RunExperiment();
        SetAsActiveWindow(0.5f);
    }

    public void ReturnToMainMenu()
    {
        experimentController.StopExperiment();
        panelBottomMenu.SetActive(false);
        panelConfigurationMenu.SetActive(true);
        panelBackground.SetActive(true);
    }

    public void MergeFilesOnFolder()
    {
        FileBrowser.SetFilters(false, new FileBrowser.Filter("CSV files", ".csv"));
        FileBrowser.ShowLoadDialog((string path) =>
        {
            FileManager.MergeCsvFilesInFolder(FileBrowser.Result, "MergedResults.csv");
        }, null, false, FileManager.GetResultsFolder(), "Choose a file in the folder containing the files to merge", "Merge all .CSV");
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

    string FloatArrayToCanvasString(float[] values, float scale = 1, bool roundToInt = false)
    {
        string s = "";
        if (values != null)
        {
            for (int i = 0; i < values.Length; i++)
            {
                s += FloatToCanvasString(values[i], scale, roundToInt);

                if (i < values.Length - 1)
                {
                    s += ", ";
                }
            }
        }
        return s;
    }

    string FloatToCanvasString(float value, float scale = 1, bool roundToInt = false)
    {
        if (roundToInt)
        {
            return String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:#####}", Mathf.RoundToInt(value * scale));
        }
        else
        {
            return String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:####.##}", value * scale);
        }
    }

    IEnumerator ExecuteAfterTime(System.Action action, float timeInSeconds)
    {
        yield return new WaitForSeconds(timeInSeconds);
        action?.Invoke();
    }

    [DllImport("user32.dll")] static extern uint GetActiveWindow();
    [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr hWnd);
}