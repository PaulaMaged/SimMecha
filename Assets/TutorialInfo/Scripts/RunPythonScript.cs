using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class RunPythonScript : MonoBehaviour
{
    // Path to the Python script relative to the Unity project folder
    private string pythonScriptPath = "Assets/Python/main.py";

    // Path to the Python executable
    private string pythonExePath = "Assets/Python/python.exe"; // Update this path if needed

    // Path to the log file
    private string logFilePath = "Assets/Python/log.txt";

    public void RunPython()
    {
        // Log the start of the process
        Log("Starting Python script...");

        if (!File.Exists(pythonExePath))
        {
            Log($"Python executable not found at: {pythonExePath}");
            return;
        }

        if (!File.Exists(pythonScriptPath))
        {
            Log($"Python script not found at: {pythonScriptPath}");
            return;
        }

        // Create a new process to run the Python script
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = pythonExePath,
            Arguments = $"\"{pythonScriptPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        Process process = new Process
        {
            StartInfo = startInfo
        };

        try
        {
            process.Start();
            Log("Python process started asynchronously.");
        }
        catch (System.Exception ex)
        {
            LogError("Failed to start Python process: " + ex.Message);
        }

        // Quit the application immediately after starting the Python script
        Log("Quitting the application.");
        Application.Quit();

        // If running in the editor, stop playing
//#if UNITY_EDITOR
//        UnityEditor.EditorApplication.isPlaying = false;
//#endif
    }

    void Log(string message)
    {
        Debug.Log(message);
        File.AppendAllText(logFilePath, message + "\n");
    }

    void LogError(string message)
    {
        Debug.LogError(message);
        File.AppendAllText(logFilePath, "ERROR: " + message + "\n");
    }
}
