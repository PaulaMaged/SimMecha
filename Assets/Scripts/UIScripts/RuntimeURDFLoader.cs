using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.UrdfImporter;
using SFB; // StandaloneFileBrowser namespace
using System.Collections.Generic;
using RuntimeInspectorNamespace;
using TMPro;

public class RobotIdentifier : MonoBehaviour
{
    public int robotId;
}
public class RobotModel
{
    public GameObject Robot;
    public int RobotId;
    public string URL;
    public List<string> Links;
    // Dictionary<string, MotorBase>;

    public RobotModel(GameObject robot, int robotId, string url, List<string> links)
    {
        this.Robot = robot;
        this.RobotId = robotId;
        this.URL = url;
        this.Links = links;
    }
}

public class RuntimeURDFLoader : MonoBehaviour
{
    public ImportSettings importSettings;
    public static List<GameObject> ImportedRobots;
    public static List<RobotModel> NewImportedRobots;
    private List<string> urdfFilePaths = new List<string>();
    private int nextRobotId = 0;
    public static Dictionary<int, GameObject> RobotIdToGameObject = new Dictionary<int, GameObject>();

    public GameObject axisSelectionPanel;
    public Button yAxisButton;
    public Button zAxisButton;
    public InputField scaleInputField;

    private string urdfFilePath;
    private GameObject currentImportedRobot;

    public RuntimeHierarchy runtimeHierarchy;

    private void Awake()
    {
        ImportedRobots = new List<GameObject>();
        NewImportedRobots = new List<RobotModel>();
        RobotIdToGameObject = new Dictionary<int, GameObject>();
    }

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
            PopUpController.Instance.ShowMessage("No file selected.");
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

                List<string> robotLinks = new List<string>();
                GetLinksFromRobot(robotLinks, robot.transform);
                
                NewImportedRobots.Add(new RobotModel(robot, identifier.robotId, urdfFilePath, robotLinks));

                DisableArticulationBodies(robot);

                currentImportedRobot = robot;

                Debug.Log($"URDF model imported and positioned successfully with ID: {identifier.robotId}");
            }
            else
            {
                PopUpController.Instance.ShowMessage("Failed to import URDF model.");
            }
        }
        else
        {
            PopUpController.Instance.ShowMessage("Invalid URDF file path.");
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
                PopUpController.Instance.ShowMessage("No object selected in the hierarchy.");
            }
        }
        else
        {
            PopUpController.Instance.ShowMessage("Invalid scale value.");
        }
    }
    
    private static void GetLinksFromRobot(List<string> links, Transform parent)
    {
        // Check if the object has a component named "UrdfLink"
        if (parent.GetComponent<UrdfLink>() != null)
        {
            // Add the link's name to the dropdown options
            links.Add(parent.name);
        }

        // Recursively check the children
        foreach (Transform child in parent)
        {
            GetLinksFromRobot(links, child);
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
