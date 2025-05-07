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
        deckIdText.text = (index).ToString();

        for (int i = 0; i < animalImages.Length; i++)
        {
            if (i < deck.decklist.Count)
            {
                var cardData = deck.decklist[i].card;
                if (cardData == null)
                {
                    continue;
                }

                var sprite = gameManager.LoadAnimalSprite(cardData.name);

                animalNames[i].text = cardData.name;
                animalImages[i].sprite = sprite;
                animalImages[i].color = Color.white;
                animalImages[i].gameObject.SetActive(true);
            }
        }
    }
}
