using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TextMesh Pro
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class ShowHierarchyMenu : MonoBehaviour
{
    public GameObject hierarchyItemPrefab; // Assign your prefab here
    public Transform contentTransform; // Assign the Content GameObject here
    public GameObject hierarchyPanel; // Assign the hierarchy panel GameObject here

    private List<string> motorList = new List<string>();

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

            Button itemButton = item.GetComponentInChildren<Button>();
            if (itemButton != null)
            {
                TMP_Text itemButtonText = itemButton.GetComponentInChildren<TMP_Text>();
                itemButtonText.text = robot.name;
            
                itemButton.onClick.AddListener(() => OnItemClick(robot, item));
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
                    linkMenuDropdown.options.Add(new TMP_Dropdown.OptionData(link.name));
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

    private void OnItemClick(GameObject robotObject, GameObject item)
    {
        
        // Find the LinkMenu and MotorMenu in the prefab
        Transform linkMenuTransform = item.transform.Find("SelectedLinkAndMotorMenus/SelectLinkMenu");
        Transform motorMenuTransform = item.transform.Find("SelectedLinkAndMotorMenus/SelectMotorMenu");

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