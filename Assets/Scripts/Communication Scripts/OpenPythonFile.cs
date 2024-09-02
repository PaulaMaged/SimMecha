using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.IO;

public class OpenPythonFile : MonoBehaviour
{
    // Path to the Python script relative to the Assets folder (ensure no leading "Assets/")
    private string relativeScriptPath;
    private string fullScriptPath;

    private void Awake()
    {
        //getting the full path of python script
        relativeScriptPath = "Python Code/output/main/_internal/user_script.py";
        fullScriptPath = Path.Combine(Application.dataPath, relativeScriptPath);
        fullScriptPath = fullScriptPath.Replace("Assets\\Assets", "Assets");
    }

    public void OpenPythonScript()
    {
        // Check if the Python script exists
        if (File.Exists(fullScriptPath))
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = fullScriptPath; // Open the script file itself
            start.UseShellExecute = true;    // Use the operating system shell to open it
            start.CreateNoWindow = true;     // Don't create a console window

            // Start the process (this will open the file in the default text editor)
            Process.Start(start);

            UnityEngine.Debug.Log($"Opened {fullScriptPath} for editing.");
        }
        else
        {
            UnityEngine.Debug.LogError($"Python script not found at path: {fullScriptPath}");
        }
    }

    public void ResetPythonScript()
    {
        string newContent = "# This script can be edited by the user\r\n\r\n# do not change this section\r\n\r\nimport time\r\nmotor_classes = []\r\n\r\n\r\nprint(\"Hello, I am the user script!\")\r\n\r\nall_volts_list = []\r\n\r\ndef init():\r\n    global all_volts_list\r\n    for i in range(len(motor_classes)):\r\n        all_volts_list.append([])\r\n\r\n\r\nstep = -1\r\n\r\n# you should define all motor voltages here and put each motor volts in a list inside all_volts_list\r\n\r\ndef update_motor_voltages():\r\n    step += 1\r\n    \r\n    # put your voltage logic here\r\n    # example, if you have 2 motors, a Permanently Magnetic Synchronous motor that takes 3 volts (3 phase) and Externally Excited Dc motor that takes 2 volts\r\n    # you should return something in this form\r\n    # all_volts_list = [[float1, float2, float3], [float4, float5]]";
        try
        {
            // Check if the script exists
            if (File.Exists(fullScriptPath))
            {
                // Overwrite the file with the new content
                File.WriteAllText(fullScriptPath, newContent);
                UnityEngine.Debug.Log($"Python script at {fullScriptPath} has been reset and updated.");
                OpenPythonScript();
            }
            else
            {
                UnityEngine.Debug.LogError($"Python script not found at path: {fullScriptPath}");
            }
        }
        catch (IOException e)
        {
            UnityEngine.Debug.LogError($"Error resetting Python script: {e.Message}");
        }
    }
}
