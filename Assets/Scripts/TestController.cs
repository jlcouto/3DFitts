using UnityEngine;
using UnityEngine.UI;

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

    public ExperimentConfiguration configuration;
    public IndexOfDifficulty currentSequence;

    TargetBehaviour[] targets;
    TargetBehaviour initialTarget;

    const int numOfTests = 1;

    int currentBlockIndex = 0;
    BlockController currentBlock;

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
        CursorBehaviour cursor, GameObject baseTarget, Transform targetPlane,
        ExperimentConfiguration configuration, int currentSequence)
    {
        this.testListener = listener;

        this.statusText = statusText;

        this.cursor = cursor;
        this.baseTarget = baseTarget;
        this.targetPlane = targetPlane;

        this.configuration = configuration;

        if (currentSequence < 0 || currentSequence >= configuration.GetSequences().Count)
        {
            currentSequence = configuration.GetSequences().Count - 1;
        }
        this.currentSequence = configuration.GetSequences()[currentSequence];        
    }

    public void InitializeTest()
    {
        if (currentStatus == TestStatus.Finished)
        {
            Debug.LogWarning("TestController: Cannot start a new test because another test is already running. Finish it first.");
            return;
        }

        cursor.RegisterNewListener(this);
        TargetPlaneBuilder.Build(baseTarget, targetPlane, currentSequence, configuration.numberOfTargets);
        targets = targetPlane.GetComponentsInChildren<TargetBehaviour>();
        initialTarget = targets[0];

        Vector3 planeRotation;
        switch (configuration.planeOrientation)
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
            testData = new TestMeasurements(configuration, currentSequence, targets);
            testData.timestamp = System.DateTime.Now.ToString("s");
            testData.initialTime = Time.realtimeSinceStartup;
            RunNextBlock();
        }
    }

    void RunNextBlock() {
        if (currentBlockIndex < configuration.numberOfBlocks) {
            currentBlock = new BlockController(configuration.experimentTask, currentBlockIndex, targets, this, cursor);
            currentBlock.StartBlockOfTrials();
            currentBlockIndex++;
        }
        else {
            FinishTest();
        }
    }

    void FinishTest() {
        testData.finalTime = Time.realtimeSinceStartup;
        testData.ComputeResults();
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
