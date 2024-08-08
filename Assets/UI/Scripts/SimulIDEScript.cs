using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SFB; // Namespace for StandaloneFileBrowser

public class SimulIDEScript : MonoBehaviour
{
    public Button yourButton;
    private string simulIDEPath;
    private string saveFilePath = "simulIDEPath.txt";

    void Start()
    {
        if (yourButton != null)
        {
            yourButton.onClick.AddListener(OpenSimulIDE);
        }

        LoadSimulIDEPath();
    }

    void OpenSimulIDE()
    {
        if (string.IsNullOrEmpty(simulIDEPath) || !File.Exists(simulIDEPath))
        {
            var extensions = new[] {
                new ExtensionFilter("Executable Files", "exe"),
                new ExtensionFilter("All Files", "*"),
            };
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Select SimulIDE Executable", "", extensions, false);

            if (paths.Length > 0)
            {
                simulIDEPath = paths[0];
                SaveSimulIDEPath(simulIDEPath);
            }
            else
            {
                UnityEngine.Debug.LogWarning("No file selected.");
                return;
            }
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = simulIDEPath,
            UseShellExecute = true
        };
        Process.Start(startInfo);
    }

    void SaveSimulIDEPath(string path)
    {
        File.WriteAllText(saveFilePath, path);
    }

    void LoadSimulIDEPath()
    {
        if (File.Exists(saveFilePath))
        {
            simulIDEPath = File.ReadAllText(saveFilePath);
        }
    }
}
