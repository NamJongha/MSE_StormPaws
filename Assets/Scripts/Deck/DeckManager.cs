using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[System.Serializable]
public class NewDeckRequest
{
    public string name;
    public List<DeckCardRequest> cards;
}

[System.Serializable]
public class DeckCardRequest
{
    public string cardId;
    public int quantity;
    public int pos;
}

public class DeckManager : MonoBehaviour
{
    public GameObject createDeckButton;
    public AnimalSlotUI[] previewSlots;
    public GameObject createDeckPanel;
    public GameObject animalButtonPrefab;
    public Transform animalButtonContainer;
    public Transform deckSlotContainer;
    public GameObject deckSlotPrefab;
    public DeckDisplay deckDisplay;
    public AudioSource click;

    private List<Card> allCards;
    private List<Card> selectedCards = new List<Card>();

    public void DisplayDeckList(List<DeckPreset> decks)
    {
        foreach (Transform child in deckSlotContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < decks.Count; i++)
        {
            var deck = decks[i];
            GameObject slotGO = Instantiate(deckSlotPrefab, deckSlotContainer);
            DeckSlotUI ui = slotGO.GetComponent<DeckSlotUI>();

            if (ui != null)
            {
                ui.SetDeck(i, deck);
                string deckId = deck.id;
                //ui.deleteButton.onClick.RemoveAllListeners();
                //ui.deleteButton.onClick.AddListener(() => DeleteDeck(deckId));

                if (deckDisplay != null)
                {
                    ui.selectButton.onClick.RemoveAllListeners();
                    ui.selectButton.onClick.AddListener(() => deckDisplay.DisplayDeck(deck.decklist));
                    ui.selectButton.onClick.AddListener(() => GameManager.Instance.DeckService.SetSelectedMyDeck(deck));
                }
            }
        }

        createDeckButton.SetActive(true);
    }

    public void MyDeckSelect(List<DeckPreset> decks)
    {
        foreach (Transform child in deckSlotContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < decks.Count; i++)
        {
            var deck = decks[i];

            GameObject slotGO = Instantiate(deckSlotPrefab, deckSlotContainer);
            DeckSlotUI ui = slotGO.GetComponent<DeckSlotUI>();

            if (ui != null)
            {
                ui.SetDeck(i, deck);

                if (deckDisplay != null)
                {
                    ui.selectButton.onClick.RemoveAllListeners();
                    ui.selectButton.onClick.AddListener(() => deckDisplay.DisplayDeck(deck.decklist));
                    ui.selectButton.onClick.AddListener(() => GameManager.Instance.DeckService.SetSelectedMyDeck(deck));
                }
            }
        }
    }

    public void InitCardList()
    {
        GameManager.Instance.DeckService.FetchAllCards(cards =>
        {
            allCards = cards;
            PopulateAnimalButtons();
        });
    }

    public void PopulateAnimalButtons()
    {
        if (allCards == null || allCards.Count == 0)
        {
            return;
        }

        foreach (Card card in allCards)
        {
            GameObject btn = Instantiate(animalButtonPrefab, animalButtonContainer);
            TMP_Text label = btn.transform.Find("AnimalName")?.GetComponent<TMP_Text>();

            if (label != null)
            {
                label.text = LanguageTranslate.GetDisplayName(card.name);
            }

            Image img = btn.transform.Find("AnimalImage")?.GetComponent<Image>();

            if (img != null)
            {
                Sprite sprite = GameManager.Instance.SpriteLoader.Load(card.name);
                img.sprite = sprite;
            }

            Button button = btn.GetComponent<Button>();

            if (button != null)
            {
                string animalId = card.name;
                button.onClick.AddListener(() => OnAnimalClicked(animalId));
            }

            AnimalHoverTooltip hover = btn.GetComponent<AnimalHoverTooltip>() ?? btn.AddComponent<AnimalHoverTooltip>();
            hover.SetCard(card);
        }
    }

    public void OnAnimalClicked(string animalId)
    {
        Card found = allCards.Find(c => animalId.Equals(c.name));

        if (found == null)
        {
            return;
        }

        if (selectedCards.Contains(found))
        {
            selectedCards.Remove(found);
        }
        else
        {
            if (selectedCards.Count >= 5)
            {
                return;
            }

            selectedCards.Add(found);
        }

        UpdateSelectedAnimalPreview();
    }

    public void UpdateSelectedAnimalPreview()
    {
        for (int i = 0; i < previewSlots.Length; i++)
        {
            if (i < selectedCards.Count)
            {
                previewSlots[i].nameText.text = LanguageTranslate.GetDisplayName(selectedCards[i].name);
                previewSlots[i].icon.sprite = GameManager.Instance.SpriteLoader.Load(selectedCards[i].name);
                previewSlots[i].icon.gameObject.SetActive(true);
            }
            else
            {
                previewSlots[i].nameText.text = "";
                previewSlots[i].icon.gameObject.SetActive(false);
            }
        }
    }

    public void confirmButton()
    {
        click.Play();

        if (selectedCards.Count != 5)
        {
            Debug.Log("Select 5 animals");
            return;
        }

        CreateDeck();
        selectedCards.Clear();
        UpdateSelectedAnimalPreview();
    }

    public void CreateDeck()
    {
        if (selectedCards.Count != 5)
        {
            Debug.Log("Select 5 cards");
            return;
        }

        NewDeckRequest req = new NewDeckRequest
        {
            name = "my_deck" + Random.Range(1, 10000),
            cards = new List<DeckCardRequest>()
        };

        for (int i = 0; i < selectedCards.Count; i++)
        {
            req.cards.Add(new DeckCardRequest
            {
                cardId = selectedCards[i].id,
                quantity = 5,
                pos = i + 1
            });
        }

        string json = JsonUtility.ToJson(req);
        StartCoroutine(PostDeckCoroutine(json));
    }

    private IEnumerator PostDeckCoroutine(string json)
    {
        UnityWebRequest request = new UnityWebRequest($"{GameManager.Instance.baseUrl}/user/me/decks", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + GameManager.Instance.GetAuthToken());

        yield return request.SendWebRequest();


        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Success");

            GameManager.Instance.DeckService.ClearDeckCache();

            GameManager.Instance.DeckService.FetchDeckPresets(decks =>
            {
                DisplayDeckList(decks);
            });

            createDeckPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Fail: " + request.error);
        }
    }

    //public void DeleteDeck(string deckId)
    //{
    //    GameManager.Instance.DeckService.DeleteDeck(deckId, DisplayDeckList);
    //}
}