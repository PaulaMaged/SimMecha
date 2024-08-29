using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SFB; // Namespace for StandaloneFileBrowser
using System.Linq;

public class SimulIDEFileScript : MonoBehaviour
{
    public Button openSimFileButton;
    private string simulIDEPath;
    private string sim1FilePath;
    private string saveFilePath = "simulIDEPath.txt";

    void Start()
    {
        if (openSimFileButton != null)
        {
            openSimFileButton.onClick.AddListener(OpenSimFile);
        }

        LoadSimulIDEPath();

        if (string.IsNullOrEmpty(simulIDEPath) || !File.Exists(simulIDEPath))
        {
            simulIDEPath = SearchForSimulIDE();
            if (!string.IsNullOrEmpty(simulIDEPath))
            {
                SaveSimulIDEPath(simulIDEPath);
            }
        }
    }

    void OpenSimFile()
    {
        if (string.IsNullOrEmpty(simulIDEPath) || !File.Exists(simulIDEPath))
        {
            PopUpController.Instance.ShowMessage("SimulIDE executable not found.");
            return;
        }

        var extensions = new[] {
            new ExtensionFilter("SimulIDE Files", "sim1"),
            new ExtensionFilter("All Files", "*"),
        };
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select SimulIDE File", "", extensions, false);

        if (paths.Length > 0)
        {
            sim1FilePath = paths[0];
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = simulIDEPath,
                Arguments = sim1FilePath,
                UseShellExecute = true
            };
            Process.Start(startInfo);
        }
        else
        {
            PopUpController.Instance.ShowMessage("No file selected.");
        }
    }

    string SearchForSimulIDE()
    {
        string[] commonPaths = {
            @"C:\Program Files\SimulIDE\simulide.exe",
            @"C:\Program Files (x86)\SimulIDE\simulide.exe",
            @"C:\Users\" + System.Environment.UserName + @"\AppData\Local\Programs\SimulIDE\simulide.exe",
            @"/usr/bin/simulide",
            @"/usr/local/bin/simulide",
            @"/opt/simulide/bin/simulide"
        };

        foreach (string path in commonPaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        // Additional search logic (e.g., scanning directories) can be added here

        return null;
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
