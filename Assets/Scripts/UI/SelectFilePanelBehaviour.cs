using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class SelectFilePanelBehaviour : MonoBehaviour
{
    public GameObject clickBlockerPanel;
    
    public Text textSelectAFile;
    public Dropdown dropdownSelectAFile;

    public GameObject panelInputText;
    public Text textEnterAFilename;
    public InputField inputEnterAFilename;
    
    public Button buttonConfirm;
    
    private Action<string> OnFileSelectionConfirmed;
    private Action OnFileSelectionCanceled;

    private string _fileFormat;
    private string _filename;

    private Rect _initialPanelRect;
    private Rect _initialInputTextPanelRect;

    public void ShowSelectFilePanel(string selectAFileText, List<string> filesAvailable, string fileFormat, string confirmButtonText,
                                    Action<string> fileSelectionConfirmed, Action fileSelectionCanceled, bool showInputText = false,
                                    string enterAFilenameText = "Enter the name of the file to save:")
    {
        _filename = "";
        _fileFormat = fileFormat;
        OnFileSelectionConfirmed = fileSelectionConfirmed;
        OnFileSelectionCanceled = fileSelectionCanceled;

        textEnterAFilename.text = enterAFilenameText;
        inputEnterAFilename.text = "";

        textSelectAFile.text = selectAFileText;
        dropdownSelectAFile.ClearOptions();
        dropdownSelectAFile.AddOptions(filesAvailable.Where(s => IsValidFilename(s)).ToList<string>());
        if (dropdownSelectAFile.options.Count > 0)
        {
            OnFileSelectedOnDropdown(dropdownSelectAFile);
        }        

        buttonConfirm.GetComponentInChildren<Text>().text = confirmButtonText;         

        clickBlockerPanel.SetActive(true);
        this.gameObject.SetActive(true);

        ShowInputField(showInputText);        
        UpdatePanel();
    }

    void ShowInputField(bool shouldShow)
    {
        if (_initialPanelRect.height == 0)
        {
            _initialPanelRect = this.gameObject.GetComponent<RectTransform>().rect;
            _initialInputTextPanelRect = panelInputText.gameObject.GetComponentInParent<RectTransform>().rect;
        }        

        panelInputText.SetActive(shouldShow);

        var transform = this.gameObject.GetComponent<RectTransform>();
        float finalHeight = _initialPanelRect.height;
        if (!shouldShow) { finalHeight -= _initialInputTextPanelRect.height; };
        transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, finalHeight);
    }
    
    public void HidePanel()
    {
        clickBlockerPanel.SetActive(false);
        this.gameObject.SetActive(false);
    }

    public void OnFileSelectedOnDropdown(Dropdown dropdown)
    {
        inputEnterAFilename.text = dropdown.options[dropdown.value].text;
        OnFileNameEdited(inputEnterAFilename);
        UpdatePanel();
    }

    public void OnFileNameValueChanged(InputField inputField)
    {
        string currentText = inputField.text;
        if (!currentText.EndsWith(_fileFormat, StringComparison.InvariantCulture))
        {
            inputField.text = currentText + _fileFormat;
        }
        if (IsValidFilename(currentText))
        {
            _filename = currentText;
        }
        else
        {
            _filename = "";
        }
        UpdatePanel();
    }

    public void OnFileNameEdited(InputField inputField)
    {
        if (!IsValidFilename(inputField.text))
        {               
            inputField.SetTextWithoutNotify(_filename);
        }
        UpdatePanel();
    }

    bool IsValidFilename(string filename)
    {
        return filename.EndsWith(_fileFormat, StringComparison.InvariantCulture) &&
               filename.Length > _fileFormat.Length &&
               filename.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) < 0;
    }

    void UpdatePanel()
    {
        buttonConfirm.interactable = IsValidFilename(_filename);        
    }

    public void ConfirmFilesSelection()
    {
        if (IsValidFilename(_filename))
        {
            HidePanel();
            OnFileSelectionConfirmed?.Invoke(_filename);            
        }        
    }

    public void CancelFileSelection()
    {
        HidePanel();
        OnFileSelectionCanceled?.Invoke();        
    }
}
