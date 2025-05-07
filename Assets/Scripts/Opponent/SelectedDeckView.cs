using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SelectedDeckView : MonoBehaviour
{
    public GameManager gameManager;

    [System.Serializable]
    public class AnimalSlot
    {
        public Image icon;
        public TMP_Text nameText;
    }

    public AnimalSlot[] animalSlots;

    void Start()
    {
        string deckId = PlayerPrefs.GetString("SelectedOpponentDeckId", "");
        if (string.IsNullOrEmpty(deckId))
        {
            return;
        }

        StartCoroutine(FetchDeckById(deckId));
    }

    IEnumerator FetchDeckById(string deckId)
    {
        string url = $"{gameManager.baseUrl}/decks/{deckId}";
        UnityWebRequest req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", "Bearer " + gameManager.GetAuthToken());

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<GameManager.SelectedOpponentResponse>(req.downloadHandler.text);
            var deck = response.data;

            for (int i = 0; i < animalSlots.Length; i++)
            {
                if (i < deck.decklist.Count)
                {
                    var card = deck.decklist[i].card;
                    animalSlots[i].icon.sprite = gameManager.LoadAnimalSprite(card.name);
                    animalSlots[i].icon.color = Color.white;
                    animalSlots[i].nameText.text = card.name;
                }
                else
                {
                    animalSlots[i].icon.gameObject.SetActive(false);
                    animalSlots[i].nameText.text = "";
                }
            }
        }
    }
}