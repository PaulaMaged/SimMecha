using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TextMesh Pro
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unity.Robotics.UrdfImporter;
using UnityEngine.EventSystems;

public class ShowHierarchyMenu : MonoBehaviour
{
    public GameObject hierarchyItemPrefab; // Assign your prefab here
    public Transform contentTransform; // Assign the Content GameObject here
    public GameObject hierarchyPanel; // Assign the hierarchy panel GameObject here

    private List<string> motorList = new List<string>();
    private Dictionary<(int robotId, string linkName), string> LinkMotorSelections = new Dictionary<(int robotId, string linkName), string>();

    void Start()
    {
        // Optionally, initialize the panel state or set up event handlers
        hierarchyPanel.SetActive(false);
        motorList.Add("motor1");
        motorList.Add("motor2");
    }

    private void CreateHierarchyItems()
    {
        foreach (GameObject robot in RuntimeURDFLoader.ImportedRobots)
        {
            // Instantiate the prefab
            GameObject item = Instantiate(hierarchyItemPrefab, contentTransform);

            Button[] buttons = item.GetComponentsInChildren<Button>();
            Button mainButton = buttons.FirstOrDefault(button => button.gameObject.name == "AddMotorToRobotButton");
            if (mainButton != null)
            {
                TMP_Text itemButtonText = mainButton.GetComponentInChildren<TMP_Text>();
                // Set button text to robot's original name and ID
                int robotId = robot.GetComponent<RobotIdentifier>().robotId;
                itemButtonText.text = $"{robot.name} (ID: {robotId})";

                // Use the robot ID in the OnItemClick listener if needed
                mainButton.onClick.AddListener(() => OnItemClick(item, robotId));
            }

            Button confirmSelectionButton = buttons.FirstOrDefault(button => button.gameObject.name == "ConfirmSelectionButton");
            if (confirmSelectionButton != null)
            {
                Debug.Log("Confirm Button is available");
                // Pass robot ID to StoreDropdownSelections if needed
                confirmSelectionButton.onClick.AddListener(() => StoreDropdownSelections(robot.GetComponent<RobotIdentifier>().robotId));
                confirmSelectionButton.gameObject.SetActive(false);
            }

            // Find the LinkMenu and MotorMenu in the prefab
            TMP_Dropdown[] dropdowns = item.GetComponentsInChildren<TMP_Dropdown>();

            TMP_Dropdown linkMenuDropdown = FindDropdownByName(dropdowns, "SelectLinkMenu");
            TMP_Dropdown motorMenuDropdown = FindDropdownByName(dropdowns, "SelectMotorMenu");

            // Populate the LinkMenu with links
            if (linkMenuDropdown != null)
            {
                foreach (Transform link in robot.transform)
                {
                    AddLinksToLinkMenu(linkMenuDropdown, link);
                }

                linkMenuDropdown.gameObject.SetActive(false);
            }

            // Populate the MotorMenu with motors
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


    private static void AddLinksToLinkMenu(TMP_Dropdown linkMenuDropdown, Transform parent)
    {
        // Check if the object has a component named "UrdfLink"
        if (parent.GetComponent<UrdfLink>() != null)
        {
            // Add the link's name to the dropdown options
            linkMenuDropdown.options.Add(new TMP_Dropdown.OptionData(parent.name));
        }

        // Recursively check the children
        foreach (Transform child in parent)
        {
            AddLinksToLinkMenu(linkMenuDropdown, child);
        }
    }

    public void StoreDropdownSelections(int robotId)
    {
        // Retrieve the current event data
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);

        // Get the button that was clicked
        Button clickedButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        if (clickedButton == null)
        {
            Debug.LogError("No button found in the clicked object.");
            return;
        }

        // Get the parent of the button
        Transform parentTransform = clickedButton.transform.parent;
        if (parentTransform == null)
        {
            Debug.LogError("No parent transform found.");
            return;
        }

        // Log the hierarchy to debug the issue
        foreach (Transform child in parentTransform)
        {
            Debug.Log("Child of parentTransform: " + child.name);
        }

        // Find the button text component
        TMP_Text buttonText = parentTransform.Find("AddMotorToRobotButton/Text (TMP)")?.GetComponent<TMP_Text>();
        if (buttonText == null)
        {
            Debug.LogError("Button text object not found.");
            return;
        }

        string robotName = buttonText.text;
        Debug.Log("Selected robot: " + robotName);

        // Find the dropdown components
        TMP_Dropdown[] dropdowns = parentTransform.GetComponentsInChildren<TMP_Dropdown>();
        TMP_Dropdown linkMenuDropdown = FindDropdownByName(dropdowns, "SelectLinkMenu");
        TMP_Dropdown motorMenuDropdown = FindDropdownByName(dropdowns, "SelectMotorMenu");

        if (linkMenuDropdown == null || motorMenuDropdown == null)
        {
            Debug.LogError("Dropdowns not found.");
            return;
        }

        string linkSelection = linkMenuDropdown.options[linkMenuDropdown.value].text;
        string motorSelection = motorMenuDropdown.options[motorMenuDropdown.value].text;

        // Store the selection using robotId instead of robotName
        LinkMotorSelections[(robotId, linkSelection)] = motorSelection;
        Debug.Log($"Stored selection: {robotId} -> {linkSelection} -> {motorSelection}");
    }

    TMP_Dropdown FindDropdownByName(TMP_Dropdown[] dropdownList, string targetName)
    {
        foreach (TMP_Dropdown dropdown in dropdownList)
        {
            if (dropdown.name == targetName)
            {
                return dropdown; // Return the matching dropdown
            }
        }
        return null; // Return null if not found
    }

    private void OnItemClick(GameObject item, int robotId)
    {
        // Find the LinkMenu and MotorMenu in the prefab
        Transform linkMenuTransform = item.transform.Find("SelectedLinkAndMotorMenus/SelectLinkMenu");
        Transform motorMenuTransform = item.transform.Find("SelectedLinkAndMotorMenus/SelectMotorMenu");
        Transform confirmSelectionButton = item.transform.Find("ConfirmSelectionButton");

        // Toggle the visibility of the LinkMenu
        if (linkMenuTransform != null)
        {
            linkMenuTransform.gameObject.SetActive(!linkMenuTransform.gameObject.activeSelf);
        }

        // Toggle the visibility of the MotorMenu
        if (motorMenuTransform != null)
        {
            motorMenuTransform.gameObject.SetActive(!motorMenuTransform.gameObject.activeSelf);
        }

        // Toggle the visibility of the Confirm Selection Button
        if (confirmSelectionButton != null)
        {
            confirmSelectionButton.gameObject.SetActive(!confirmSelectionButton.gameObject.activeSelf);
        }
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
}
