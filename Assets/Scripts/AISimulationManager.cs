using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class AISimulationManager : MonoBehaviour
{
    [Header("UI Components")]
    public Transform myDeckSlotParent;
    public Transform aiDeckSlotParent;
    public GameObject myDeckSlotPrefab;
    public GameObject aiDeckSlotPrefab;
    public TMP_Text weatherProbabilityText;
    public Button simulateButton;
    public TMP_Dropdown weatherDropdown;

    private List<DeckPreset> myDecks = new();
    private List<OpponentDeck> aiDecks = new();

    private DeckPreset selectedMyDeck;
    private OpponentDeck selectedAIDeck;
    private string selectedWeather;

    private GameObject currentMyDeckHighlight;
    private GameObject currentAIDeckHighlight;

    void Start()
    {
        SetupWeatherDropdown();

        GameManager.Instance.DeckService.FetchDeckPresets((list) =>
        {
            myDecks = list;
            PopulateMyDeckList();
        });

        StartCoroutine(FetchAIDecksFromServer());
        simulateButton.onClick.AddListener(RunSimulation);
    }


    IEnumerator FetchAIDecksFromServer()
    {
        string url = GameManager.Instance.baseUrl + "/decks/random";
        UnityWebRequest req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", "Bearer " + GameManager.Instance.GetAuthToken());
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<OpponentDeckListResponse>(req.downloadHandler.text);
            aiDecks = response.data.items;
            PopulateAIDeckList();
        }
        else
        {
            Debug.LogError("AI Deck Load Fail: " + req.error);
        }
    }

    void PopulateMyDeckList()
    {
        foreach (Transform child in myDeckSlotParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < myDecks.Count; i++)
        {
            var slot = Instantiate(myDeckSlotPrefab, myDeckSlotParent);
            var ui = slot.GetComponent<DeckSlotUI>();

            ui.SetDeck(i, myDecks[i]);
            int captured = i;

            ui.selectButton.onClick.RemoveAllListeners();
            ui.selectButton.onClick.AddListener(() => {
                selectedMyDeck = myDecks[captured];
                PlayerPrefs.SetString("SelectedMyDeckId", selectedMyDeck.id);//추가

                if (currentMyDeckHighlight != null)
                {
                    var prevPanel = currentMyDeckHighlight.transform.Find("Panel");
                    if (prevPanel != null)
                    {
                        var img = prevPanel.GetComponent<Image>();
                        if (img != null)
                        {
                            img.color = Color.white;
                        }
                    }
                }

                currentMyDeckHighlight = slot;

                var newPanel = currentMyDeckHighlight.transform.Find("Panel");
                if (newPanel != null)
                {
                    var img = newPanel.GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = Color.yellow;
                    }
                }
            });
        }
    }

    void PopulateAIDeckList()
    {
        foreach (Transform child in aiDeckSlotParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < aiDecks.Count; i++)
        {
            var slot = Instantiate(aiDeckSlotPrefab, aiDeckSlotParent);
            var ui = slot.GetComponent<OpponentDeckSlotUI>();
            ui.SetDeck(aiDecks[i], i);
            int captured = i;

            ui.selectButton.onClick.RemoveAllListeners();
            ui.selectButton.onClick.AddListener(() => {
                selectedAIDeck = aiDecks[captured];
                PlayerPrefs.SetString("SelectedOpponentDeckId", selectedAIDeck.id); //추가
                PlayerPrefs.SetString("SelectedOpponentUserId", selectedAIDeck.user.id);

                if (currentAIDeckHighlight != null)
                {
                    var prevPanel = currentAIDeckHighlight.transform.Find("Panel");
                    if (prevPanel != null)
                    {
                        var img = prevPanel.GetComponent<Image>();
                        if (img != null)
                        {
                            img.color = Color.white;
                        }
                    }
                }

                currentAIDeckHighlight = slot;

                var newPanel = currentAIDeckHighlight.transform.Find("Panel");
                if (newPanel != null)
                {
                    var img = newPanel.GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = Color.red;
                    }
                }
            });
        }
    }


    public void RunSimulation()
    {
        if (selectedMyDeck == null || selectedAIDeck == null || string.IsNullOrEmpty(selectedWeather))
        {
            Debug.LogWarning("No Select");
            return;
        }

        PlayerPrefs.SetInt("IsAISimulation", 1);
        PlayerPrefs.SetString("SimulatedWeather", selectedWeather);
        PlayerPrefs.Save();
        GameManager.Instance.DeckService.SetSelectedMyDeck(selectedMyDeck);
        GameManager.Instance.DeckService.SetSelectedOpponentDeck(selectedAIDeck);

        SceneManager.LoadScene("AIMode");
    }

    public void OnWeatherDropdownChanged(int index)
    {
        if (index < 0 || index >= weatherDropdown.options.Count)
        {
            return;
        }

        selectedWeather = weatherDropdown.options[index].text.ToUpper();
    }

    private void SetupWeatherDropdown()
    {
        weatherDropdown.ClearOptions();

        List<string> weatherOptions = new()
    {
        "CLEAR", "CLOUDS", "RAIN", "SNOW", "MIST", "FOG", "THUNDERSTORM", "SAND", "GUST", "TORNADO"
    };

        weatherDropdown.AddOptions(weatherOptions);

        weatherDropdown.value = 0;
        weatherDropdown.RefreshShownValue();
        selectedWeather = weatherOptions[0];

        weatherDropdown.onValueChanged.RemoveAllListeners();
        weatherDropdown.onValueChanged.AddListener(OnWeatherDropdownChanged);
    }

    [System.Serializable]
    public class WeatherData
    {
        public Dictionary<string, int> weatherProbabilities;
        public Dictionary<string, string> cityWeathers;
    }
}