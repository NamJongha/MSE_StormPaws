using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Battle System Script
/// </summary>

public class BattleService
{
    public BattleService()
    {
        playerCharacters = new List<GameObject>();
        opponentCharacters = new List<GameObject>();
        playerCharacterIndex = 0;
        opponentCharacterIndex = 0;
    }

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
        playerCharacters = new List<GameObject>();
        opponentCharacters = new List<GameObject>();
    }

    public void ResetState()
    {
        playerCharacterIndex = 0;
        opponentCharacterIndex = 0;

        isBattleOver = false;

        playerCharacters = new List<GameObject>();
        opponentCharacters = new List<GameObject>();

        playerDeckId = PlayerPrefs.GetString("SelectedPlayerDeckId", "");
        opponentDeckId = PlayerPrefs.GetString("SelectedOpponentDeckId", "");
    }

    public IEnumerator FetchBattleEnvironment(Action<BattleEnvData> onSuccess, Action<string> onError)
    {
        string url = $"{GameManager.Instance.baseUrl}/battle/environment";
        UnityWebRequest request = UnityWebRequest.Get(url);
        //TokenManager.SendServerToken(request);
        request.SetRequestHeader("Authorization", "Bearer " + GameManager.Instance.GetAuthToken());

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            BattleEnvResponse response = JsonUtility.FromJson<BattleEnvResponse>(request.downloadHandler.text);
            if (response.success)
                onSuccess?.Invoke(response.data);
            else
                onError?.Invoke("ÀüÅõ È¯°æ ÀÀ´ä ½ÇÆÐ");
        }
        else
        {
            onError?.Invoke(request.error);
        }
    }

    //get battle simulation result
    public IEnumerator FetchBattleSimulationLog()
    {
        string url = $"{GameManager.Instance.baseUrl}/battle/result";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "Bearer " + GameManager.Instance.GetAuthToken());

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            //change result json into meaningful data
            string json = request.downloadHandler.text;
            BattleSimulationLog battleSimulation = JsonUtility.FromJson<BattleSimulationLog>(json);
            GameManager.Instance.StartCoroutine(PlayBattleSimulation(battleSimulation.data.logs));
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
                playerCharacters.Clear();
                playerCharacterIndex = 0;
                opponentCharacters.Clear();
                opponentCharacterIndex = 0;
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
    /*How It Works
     * It saves 5 characters of each deck in BattleUnitSpawner.cs
     * After fetching simulation log from server, it reads log
     * The log contains Deck Id, which can devide which player's character attacked.
     * It compares the deck Id of attacker and check which player attacked
     * Then, character of that deck acts attack
     * -> This is for preventing that when each character is same, the Id is also same,
     * so the character will attack and hit twice at a time if it checks only character id.
     */
    private void TriggerAttack(string actorDeckId, int damage, int remainingHp)
    {
        if (actorDeckId == playerDeckId)
        {
            playerCharacters[playerCharacterIndex].GetComponent<Animator>().SetTrigger("Attack");
            GameManager.Instance.StartCoroutine(ShowDamage(opponentCharacters[opponentCharacterIndex], damage, "opponent"));

            if (remainingHp <= 0)
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
            GameManager.Instance.StartCoroutine(ShowDamage(playerCharacters[playerCharacterIndex], damage, "player"));
            if (remainingHp <= 0)
            {
                playerCharacters[playerCharacterIndex].SetActive(false);
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

    private IEnumerator ShowDamage(GameObject target, int damage, string player)
    {
        if (player == "player")
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

    public void FetchBattleRecords(Action<List<BattleRecord>> callback)
    {
        GameManager.Instance.StartCoroutine(GameManager.Instance.GetRequest($"{GameManager.Instance.baseUrl}/battle/logs", (json) =>
        {
            BattleRecordListWrapper wrapper = JsonUtility.FromJson<BattleRecordListWrapper>(json);
            callback?.Invoke(wrapper.data);
        }));
    }
}

// JSON Response Models
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
public class BattleRecordListWrapper
{
    public List<BattleRecord> data;
}