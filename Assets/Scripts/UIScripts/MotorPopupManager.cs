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
    public GameObject PermMagnetSynch;

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
        Debug.Log($"ShowMotorPopup called with robotId: {robotId}, linkName: {linkName}, motorType: {motorType}");

        if (motorPopupInstance != null)
        {
            Destroy(motorPopupInstance);
            motorPopupInstance = null;
        }

        MotorBase motor = CreateMotorInstance(motorType);
        if (motor == null)
        {
            Debug.LogError($"Unknown motor type: {motorType}");
            return;
        }

        var defaultAttributes = motor.GetAttributes();
        foreach (var attribute in defaultAttributes)
        {
            if (!linkMotorSelections.ContainsKey(attribute.Key))
            {
                linkMotorSelections.Add(attribute.Key, attribute.Value);
            }
        }

        motorPopupInstance = InstantiateMotorPrefab(motorType);
        if (motorPopupInstance != null)
        {
            Debug.Log($"Motor popup instance successfully instantiated.");
        }
        else
        {
            Debug.LogError($"Motor popup instance failed to instantiate.");
        }
        motorPopupInstance.transform.SetParent(FindObjectOfType<Canvas>().transform, false);

        SetupInputFields(motor, linkMotorSelections);

        Button confirmButton = motorPopupInstance.GetComponentInChildren<Button>(true);
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(() => ConfirmPopup(robotId, linkName, linkMotorSelections, motorType));


        }
        else
        {
            Debug.LogError($"Confirm button not found in the motor popup prefab.");
        }

        TMP_Text motorNameText = motorPopupInstance.transform.Find("MotorName")?.GetComponent<TMP_Text>();
        if (motorNameText != null)
        {
            motorNameText.text = motor.MotorName;
        }
        else
        {
            Debug.LogError($"MotorName text component not found in the prefab.");
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
            case "PermMagnetSynch":
                return new PermMagnetSynch();
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
            case "PermMagnetSynch":
                return Instantiate(PermMagnetSynch, Vector3.zero, Quaternion.identity);
            default:
                Debug.LogError($"Unknown motor type: {motorType}");
                return null;
        }
    }

    private void SetupInputFields(MotorBase motor, Dictionary<string, object> linkMotorSelections)
    {
        Debug.Log($"Setting up input fields for motor: {motor.MotorName}");
        foreach (var attribute in motor.GetAttributes())
        {
            TMP_InputField inputField = FindInputField($"{attribute.Key}Input");
            if (inputField != null)
            {
                inputField.text = attribute.Value.ToString();

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

    public void ConfirmPopup(int robotId, string linkName, Dictionary<string, object> motorAttributes, string motorName)
    {
        Debug.Log($"ConfirmPopup called.");
        PopUpController.Instance.ShowMessage($"Stored selection: {robotId} -> {linkName} -> {motorName}");

        try
        {
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

           
            showHierarchyMenu.UpdateLinkMotorSelections(robotId, linkName, motorAttributes);

            PrintDictionaryContents(robotId, linkName);
            CloseMotorPopup();

            Debug.Log($"ConfirmPopup finished successfully.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception in ConfirmPopup: {ex.Message}");
        }
    }

    private void PrintDictionaryContents(int robotId, string linkName)
    {
        Debug.Log($"PrintDictionaryContents called.");
        var linkMotorSelections = showHierarchyMenu.GetLinkMotorSelections();
        if (linkMotorSelections.TryGetValue((robotId, linkName), out var motorParams))
        {
            Debug.Log($"Robot ID: {robotId}");
            Debug.Log($"Link Name: {linkName}");
            Debug.Log($"Motor Parameters:");
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
            Debug.Log($"Closing motorPopupInstance.");
            Destroy(motorPopupInstance);
        }
    }
}
