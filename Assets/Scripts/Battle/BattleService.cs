using System;
using System.Collections;
using System.Collections.Generic;
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

    void Awake()
    {
        playerCharacters = new List<GameObject>();
        opponentCharacters = new List<GameObject>();
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
                onError?.Invoke("전투 환경 응답 실패");
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
            BattleSimulation battleSimulation = JsonUtility.FromJson<BattleSimulation>(json);
            GameManager.Instance.StartCoroutine(PlayBattleSimulation(battleSimulation.simulation));
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

        if (actorId == "player")
        {
            if (content == "attack")
            {
                playerCharacters[playerCharacterIndex].GetComponent<Animator>().SetTrigger("Attack");
            }
            else if (content == "death")
            {
                playerCharacterIndex += 1;
            }
        }
        else
        {
            if (content == "attack")
            {
                opponentCharacters[opponentCharacterIndex].GetComponent<Animator>().SetTrigger("Attack");
            }
            else if (content == "death")
            {
                opponentCharacterIndex += 1;
            }
        }

        // Effect addable below here
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
public class BattleSimulation
{
    public List<BattleAction> simulation;
}

[System.Serializable]
public class BattleAction
{
    public float timeStamp;
    public string actorId;
    public string content;
}

[System.Serializable]
public class BattleRecordListWrapper
{
    public List<BattleRecord> data;
}