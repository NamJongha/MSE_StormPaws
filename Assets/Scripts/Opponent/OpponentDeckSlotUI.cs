using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpponentDeckSlotUI : MonoBehaviour
{
    public TMP_Text ownerNameText;
    public Image[] animalImages;
    public Button selectButton;

    private string deckId;

    public void SetDeck(GameManager.OpponentDeck deck)
    {
        deckId = deck.id;
        ownerNameText.text = deck.ownerName;

        for (int i = 0; i < animalImages.Length; i++)
        {
            if (i < deck.decklist.Count)
            {
                var card = deck.decklist[i].card;
                animalImages[i].sprite = GameManager.Instance.LoadAnimalSprite(card.name);
                animalImages[i].gameObject.SetActive(true);
            }
            else
            {
                animalImages[i].gameObject.SetActive(false);
            }
        }

        selectButton.onClick.AddListener(() =>
        {
            GameManager.Instance.SetSelectedOpponentDeckId(deckId);
            Debug.Log($"º±≈√µ» µ¶ ID: {deckId}");
        });
    }
}
