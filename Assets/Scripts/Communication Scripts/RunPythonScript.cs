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


    public void RunPython()
    {

        pythonExePath = "Physics Simulation_Data/Python Code/output/main/main.exe";

        if (pythonExePath == null)
        {
            Debug.Log("Python executable not found.");
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


        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }
        catch (Exception ex)
        {
        }
    }



}