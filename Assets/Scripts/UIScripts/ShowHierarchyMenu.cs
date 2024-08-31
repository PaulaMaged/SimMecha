using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TextMesh Pro
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Linq;
using UIScripts;
using Unity.Robotics.UrdfImporter;

public class ShowHierarchyMenu : MonoBehaviour
{
    public GameObject hierarchyItemPrefab; // Assign your prefab here
    public Transform contentTransform; // Assign the Content GameObject here
    public GameObject hierarchyPanel; // Assign the hierarchy panel GameObject here

    public MotorPopupManager motorPopupManager;
    public AddConstraintController _addConstraintController;

    private List<string> motorList = new List<string> {"DoublyFedInduction", "ExtExcitedDc", "ExtExcitedSynch", "PermExcitedDc", "PermMagnetSynch", "SeriesDc", "ShuntDc", "SquirrelCageInduction", "SynchReluctance" };
    private static Dictionary<(int robotId, string linkName), Dictionary<string, object>> LinkMotorSelections = new Dictionary<(int robotId, string linkName), Dictionary<string, object>>();
    // Dictionary((robotId, linkName), Dictionary<string, Object>);

    private static List<string> motorNames = new List<string>();
    private static List<int> robotId = new List<int>();
    private static List<string> linkNames = new List<string>();
    private static List<Dictionary<string, object>> motorParameters = new List<Dictionary<string, object>>();
    public RuntimeURDFLoader RuntimeURDFLoader;

    void Start()
    {
        hierarchyPanel.SetActive(false);
        
    }

    public void OnPopulateButtonClick()
    {
        PopulateFinalLists();
        List<GameObject> importedRobots = RuntimeURDFLoader.ImportedRobots;
        List<RobotModel> newImportedRobots = RuntimeURDFLoader.NewImportedRobots;
        List<string> urdfFilePaths = RuntimeURDFLoader.urdfFilePaths; 
        Dictionary<int, GameObject> robotIdToGameObject = RuntimeURDFLoader.RobotIdToGameObject;
        _addConstraintController.PopulateConstraintStringsList();

        List<string> names = GetMotorNames();
        List<int> ids = GetRobotId();
        List<string> links = GetLinkNames();
        List<Dictionary<string, object>> parameters = GetMotorParameters();
        List<string> constraints = _addConstraintController.constraintStrings;

        PopUpController.Instance.ShowMessage("Constraints: " + string.Join(", ", constraints));

        Debug.Log("URDF File Paths: " + string.Join(", ", urdfFilePaths));
        foreach (var robot in importedRobots)
        {
            Debug.Log($"- Robot GameObject: {robot.name}");
        }
        Debug.Log("New Imported Robots:");
        foreach (var robotModel in newImportedRobots)
        {
            Debug.Log($"- Robot ID: {robotModel.RobotId}, URL: {robotModel.URL}, Links: {string.Join(", ", robotModel.Links)}");
        }

        Debug.Log("Robot ID to GameObject Mapping:");
        foreach (var kvp in robotIdToGameObject)
        {
            Debug.Log($"- Robot ID: {kvp.Key}, GameObject: {kvp.Value.name}");
        }
        Debug.Log("Motor Names: " + string.Join(", ", names));
        Debug.Log("Robot IDs: " + string.Join(", ", ids));
        Debug.Log("Link Names: " + string.Join(", ", links));
        Debug.Log("Motor Parameters: " + string.Join(", ", parameters.Select(p => p.ToString())));
    }

