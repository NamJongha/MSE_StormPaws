using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using UnityEditor.PackageManager.Requests;
using UnityEngine.UIElements;
using System.Collections.Generic;

/// <summary>
/// Battle Scene Manager Script
/// </summary>

public class BattleManager : MonoBehaviour
{
    public Transform backgroundContainer;
    private GameManager gameManager;

    public TMP_Text weatherText;
    public TMP_Text cityText;

    private List<GameObject> playerCharacters;
    private List<GameObject> opponentCharacters;

    private int playerCharacterIndex;
    private int opponentCharacterIndex;

    //private Dictionary<string, GameObject> actorMap = new Dictionary<string, GameObject>
    //{
    //    
    //};

    void Awake()
    {
        playerCharacters = new List<GameObject>();
        opponentCharacters = new List<GameObject>();
    }

    void Start()
    {
        StartCoroutine(FetchBattleEnvironment());
    }

    //get weather data
    private IEnumerator FetchBattleEnvironment()
    {
        string url = $"{gameManager.baseUrl}/battle/environment";
        UnityWebRequest request = UnityWebRequest.Get(url);
        //TokenManager.SendServerToken(request);
        request.SetRequestHeader("Authorization", "Bearer " + gameManager.GetAuthToken());

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            BattleEnvResponse response = JsonUtility.FromJson<BattleEnvResponse>(request.downloadHandler.text);
            string weather = response.data.weather.ToLower();
            string city = response.data.city;

            weatherText.text = GetWeatherKorean(weather);
            cityText.text = city;

            string prefabPath = $"Backgrounds/{weather}_{city}";
            GameObject prefab = Resources.Load<GameObject>(prefabPath);

            if (prefab != null)
            {
                Instantiate(prefab, backgroundContainer);
            }
            else
            {
                Debug.LogWarning("No Prefab: " + prefabPath);
            }
        }
        else
        {
            Debug.LogError("Fail: " + request.error);
        }
    }

    //get battle simulation result
    private IEnumerator FetchBattleSimulationLog()
    {
        string url = $"{gameManager.baseUrl}/battle/result";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "Bearer " + gameManager.GetAuthToken());

        yield return request.SendWebRequest();

        if(request.result == UnityWebRequest.Result.Success)
        {
            //change result json into meaningful data
            string json = request.downloadHandler.text;
            BattleSimulation battleSimulation = JsonUtility.FromJson<BattleSimulation>(json);
            StartCoroutine(PlayBattleSimulation(battleSimulation.simulation));
        }
        else
        {
            Debug.LogError("Failed to fetch battle result");
        }
    }

    //check the timestamp in log and do acutal attack according to the log /njh
    private IEnumerator PlayBattleSimulation(List<BattleAction> actions)
    {
        float startTime = Time.time;
        foreach (var action in actions)
        {
            float elapsed = Time.time - startTime;
            float waitTime = action.timeStamp - elapsed;

            if (waitTime > 0f)
                yield return new WaitForSeconds(waitTime);

            TriggerAttack(action.actorId, action.content);
        }
    }

    //njh
    private void TriggerAttack(string actorId, string content)
    {
        /*if (!actorMap.ContainsKey(actorId) || !actorMap.ContainsKey(targetId))
        {
            Debug.LogWarning($"Unknown actor or target: {actorId} -> {targetId}");
            return;
        }

        //Doing this with actorId might not work because same character of different deck have same ID
        //this might cause attacking twice when both character is same animal
        GameObject actor = actorMap[actorId];
        GameObject target = actorMap[targetId];

        // Attacking animation
        Animator animator = actor.GetComponent<Animator>();
        if (animator != null)
            animator.SetTrigger("Attack");

        // Hit animation
        Animator targetAnimator = target.GetComponent<Animator>();
        if (targetAnimator != null)
            targetAnimator.SetTrigger("Hit");
        */

        if(actorId == "player")
        {
            if (content == "attack")
            {
                playerCharacters[playerCharacterIndex].GetComponent<Animator>().SetTrigger("Attack");
            }
            else if(content == "death")
            {
                playerCharacterIndex += 1;
            }
        }
        else
        {
            if(content == "attack")
            {
                opponentCharacters[opponentCharacterIndex].GetComponent<Animator>().SetTrigger("Attack");
            }
            else if(content == "death")
            {
                opponentCharacterIndex += 1;
            }
        }

        // Effect addable below here
    }

    //I think It wouldn't be needed since the class's language is english
    private string GetWeatherKorean(string weather)
    {
        switch (weather.ToLower())
        {
            case "rain": return "ºñ";
            case "sun": return "¸¼À½";
            case "snow": return "´«";
            case "cloud": return "Èå¸²";
            default: return weather;
        }
    }

    public void SetMyDeck(GameObject character)
    {
        playerCharacters.Add(character);
    }

    public void SetOpponentDeck(GameObject character)
    {
        opponentCharacters.Add(character);
    }
}

[System.Serializable]
public class BattleEnvResponse
{
    public bool success;
    public string message;
    public BattleEnvData data;
}

[System.Serializable]
public class BattleEnvData
{
    public string weather;
    public string city;
}

[System.Serializable]
public class BattleAction
{
    public float timeStamp;
    public string actorId;
    public string content;
}

[System.Serializable]
public class BattleSimulation
{
    public List<BattleAction> simulation;
}
