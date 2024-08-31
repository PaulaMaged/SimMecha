using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.UrdfImporter;
using SFB; // StandaloneFileBrowser namespace
using TMPro;
using RuntimeInspectorNamespace;
using UIScripts;

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
    public List<string> urdfFilePaths = new List<string>();
    private int nextRobotId = 0;
    public static Dictionary<int, GameObject> RobotIdToGameObject = new Dictionary<int, GameObject>();

    public GameObject axisSelectionPanel;
    public Button yAxisButton;
    public Button zAxisButton;
    public InputField scaleInputField;

    private string urdfFilePath;
    private GameObject currentImportedRobot;
    public Button deleteButton;
    public RuntimeHierarchy runtimeHierarchy;
    // Add a reference to ShowHierarchyMenu
    public ShowHierarchyMenu showHierarchyMenu;
    public AddConstraintController AddConstraintController;

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
        deleteButton.onClick.AddListener(DeleteSelectedRobot);

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

                // Assign the ID based on the next available index
                identifier.robotId = nextRobotId;
                RobotIdToGameObject[nextRobotId] = robot;
                ImportedRobots.Add(robot);

                List<string> robotLinks = new List<string>();
                GetLinksFromRobot(robotLinks, robot.transform);

                NewImportedRobots.Add(new RobotModel(robot, nextRobotId, urdfFilePath, robotLinks));

                DisableArticulationBodies(robot);

                currentImportedRobot = robot;

                // Increment nextRobotId for the next robot
                nextRobotId++;

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


    public void DeleteSelectedRobot()
    {
        if (runtimeHierarchy.CurrentSelection.Count > 0)
        {
            GameObject selectedObject = runtimeHierarchy.CurrentSelection[0].gameObject;
            RobotIdentifier identifier = selectedObject.GetComponent<RobotIdentifier>();

            if (identifier != null)
            {
                int robotId = identifier.robotId;

                // Remove the URDF file path associated with the robot
                if (robotId < urdfFilePaths.Count)
                {
                    Debug.Log($"Removing URDF file path: {urdfFilePaths[robotId]}");
                    urdfFilePaths.RemoveAt(robotId);
                }

                RobotIdToGameObject.Remove(robotId);
                ImportedRobots.Remove(selectedObject);
                NewImportedRobots.RemoveAll(robot => robot.RobotId == robotId);

                ClearMotorLinkSelectionsForRobot(robotId);
                ClearLinkConstraintsForRobot(robotId); 

                Destroy(selectedObject);

                ReassignRobotIds();

                Debug.Log($"Robot with ID {robotId} deleted successfully.");
            }
            else
            {
                PopUpController.Instance.ShowMessage("You cannot delete this object!");
            }
        }
        else
        {
            Debug.LogWarning("No object selected in the hierarchy.");
        }
    }

    private void ClearLinkConstraintsForRobot(int robotId)
    {
        if (showHierarchyMenu == null)
        {
            Debug.LogError("ShowHierarchyMenu reference is missing.");
            return;
        }

        var linkConstraints = AddConstraintController._linkConstraints;

        var keysToRemove = linkConstraints.Keys
            .Where(key =>
            {
                var (firstTuple, secondTuple) = key;
                return firstTuple.robotId == robotId || secondTuple.robotId == robotId;
            })
            .ToList();

        Debug.Log($"Found {keysToRemove.Count} constraints to remove for robot ID {robotId}.");

        foreach (var key in keysToRemove)
        {
            linkConstraints.Remove(key);
            Debug.Log($"Removed link constraint for robot ID {robotId}, links: {key.Item1.link} and {key.Item2.link}.");
        }

        AddConstraintController._linkConstraints = linkConstraints;
        Debug.Log($"Cleared link constraints for robot ID {robotId}.");
    }

    private void ReassignRobotIds()
    {
        for (int i = 0; i < ImportedRobots.Count; i++)
        {
            var robot = ImportedRobots[i];
            var identifier = robot.GetComponent<RobotIdentifier>();

            if (identifier != null)
            {
                identifier.robotId = i;
                RobotIdToGameObject[i] = robot;

                var robotLinks = new List<string>();
                GetLinksFromRobot(robotLinks, robot.transform);
                var robotModel = NewImportedRobots.FirstOrDefault(r => r.RobotId == i);
                if (robotModel != null)
                {
                    robotModel.Robot = robot;
                    robotModel.URL = urdfFilePaths.ElementAtOrDefault(i); 
                    robotModel.Links = robotLinks;
                }
            }
        }

        nextRobotId = ImportedRobots.Count;

        var updatedLinkMotorSelections = new Dictionary<(int robotId, string linkName), Dictionary<string, object>>();

        foreach (var entry in showHierarchyMenu.GetLinkMotorSelections())
        {
            var oldKey = entry.Key;
            var linkName = oldKey.linkName;
            var motorAttributes = entry.Value;

            if (RobotIdToGameObject.TryGetValue(oldKey.robotId, out var robot))
            {
                int newRobotId = ImportedRobots.IndexOf(robot);
                updatedLinkMotorSelections[(newRobotId, linkName)] = motorAttributes;
            }
        }

        showHierarchyMenu.ClearLinkMotorSelections();
        foreach (var selection in updatedLinkMotorSelections)
        {
            showHierarchyMenu.UpdateLinkMotorSelections(selection.Key.robotId, selection.Key.linkName, selection.Value);
        }

        Debug.Log("Robot IDs reassigned and link-motor selections updated.");
    }


    private void ClearMotorLinkSelectionsForRobot(int robotId)
    {
        if (showHierarchyMenu == null)
        {
            Debug.LogError("ShowHierarchyMenu reference is missing.");
            return;
        }

        var linkMotorSelections = showHierarchyMenu.GetLinkMotorSelections();

        var keysToRemove = linkMotorSelections.Keys
            .Where(key => key.robotId == robotId)
            .ToList();

        Debug.Log($"Found {keysToRemove.Count} entries to remove for robot ID {robotId}.");

        foreach (var key in keysToRemove)
        {
            linkMotorSelections.Remove(key);
            Debug.Log($"Removed motor link selection for robot ID {robotId}, link: {key.linkName}.");
        }

        Debug.Log($"Cleared motor link selections for robot ID {robotId}.");
    }



    private static void GetLinksFromRobot(List<string> links, Transform parent)
    {
        if (parent.GetComponent<UrdfLink>() != null)
        {
            links.Add(parent.name);
        }

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
