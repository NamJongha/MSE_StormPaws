using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameManager;

public class OpponentDeckSlotUI : MonoBehaviour
{
    public GameManager gameManager;

    public TMP_Text ownerNameText;
    public Image[] animalImages;
    public Button selectButton;
    public TMP_Text num;

    public TMP_Text[] animalNameTexts;

    private string deckId;
    private GameManager.OpponentDeck selectedDeck;

    public void SetDeck(GameManager.OpponentDeck deck, int index)
    {
        selectedDeck = deck;
        deckId = deck.id;
        ownerNameText.text = $"{deck.ownerName}";
        num.text = $"{index + 1}";

        for (int i = 0; i < animalImages.Length; i++)
        {
            if (i < deck.decklist.Count)
            {
                var card = deck.decklist[i].card;
                animalImages[i].sprite = gameManager.LoadAnimalSprite(card.name);
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
        selectButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MyDeckSelect");
        });
    }
}
