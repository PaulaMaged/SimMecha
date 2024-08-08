using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TextMesh Pro
using System.Collections.Generic;

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

            // Set the robot name in the button text
            TMP_Text itemText = item.GetComponentInChildren<TMP_Text>();
            if (itemText != null)
            {
                itemText.text = robot.name;
            }

            Button itemButton = item.GetComponent<Button>();
            if (itemButton != null)
            {
                // Pass the robot GameObject to the OnItemClick method
                itemButton.onClick.AddListener(() => OnItemClick(robot, item));
            }

            // Find the LinkMenu and MotorMenu in the prefab
            Transform linkMenuTransform = item.transform.Find("SelectedLinkAndMotorMenus/SelectLinkMenu/Template");
            Transform motorMenuTransform = item.transform.Find("SelectedLinkAndMotorMenus/SelectMotorMenu/Template");

            // Populate the LinkMenu with links
            if (linkMenuTransform != null)
            {
                foreach (Transform link in robot.transform)
                {
                    GameObject linkItem = Instantiate(linkMenuTransform.gameObject, linkMenuTransform.parent);
                    linkItem.SetActive(true);

                    TMP_Text linkItemText = linkItem.GetComponentInChildren<TMP_Text>();
                    if (linkItemText != null)
                    {
                        linkItemText.text = link.name;
                    }
                }

                linkMenuTransform.gameObject.SetActive(false); // Hide the template
            }

            // Populate the MotorMenu with motors
            if (motorMenuTransform != null)
            {
                foreach (string motor in motorList)
                {
                    GameObject motorItem = Instantiate(motorMenuTransform.gameObject, motorMenuTransform.parent);
                    motorItem.SetActive(true);

                    TMP_Text motorItemText = motorItem.GetComponentInChildren<TMP_Text>();
                    if (motorItemText != null)
                    {
                        motorItemText.text = motor;
                    }
                }

                motorMenuTransform.gameObject.SetActive(false); // Hide the template
            }
        }
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

    private void PopulateHierarchy(GameObject root)
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
        }
    }
}