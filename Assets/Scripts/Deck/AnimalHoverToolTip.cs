using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnimalHoverTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Card card;

    [Header("UI References")]
    public GameObject tooltipPanel;
    public TMP_Text tooltipText;

    private void Start()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    public void SetCard(Card cardData)
    {
        card = cardData;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPanel == null || tooltipText == null)
        {
            return;
        }

        tooltipPanel.SetActive(true);
        ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    private void ShowTooltip()
    {
        if (card == null)
        {
            tooltipText.text = "No Info Available";
            return;
        }

        tooltipText.text = GetTooltipText(card);
    }

    private string GetTooltipText(Card c)
    {
        return
            $"Name: {LanguageTranslate.GetDisplayName(c.name)}\n" +
            $"HP: {c.health}\n" +
            $"Attack: {c.attackPower}\n" +
            $"Type: {LanguageTranslate.GetCardType(c.cardType)}";
    }
}
