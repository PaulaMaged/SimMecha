using UnityEngine;
using Unity.Robotics.UrdfImporter;
using SFB; // StandaloneFileBrowser namespace
using System.Collections;
using System.Collections.Generic;

public class RuntimeURDFLoader : MonoBehaviour
{
    public ImportSettings importSettings;
    private GameObject robot;
    public static List<GameObject> ImportedRobots;
    private ArticulationBody[] articulationBodies; 
    private Vector3 previousPosition;

    void Start()
    {
        // Enable runtime mode
        RuntimeUrdf.SetRuntimeMode(true);

        // Initialize default import settings if needed
        if (importSettings == null)
        {
            importSettings = new ImportSettings
            {
                chosenAxis = ImportSettings.axisType.yAxis, // Set to Z axis
                totalLinks = 0,
                linksLoaded = 0
            };
        }

        ImportedRobots = new List<GameObject>();
    }

    // Method to open file dialog and import URDF file
    public void OpenFileAndImportURDF()
    {
        // Open file dialog to select URDF file
        var extensions = new[] {
            new ExtensionFilter("URDF Files", "urdf")
        };
        var paths = StandaloneFileBrowser.OpenFilePanel("Open URDF File", "", extensions, false);

        if (paths.Length > 0)
        {
            string urdfFilePath = paths[0];
            ImportURDF(urdfFilePath);
        }
        else
        {
            Debug.LogError("No file selected.");
        }
    }

    // Method to import URDF file at runtime
    private void ImportURDF(string urdfFilePath)
    {
        if (!string.IsNullOrEmpty(urdfFilePath))
        {
            // Call the URDF Importer method to import the URDF file
            robot = UrdfRobotExtensions.CreateRuntime(urdfFilePath, importSettings);

            if (robot != null)
            {
                // Set the desired coordinates
                Vector3 desiredPosition = new Vector3(0, 5, 0); // Example coordinates
                robot.transform.position = desiredPosition;

                ImportedRobots.Add(robot);

                // Set the tag to "Selectable" for the root object and all its children
                // SetTagRecursively(robot, "Selectable");

                // Cache articulation bodies
                articulationBodies = robot.GetComponentsInChildren<ArticulationBody>();

                // Initialize previous position
                previousPosition = robot.transform.position;

                Debug.Log("URDF model imported, positioned, and tagged successfully.");
            }
            else
            {
                Debug.LogError("Failed to import URDF model.");
            }
        }
        else
        {
            Debug.LogError("Invalid URDF file path.");
        }
    }

    private void SetTagRecursively(GameObject obj, string tag)
    {
        obj.tag = tag;
        foreach (Transform child in obj.transform)
        {
            SetTagRecursively(child.gameObject, tag);
        }
    }

    private void Update()
    {
        if (robot != null)
        {
            // Check if the robot is moving
            if (Vector3.Distance(robot.transform.position, previousPosition) > 0.01f)
            {
                // Disable all articulation bodies
                SetArticulationBodiesEnabled(false);
            }
            else
            {
                // Enable all articulation bodies
                SetArticulationBodiesEnabled(true);
            }

            // Update previous position
            previousPosition = robot.transform.position;
        }
    }

    private void SetArticulationBodiesEnabled(bool enabled)
    {
        foreach (var body in articulationBodies)
        {
            body.enabled = enabled;
        }
    }

    // Method to trigger the open file dialog and import (e.g., from a UI button)
    public void OnImportButtonClicked()
    {
        OpenFileAndImportURDF();
    }
}