    private void CreateHierarchyItems()
    {
        foreach (GameObject robot in RuntimeURDFLoader.ImportedRobots)
        {
            GameObject item = Instantiate(hierarchyItemPrefab, contentTransform);
            Button[] buttons = item.GetComponentsInChildren<Button>();

            Button mainButton = buttons.FirstOrDefault(button => button.gameObject.name == "AddMotorToRobotButton");
            if (mainButton != null)
            {
                TMP_Text itemButtonText = mainButton.GetComponentInChildren<TMP_Text>();
                int robotId = robot.GetComponent<RobotIdentifier>().robotId;
                itemButtonText.text = $"{robot.name} (ID: {robotId})";
                mainButton.onClick.AddListener(() => OnItemClick(item, robotId));
            }

            Button confirmSelectionButton = buttons.FirstOrDefault(button => button.gameObject.name == "ConfirmSelectionButton");
            if (confirmSelectionButton != null)
            {
                confirmSelectionButton.onClick.AddListener(() => StoreDropdownSelections(robot.GetComponent<RobotIdentifier>().robotId));
                confirmSelectionButton.gameObject.SetActive(false);
            }

            Button deleteMotorLinkButton = buttons.FirstOrDefault(button => button.gameObject.name == "DeleteMotorLinkButton");
            if (deleteMotorLinkButton != null)
            {
                deleteMotorLinkButton.onClick.AddListener(() => DeleteFromDictionary(robot.GetComponent<RobotIdentifier>().robotId));
                deleteMotorLinkButton.gameObject.SetActive(false);
            }

            TMP_Dropdown[] dropdowns = item.GetComponentsInChildren<TMP_Dropdown>();
            TMP_Dropdown linkMenuDropdown = FindDropdownByName(dropdowns, "SelectLinkMenu");
            TMP_Dropdown motorMenuDropdown = FindDropdownByName(dropdowns, "SelectMotorMenu");

            if (linkMenuDropdown != null)
            {
                foreach (Transform link in robot.transform)
                {
                    AddLinksToLinkMenu(linkMenuDropdown, link);
                }
                linkMenuDropdown.gameObject.SetActive(false);
            }

            if (motorMenuDropdown != null)
            {
                foreach (string motor in motorList)
                {
                    motorMenuDropdown.options.Add(new TMP_Dropdown.OptionData(motor));
                }
                motorMenuDropdown.gameObject.SetActive(false);
            }
        }
    }

    private void DeleteFromDictionary(int robotId)
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        Button clickedButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        if (clickedButton == null) return;

        Transform parentTransform = clickedButton.transform.parent.transform.parent;
        if (parentTransform == null) return;

        TMP_Dropdown[] dropdowns = parentTransform.GetComponentsInChildren<TMP_Dropdown>();
        TMP_Dropdown linkMenuDropdown = FindDropdownByName(dropdowns, "SelectLinkMenu");
        TMP_Dropdown motorMenuDropdown = FindDropdownByName(dropdowns, "SelectMotorMenu");

        if (linkMenuDropdown == null || motorMenuDropdown == null) return;

        string linkSelection = linkMenuDropdown.options[linkMenuDropdown.value].text;
        string motorSelection = motorMenuDropdown.options[motorMenuDropdown.value].text;

