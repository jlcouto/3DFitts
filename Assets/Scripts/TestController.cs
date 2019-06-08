using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Leap.Unity.Interaction;

public interface ITestListener
{
    void OnTestStarted();
    void OnTestEnded(TestMeasurements testMeasurements);
}

public class TestController : ICursorListener, IBlockListener
{
    public Text statusText;

    public CursorBehaviour cursor;
    public GameObject baseTarget;
    public Transform targetPlane;
    public PlaneOrientation orientation = PlaneOrientation.PlaneXY;

    public ExperimentTask task = ExperimentTask.ReciprocalTapping;
    public int numberOfTargets = 13;
    public float targetWidth = 0.01f;
    public float targetDistance = 0.15f;

    public int numOfBlocksPerTest = 3;

    TargetBehaviour[] targets;
    TargetBehaviour initialTarget;

    const int numOfTests = 1;

    int currentBlockIndex = 0;
    BlockController currentBlock;

    TestConfiguration testConfiguration;
    TestMeasurements testData;

    ITestListener testListener;

    enum TestStatus
    {
        Waiting,
        Running,
        Finished
    }
    TestStatus currentStatus;

    public TestController(ITestListener listener, Text statusText,
        CursorBehaviour cursor, GameObject baseTarget, Transform targetPlane, ExperimentTask task,
        PlaneOrientation orientation, int numberOfTargets, float targetWidth, float targetDistance, int numOfBlocksPerTest = 1)
    {
        this.testListener = listener;

        this.statusText = statusText;

        this.cursor = cursor;
        this.baseTarget = baseTarget;
        this.targetPlane = targetPlane;
        this.orientation = orientation;

        this.task = task;
        this.numberOfTargets = numberOfTargets;
        this.targetWidth = targetWidth;
        this.targetDistance = targetDistance;
        this.numOfBlocksPerTest = numOfBlocksPerTest;
    }

    public void InitializeTest()
    {
        if (currentStatus == TestStatus.Finished)
        {
            Debug.LogWarning("TestController: Cannot start a new test because another test is already running. Finish it first.");
            return;
        }

        cursor.RegisterNewListener(this);
        TargetPlaneBuilder.Build(baseTarget, targetPlane, numberOfTargets, targetWidth, targetDistance);
        targets = targetPlane.GetComponentsInChildren<TargetBehaviour>();
        initialTarget = targets[0];

        Vector3 planeRotation;
        switch (orientation)
        {
            case PlaneOrientation.PlaneXY:
                planeRotation = new Vector3(-90, 0, 0);
                break;
            case PlaneOrientation.PlaneYZ:
                planeRotation = new Vector3(0, 0, 90);
                break;
            case PlaneOrientation.PlaneZX:
            default:
                planeRotation = new Vector3(0, 0, 0);
                break;
        }
        targetPlane.transform.localRotation = Quaternion.Euler(planeRotation);

        SetCurrentStatus(TestStatus.Waiting);

        initialTarget.SetAsStartingTestTarget();
    }

    void StartTest() {
        if (currentStatus != TestStatus.Running) {        
            SetCurrentStatus(TestStatus.Running);
            testConfiguration = new TestConfiguration(targets, task, orientation, targetWidth, targetDistance, numOfBlocksPerTest);
            testData = new TestMeasurements(testConfiguration);
            testData.timestamp = System.DateTime.Now.ToString("s");
            testData.initialTime = Time.realtimeSinceStartup;
            RunNextBlock();
        }
    }

    void RunNextBlock() {
        if (currentBlockIndex < numOfBlocksPerTest) {
            currentBlock = new BlockController(task, currentBlockIndex, targets, this, cursor);
            currentBlock.StartBlockOfTrials();
            currentBlockIndex++;
        }
        else {
            FinishTest();
        }
    }

    void FinishTest() {
        testData.finalTime = Time.realtimeSinceStartup;
        SetCurrentStatus(TestStatus.Finished);
        testListener.OnTestEnded(testData);
    }

    public void AbortTest()
    {
        if (currentBlock != null)
        {
            currentBlock.AbortBlock();
        }
        currentBlock = null;
        cursor.RemoveListener(this);
        testListener = null;
    }

    public bool isRunning()
    {
        return currentStatus == TestStatus.Running;
    }

    public void CursorAcquiredTarget(TargetBehaviour target) {
        //Debug.Log("TestController CursorAcquiredTarget");
        if (target != null && target.type == TargetType.StartingTestTarget) {
            cursor.PlayCorrectAudio();
            cursor.RemoveListener(this);
            StartTest();
        }
    }

    public void CursorEnteredTarget(TargetBehaviour target) { }
    public void CursorExitedTarget(TargetBehaviour target) { }
    public void CursorDragTargetStarted(TargetBehaviour target) { }
    public void CursorDragTargetEnded(TargetBehaviour draggedTarget, TargetBehaviour receivingTarget) { }

    public void OnBlockEnded(BlockMeasurements measurements) {
        testData.blocksData.Add(measurements);
        RunNextBlock();
    }

    void SetCurrentStatus(TestStatus status)
    {
        currentStatus = status;
        switch (status) {
            case TestStatus.Waiting:
                {
                    statusText.text = "Waiting...";
                    statusText.color = Color.yellow;
                    break;
                }
            case TestStatus.Running:
                {
                    statusText.text = "Running...";
                    statusText.color = Color.blue;
                    break;
                }
            case TestStatus.Finished:
                {
                    statusText.text = "Finished!";
                    statusText.color = Color.green;
                    break;
                }
        }

    }
}
