using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;

/// <summary>
/// ScrollView Opponent List Script
/// </summary>

public class OpponentSelect : MonoBehaviour
{
    public GameManager gameManager;
    public OpponentDeckUI selectedOpponentUI;

    [System.Serializable]
    public class OpponentDeckUI
    {
        public TMP_Text indexText;
        public TMP_Text ownerNameText;
        public OpponentAnimalUI[] animalSlots;
        public Button selectButton;
    }

    [System.Serializable]
    public class OpponentAnimalUI
    {
        public Image icon;
        public TMP_Text nameText;
    }

    [System.Serializable]
    public class  SelectedOpponentRequest
    {
        public string deckId;
    }

    public GameObject opponentDeckPrefab;
    public Transform opponentDeckContainer;

    // Show Opponent Deck
    public void ShowSelectedOpponentDeck()
    {
        gameManager.FetchSelectedOpponentDeck((deck) =>
        {
            selectedOpponentUI.ownerNameText.text = deck.ownerName;

            for (int i = 0; i < selectedOpponentUI.animalSlots.Length; i++)
            {
                if (i < deck.decklist.Count)
                {
                    var card = deck.decklist[i].card;

                    selectedOpponentUI.animalSlots[i].nameText.text = card.name;
                    selectedOpponentUI.animalSlots[i].icon.sprite = gameManager.LoadAnimalSprite(card.name);
                    selectedOpponentUI.animalSlots[i].icon.color = Color.white;
                    selectedOpponentUI.animalSlots[i].icon.gameObject.SetActive(true);
                }
                else
                {
                    selectedOpponentUI.animalSlots[i].icon.gameObject.SetActive(false);
                    selectedOpponentUI.animalSlots[i].nameText.text = "";
                }
            }
        });
    }

    public void DisplayOtherDecks(List<GameManager.OpponentDeck> decks)
    {
        foreach (Transform child in opponentDeckContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < decks.Count; i++)
        {
            var deck = decks[i];
            GameObject item = Instantiate(opponentDeckPrefab, opponentDeckContainer);
            OpponentDeckUIBehaviour behaviour = item.GetComponent<OpponentDeckUIBehaviour>();

            if (behaviour == null)
            {
                Debug.LogError("No Component");
                continue;
            }

            OpponentDeckUI ui = behaviour.ui;

            ui.indexText.text = (i + 1).ToString();
            ui.ownerNameText.text = deck.ownerName;

            for (int j = 0; j < ui.animalSlots.Length; j++)
            {
                if (j < deck.decklist.Count)
                {
                    var card = deck.decklist[j].card;
                    ui.animalSlots[j].nameText.text = card.name;
                    ui.animalSlots[j].icon.sprite = gameManager.LoadAnimalSprite(card.name);
                    ui.animalSlots[j].icon.color = Color.white;
                    ui.animalSlots[j].icon.gameObject.SetActive(true);
                }
                else
                {
                    ui.animalSlots[j].icon.gameObject.SetActive(false);
                    ui.animalSlots[j].nameText.text = "";
                }
            }

            string deckId = deck.id;
            ui.selectButton.onClick.RemoveAllListeners();
            ui.selectButton.onClick.AddListener(() => OnSelectOpponentButtonClicked(deckId));
        }
    }

    public void OnSelectOpponentButtonClicked(string opponentDeckId)
    {
        StartCoroutine(SendSelectedOpponent(opponentDeckId));
    }

    private IEnumerator SendSelectedOpponent(string deckId)
    {
        string url = $"{gameManager.baseUrl}/battle/select-opponent";
        SelectedOpponentRequest payload = new SelectedOpponentRequest { deckId = deckId };
        string json = JsonUtility.ToJson(payload);

        UnityWebRequest req = new UnityWebRequest(url, "POST");
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
