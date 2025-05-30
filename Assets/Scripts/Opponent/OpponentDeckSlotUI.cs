using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpponentDeckSlotUI : MonoBehaviour
{
    public TMP_Text ownerNameText;
    public TMP_Text num;
    public Image[] animalImages;
    public TMP_Text[] animalNameTexts;
    public Button selectButton;

    private OpponentDeck selectedDeck;

    public void SetDeck(OpponentDeck deck, int index)
    {
        selectedDeck = deck;
        ownerNameText.text = deck.ownerName;
        num.text = (index + 1).ToString();

        var spriteLoader = GameManager.Instance.SpriteLoader;

        for (int i = 0; i < animalImages.Length; i++)
        {
            if (i < deck.decklist.Count)
            {
                var card = deck.decklist[i].card;
                animalImages[i].sprite = spriteLoader.Load(card.name);
                animalImages[i].gameObject.SetActive(true);
                animalNameTexts[i].text = card.name;
                animalNameTexts[i].gameObject.SetActive(true);
            }
            else
            {
                animalImages[i].gameObject.SetActive(false);
                animalNameTexts[i].gameObject.SetActive(false);
            }
        }

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnSelect);
    }

    private void OnSelect()
    {
        GameManager.Instance.DeckService.SetSelectedOpponentDeck(selectedDeck);
    }
}