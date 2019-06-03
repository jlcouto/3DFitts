using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FillDropDownItemsWithCodes : MonoBehaviour
{
    public string prefix = "";
    public int numberOfItems = 0;

    // Start is called before the first frame update
    void Start()
    {
        PopulateDropDownWithCodes();
    }

    void PopulateDropDownWithCodes()
    {
        var dropdown = GetComponent<Dropdown>();
        if (dropdown != null)
        {
            dropdown.ClearOptions();

            List<string> options = new List<string>();
            for (int i = 0; i < numberOfItems; i++)
            {
                options.Add(string.Format("{0}{1:D2}", prefix, i));
            }

            dropdown.AddOptions(options);
        }
    }
}
