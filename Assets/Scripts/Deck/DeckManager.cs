using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static GameManager;
using static UnityEngine.Rendering.GPUSort;

/// <summary>
/// Deck Creation & Delete Manager Script
/// </summary>

public class DeckManager : MonoBehaviour
{
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

    [System.Serializable]
    public class AnimalSlotUI
    {
        public Image icon;
        public TMP_Text nameText;
    }

    [System.Serializable]
    public class SelectedDeckRequest
    {
        public string deckId;
    }

    public GameObject createDeckButton;
    public AnimalSlotUI[] previewSlots;
    public GameObject createDeckPanel;

    public GameObject animalButtonPrefab;
    public Transform animalButtonContainer;

    public Transform deckSlotContainer;
    public GameObject deckSlotPrefab;

    public GameManager gameManager;
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

        Debug.Log("덱 개수: " + decks.Count);

        for (int i = 0; i < decks.Count; i++)
        {
            var deck = decks[i];

            // Create Prefab Instance
            GameObject slotGO = Instantiate(deckSlotPrefab, deckSlotContainer);
            Debug.Log($"슬롯 생성됨: {slotGO.name}");

            // DeckSlotUI script
            DeckSlotUI ui = slotGO.GetComponent<DeckSlotUI>();

            if (ui != null)
            {
                // 슬롯 내용 설정
                ui.SetDeck(i, deck, gameManager);

                // 삭제 버튼 연결
                string deckId = deck.id;
                ui.deleteButton.onClick.RemoveAllListeners();
                ui.deleteButton.onClick.AddListener(() => DeleteDeck(deckId));

                if (deckDisplay != null)
                {
                    ui.selectButton.onClick.RemoveAllListeners();
                    ui.selectButton.onClick.AddListener(() => deckDisplay.DisplayDeck(deck.decklist));
                }
            }
            else
            {
                Debug.LogError("No DeckSlotUI Component");
            }
        }

