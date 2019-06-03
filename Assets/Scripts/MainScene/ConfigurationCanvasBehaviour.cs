using System.Collections;
using System.Collections.Generic;
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

    public InputField cursorWidth;
    public InputField amplitudes;
    public InputField widths;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSelectedExperimentMode()
    {

    }

    public void RunExperiment()
    {
        SceneManager.LoadScene("2DMouseExperimentOnScreen");
    }
}
