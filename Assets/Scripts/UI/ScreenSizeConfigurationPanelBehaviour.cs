using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenSizeConfigurationPanelBehaviour : MonoBehaviour
{
    public GameObject clickBlockerPanel;

    public void ShowScreenSizeConfigPanel()
    {
        clickBlockerPanel.SetActive(true);
        this.gameObject.SetActive(true);
    }

    public void HidePanel() {
        clickBlockerPanel.SetActive(false);
        this.gameObject.SetActive(false);
    }

    public void OnOKButton() {
    }

    public void OnCancelButton() {

    }

    public void OnResetButton() {

    }
}
