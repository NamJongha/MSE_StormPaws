﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles battle environment visuals and weather settings.
/// </summary>

public class BattleManager : MonoBehaviour
{
    public Transform backgroundContainer;
    public TMP_Text weatherText;
    public TMP_Text cityText;
    public GameObject introGroup;

    public Material fogSkybox;
    public Material rainySkybox;
    public Material tornadoSkybox;
    public Material clearSkybox;
    public Material cloudSkybox;
    public Material defaultSkybox;

    private Dictionary<string, GameObject> backgroundMap = new();
    private GameObject currentBackground;

    public GameObject playerDamage;
    public GameObject opponentDamage;

    [SerializeField]
    private GameObject resultUi, recordManager;

    private Dictionary<string, string> weatherMap = new()
    {
        {"CLEAR", "00000000-0000-0000-0000-000000000001"},
        {"CLOUDS", "00000000-0000-0000-0000-000000000002"},
        {"RAIN", "00000000-0000-0000-0000-000000000003"},
        {"SNOW", "00000000-0000-0000-0000-000000000004"},
        {"FOG", "00000000-0000-0000-0000-000000000005"},
        {"MIST", "00000000-0000-0000-0000-000000000006"},
        {"THUNDERSTORM", "00000000-0000-0000-0000-000000000007"},
        {"DUST", "00000000-0000-0000-0000-000000000008"},
        {"TORNADO", "00000000-0000-0000-0000-000000000009"},
        {"GUST", "00000000-0000-0000-0000-000000000010"},
        {"UNKNOWN", "00000000-0000-0000-0000-000000000011"},
    };

    private void Awake()
    {
        // Store all background objects and deactivate them
        foreach (Transform child in backgroundContainer)
        {
            backgroundMap[child.name.ToUpper()] = child.gameObject;
            child.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        // Setup battle service UI references
        GameManager.Instance.BattleService.SetDamageTextObjects(playerDamage, opponentDamage);
        GameManager.Instance.BattleService.SetResultUI(resultUi);
        GameManager.Instance.BattleService.SetRecordManager(recordManager.GetComponent<BattleResultUI>());

        bool isAISimulation = PlayerPrefs.GetInt("IsAISimulation", 0) == 1;

        if (isAISimulation)
        {
            Debug.Log("StartAIBattle");
            string weather = PlayerPrefs.GetString("SimulatedWeather", "CLEAR").ToUpper();

            // Setup environment manually for AI mode
            BattleEnvData env = new BattleEnvData { weatherType = weather, city = "Simulation" };
            OnEnvironmentReceived(env);

            // Save corresponding UUID for AI simulation
            weatherMap.TryGetValue(weather, out string value);
            PlayerPrefs.SetString("SimulatedWeatherId", value);

            StartCoroutine(FetchEnvironmentThenStartSimulation(true, env));
        }
        else
        {
            Debug.Log("fetch weather");
            StartCoroutine(FetchEnvironmentThenStartSimulation(false, null));
        }
    }

    private IEnumerator FetchEnvironmentThenStartSimulation(bool isAISimulation, BattleEnvData weatherData)
    {
        // Fetch weather info from server
        if (!isAISimulation)
        {
            BattleEnvData fetchedEnv = null;
            bool isDone = false;

            yield return GameManager.Instance.StartCoroutine(
                GameManager.Instance.BattleService.FetchBattleEnvironment(
                    (env) =>
                    {
                        fetchedEnv = env;
                        isDone = true;
                    },
                    (error) =>
                    {
                        Debug.LogError("Fail: " + error);
                        isDone = true;
                    }
                )
            );

            while (!isDone) yield return null;

            if (fetchedEnv != null)
            {
                fetchedEnv.weatherType = fetchedEnv.weatherType?.ToUpper();
                OnEnvironmentReceived(fetchedEnv);

                weatherText.text = fetchedEnv.weatherType;
                cityText.text = fetchedEnv.city;

                introGroup.SetActive(true);
                yield return new WaitForSeconds(2.5f);
                introGroup.SetActive(false);
            }

            // Start battle
            yield return GameManager.Instance.StartCoroutine(
                GameManager.Instance.BattleService.FetchBattleSimulationLog(false)
            );
        }
        else
        {
            // For AI mode: use provided weather data
            OnEnvironmentReceived(weatherData);

            weatherText.text = weatherData.weatherType;
            cityText.text = weatherData.city;

            introGroup.SetActive(true);
            yield return new WaitForSeconds(2.5f);
            introGroup.SetActive(false);

            yield return GameManager.Instance.StartCoroutine(
                GameManager.Instance.BattleService.FetchBattleSimulationLog(true)
            );
        }
    }

    private void OnEnvironmentReceived(BattleEnvData env)
    {
        if (env == null || string.IsNullOrEmpty(env.weatherType))
        {
            return;
        }

        string weatherKey = env.weatherType.ToUpper();
        weatherText.text = weatherKey;
        cityText.text = env.city;

        Debug.Log($"Set Weather: {weatherKey}");
        Debug.Log($"Background Found: {backgroundMap.ContainsKey(weatherKey)}");

        if (backgroundMap.TryGetValue(weatherKey, out var bg))
        {
            currentBackground = bg;
        }
        else
        {
            currentBackground = null;
        }

        if (currentBackground != null)
        {
            currentBackground.SetActive(true);
        }

        currentBackground?.SetActive(true);

        // Set skybox based on weather
        switch (weatherKey)
        {
            case "FOG":
            case "MIST":
                RenderSettings.skybox = fogSkybox;
                break;

            case "RAIN":
            case "THUNDERSTORM":
            case "SNOW":
            case "SAND":
                RenderSettings.skybox = rainySkybox;
                break;

            case "TORNADO":
                RenderSettings.skybox = tornadoSkybox;
                break;

            case "CLEAR":
                RenderSettings.skybox = clearSkybox;
                break;

            case "CLOUDS":
                RenderSettings.skybox = cloudSkybox;
                break;

            case "GUST":
                RenderSettings.skybox = defaultSkybox;
                break;

            default:
                RenderSettings.skybox = defaultSkybox;
                break;
        }

        DynamicGI.UpdateEnvironment();
    }
}
