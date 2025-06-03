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
        StartCoroutine(GameManager.Instance.BattleService.FetchBattleEnvironment(
            OnEnvironmentReceived,
            (error) => { Debug.LogError("Fail: " + error); }
        ));
        GameManager.Instance.BattleService.FetchBattleSimulationLog();
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
            case "Fog":
            case "Mist":
                RenderSettings.skybox = fogSkybox;
                break;

            case "Rain":
            case "Thunderstorm":
            case "Snow":
            case "Sand":
                RenderSettings.skybox = rainySkybox;
                break;

            case "Tornado":
                RenderSettings.skybox = tornadoSkybox;
                break;

            case "Clear":
                RenderSettings.skybox = clearSkybox;
                break;

            case "Clouds":
                RenderSettings.skybox = cloudSkybox;
                break;

            case "Gust":
                RenderSettings.skybox = defaultSkybox;
                break;

            default:
                RenderSettings.skybox = defaultSkybox;
                break;
        }

        DynamicGI.UpdateEnvironment();
    }
}