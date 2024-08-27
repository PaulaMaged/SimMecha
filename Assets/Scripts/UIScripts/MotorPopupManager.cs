using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class MotorPopupManager : MonoBehaviour
{
    public GameObject DoublyFedInduction;
    public GameObject ExtExcitedDc;
    public GameObject ExtExcitedSynch;
    public GameObject PermExcitedDc;
    public GameObject SeriesDc;
    public GameObject ShuntDc;
    public GameObject SquirrelCageInduction;
    public GameObject SynchReluctance;

    private GameObject motorPopupInstance;
    private ShowHierarchyMenu showHierarchyMenu;

    private void Start()
    {
        showHierarchyMenu = FindObjectOfType<ShowHierarchyMenu>();
        if (showHierarchyMenu == null)
        {
            Debug.LogError("ShowHierarchyMenu not found in the scene.");
        }
    }

    public void ShowMotorPopup(int robotId, string linkName, string motorType, Dictionary<string, object> linkMotorSelections)
    {
        if (motorPopupInstance != null)
        {
            Destroy(motorPopupInstance);
            motorPopupInstance = null; // Reset the motorPopupInstance to ensure it's cleared
        }

        MotorBase motor = CreateMotorInstance(motorType);
        if (motor == null)
        {
            Debug.LogError("Unknown motor type: " + motorType);
            return;
        }

        // Ensure all default attributes are included in linkMotorSelections
        var defaultAttributes = motor.GetAttributes();
        foreach (var attribute in defaultAttributes)
        {
            if (!linkMotorSelections.ContainsKey(attribute.Key))
            {
                linkMotorSelections.Add(attribute.Key, attribute.Value);
            }
        }

        motorPopupInstance = InstantiateMotorPrefab(motorType);
        motorPopupInstance.transform.SetParent(FindObjectOfType<Canvas>().transform, false);

        SetupInputFields(motor, linkMotorSelections);

        Button confirmButton = motorPopupInstance.GetComponentInChildren<Button>(true);
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(() => ConfirmPopup(robotId, linkName, linkMotorSelections));
        }
        else
        {
            Debug.LogError("Confirm button not found in the motor popup prefab.");
        }

        TMP_Text motorNameText = motorPopupInstance.transform.Find("MotorName")?.GetComponent<TMP_Text>();
        if (motorNameText != null)
        {
            motorNameText.text = motor.MotorName;
        }
        else
        {
            Debug.LogError("MotorName text component not found in the prefab.");
        }
    }



    private MotorBase CreateMotorInstance(string motorType)
    {
        switch (motorType)
        {
            
            case "DoublyFedInduction":
                return new DoublyFedInduction();
            case "ExtExcitedDc":
                return new ExtExcitedDc();
            case "ExtExcitedSynch":
                return new ExtExcitedSynch();
            case "PermExcitedDc":
                return new PermExcitedDc();
            case "SeriesDc":
                return new SeriesDc();
            case "ShuntDc":
                return new ShuntDc();
            case "SquirrelCageInduction":
                return new SquirrelCageInduction();
            case "SynchReluctance":
                return new SynchReluctance();
            default:
                return null;
        }
    }

    private GameObject InstantiateMotorPrefab(string motorType)
    {
        switch (motorType)
        {
           
            case "DoublyFedInduction":
                return Instantiate(DoublyFedInduction, Vector3.zero, Quaternion.identity);
            case "ExtExcitedDc":
                return Instantiate(ExtExcitedDc, Vector3.zero, Quaternion.identity);
            case "ExtExcitedSynch":
                return Instantiate(ExtExcitedSynch, Vector3.zero, Quaternion.identity);
            case "PermExcitedDc":
                return Instantiate(PermExcitedDc, Vector3.zero, Quaternion.identity);
            case "SeriesDc":
                return Instantiate(SeriesDc, Vector3.zero, Quaternion.identity);
            case "ShuntDc":
                return Instantiate(ShuntDc, Vector3.zero, Quaternion.identity);
            case "SquirrelCageInduction":
                return Instantiate(SquirrelCageInduction, Vector3.zero, Quaternion.identity);
            case "SynchReluctance":
                return Instantiate(SynchReluctance, Vector3.zero, Quaternion.identity);
            default:
                Debug.LogError("Unknown motor type: " + motorType);
                return null;
        }
    }

    private void SetupInputFields(MotorBase motor, Dictionary<string, object> linkMotorSelections)
    {
        foreach (var attribute in motor.GetAttributes())
        {
            TMP_InputField inputField = FindInputField(attribute.Key + "Input");
            if (inputField != null)
            {
                // Set the initial value
                inputField.text = attribute.Value.ToString();

                // No dictionary update here
            }
            else
            {
                Debug.LogError($"{attribute.Key}InputField not found in the prefab.");
            }
        }
    }


    private TMP_InputField FindInputField(string fieldName)
    {
        return motorPopupInstance.transform.GetComponentsInChildren<TMP_InputField>(true)
            .FirstOrDefault(inputField => inputField.name == fieldName);
    }

    public void ConfirmPopup(int robotId, string linkName, Dictionary<string, object> motorAttributes)
    {
        if (showHierarchyMenu == null)
        {
            Debug.LogError("ShowHierarchyMenu reference is missing.");
            return;
        }

        TMP_InputField[] inputFields = motorPopupInstance.GetComponentsInChildren<TMP_InputField>(true);
        foreach (var inputField in inputFields)
        {
            string key = inputField.name.Replace("Input", "");

            if (float.TryParse(inputField.text, out float floatValue))
            {
                motorAttributes[key] = floatValue;
            }
            else if (int.TryParse(inputField.text, out int intValue))
            {
                motorAttributes[key] = intValue;
            }
            else
            {
                motorAttributes[key] = inputField.text; // Store as string if not a number
            }
        }

        // Log before updating
        Debug.Log($"Before Update: robotId: {robotId}, linkName: {linkName}, motorAttributes: {motorAttributes.Count}");
        showHierarchyMenu.UpdateLinkMotorSelections(robotId, linkName, motorAttributes);
        Debug.Log($"After Update: robotId: {robotId}, linkName: {linkName}");

        var linkMotorSelections = showHierarchyMenu.GetLinkMotorSelections();
        if (linkMotorSelections.TryGetValue((robotId, linkName), out var motorParams))
        {
            Debug.Log($"Robot ID: {robotId}");
            Debug.Log($"Link Name: {linkName}");
            Debug.Log("Motor Parameters:");
            foreach (var param in motorParams)
            {
                Debug.Log($"{param.Key}: {param.Value}");
            }
        }
        else
        {
            Debug.LogError($"No entry found for Robot ID: {robotId} and Link Name: {linkName}");
        }

        CloseMotorPopup();
    }



    private void PrintDictionaryContents(int robotId, string linkName)
    {
        var linkMotorSelections = showHierarchyMenu.GetLinkMotorSelections();
        if (linkMotorSelections.TryGetValue((robotId, linkName), out var motorParams))
        {
            Debug.Log($"Robot ID: {robotId}");
            Debug.Log($"Link Name: {linkName}");
            Debug.Log("Motor Parameters:");
            foreach (var param in motorParams)
            {
                Debug.Log($"{param.Key}: {param.Value}");
            }
        }
        else
        {
            Debug.LogError($"No entry found for Robot ID: {robotId} and Link Name: {linkName}");
        }
    }

    private void CloseMotorPopup()
    {
        if (motorPopupInstance != null)
        {
            Debug.Log("Closing motorPopupInstance.");
            Destroy(motorPopupInstance);
        }
    }
}
