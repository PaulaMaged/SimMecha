using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Unity.Robotics.UrdfImporter;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Threading.Tasks;

public class RunPythonScript : MonoBehaviour
{

    // Path to the Python executable
    private string pythonExePath = null;

    // Path to the log file
    private string logFilePath = "Assets/Python Code/log.txt";

    public void RunPython()
    {
        // Log the start of the process
        Log("Starting Python script...");

        pythonExePath = "Assets/Python Code/output/main/main.exe";

        if (pythonExePath == null)
        {
            Debug.Log("Python executable not found.");
            LogError("Python executable not found.");
            return;
        }


        // Create a new process to run the Python script
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = pythonExePath,
            //Arguments = $"\"{pythonScriptPath}\"",
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