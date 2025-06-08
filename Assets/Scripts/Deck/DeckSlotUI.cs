using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckSlotUI : MonoBehaviour
{
    public DeckDisplay deckDisplay;

    public TMP_Text deckIdText;
    public Image[] animalImages;
    public TMP_Text[] animalNames;

    public Button deleteButton;
    public Button selectButton;

    public void SetDeck(int index, DeckPreset deck)
    {
        deckIdText.text = (index + 1).ToString();

        var spriteLoader = GameManager.Instance.SpriteLoader;

        for (int i = 0; i < animalImages.Length; i++)
        {
            if (i < deck.decklist.Count && deck.decklist[i].card != null)
            {
                var cardData = deck.decklist[i].card;

                animalNames[i].text = LanguageTranslate.GetDisplayName(cardData.name);
                animalImages[i].sprite = spriteLoader.Load(cardData.name);
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