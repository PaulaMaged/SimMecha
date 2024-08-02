using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class RunPythonScript : MonoBehaviour
{
    // Path to the Python script relative to the Unity project folder
    private string pythonScriptPath = "Assets/Python/main.py";

    // Path to the Python executable
    private string pythonExePath = null;

    // Path to the log file
    private string logFilePath = "Assets/Python/log.txt";

    public void RunPython()
    {
        // Log the start of the process
        Log("Starting Python script...");

<<<<<<< HEAD
        // Search for python.exe in common locations
        pythonExePath = FindPythonExecutable();

        if (pythonExePath == null)
        {
            Debug.Log("Python executable not found.");
            LogError("Python executable not found.");
=======
        if (!File.Exists(pythonExePath))
        {
            Log($"Python executable not found at: {pythonExePath}");
>>>>>>> origin/dev
            return;
        }

        if (!File.Exists(pythonScriptPath))
        {
<<<<<<< HEAD
            Debug.Log($"Python script not found at: {pythonScriptPath}");
            LogError($"Python script not found at: {pythonScriptPath}");
=======
            Log($"Python script not found at: {pythonScriptPath}");
>>>>>>> origin/dev
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
<<<<<<< HEAD
    }

    private string FindPythonExecutable()
    {
        string[] commonPaths = {
            @"C:\Python39\python.exe",
            @"C:\Python38\python.exe",
            @"C:\Python37\python.exe",
            @"C:\Python36\python.exe",
            @"C:\Python35\python.exe",
            @"C:\Python34\python.exe",
            @"C:\Python33\python.exe",
            @"C:\Python32\python.exe",
            @"C:\Python31\python.exe",
            @"C:\Python30\python.exe"
        };

        // Check common installation paths
        foreach (string path in commonPaths)
        {
            if (File.Exists(path))
            {
                Debug.Log("Found path in example paths: " + path);
                return path;
            }
        }

        // Check the PATH environment variable
        string pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (pathEnv != null)
        {
            string[] pathDirs = pathEnv.Split(Path.PathSeparator);
            foreach (string dir in pathDirs)
            {
                string fullPath = Path.Combine(dir, "python.exe");
                if (File.Exists(fullPath))
                {
                    Debug.Log("Found path in environment variables: " +  fullPath);
                    return fullPath;
                }
            }
        }

        return null;
=======

        // Quit the application immediately after starting the Python script
        // Log("Quitting the application.");
        // Application.Quit();

        // If running in the editor, stop playing
//#if UNITY_EDITOR
//        UnityEditor.EditorApplication.isPlaying = false;
//#endif
>>>>>>> origin/dev
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
