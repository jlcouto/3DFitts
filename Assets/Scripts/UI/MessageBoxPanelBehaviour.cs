using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageBoxPanelBehaviour : MonoBehaviour
{
    public GameObject clickBlockerPanel;

    public Text textMessage;
    public Button buttonConfirm;
    public Button buttonMiddle;
    public Button buttonCancel;

    private Action _onSuccess;
    private Action _onMiddleButton;
    private Action _onFailure;

    public void ShowPanel(string message, Action OnSuccess, Action OnFailure,
                          string confirmLabel = "Confirm",
                          string cancelLabel = "Cancel",
                          string middleLabel = null,
                          Action OnMiddle = null)
    {
        _onSuccess = OnSuccess;
        _onMiddleButton = OnMiddle;
        _onFailure = OnFailure;

        textMessage.text = message;
        buttonConfirm.GetComponentInChildren<Text>().text = confirmLabel;
        buttonMiddle.GetComponentInChildren<Text>().text = middleLabel;
        buttonCancel.GetComponentInChildren<Text>().text = cancelLabel;

        clickBlockerPanel.SetActive(true);
        this.gameObject.SetActive(true);
    }

    public void HidePanel()
    {
        clickBlockerPanel.SetActive(false);
        this.gameObject.SetActive(false);
    }

    public void OnConfirmButton()
    {
        HidePanel();
        _onSuccess?.Invoke();
    }

    public void OnMiddleButton()
    {
        HidePanel();
        _onMiddleButton?.Invoke();
    }

    public void OnCancelButton()
    {
        HidePanel();
        _onFailure?.Invoke();
    }
}
