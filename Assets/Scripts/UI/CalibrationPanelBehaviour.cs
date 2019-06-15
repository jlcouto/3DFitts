using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CalibrationPanelBehaviour : MonoBehaviour
{
    public Text textTitle;
    public Text textMessage;

    public void ShowPanel(string title, string message)
    {
        this.textTitle.text = title;
        this.textMessage.text = message;
        this.gameObject.SetActive(true);
    }

    public void HidePanel()
    {
        this.gameObject.SetActive(false);
    }
}
