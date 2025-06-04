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
        StartCoroutine(FetchWeatherProbability());
        GameManager.Instance.DeckService.FetchDeckPresets((list) =>
        {
            myDecks = list;
            PopulateMyDeckList();
        });
        StartCoroutine(FetchAIDecksFromServer());
        simulateButton.onClick.AddListener(RunSimulation);
    }

    IEnumerator FetchWeatherProbability()
    {
        string url = GameManager.Instance.baseUrl + "/weather/cities";
        UnityWebRequest req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", "Bearer " + GameManager.Instance.GetAuthToken());

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var data = JsonUtility.FromJson<WeatherData>(req.downloadHandler.text);
            var builder = new System.Text.StringBuilder();

            List<string> weatherOptions = new(data.weatherProbabilities.Keys);

            builder.AppendLine("🌤 Today's Weather Probability:");
            foreach (var entry in data.weatherProbabilities)
            {
                builder.AppendLine($"- {entry.Key}: {entry.Value}%");
            }

            weatherProbabilityText.text = builder.ToString();

            weatherDropdown.ClearOptions();
            weatherDropdown.AddOptions(weatherOptions);

            weatherDropdown.onValueChanged.AddListener(i =>
            {
                selectedWeather = weatherOptions[i];
            });

            if (weatherOptions.Count > 0)
            {
                weatherDropdown.value = 0;
                weatherDropdown.RefreshShownValue();
                selectedWeather = weatherOptions[0];
            }
        }
        else
        {
            weatherProbabilityText.text = "Weather Info Load Fail";
        }
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
            var ui = slot.GetComponent<OpponentDeckSlotUI>();

            var fakeOpponentDeck = new OpponentDeck
            {
                id = myDecks[i].id,
                decklist = myDecks[i].decklist,
                user = new OpponentDeck.User { name = myDecks[i].deckName }
            };

            ui.SetDeck(fakeOpponentDeck, i);
            int captured = i;
            ui.selectButton.onClick.AddListener(() => {
                selectedMyDeck = myDecks[captured];

                if (currentMyDeckHighlight != null)
                {
                    currentMyDeckHighlight.GetComponent<Image>().color = Color.white;
                }

                currentMyDeckHighlight = slot;
                currentMyDeckHighlight.GetComponent<Image>().color = Color.yellow;
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
            ui.selectButton.onClick.AddListener(() => {
                selectedAIDeck = aiDecks[captured];

                if (currentAIDeckHighlight != null)
                {
                    currentAIDeckHighlight.GetComponent<Image>().color = Color.white;
                }

                currentAIDeckHighlight = slot;
                currentAIDeckHighlight.GetComponent<Image>().color = Color.yellow;
            });
        }
    }

    void RunSimulation()
    {
        if (selectedMyDeck == null || selectedAIDeck == null || string.IsNullOrEmpty(selectedWeather))
        {
            Debug.LogWarning("No Select");
            return;
        }

        PlayerPrefs.SetInt("IsAISimulation", 1);
        PlayerPrefs.SetString("SimulatedWeather", selectedWeather);
        GameManager.Instance.DeckService.SetSelectedMyDeck(selectedMyDeck);
        GameManager.Instance.DeckService.SetSelectedOpponentDeck(selectedAIDeck);

        SceneManager.LoadScene("AIMode");
    }

    [System.Serializable]
    public class WeatherData
    {
        public Dictionary<string, int> weatherProbabilities;
        public Dictionary<string, string> cityWeathers;
    }
}