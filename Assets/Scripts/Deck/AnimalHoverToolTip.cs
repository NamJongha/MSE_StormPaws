using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnimalHoverTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string animalName;
    public GameObject tooltipPanel;
    public TMP_Text tooltipText;

    public List<GameManager.Card> allCards;

    private void Start()
    {
        tooltipPanel.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltipPanel.SetActive(true);
        ShowTooltip(animalName);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipPanel.SetActive(false);
    }

    private void ShowTooltip(string name)
    {
        GameManager.Card card = allCards.Find(c => c.name == name);

        if (card != null)
        {
            tooltipText.text =
                $"Name: {card.name}\n" +
                $"Health: {card.health}\n" +
                $"Attack: {card.attackPower}\n" +
                $"Speed: {card.attackSpeed}\n" +
                $"Type: {card.cardType}";
        }
        else
        {
            tooltipText.text = "No Info";
        }
    }
}
