using TMPro;
using UnityEngine.EventSystems;
using UnityEngine;

public class AnimalHoverTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private GameManager.Card card;
    public GameObject tooltipPanel;
    public TMP_Text tooltipText;

    public void Start()
    {
        tooltipPanel.SetActive(false);
    }

    public void SetCard(GameManager.Card c)
    {
        card = c;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltipPanel.SetActive(true);
        ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipPanel.SetActive(false);
    }

    private void ShowTooltip()
    {
        if (card == null)
        {
            tooltipText.text = "No Info";
            Debug.LogWarning("툴팁 카드 없음");
            return;
        }

        tooltipText.text =
            $"Name: {card.name}\n" +
            $"HP: {card.health}\n" +
            $"ATK: {card.attackPower}\n" +
            $"Type: {card.cardType}";
    }
}