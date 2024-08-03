using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

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

        if (string.IsNullOrEmpty(simulIDEPath) || !File.Exists(simulIDEPath))
        {
            simulIDEPath = SearchForSimulIDE();
            if (!string.IsNullOrEmpty(simulIDEPath))
            {
                SaveSimulIDEPath(simulIDEPath);
            }
            else
            {
                UnityEngine.Debug.LogWarning("SimulIDE executable not found.");
            }
        }
    }

    void OpenSimulIDE()
    {
        if (string.IsNullOrEmpty(simulIDEPath) || !File.Exists(simulIDEPath))
        {
            UnityEngine.Debug.LogWarning("SimulIDE executable not set or not found.");
            return;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = simulIDEPath,
            UseShellExecute = true
        };
        Process.Start(startInfo);
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
