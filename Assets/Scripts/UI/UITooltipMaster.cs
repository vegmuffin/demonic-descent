using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UITooltipMaster : MonoBehaviour
{
    public static UITooltipMaster instance;

    [SerializeField] private Color activeTooltipColor = default;
    [SerializeField] private Color inactiveTooltipColor = default;

    private TMP_Text tooltipName;
    private TMP_Text tooltipDescription;
    private Image tooltipImage;

    private void Awake()
    {
        instance = this;

        tooltipName = transform.Find("TooltipName").GetComponent<TMP_Text>();
        tooltipDescription = transform.Find("TooltipDescription").GetComponent<TMP_Text>();
        tooltipImage = transform.GetComponent<Image>();
    }
    
    public void ActivateTooltip(string name, string description)
    {
        tooltipName.text = name;
        tooltipDescription.text = description;
        tooltipImage.color = activeTooltipColor;
    }

    public void EndTooltip()
    {
        tooltipName.text = string.Empty;
        tooltipDescription.text = string.Empty;
        tooltipImage.color = inactiveTooltipColor;
    }
}
