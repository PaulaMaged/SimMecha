using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI; // For UI elements
using SFB; // StandaloneFileBrowser namespace

public class FilePicker : MonoBehaviour
{
    public Text filePathText; // Reference to a UI Text element to display the selected file path
    private string selectedFilePath;

    public void OpenFilePicker()
    {
        var extensions = new[] {
            new ExtensionFilter("URDF Files", "urdf")
        };
        var path = StandaloneFileBrowser.OpenFilePanel("Open URDF File", "", extensions, false);
        if (path.Length > 0)
        {
            selectedFilePath = path[0];
            filePathText.text = selectedFilePath; // Display the file path in a UI Text element
        }
    }

    public string GetSelectedFilePath()
    {
        return selectedFilePath;
    }
}
