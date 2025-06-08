using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Battle Environment Manager
/// </summary>
public class BattleManager : MonoBehaviour
{
    public Transform backgroundContainer;
    public TMP_Text weatherText;
    public TMP_Text cityText;

    public Material fogSkybox;
    public Material rainySkybox;
    public Material tornadoSkybox;
    public Material clearSkybox;
    public Material cloudSkybox;
    public Material defaultSkybox;

    private Dictionary<string, GameObject> backgroundMap = new();
    private GameObject currentBackground;

    private static readonly Dictionary<string, string> koreanToEnglishWeather = new()
    {
        { "클리어", "Clear" },
        { "클라우드", "Clouds" },
        { "레인", "Rain" },
        { "스노우", "Snow" },
        { "포그", "Fog" },
        { "미스트", "Mist" },
        { "썬더스톰", "Thunderstorm" },
        { "황사", "Sand" },
        { "토네이도", "Tornado" },
        { "돌풍", "Gust" }
    };

    private string ConvertWeatherToEnglish(string koreanWeather)
    {
        return koreanToEnglishWeather.TryGetValue(koreanWeather, out var eng)
            ? eng
            : "Default";
    }

    private void Awake()
    {
        foreach (Transform child in backgroundContainer)
        {
            backgroundMap[child.name.ToLower()] = child.gameObject;
            child.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        bool isAISimulation = PlayerPrefs.GetInt("IsAISimulation", 0) == 1;

        if (isAISimulation)
        {
            string rawWeather = PlayerPrefs.GetString("SimulatedWeather", "클리어");
            string weather = ConvertWeatherToEnglish(rawWeather);

            BattleEnvData env = new BattleEnvData { weather = weather, city = "Simulation" };
            OnEnvironmentReceived(env);

            GameManager.Instance.StartCoroutine(GameManager.Instance.BattleService.PlayAISimulation());
        }
        else
        {
            StartCoroutine(GameManager.Instance.BattleService.FetchBattleEnvironment(
                (env) =>
                {
                    env.weather = ConvertWeatherToEnglish(env.weather);
                    OnEnvironmentReceived(env);
                },
                (error) => { Debug.LogError("Fail: " + error); }
            ));

            GameManager.Instance.BattleService.FetchBattleSimulationLog();
        }
    }

    private void OnEnvironmentReceived(BattleEnvData env)
    {
        if (env == null)
        {
            Debug.LogError("Fail");
            return;
        }

        weatherText.text = env.weather.ToLower();
        cityText.text = env.city;

        string weatherKey = env.weather.ToLower();

        if (currentBackground != null)
        {
            currentBackground.SetActive(false);
        }

        if (backgroundMap.TryGetValue(weatherKey, out var bg))
        {
            currentBackground = bg;
        }
        else
        {
            backgroundMap.TryGetValue("default", out currentBackground);
        }

        currentBackground?.SetActive(true);

        // Skybox
        switch (weatherKey)
        {
            case "fog":
            case "mist":
                RenderSettings.skybox = fogSkybox;
                break;

            case "rain":
            case "thunderstorm":
            case "snow":
            case "sand":
                RenderSettings.skybox = rainySkybox;
                break;

            case "tornado":
                RenderSettings.skybox = tornadoSkybox;
                break;

            case "clear":
                RenderSettings.skybox = clearSkybox;
                break;

            case "clouds":
                RenderSettings.skybox = cloudSkybox;
                break;

            case "gust":
                RenderSettings.skybox = defaultSkybox;
                break;

            default:
                RenderSettings.skybox = defaultSkybox;
                break;
        }

        DynamicGI.UpdateEnvironment();
    }
}
