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
        public string result;
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
        public TMP_Text numberText;
        public TMP_Text opponentText;
        public TMP_Text weatherText;
        public TMP_Text resultText;
        public AnimalSlotUI[] myDeckSlots;
    }


    [System.Serializable]
    public class NewDeckRequest
    {
        public string[] animals;
    }

    public DeckSlotUI[] deckSlotUIs;
    public GameObject createDeckButton;

    public TextMeshProUGUI nicknameText;
    public TextMeshProUGUI emailText;

    private PersonalInfo cachedInfo = null;
    private List<DeckPreset> cachedDecks = null;
    private List<BattleRecord> cachedBattles = null;

    private List<string> selectedAnimalIds = new List<string>();

    public AnimalSlotUI[] previewSlots;

    public GameObject createDeckPanel;

    private string baseUrl = "http://localhost:8080/api";

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

    public GameObject battleRecordPrefab;
    public Transform battleRecordContainer;

    private void DisplayBattleRecords(List<BattleRecord> records)
    {
        foreach (Transform child in battleRecordContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < records.Count; i++)
        {
            var record = records[i];
            GameObject item = Instantiate(battleRecordPrefab, battleRecordContainer);
            BattleRecordUI ui = item.GetComponent<BattleRecordUI>();

            ui.numberText.text = (i + 1).ToString();
            ui.opponentText.text = record.opponent;
            ui.weatherText.text = record.weather;
            ui.resultText.text = record.result;

            var myDeck = record.myDeck.Split(',');
            for (int j = 0; j < ui.myDeckSlots.Length; j++)
            {
                if (j < myDeck.Length)
                {
                    string animalId = myDeck[j];
                    ui.myDeckSlots[j].icon.sprite = LoadAnimalSprite(animalId);
                    ui.myDeckSlots[j].icon.gameObject.SetActive(true);
                }
                else
                {
                    ui.myDeckSlots[j].icon.gameObject.SetActive(false);
                }
            }
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
                        slot.animalSlots[j].icon.color = Color.white;
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
                slot.deckIdText.text = "Empty Deck Slot";

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

    public void OnAnimalClicked(string animalId)
    {
        if (selectedAnimalIds.Contains(animalId))
        {
            selectedAnimalIds.Remove(animalId);
        }
        else
        {
            if (selectedAnimalIds.Count >= 5)
            {
                Debug.Log("최대 5마리까지 선택 가능");
                return;
            }
            selectedAnimalIds.Add(animalId);
        }

        UpdateSelectedAnimalPreview();
    }

    public void OnConfirmCreateDeckButton()
    {
        if (selectedAnimalIds.Count != 5)
        {
            Debug.Log("동물 5마리를 정확히 선택");
            return;
        }

        CreateDeck(selectedAnimalIds.ToArray());
        selectedAnimalIds.Clear();
        UpdateSelectedAnimalPreview();
    }

    private void UpdateSelectedAnimalPreview()
    {
        for (int i = 0; i < previewSlots.Length; i++)
        {
            if (i < selectedAnimalIds.Count)
            {
                string animalId = selectedAnimalIds[i];
                previewSlots[i].nameText.text = animalId;
                previewSlots[i].icon.sprite = LoadAnimalSprite(animalId);
                previewSlots[i].icon.gameObject.SetActive(true);
            }
            else
            {
                previewSlots[i].nameText.text = "";
                previewSlots[i].icon.gameObject.SetActive(false);
            }
        }
    }

    public void CreateDeck(string[] animalIds)
    {
        if (animalIds.Length != 5)
        {
            Debug.LogWarning("동물은 반드시 5마리");
            return;
        }

        NewDeckRequest reqData = new NewDeckRequest { animals = animalIds };
        string json = JsonUtility.ToJson(reqData);
        StartCoroutine(PostDeckCoroutine(json));
    }

    private IEnumerator PostDeckCoroutine(string json)
    {
        UnityWebRequest req = new UnityWebRequest($"{baseUrl}/deck", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
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
        else
        {
            Debug.LogError($"덱 생성 실패: {req.error}");
        }
    }

    private Sprite LoadAnimalSprite(string animalId)
    {
        return Resources.Load<Sprite>($"Animals/{animalId}");
    }

    public void OnClickCreateDeckButton()
    {
        createDeckPanel.SetActive(true);
        PopulateAnimalButtons(); // 동물 목록 UI 뿌려줌
    }

    public GameObject animalButtonPrefab;
    public Transform animalButtonContainer;

    private List<string> allAnimalIds = new List<string> {"horse", "lion", "tiger", "polarbear", "ostrich", "octopus", "geko", "elephant", "hamster", "monkey", "whale", "sloth", "giraffe", "frog"};

    public void PopulateAnimalButtons()
    {
        foreach (Transform child in animalButtonContainer)
        {
            Destroy(child.gameObject); // 초기화
        }

        foreach (string animalId in allAnimalIds)
        {
            GameObject btn = Instantiate(animalButtonPrefab, animalButtonContainer);

            // 텍스트 설정
            TMP_Text label = btn.transform.Find("AnimalName").GetComponent<TMP_Text>();
            if (label != null)
                label.text = animalId;

            // 이미지 설정
            Image img = btn.transform.Find("AnimalImage").GetComponent<Image>();
            if (img != null)
            {
                img.sprite = LoadAnimalSprite(animalId);
            }

            // 클릭 이벤트 설정
            Button button = btn.GetComponent<Button>();
            if (button != null)
            {
                string capturedId = animalId; // 클로저 문제 방지
                button.onClick.AddListener(() => OnAnimalClicked(capturedId));
            }

            string path = $"Animals/{animalId}";
            var sprite = Resources.Load<Sprite>(path);
            if (sprite == null)
                Debug.LogError($"[Load Fail] 경로: {path} - 스프라이트 못 찾음");
            else
                Debug.Log($"[Load OK] {sprite.name}");
        }
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