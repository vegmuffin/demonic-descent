using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UITooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string tooltipName = default;
    [SerializeField] [TextArea] private string tooltipText = default;

    public void OnPointerEnter(PointerEventData eventData)
    {
        UITooltipMaster.instance.ActivateTooltip(tooltipName, tooltipText);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UITooltipMaster.instance.EndTooltip();
    }

}
