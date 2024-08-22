using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.UrdfImporter;
using SFB; // StandaloneFileBrowser namespace
using System.Collections.Generic;
using RuntimeInspectorNamespace;

public class RobotIdentifier : MonoBehaviour
{
    public int robotId;
}

public class RuntimeURDFLoader : MonoBehaviour
{
    public ImportSettings importSettings;
    public static List<GameObject> ImportedRobots = new List<GameObject>();
    private List<string> urdfFilePaths = new List<string>();
    private int nextRobotId = 1;
    public static Dictionary<int, GameObject> RobotIdToGameObject = new Dictionary<int, GameObject>();

    public GameObject axisSelectionPanel;
    public Button yAxisButton;
    public Button zAxisButton;
    public InputField scaleInputField;

    private string urdfFilePath;
    private GameObject currentImportedRobot;

    public RuntimeHierarchy runtimeHierarchy;

    void Start()
    {
        RuntimeUrdf.SetRuntimeMode(true);

        if (importSettings == null)
        {
            importSettings = new ImportSettings
            {
                chosenAxis = ImportSettings.axisType.yAxis,
                totalLinks = 0,
                linksLoaded = 0
            };
        }

        ImportedRobots = new List<GameObject>();
        RobotIdToGameObject = new Dictionary<int, GameObject>();

        yAxisButton.onClick.AddListener(() => OnAxisSelected(ImportSettings.axisType.yAxis));
        zAxisButton.onClick.AddListener(() => OnAxisSelected(ImportSettings.axisType.zAxis));

        scaleInputField.onEndEdit.AddListener(OnScaleInputChanged);

        axisSelectionPanel.SetActive(false);
    }

    public void OpenFileAndImportURDF()
    {
        var extensions = new[] {
            new ExtensionFilter("URDF Files", "urdf")
        };
        var paths = StandaloneFileBrowser.OpenFilePanel("Open URDF File", "", extensions, false);

        if (paths.Length > 0)
        {
            urdfFilePath = paths[0];
            axisSelectionPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("No file selected.");
        }
    }

    private void OnAxisSelected(ImportSettings.axisType selectedAxis)
    {
        importSettings.chosenAxis = selectedAxis;
        axisSelectionPanel.SetActive(false);
        ImportURDF(urdfFilePath);
        urdfFilePaths.Add(urdfFilePath);
        Debug.Log("URDF file path added to the list: " + urdfFilePath);
    }

    private void ImportURDF(string urdfFilePath)
    {
        if (!string.IsNullOrEmpty(urdfFilePath))
        {
            GameObject robot = UrdfRobotExtensions.CreateRuntime(urdfFilePath, importSettings);

            if (robot != null)
            {
                robot.transform.SetParent(null);
                Vector3 desiredPosition = new Vector3(0, 3, 0);
                robot.transform.position = desiredPosition;
                robot.transform.SetAsLastSibling();

                RobotIdentifier identifier = robot.AddComponent<RobotIdentifier>();
                identifier.robotId = nextRobotId++;

                RobotIdToGameObject[identifier.robotId] = robot;
                ImportedRobots.Add(robot);

                DisableArticulationBodies(robot);

                currentImportedRobot = robot;

                Debug.Log($"URDF model imported and positioned successfully with ID: {identifier.robotId}");
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

    private void DisableArticulationBodies(GameObject robot)
    {
        ArticulationBody[] articulationBodies = robot.GetComponentsInChildren<ArticulationBody>();
        foreach (var body in articulationBodies)
        {
            body.enabled = false;
        }
    }

    private void OnScaleInputChanged(string scaleValue)
    {
        if (float.TryParse(scaleValue, out float scale))
        {
            // Get the selected object from the hierarchy
            if (runtimeHierarchy.CurrentSelection.Count > 0)
            {
                GameObject selectedObject = runtimeHierarchy.CurrentSelection[0].gameObject;
                ApplyScale(selectedObject, scale);
                Debug.Log($"Applied uniform scale of {scale} to the selected object and all its children.");
            }
            else
            {
                Debug.LogWarning("No object selected in the hierarchy.");
            }
        }
        else
        {
            Debug.LogWarning("Invalid scale value.");
        }
    }

    private void ApplyScale(GameObject parent, float scaleValue)
    {
        parent.transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);

    }

    public void OnImportButtonClicked()
    {
        OpenFileAndImportURDF();
    }
}
