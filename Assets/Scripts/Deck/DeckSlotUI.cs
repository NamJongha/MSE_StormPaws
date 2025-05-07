using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;
using static UnityEngine.Rendering.GPUSort;

/// <summary>
/// A Slot of Deck List Script
/// </summary>
/// 
public class DeckSlotUI : MonoBehaviour
{
    public DeckDisplay deckDisplay;

    public TMP_Text deckIdText;
    public Image[] animalImages;
    public TMP_Text[] animalNames;
    public Button deleteButton;
    public Button selectButton;

    public void SetDeck(int index, DeckPreset deck, GameManager gameManager)
    {
        deckIdText.text = (index + 1).ToString();

        for (int i = 0; i < animalImages.Length; i++)
        {
            if (i < deck.decklist.Count && deck.decklist[i].card != null)
            {
                var cardData = deck.decklist[i].card;
                var sprite = gameManager.LoadAnimalSprite(cardData.name);

                animalNames[i].text = cardData.name;
                animalImages[i].sprite = sprite;
                animalImages[i].color = Color.white;
                animalImages[i].gameObject.SetActive(true);
            }
            else
            {
                animalNames[i].text = "";
                animalImages[i].sprite = null;
                animalImages[i].gameObject.SetActive(false);
            }
        }
    }
}
