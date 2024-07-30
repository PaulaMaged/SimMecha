using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class RunSimulation : EditorWindow
{
    [MenuItem("Robotics/Run Simulation")]
    public static void ShowWindow()
    {
        GetWindow<RunSimulation>("Run Simulation");
    }

    private string urdfPath = "path/to/your/robot.urdf";

    void OnGUI()
    {
        GUILayout.Label("Run PyBullet Simulation", EditorStyles.boldLabel);

        urdfPath = EditorGUILayout.TextField("URDF Path", urdfPath);

        if (GUILayout.Button("Start Simulation"))
        {
            StartSimulation(urdfPath);
        }
    }

    private void StartSimulation(string urdfPath)
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = "python"; // Make sure 'python' is in your PATH or use full path to python executable
        start.Arguments = $"simulate.py \"{urdfPath}\"";
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;

        Process process = new Process();
        process.StartInfo = start;
        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        Debug.Log(output);
        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError(error);
        }
    }
}
