using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class MotorPopupManager : MonoBehaviour
{
    public GameObject dcMotorPrefab; 
    public GameObject stepperMotorPrefab;
    public GameObject DoublyFedInduction;
    public GameObject ExtExcitedDc;
    public GameObject ExtExcitedSynch;
    public GameObject PermExcitedDc;

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
        }

        MotorBase motor = CreateMotorInstance(motorType);
        if (motor == null)
        {
            Debug.LogError("Unknown motor type: " + motorType);
            return;
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
            case "DC_Motor":
                return new DC_Motor();
            case "StepperMotor":
                return new StepperMotor();
            case "DoublyFedInduction":
                return new DoublyFedInduction();
            case "ExtExcitedDc":
                return new ExtExcitedDc();
            case "ExtExcitedSynch":
                return new ExtExcitedSynch();
            case "PermExcitedDc":
                return new PermExcitedDc();
            default:
                return null;
        }
    }

    private GameObject InstantiateMotorPrefab(string motorType)
    {
        switch (motorType)
        {
            case "DC_Motor":
                return Instantiate(dcMotorPrefab, Vector3.zero, Quaternion.identity);
            case "StepperMotor":
                return Instantiate(stepperMotorPrefab, Vector3.zero, Quaternion.identity);
            case "DoublyFedInduction":
                return Instantiate(DoublyFedInduction, Vector3.zero, Quaternion.identity);
            case "ExtExcitedDc":
                return Instantiate(ExtExcitedDc, Vector3.zero, Quaternion.identity);
            case "ExtExcitedSynch":
                return Instantiate(ExtExcitedSynch, Vector3.zero, Quaternion.identity);
            case "PermExcitedDc":
                return Instantiate(PermExcitedDc, Vector3.zero, Quaternion.identity);
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
                inputField.onValueChanged.AddListener(value =>
                {
                    if (float.TryParse(value, out float floatValue))
                    {
                        linkMotorSelections[attribute.Key] = floatValue;
                    }
                    else if (int.TryParse(value, out int intValue))
                    {
                        linkMotorSelections[attribute.Key] = intValue;
                    }
                });
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

        showHierarchyMenu.UpdateLinkMotorSelections(robotId, linkName, motorAttributes);

        PrintDictionaryContents(robotId, linkName);

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
