using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUpController : MonoBehaviour
{
    public static PopUpController Instance;
    
    [SerializeField] private GameObject debugPopup;      // The pop-up panel
    [SerializeField] private TextMeshProUGUI messageText; // The text component to display the message
    [SerializeField] private Button closeButton;         // The button to close the pop-up

    private void Awake()
    {
        Instance = this;
        this.debugPopup.SetActive(false);
        closeButton.onClick.AddListener(ClosePopup);    // Add listener to the close button
    }

    public void ShowMessage(string message)
    {
        messageText.text = message;                     // Set the message text
        debugPopup.SetActive(true);                     // Show the pop-up
    }

    private void ClosePopup()
    {
        debugPopup.SetActive(false);                    // Hide the pop-up
    }
}