        if (LinkMotorSelections.Remove((robotId, linkSelection)))
        {
            PopUpController.Instance.ShowMessage($"Deleted selection: {robotId} -> {linkSelection} -> {motorSelection}");
        }
        else
        {
            PopUpController.Instance.ShowMessage("Link Motor selection was not found");
        }
    }

    private static void AddLinksToLinkMenu(TMP_Dropdown linkMenuDropdown, Transform parent)
    {
        if (parent.GetComponent<UrdfLink>() != null)
        {
            linkMenuDropdown.options.Add(new TMP_Dropdown.OptionData(parent.name));
        }
        foreach (Transform child in parent)
        {
            AddLinksToLinkMenu(linkMenuDropdown, child);
        }
    }

    public void StoreDropdownSelections(int robotId)
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        Button clickedButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        if (clickedButton == null) return;

        Transform parentTransform = clickedButton.transform.parent.transform.parent;
        if (parentTransform == null) return;

        TMP_Dropdown[] dropdowns = parentTransform.GetComponentsInChildren<TMP_Dropdown>();
        TMP_Dropdown linkMenuDropdown = FindDropdownByName(dropdowns, "SelectLinkMenu");
        TMP_Dropdown motorMenuDropdown = FindDropdownByName(dropdowns, "SelectMotorMenu");

        if (linkMenuDropdown == null || motorMenuDropdown == null) return;

        string linkSelection = linkMenuDropdown.options[linkMenuDropdown.value].text;
        string motorSelection = motorMenuDropdown.options[motorMenuDropdown.value].text;

        var key = (robotId, linkSelection);

        if (!LinkMotorSelections.ContainsKey(key))
        {
            LinkMotorSelections[key] = new Dictionary<string, object>();
        }

        LinkMotorSelections[key]["motorName"] = motorSelection;

        motorPopupManager.ShowMotorPopup(robotId, linkSelection, motorSelection, LinkMotorSelections[key]);

    }




    TMP_Dropdown FindDropdownByName(TMP_Dropdown[] dropdownList, string targetName)
    {
        foreach (TMP_Dropdown dropdown in dropdownList)
        {
            if (dropdown.name == targetName)
            {
                return dropdown;
            }
        }
        return null;
    }

    private void OnItemClick(GameObject item, int robotId)
    {
        Transform linkMenuTransform = item.transform.Find("SelectedLinkAndMotorMenus/SelectLinkMenu");
        Transform motorMenuTransform = item.transform.Find("SelectedLinkAndMotorMenus/SelectMotorMenu");
        Transform confirmSelectionButton = item.transform.Find("Buttons/ConfirmSelectionButton");
        Transform deleteMotorLinkButton = item.transform.Find("Buttons/DeleteMotorLinkButton");

        if (linkMenuTransform != null)
        {
            linkMenuTransform.gameObject.SetActive(!linkMenuTransform.gameObject.activeSelf);
        }

        if (motorMenuTransform != null)
        {
            motorMenuTransform.gameObject.SetActive(!motorMenuTransform.gameObject.activeSelf);
        }

        if (confirmSelectionButton != null)
        {
            confirmSelectionButton.gameObject.SetActive(!confirmSelectionButton.gameObject.activeSelf);
        }

        if (deleteMotorLinkButton != null)
        {
            deleteMotorLinkButton.gameObject.SetActive(!deleteMotorLinkButton.gameObject.activeSelf);
        }
    }
    public void ClearLinkMotorSelections()
    {
        LinkMotorSelections.Clear(); // or similar logic to clear selections
    }

    public Dictionary<(int robotId, string linkName), Dictionary<string, object>> GetLinkMotorSelections()
    {
        return LinkMotorSelections;
    }
    public void UpdateLinkMotorSelections(int robotId, string linkName, Dictionary<string, object> motorAttributes)
    {
        LinkMotorSelections[(robotId, linkName)] = motorAttributes;
    }

    private void PopulateHierarchy()
    {
        ClearHierarchy();
        CreateHierarchyItems();
    }

    private void ClearHierarchy()
    {
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }
    }

    public void ToggleHierarchyMenu()
    {
        if (hierarchyPanel.activeSelf)
        {
            hierarchyPanel.SetActive(false);
        }
        else
        {
            hierarchyPanel.SetActive(true);
            PopulateHierarchy();
        }
    }

    public static void PopulateFinalLists()
    {


        foreach (var parentKey in LinkMotorSelections.Keys)
        {

            robotId.Add(parentKey.robotId);
            linkNames.Add(parentKey.linkName);

            Dictionary<string, object> currentMotorParam = LinkMotorSelections[parentKey];

            if (currentMotorParam.ContainsKey("motorName"))
            {
                motorNames.Add(currentMotorParam["motorName"].ToString());
            }

            currentMotorParam.Remove("motorName");
            motorParameters.Add(currentMotorParam);
        }

    }

    public List<string> GetMotorNames()
    {
        return motorNames; 
    }

    public List<int> GetRobotId()
    {
        return robotId; 
    }

    public List<string> GetLinkNames()
    {
        return linkNames; 
    }

    public List<Dictionary<string, object>> GetMotorParameters()
    {
        return motorParameters; 
    }

}
