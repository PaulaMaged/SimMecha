using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Unity.Robotics.UrdfImporter;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class RunPythonScript : MonoBehaviour
{
    // Path to the Python script relative to the Unity project folder
    private string pythonScriptPath = "Assets/Python Code/main.py";

    // Path to the Python executable
    private string pythonExePath = @"D:\Paula\C (Extension)\Program Files\Python\python.exe";

    // Path to the log file
    private string logFilePath = "Assets/Python Code/log.txt";

    public void RunPython()
    {
        // Log the start of the process
        Log("Starting Python script...");

        // Search for python.exe in common locations
        pythonExePath = FindPythonExecutable();

        if (pythonExePath == null)
        {
            Debug.Log("Python executable not found.");
            LogError("Python executable not found.");
            return;
        }

        if (!File.Exists(pythonScriptPath))
        {
            Debug.Log($"Python script not found at: {pythonScriptPath}");
            LogError($"Python script not found at: {pythonScriptPath}");
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

        // Set up event handlers to capture output and error
        process.OutputDataReceived += (sender, args) => {
            if (args.Data != null)
            {
                Log(args.Data);
            }
        };

        process.ErrorDataReceived += (sender, args) => {
            if (args.Data != null)
            {
                LogError(args.Data);
            }
        };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            Log("Python process started asynchronously.");
        }
        catch (Exception ex)
        {
            LogError("Failed to start Python process: " + ex.Message);
        }
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
                    Debug.Log("Found path in environment variables: " + fullPath);
                    return fullPath;
                }
            }
        }

        return null;
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
