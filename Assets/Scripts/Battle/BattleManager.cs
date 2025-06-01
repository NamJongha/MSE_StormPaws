using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using UnityEditor.PackageManager.Requests;
using UnityEngine.UIElements;
using System.Collections.Generic;
using JetBrains.Annotations;

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

    private string playerDeckId;
    private string opponentDeckId;

    [SerializeField]
    private GameObject playerDamage;
    [SerializeField]
    private GameObject opponentDamage;

    private bool isBattleOver = false;

    void Awake()
    {
        playerCharacterIndex = 0;
        opponentCharacterIndex = 0;

        isBattleOver = false;

        playerCharacters = new List<GameObject>();
        opponentCharacters = new List<GameObject>();
    }

    void Start()
    {
        playerDeckId = PlayerPrefs.GetString("SelectedPlayerDeckId", "");
        opponentDeckId = PlayerPrefs.GetString("SelectedOpponentDeckId", "");
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
            BattleSimulationLog battleSimulation = JsonUtility.FromJson<BattleSimulationLog>(json);
            StartCoroutine(PlayBattleSimulation(battleSimulation.data.logs));
        }
        else
        {
            Debug.LogError("Failed to fetch battle result");
        }
    }

    //check the timestamp in log and do acutal attack according to the log /njh
    private IEnumerator PlayBattleSimulation(List<BattleLog> logs)
    {

        float startTime = Time.time;
        foreach (var log in logs)
        {
            //stop coroutine if the battle is over
            if (isBattleOver == true)
            {
                yield break;
            }

            float elapsed = Time.time - startTime;
            float waitTime = log.timestamp - elapsed;

            if (waitTime > 0f)
                yield return new WaitForSeconds(waitTime);

            TriggerAttack(log.attackerDeckId, log.damage, log.targetRemainingHp);
        }
    }

    //njh
    private void TriggerAttack(string actorDeckId, int damage, int remainingHp)
    {
        //Using only card ID(actoirId) might not work because same character of different deck have same ID
        //this might cause attacking twice when both character is same animal

        if(actorDeckId == playerDeckId)
        {
            playerCharacters[playerCharacterIndex].GetComponent<Animator>().SetTrigger("Attack");
            StartCoroutine(ShowDamage(opponentCharacters[opponentCharacterIndex], damage, "opponent"));

            if(remainingHp <= 0)
            {
                opponentCharacters[opponentCharacterIndex].SetActive(false);
                if (opponentCharacterIndex < 5)//if character is still left
                {
                    opponentCharacterIndex += 1;
                }
                else
                {
                    //end battle
                    isBattleOver = true;
                }
            }
        }
        else
        {
            opponentCharacters[opponentCharacterIndex].GetComponent<Animator>().SetTrigger("Attack");
            StartCoroutine(ShowDamage(playerCharacters[playerCharacterIndex], damage, "player"));
            if (remainingHp <= 0)
            {
                playerCharacters[opponentCharacterIndex].SetActive(false);
                if (playerCharacterIndex < 5)
                {
                    playerCharacterIndex += 1;
                }
                else
                {
                    //end battle
                    isBattleOver = true;
                }
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

    private IEnumerator ShowDamage(GameObject target, int damage, string player)
    {
        if(player == "player")
        {
            playerDamage.transform.position = target.transform.position + new Vector3(0, 1, 0); //height offset
            playerDamage.GetComponent<TMP_Text>().text = damage.ToString();
            playerDamage.SetActive(true);
            yield return new WaitForSeconds(2f);
            playerDamage.SetActive(false);
        }
        else
        {
            opponentDamage.transform.position = target.transform.position + new Vector3(0, 1, 0); //height offset
            opponentDamage.GetComponent<TMP_Text>().text = damage.ToString();
            opponentDamage.SetActive(true);
            yield return new WaitForSeconds(2f);
            opponentDamage.SetActive(false);
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
public class BattleLog
{
    public float timestamp;
    public string attackerDeckId;
    public string attackerCardId;
    public string targetDeckId;
    public string targetCardId;
    public int damage;
    public int targetRemainingHp;
}

[System.Serializable]
public class BattleData
{
    public string winnerId;
    public List<BattleLog> logs;
}

[System.Serializable]
public class BattleSimulationLog
{
    public bool success;
    public string message;
    public BattleData data;
}
