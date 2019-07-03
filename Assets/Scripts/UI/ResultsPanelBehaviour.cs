using System;
using UnityEngine;
using UnityEngine.UI;

public class ResultsPanelBehaviour : MonoBehaviour
{
    public Text numTrials;
    public Text numErrors;
    public Text amplitude;
    public Text effectiveAmplitude;
    public Text width;
    public Text effectiveWidth;
    public Text indexOfDifficulty;
    public Text effectiveIndexOfDifficulty;
    public Text errorRate;
    public Text movementTime;
    public Text throughput;

    private Action _onClose;

    private const float aThousand = 1000f;

    public void ShowPanel(TestMeasurements results, Action OnClose)
    {
        _onClose = OnClose;

        int nTargets = results.configuration.numberOfTargets;
        int nErrors = Mathf.RoundToInt(nTargets * (float)results.computedResults.errorRate);
        numTrials.text = nTargets.ToString();
        numErrors.text = nErrors.ToString();

        amplitude.text = DoubleToString(results.sequenceInfo.targetsDistance, aThousand) + " (mm)";
        effectiveAmplitude.text = DoubleToString(results.computedResults.effectiveAmplitude, aThousand) + " (mm)";

        width.text = DoubleToString(results.sequenceInfo.targetWidth, aThousand) + " (mm)";
        effectiveWidth.text = DoubleToString(results.computedResults.effectiveWidth, aThousand) + " (mm)";

        indexOfDifficulty.text = DoubleToString(results.sequenceInfo.indexOfDifficulty) + " bits";
        effectiveIndexOfDifficulty.text = DoubleToString(results.computedResults.effectiveIndexOfDifficulty) + " bits";

        errorRate.text = DoubleToString(results.computedResults.errorRate, 100) + "%";
        movementTime.text = DoubleToString(results.computedResults.averageMovementTime, aThousand) + " ms/trial";
        throughput.text = DoubleToString(results.computedResults.throughput) + " bits/s";

        this.gameObject.SetActive(true);
    }

    private string DoubleToString(double value, double scale = 1)
    {
        return string.Format("{0:##0.##}", value * scale);
    }

    public void HidePanel()
    {
        this.gameObject.SetActive(false);
    }

    public void OnCloseButton()
    {
        HidePanel();
        _onClose?.Invoke();
    }
}
