using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;
using System.Collections;
using static MyPage;
using TMPro;
using UnityEngine.UI;

public class MyPage : MonoBehaviour
{
    [System.Serializable]
    public class PersonalInfo
    {
        public string id;
        public string email;
        public string nickname;
    }

    [System.Serializable]
    public class DeckPreset
    {
        public string id;
        public string[] animals;
    }

    [System.Serializable]
    public class BattleRecord
    {
        public string weather;
        public string opponent;
        public string myDeck;
        public string opponentDeck;
        public string time;
    }

    [System.Serializable]
    public class AnimalSlotUI
    {
        public Image icon;
        public TMP_Text nameText;
    }

    [System.Serializable]
    public class DeckSlotUI
    {
        public TMP_Text deckIdText;
        public AnimalSlotUI[] animalSlots;
        public Button deleteButton;
    }

    [System.Serializable]
    public class BattleRecordUI
    {
        public GameObject panel;
        public TMP_Text weatherText;
        public TMP_Text opponentText;
        public TMP_Text myDeckText;
        public TMP_Text opponentDeckText;
        public TMP_Text timeText;
    }

    public BattleRecordUI[] battleRecordUIs;

    public DeckSlotUI[] deckSlotUIs;
    public GameObject createDeckButton;

    public TextMeshProUGUI nicknameText;
    public TextMeshProUGUI emailText;

    private PersonalInfo cachedInfo = null;
    private List<DeckPreset> cachedDecks = null;
    private List<BattleRecord> cachedBattles = null;

    private string baseUrl = "https://your-api.com/api";

    private string GetAuthToken()
    {
        return PlayerPrefs.GetString("jwt");
    }

    public void FetchPersonalInfo(Action<PersonalInfo> callback)
    {
        StartCoroutine(GetRequest($"{baseUrl}/user", (json) => {
            PersonalInfo data = JsonUtility.FromJson<PersonalInfo>(json);
            callback?.Invoke(data);
        }));
    }

    public void FetchDeckPresets(Action<List<DeckPreset>> callback)
    {
        StartCoroutine(GetRequest($"{baseUrl}/deck", (json) => {
            DeckPresetListWrapper wrapper = JsonUtility.FromJson<DeckPresetListWrapper>(json);
            callback?.Invoke(wrapper.data);
        }));
    }

    public void FetchBattleRecords(Action<List<BattleRecord>> callback)
    {
        StartCoroutine(GetRequest($"{baseUrl}/battle/logs", (json) => {
            BattleRecordListWrapper wrapper = JsonUtility.FromJson<BattleRecordListWrapper>(json);
            callback?.Invoke(wrapper.data);
        }));
    }

    private IEnumerator GetRequest(string url, Action<string> onSuccess)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "Bearer " + GetAuthToken());

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError($"API Error: {request.error}");
        }
    }

    // for buttons
    public void PersonalInfoButton()
    {
        if (cachedInfo != null)
        {
            nicknameText.text = cachedInfo.nickname;
            emailText.text = cachedInfo.email;

            return;
        }

        FetchPersonalInfo((info) =>
        {
            cachedInfo = info;

            nicknameText.text = info.nickname;
            emailText.text = info.email;
        });
    }

    public void DeckPresetsButton()
    {
        if (cachedDecks != null)
        {
            DisplayDeckList(cachedDecks);

            return;
        }

        FetchDeckPresets((deckList) =>
        {
            cachedDecks = deckList;

            DisplayDeckList(deckList);
        });
    }

    public void BattleRecordsButton()
    {
        if (cachedBattles != null)
        {
            DisplayBattleRecords(cachedBattles);

            return;
        }

        FetchBattleRecords((battleList) =>
        {
            cachedBattles = battleList;

            DisplayBattleRecords(battleList);
        });
    }
    private void DisplayBattleRecords(List<BattleRecord> records)
    {
        for (int i = 0; i < records.Count && i < battleRecordUIs.Length; i++)
        {
            var record = records[i];
            var ui = battleRecordUIs[i];

            ui.weatherText.text = record.weather;
            ui.opponentText.text = record.opponent;
            ui.myDeckText.text = record.myDeck;
            ui.opponentDeckText.text = record.opponentDeck;
            ui.timeText.text = record.time;
            ui.panel.SetActive(true);
        }

        for (int i = records.Count; i < battleRecordUIs.Length; i++)
        {
            battleRecordUIs[i].panel.SetActive(false);
        }
    }

    private void DisplayDeckList(List<DeckPreset> decks)
    {
        for (int i = 0; i < deckSlotUIs.Length; i++)
        {
            var slot = deckSlotUIs[i];

            if (i < decks.Count)
            {
                var deck = decks[i];
                slot.deckIdText.text = deck.id;

                for (int j = 0; j < slot.animalSlots.Length; j++)
                {
                    if (j < deck.animals.Length)
                    {
                        string animalId = deck.animals[j];
                        slot.animalSlots[j].nameText.text = animalId;
                        slot.animalSlots[j].icon.sprite = LoadAnimalSprite(animalId);
                        slot.animalSlots[j].icon.gameObject.SetActive(true);
                    }
                    else
                    {
                        slot.animalSlots[j].icon.gameObject.SetActive(false);
                        slot.animalSlots[j].nameText.text = "";
                    }
                }

                slot.deleteButton.gameObject.SetActive(true);
                string targetDeckId = deck.id;
                slot.deleteButton.onClick.RemoveAllListeners();
                slot.deleteButton.onClick.AddListener(() => DeleteDeck(targetDeckId));
            }
            else
            {
                slot.deckIdText.text = "ºó µ¦ ½½·Ô";

                foreach (var animalSlot in slot.animalSlots)
                {
                    animalSlot.icon.gameObject.SetActive(false);
                    animalSlot.nameText.text = "";
                }

                slot.deleteButton.gameObject.SetActive(false);
            }
        }

        createDeckButton.SetActive(decks.Count < 5);
    }

    public void DeleteDeck(string deckId)
    {
        StartCoroutine(DeleteDeckCoroutine(deckId));
    }

    private IEnumerator DeleteDeckCoroutine(string deckId)
    {
        UnityWebRequest req = UnityWebRequest.Delete($"{baseUrl}/deck/{deckId}");
        req.SetRequestHeader("Authorization", "Bearer " + GetAuthToken());

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            FetchDeckPresets((deckList) =>
            {
                cachedDecks = deckList;
                DisplayDeckList(deckList);
            });
        }
    }

    private Sprite LoadAnimalSprite(string animalId)
    {
        return Resources.Load<Sprite>($"Animals/{animalId}");
    }
}

[System.Serializable]
public class DeckPresetListWrapper
{
    public List<DeckPreset> data;
}

[System.Serializable]
public class BattleRecordListWrapper
{
    public List<BattleRecord> data;
}