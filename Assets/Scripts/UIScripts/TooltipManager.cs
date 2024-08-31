using UnityEngine;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;
    public GameObject tooltipObject;
    public TMP_Text tooltipText;
    private int offset = -410;

    void Awake()
    {
        Instance = this;
        HideTooltip();
    }

    public void ShowTooltip(string message, Vector2 position)
    {
        tooltipObject.SetActive(true);
        tooltipText.text = message;

        // Adjust the position to the left
        Vector2 leftOffset = new Vector2(-(tooltipObject.GetComponent<RectTransform>().rect.width / 2 + offset), 0);
        tooltipObject.transform.position = position + leftOffset;
    }

    public void HideTooltip()
    {
        tooltipObject.SetActive(false);
    }
}