        createDeckButton.SetActive(true);
    }

    public void MyDeckSelect(List<DeckPreset> decks)
    {
        foreach (Transform child in deckSlotContainer)
        {
            Destroy(child.gameObject);
        }

        Debug.Log("덱 개수: " + decks.Count);

        for (int i = 0; i < decks.Count; i++)
        {
            var deck = decks[i];

            // Create Prefab Instance
            GameObject slotGO = Instantiate(deckSlotPrefab, deckSlotContainer);
            Debug.Log($"슬롯 생성됨: {slotGO.name}");

            // DeckSlotUI script
            DeckSlotUI ui = slotGO.GetComponent<DeckSlotUI>();

            if (ui != null)
            {
                // 슬롯 내용 설정
                ui.SetDeck(i, deck, gameManager);

                if (deckDisplay != null)
                {
                    ui.selectButton.onClick.RemoveAllListeners();
                    ui.selectButton.onClick.AddListener(() => deckDisplay.DisplayDeck(deck.decklist));
                }
            }
            else
            {
                Debug.LogError("No DeckSlotUI Component");
            }
        }
    }


    // Deck Delete
    public void DeleteDeck(string deckId)
    {
        StartCoroutine(DeleteDeckCoroutine(deckId));
    }

    private IEnumerator DeleteDeckCoroutine(string deckId)
    {
        UnityWebRequest req = UnityWebRequest.Delete($"{gameManager.baseUrl}/deck/{deckId}");
        req.SetRequestHeader("Authorization", "Bearer " + gameManager.GetAuthToken());

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            gameManager.FetchDeckPresets(DisplayDeckList);
        }
        else
        {
            Debug.LogError("Fail: " + req.error);
        }
    }

    public void InitCardList()
    {
        gameManager.FetchAllCards((cards) =>
        {
            allCards = cards;
            PopulateAnimalButtons();
        });
    }

    public void PopulateAnimalButtons()
    {
        if (allCards == null)
        {
            Debug.LogError("allCards 자체가 null입니다. InitCardList() 안 불렸거나, 서버 응답 실패");
            return;
        }

        if (allCards.Count == 0)
        {
            Debug.LogWarning("allCards.Count == 0. 서버에 카드가 없거나 파싱에 실패했습니다");
            return;
        }

        Debug.Log($"카드 수: {allCards.Count}개");

        foreach (Card card in allCards)
        {
            if (card == null)
            {
                Debug.LogWarning("Card is null");
                continue;
            }

            if (string.IsNullOrEmpty(card.name))
            {
                Debug.LogWarning("Card name is null or empty");
                continue;
            }

            GameObject btn = Instantiate(animalButtonPrefab, animalButtonContainer);

            // Text
            TMP_Text label = btn.transform.Find("AnimalName")?.GetComponent<TMP_Text>();
            if (label != null)
            {
                label.text = card.name;
            }
            else
            {
                Debug.LogWarning("Label (AnimalName) not found in prefab");
            }

            // Image
            Image img = btn.transform.Find("AnimalImage")?.GetComponent<Image>();
            if (img != null)
            {
                Sprite sprite = gameManager.LoadAnimalSprite(card.name);
                if (sprite == null)
                {
                    Debug.LogWarning($"Sprite not found for card: {card.name}");
                }
                img.sprite = sprite;
            }
            else
            {
                Debug.LogWarning("Image (AnimalImage) not found in prefab");
            }

            // Click
            Button button = btn.GetComponent<Button>();
            if (button != null)
            {
                string animalId = card.name;
                button.onClick.AddListener(() => OnAnimalClicked(animalId));
            }
            else
            {
                Debug.LogWarning("Button component not found on animalButtonPrefab");
            }

            Debug.Log($"[버튼 생성] card.name: '{card.name}'");

            // Tool tip
            AnimalHoverTooltip hover = btn.GetComponent<AnimalHoverTooltip>();
            if (hover == null) hover = btn.AddComponent<AnimalHoverTooltip>();

            hover.SetCard(card);
        }

    }

    // Selecting Animal for Deck Creation
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
                Debug.Log("Max: 5 Animals");
                return;
            }

            selectedCards.Add(found);
        }

        UpdateSelectedAnimalPreview();
    }

    // Preview Panel for Deck Creation
    public void UpdateSelectedAnimalPreview()
    {
        for (int i = 0; i < previewSlots.Length; i++)
        {
            if (i < selectedCards.Count)
            {
                previewSlots[i].nameText.text = selectedCards[i].name;
                previewSlots[i].icon.sprite = gameManager.LoadAnimalSprite(selectedCards[i].name);
                previewSlots[i].icon.gameObject.SetActive(true);
            }
            else
            {
                previewSlots[i].nameText.text = "";
                previewSlots[i].icon.gameObject.SetActive(false);
            }
        }
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
        UnityWebRequest request = new UnityWebRequest($"{gameManager.baseUrl}/user/me/decks", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + gameManager.GetAuthToken());

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Success");
            gameManager.FetchDeckPresets(DisplayDeckList);
        }
        else
        {
            Debug.LogError("Fail: " + request.error);
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

        createDeckPanel.SetActive(false);

        CreateDeck();
        selectedCards.Clear();
        UpdateSelectedAnimalPreview();
    }

    public void OnSelectMyDeckButtonClicked(string deckId)
    {
        StartCoroutine(SendSelectedMyDeck(deckId));
    }

    private IEnumerator SendSelectedMyDeck(string deckId)
    {
        SelectedDeckRequest payload = new SelectedDeckRequest { deckId = deckId };
        string json = JsonUtility.ToJson(payload);

        UnityWebRequest req = new UnityWebRequest($"{gameManager.baseUrl}/battle/select-my-deck", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + gameManager.GetAuthToken());

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Success");
        }
        else
        {
            Debug.LogError("Fail: " + req.error);
        }
    }

}
