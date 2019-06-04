using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    string defaultDwellTime = "500";
    string defaultCursorWidth = "15";
    string defaultAmplitudes = "74, 103, 133";
    string defaultWidths = "13, 5.5";

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
        observations.text = "";

        ResetValuesToDefault();
    }

    public void ResetValuesToDefault()
    {
        ResetDropdown(experimentMode);
        ResetDropdown(experimentTask);
        ResetDropdown(cursorPositioningMethod);
        ResetDropdown(cursorSelectionMethod);
        ResetDropdown(planeOrientation);
        ResetDropdown(numberOfTargets, 3);

        dwellTime.SetTextWithoutNotify(defaultDwellTime);
        OnDwellTimeValueChanged(defaultDwellTime);

        cursorWidth.SetTextWithoutNotify(defaultCursorWidth);
        OnCursorWidthValueChanged(defaultCursorWidth);

        amplitudes.SetTextWithoutNotify(defaultAmplitudes);
        OnAmplitudesValueChanged(defaultAmplitudes);

        widths.SetTextWithoutNotify(defaultWidths);
        OnWidthsValueChanged(defaultWidths);        
    }

    void ResetDropdown(Dropdown d, int value = 0)
    {
        d.SetValueWithoutNotify(value);
        OnDropdownValueChanged(d);
    }

    public void OnDropdownValueChanged(Dropdown dropdown)
    {
        // TODO: one day, refactor the canvas so the dropdown items order will automatically
        // follow the order in the equivalent enums
        if (dropdown == experimentMode)
        {
            ExperimentMode newValue = (ExperimentMode)experimentMode.value;
            CanvasExperimentConfigurationValues.experimentMode = newValue;

            switch (CanvasExperimentConfigurationValues.experimentMode)
            {
                case ExperimentMode.Experiment2D:
                    // For now, only mouse can be used for 2D experiment
                    cursorPositioningMethod.value = (int)CursorPositioningMethod.Mouse;
                    cursorPositioningMethod.interactable = false;

                    // For now, you can only test the plane of the screen
                    planeOrientation.value = (int)PlaneOrientation.PlaneXY;
                    planeOrientation.interactable = false;

                    break;
                case ExperimentMode.Experiment3DOnMeta2:
                    cursorPositioningMethod.interactable = true;
                    planeOrientation.interactable = true;
                    break;
            }
        }
        else if (dropdown == experimentTask)
        {
            CanvasExperimentConfigurationValues.experimentTask = (ExperimentTask)dropdown.value;
        }
        else if (dropdown == cursorPositioningMethod)
        {
            CanvasExperimentConfigurationValues.cursorPositioningMethod = (CursorPositioningMethod)dropdown.value;
        }
        else if (dropdown == cursorSelectionMethod)
        {
            CanvasExperimentConfigurationValues.cursorSelectionMethod = (CursorSelectionMethod)dropdown.value;
            dwellTime.interactable = CanvasExperimentConfigurationValues.cursorSelectionMethod == CursorSelectionMethod.DwellTime;
        }
        else if (dropdown == planeOrientation)
        {
            CanvasExperimentConfigurationValues.planeOrientation = (PlaneOrientation)dropdown.value;
        }
        else if (dropdown == numberOfTargets)
        {
            CanvasExperimentConfigurationValues.numberOfTargets = int.Parse(dropdown.options[dropdown.value].text);
        }
        else if (dropdown == participantCode)
        {
            CanvasExperimentConfigurationValues.participantCode = dropdown.options[dropdown.value].text;
        }
        else if (dropdown == conditionCode)
        {
            CanvasExperimentConfigurationValues.conditionCode = dropdown.options[dropdown.value].text;
        }
        else if (dropdown == sessionCode)
        {
            CanvasExperimentConfigurationValues.sessionCode = dropdown.options[dropdown.value].text;
        }
        else if (dropdown == groupCode)
        {
            CanvasExperimentConfigurationValues.groupCode = dropdown.options[dropdown.value].text;
        }
    }

    public void OnObservationsValueChanged(string newValue)
    {
        CanvasExperimentConfigurationValues.observations = newValue;
    }

    public void OnDwellTimeValueChanged(string newValue)
    {
        if (CanvasExperimentConfigurationValues.TrySetDwellTimeFromString(newValue))
        {
            dwellTime.text = (CanvasExperimentConfigurationValues.dwellTime * 1000).ToString();
        }
        else
        {
            dwellTime.text = "-";
        }
    }

    public void OnCursorWidthValueChanged(string newValue)
    {
        if (CanvasExperimentConfigurationValues.TrySetCursorWidthFromString(newValue))
        {
            cursorWidth.text = (CanvasExperimentConfigurationValues.cursorWidth * 1000).ToString();
        }
        else
        {
            cursorWidth.text = "-";
        }
    }

    public void OnAmplitudesValueChanged(string newValue)
    {
        if (!CanvasExperimentConfigurationValues.TrySetAmplitudesFromString(newValue))
        {
            amplitudes.text = "-";
        }
    }

    public void OnWidthsValueChanged(string newValue)
    {
        if (!CanvasExperimentConfigurationValues.TrySetWidthsFromString(newValue))
        {
            widths.text = "-";
        }
    }

    public void RunExperiment()
    {
        switch (CanvasExperimentConfigurationValues.experimentMode)
        {
            case ExperimentMode.Experiment2D:
                SceneManager.LoadScene("2DMouseExperimentOnScreen");
                break;
            case ExperimentMode.Experiment3DOnMeta2:
                SceneManager.LoadScene("3DExperimentMeta2");
                break;
        }


    }
}
