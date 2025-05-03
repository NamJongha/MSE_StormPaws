using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

/// <summary>
/// A Slot of Deck List Script
/// </summary>
/// 
public class DeckSlotUI : MonoBehaviour
{
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
            if (i < deck.decklist.Count)
            {
                var card = deck.decklist[i].card;
                animalNames[i].text = card.name;
                animalImages[i].sprite = gameManager.LoadAnimalSprite(card.name);
                animalImages[i].color = Color.white;
                animalImages[i].gameObject.SetActive(true);
            }
            else
            {
                animalNames[i].text = "";
                animalImages[i].gameObject.SetActive(false);
            }
        }
    }
}
