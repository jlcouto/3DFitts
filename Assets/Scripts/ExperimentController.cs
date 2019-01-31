using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.IO;
using Leap.Unity.Interaction;

public class ExperimentController : MonoBehaviour, ICursorListener, IBlockListener
{
    public Text statusText;
    public Text experimentText;
    public Text repetitionText;
    public Text noteText;

    public AudioSource correctTargetAudio;
    public AudioSource wrongTargetAudio;

    public CursorBehaviour cursor;
    public GameObject baseTarget;
    public Transform targetPlane;

    public int numberOfTargets = 13;
    public float targetWidth = 0.01f;
    public float targetDistance = 0.15f;

    public int numOfBlocksPerExperiment = 3;

    TargetBehaviour[] targets;
    TargetBehaviour initialTarget;
    bool initialized = false;

    const int numOfExperiments = 1;
    int currentExperimentIndex = 0;
    int currentBlockIndex = 0;
    BlockController currentBlock;

    ExperimentConfiguration experimentConfiguration;
    ExperimentMeasurements experimentData;

    enum ExperimentStatus
    {
        Waiting,
        Running,
        Finished
    }
    ExperimentStatus currentStatus;

    void Start () {
        cursor.RegisterNewListener(this);

        //if (cursor is LeapMotionControllerCursorBehaviour)
        //{
        //    ((LeapMotionControllerCursorBehaviour)cursor).AddInteractionBehaviourToObject(baseTarget);
        //}
        TargetPlaneBuilder.Build(baseTarget, targetPlane, numberOfTargets, targetWidth, targetDistance);
        targets = targetPlane.GetComponentsInChildren<TargetBehaviour>();
        initialTarget = targets[0];

        SetCurrentStatus(ExperimentStatus.Waiting);
        UISetNoteText("");
    }

    void Update() {
        if (!initialized) {
            initialized = true;
            initialTarget.SetAsNextTarget();
        }
    }

    void StartExperiment() {
        if (currentStatus != ExperimentStatus.Running) {   
            SetCurrentStatus(ExperimentStatus.Running);
            UISetNoteText("");
            experimentConfiguration = new ExperimentConfiguration(targets, targetWidth, targetDistance, numOfBlocksPerExperiment);
            experimentData = new ExperimentMeasurements(experimentConfiguration);
            experimentData.timestamp = System.DateTime.Now.ToString("s");
            experimentData.initialTime = Time.realtimeSinceStartup;
            RunNextBlock();
        }
    }

    void RunNextBlock() {
        if (currentBlockIndex < numOfBlocksPerExperiment) {
            currentBlock = new BlockController(currentBlockIndex, targets, this, cursor);
            currentBlock.StartBlockOfTrials();
            currentBlockIndex++;
        }
        else {
            FinishExperiment();
        }
        UISetExperimentConfiguration();
    }

    void FinishExperiment() {
        experimentData.finalTime = Time.realtimeSinceStartup;
        ExportResultsToFile(experimentData);
        SetCurrentStatus(ExperimentStatus.Finished);
    }

    public void CursorAcquiredTarget(TargetBehaviour target) {
        if (target != null && target.targetId == initialTarget.targetId) {
            cursor.PlayCorrectAudio();
            cursor.RemoveCurrentListener();
            StartExperiment();
        }
    }

    public void OnBlockEnded(BlockMeasurements measurements) {
        experimentData.blocksData.Add(measurements);
        RunNextBlock();
    }

    public void CursorEnteredTarget(TargetBehaviour target) { }
    public void CursorExitedTarget(TargetBehaviour target) { }

    void SetCurrentStatus(ExperimentStatus status)
    {
        currentStatus = status;
        switch (status) {
            case ExperimentStatus.Waiting:
                {
                    statusText.text = "Waiting...";
                    statusText.color = Color.yellow;
                    break;
                }
            case ExperimentStatus.Running:
                {
                    statusText.text = "Running...";
                    statusText.color = Color.blue;
                    break;
                }
            case ExperimentStatus.Finished:
                {
                    statusText.text = "Finished!";
                    statusText.color = Color.green;
                    break;
                }
        }

    }

    void UISetExperimentConfiguration()
    {
        experimentText.text = "Experiment: " + currentExperimentIndex;
        repetitionText.text = "Blocks: " + currentBlockIndex + "/" + numOfBlocksPerExperiment;
    }

    void UISetNoteText(string text) {
        noteText.text = text;
    }

    void ExportResultsToFile(ExperimentMeasurements results) {
        var output = results.SerializeToDictionary();
        string jsonData = JsonConvert.SerializeObject(output);
        string filename = GetFilenameForExperiment(results);
        string path = GetExperimentsFolder() + filename;
        File.WriteAllText(path, jsonData);
        UISetNoteText("File saved: " + filename);
    }

    string GetFilenameForExperiment(ExperimentMeasurements experiment)
    {
        var timestamp = experiment.timestamp.Replace(":", "_");
        return experimentConfiguration.experimentId + "_" + timestamp + ".json";
    }

    string GetExperimentsFolder()
    {
        return "./Experiments/";
    }
}
