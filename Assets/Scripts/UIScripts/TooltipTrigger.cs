using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string tooltipMessage;
    public bool useRightTooltip = false; // Add a flag to choose the tooltip manager

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (useRightTooltip)
        {
            TooltipManagerRight.Instance.ShowTooltip(tooltipMessage, eventData.position);
        }
        else
        {
            TooltipManager.Instance.ShowTooltip(tooltipMessage, eventData.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (useRightTooltip)
        {
            TooltipManagerRight.Instance.HideTooltip();
        }
        else
        {
            TooltipManager.Instance.HideTooltip();
        }
    }
}
