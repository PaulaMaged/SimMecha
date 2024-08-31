using UnityEngine;
using TMPro;

public class TooltipManagerRight : MonoBehaviour
{
    public static TooltipManagerRight Instance;
    public GameObject tooltipObject;
    public TMP_Text tooltipText;
    private int offset = 430;
    void Awake()
    {
        Instance = this;
        HideTooltip();
    }

    public void ShowTooltip(string message, Vector2 position)
    {
        tooltipObject.SetActive(true);
        tooltipText.text = message;
        // Adjust the position to the right
        Vector2 rightOffset = new Vector2(tooltipObject.GetComponent<RectTransform>().rect.width / 2 + offset, 0);
        tooltipObject.transform.position = position + rightOffset ;
    }

    public void HideTooltip()
    {
        tooltipObject.SetActive(false);
    }
}
