using UnityEngine;
using Unity.Robotics.UrdfImporter;
using SFB; // StandaloneFileBrowser namespace
using System.Collections;
using System.Collections.Generic;

public class RuntimeURDFLoader : MonoBehaviour
{
    public ImportSettings importSettings;
    public static List<GameObject> ImportedRobots = new List<GameObject>(); // Static list
    private List<ArticulationBody[]> articulationBodiesList;
    private List<Vector3> previousPositions;
    private List<Quaternion> previousRotations;
    private List<string> urdfFilePaths = new List<string>();

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
        articulationBodiesList = new List<ArticulationBody[]>();
        previousPositions = new List<Vector3>();
        previousRotations = new List<Quaternion>();
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
            urdfFilePaths.Add(urdfFilePath);
            Debug.Log("URDF file path added to the list: " + urdfFilePath);

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
            GameObject robot = UrdfRobotExtensions.CreateRuntime(urdfFilePath, importSettings);

            if (robot != null)
            {
                // Ensure the imported URDF is not a child of any other object
                robot.transform.SetParent(null);

                // Set the desired coordinates
                Vector3 desiredPosition = new Vector3(0, 5, 0); // Example coordinates
                robot.transform.position = desiredPosition;

                // Force the GameObject to be the last in the hierarchy
                robot.transform.SetAsLastSibling(); // This will move it to the last index in the hierarchy

                ImportedRobots.Add(robot);

                // Cache articulation bodies
                ArticulationBody[] articulationBodies = robot.GetComponentsInChildren<ArticulationBody>();
                articulationBodiesList.Add(articulationBodies);

                // Initialize previous position and rotation for this robot
                previousPositions.Add(robot.transform.position);
                previousRotations.Add(robot.transform.rotation);

                Debug.Log("URDF model imported and positioned successfully.");
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

    private void Update()
    {
        for (int i = 0; i < ImportedRobots.Count; i++)
        {
            GameObject robot = ImportedRobots[i];
            ArticulationBody[] articulationBodies = articulationBodiesList[i];

            if (robot != null)
            {
                // Check if the robot is moving or rotating
                if (Vector3.Distance(robot.transform.position, previousPositions[i]) > 0.01f ||
                    Quaternion.Angle(robot.transform.rotation, previousRotations[i]) > 0.1f)
                {
                    // Disable all articulation bodies
                    SetArticulationBodiesEnabled(articulationBodies, false);
                }
                else
                {
                    // Enable all articulation bodies
                    SetArticulationBodiesEnabled(articulationBodies, true);
                }

                // Update previous position and rotation
                previousPositions[i] = robot.transform.position;
                previousRotations[i] = robot.transform.rotation;
            }
        }
    }

    private void SetArticulationBodiesEnabled(ArticulationBody[] articulationBodies, bool enabled)
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

    public List<GameObject> GetImportedRobots()
    {
        return ImportedRobots;
    }
    public List<string> GetUrls()
    {
        return urdfFilePaths;
    }
}
