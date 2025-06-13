using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

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

    private string playerId;
    private string opponentId;

    private GameObject playerDamage;
    private GameObject opponentDamage;

    private bool isBattleOver = false;
    private string weatherId;

    private string winnerId;
    private BattleEnvData weatherData;
    private float battleTime;

    private GameObject resultUI;

    private BattleResultUI battleResultUI;

    private Dictionary<GameObject, (HPBar bar, int currentHp, int maxHp)> hpBars = new();
    private const int defaultMaxHp = 100;
    private HPBar currentPlayerHPBar;
    private HPBar currentOpponentHPBar;

    public void ResetState()
    {
        playerCharacterIndex = 0;
        opponentCharacterIndex = 0;

        isBattleOver = false;

        playerCharacters = new List<GameObject>();
        opponentCharacters = new List<GameObject>();

        playerDeckId = PlayerPrefs.GetString("SelectedMyDeckId", "");
        opponentDeckId = PlayerPrefs.GetString("SelectedOpponentDeckId", "");
    }

    public IEnumerator FetchBattleEnvironment(Action<BattleEnvData> onSuccess, Action<string> onError)
    {
        string url = $"{GameManager.Instance.baseUrl}/weather/random";
        UnityWebRequest request = UnityWebRequest.Get(url);
        //TokenManager.SendServerToken(request);
        request.SetRequestHeader("Authorization", "Bearer " + GameManager.Instance.GetAuthToken());

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            BattleEnvResponse response = JsonUtility.FromJson<BattleEnvResponse>(request.downloadHandler.text);

            if (response != null)
            {
                weatherId = response.data.id;
                weatherData = response.data;
                onSuccess?.Invoke(response.data);
            }
        }
    }

    //get battle simulation result
    public IEnumerator FetchBattleSimulationLog()
    {
        Debug.Log("Start Battle");

        playerDeckId = PlayerPrefs.GetString("SelectedMyDeckId", "");
        opponentDeckId = PlayerPrefs.GetString("SelectedOpponentDeckId", "");

        playerId = PlayerPrefs.GetString("PlayerId", "");
        opponentId = PlayerPrefs.GetString("SelectedOpponentUserId", "");

        BattleRequestDto requestData = new BattleRequestDto
        {
            attackerDeckId = playerDeckId,
            attackerUserId = playerId,
            defenderDeckId = opponentDeckId,
            defenderUserId = opponentId,
            weatherLogId = weatherId,
        };

        string jsonData = JsonUtility.ToJson(requestData);
        //Debug.Log(jsonData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        string url = $"{GameManager.Instance.baseUrl}/battles/pvp";
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + GameManager.Instance.GetAuthToken());

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            //Debug.Log("Successfully fetched simulation");

            //change result json into meaningful data
            string json = request.downloadHandler.text;
            BattleSimulationLog battleSimulation = JsonUtility.FromJson<BattleSimulationLog>(json);
            GameManager.Instance.StartCoroutine(PlayBattleSimulation(battleSimulation.data.logs));
        }
        else
        {
            Debug.LogError("Failed to fetch battle result");
            Debug.Log($"Response Code: {request.responseCode}");
            Debug.Log($"Error Message: {request.downloadHandler.text}");
        }
    }

    //check the timestamp in log and do acutal attack according to the log /njh
    private IEnumerator PlayBattleSimulation(List<BattleLog> logs)
    {
        float startTime = Time.time;
        foreach (var log in logs)
        {
            Debug.Log($"[PlayBattleSimulation] Processing log at timestamp: {log.timestamp}");
            Debug.Log($"Attacker: {log.attackerDeckId}, AttackerCard: {log.attackerCardId}, Damage: {log.damage}, TargetRemainingHP: {log.targetRemainingHp}");
            Debug.Log($"Current Player Index: {playerCharacterIndex}, Opponent Index: {opponentCharacterIndex}");

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
            float waitTime = (log.timestamp - elapsed) * 2f;

            waitTime = Mathf.Max(waitTime, 0.8f);
            yield return new WaitForSeconds(waitTime);

            TriggerAttack(log.attackerDeckId, log.damage, log.targetRemainingHp);
            battleTime = elapsed;
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
    //LJH: adding battle log
    private void TriggerAttack(string actorDeckId, int damage, int remainingHp)
    {
        ShowCurrentHPBar();

        if (actorDeckId == playerDeckId)
        {
            if (opponentCharacterIndex >= opponentCharacters.Count)
            {
                Debug.LogWarning("❗️opponentCharacterIndex out of range");
                return;
            }

            GameObject unit = opponentCharacters[opponentCharacterIndex];

            Debug.Log("player attack " + damage);
            GameManager.Instance.StartCoroutine(ShowDamage(unit, damage, "opponent"));
            UpdateHP(unit, remainingHp);

            if (remainingHp <= 0)
            {
                BattleUIHelper.Instance.Log($"Opponent's No.{opponentCharacterIndex + 1} character has fallen!");
                Debug.Log("Opponent character dead");
                unit.SetActive(false);

                if (hpBars.TryGetValue(unit, out var data))
                    data.bar.gameObject.SetActive(false);

                opponentCharacterIndex++;
                if (opponentCharacterIndex >= opponentCharacters.Count)
                {
                    Debug.Log("Battle Ended");
                    isBattleOver = true;
                    GameManager.Instance.StartCoroutine(ShowResult());
                }
            }
        }
        else
        {
            if (playerCharacterIndex >= playerCharacters.Count)
            {
                Debug.LogWarning("❗️playerCharacterIndex out of range");
                return;
            }

            GameObject unit = playerCharacters[playerCharacterIndex];

            Debug.Log("opponent attack " + damage);
            GameManager.Instance.StartCoroutine(ShowDamage(unit, damage, "player"));
            UpdateHP(unit, remainingHp);

            if (remainingHp <= 0)
            {
                BattleUIHelper.Instance.Log($"Your No.{playerCharacterIndex + 1} character has fallen!");
                Debug.Log("Player character dead");
                unit.SetActive(false);

                if (hpBars.TryGetValue(unit, out var data))
                    data.bar.gameObject.SetActive(false);

                playerCharacterIndex++;
                if (playerCharacterIndex >= playerCharacters.Count)
                {
                    Debug.Log("Battle Ended");
                    isBattleOver = true;
                    GameManager.Instance.StartCoroutine(ShowResult());
                }
            }
        }
    }

    

    private IEnumerator ShowResult()
    {
        yield return new WaitForSeconds(2f);
        BattleResult result = new();

        result.result = (winnerId == playerDeckId) ? "WIN" : "LOSE";
        result.weather = weatherData.weatherType;
        result.city = weatherData.city;
        result.timestamp = battleTime.ToString();

        bool isMyDeckLoaded = false;
        bool isOpponentDeckLoaded = false;

        GameManager.Instance.DeckService.FetchDeckById(playerDeckId, DeckPreset =>
        {
            result.myDeckList = DeckPreset.decklist;
            isMyDeckLoaded = true;
        });

        GameManager.Instance.DeckService.FetchDeckById(opponentDeckId, DeckPreset =>
        {
            result.opponentDeckList = DeckPreset.decklist;
            isOpponentDeckLoaded = true;
        });

        yield return new WaitUntil(() => isMyDeckLoaded && isOpponentDeckLoaded);

        battleResultUI.SetUI(result);
        resultUI.SetActive(true);
    }

    private IEnumerator ShowDamage(GameObject target, int damage, string player)
    {
        if (player == "player")
        {
            //playerDamage.transform.position = target.transform.position + new Vector3(0, 1, 0); //height offset
            playerDamage.GetComponent<TMP_Text>().text = "-" + damage.ToString();
            playerDamage.SetActive(true);
            yield return new WaitForSeconds(2f);
            playerDamage.SetActive(false);
        }
        else
        {
            //opponentDamage.transform.position = target.transform.position + new Vector3(0, 1, 0); //height offset
            opponentDamage.GetComponent<TMP_Text>().text =  "-" + damage.ToString();
            opponentDamage.SetActive(true);
            yield return new WaitForSeconds(2f);
            opponentDamage.SetActive(false);
        }
    }

    public void RegisterHPBar(GameObject unit, HPBar bar)
    {
        bar.SetHP(defaultMaxHp, defaultMaxHp);
        bar.gameObject.SetActive(false);
        hpBars[unit] = (bar, defaultMaxHp, defaultMaxHp);
    }

    private void UpdateHP(GameObject unit, int newHp)
    {
        if (hpBars.TryGetValue(unit, out var data))
        {
            data.bar.SetHP(newHp, data.maxHp);
            hpBars[unit] = (data.bar, newHp, data.maxHp);
        }
    }

    void ShowCurrentHPBar()
    {
        if (playerCharacterIndex < playerCharacters.Count)
        {
            var unit = playerCharacters[playerCharacterIndex];
            if (hpBars.TryGetValue(unit, out var data))
            {
                if (currentPlayerHPBar != null)
                    currentPlayerHPBar.gameObject.SetActive(false);

                currentPlayerHPBar = data.bar;
                currentPlayerHPBar.gameObject.SetActive(true);
            }
        }

        if (opponentCharacterIndex < opponentCharacters.Count)
        {
            var unit = opponentCharacters[opponentCharacterIndex];
            if (hpBars.TryGetValue(unit, out var data))
            {
                if (currentOpponentHPBar != null)
                    currentOpponentHPBar.gameObject.SetActive(false);

                currentOpponentHPBar = data.bar;
                currentOpponentHPBar.gameObject.SetActive(true);
            }
        }
    }
    
    public void SetDamageTextObjects(GameObject playerText, GameObject opponentText)
    {
        playerDamage = playerText;
        opponentDamage = opponentText;
    }

    public void SetResultUI(GameObject o)
    {
        resultUI = o;
    }

    public void SetRecordManager(BattleResultUI ui)
    {
        battleResultUI = ui;
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
        string url = $"{GameManager.Instance.baseUrl}/battles/records/me?page=1&size=100";

        GameManager.Instance.StartCoroutine(GameManager.Instance.GetRequest(url, (json) =>
        {
            BattleRecordResponse response = JsonUtility.FromJson<BattleRecordResponse>(json);
            if (response != null && response.data != null && response.data.items != null)
            {
                List<BattleRecord> battleRecords = new();

                foreach (var item in response.data.items)
                {
                    battleRecords.Add(new BattleRecord
                    {
                        battleId = item.myDeck?.id ?? "(no id)",
                        weather = item.weather,
                        result = item.result,
                        opponent = item.opponentDeck?.user?.name ?? "(unknown)",
                        myDeck = string.Join(",", item.myDeck?.decklist?.Select(x => x.card.name) ?? new List<string>()),
                        opponentDeck = string.Join(",", item.opponentDeck?.decklist?.Select(x => x.card.name) ?? new List<string>())
                    });
                }

                callback?.Invoke(battleRecords);
            }
            else
            {
                callback?.Invoke(new List<BattleRecord>());
            }
        },
        (error) =>
        {
            Debug.LogError("Battle Record Load Fail: " + error);
        }));
    }

    // For AI Simulation
    public IEnumerator PlayAISimulation()
    {
        var logs = new List<BattleLog>();
        float currentTime = 0f;
        System.Random rand = new();

        playerDeckId = PlayerPrefs.GetString("SelectedMyDeckId", "");
        opponentDeckId = PlayerPrefs.GetString("SelectedOpponentDeckId", "");

        for (int i = 0; i < 5; i++)
        {
            logs.Add(new BattleLog
            {
                timestamp = currentTime,
                attackerDeckId = (i % 2 == 0) ? playerDeckId : opponentDeckId,
                targetDeckId = (i % 2 == 0) ? opponentDeckId : playerDeckId,
                attackerCardId = "sim_attacker_" + i,
                targetCardId = "sim_target_" + i,
                damage = rand.Next(8, 20),
                targetRemainingHp = rand.Next(0, 30)
            });

            currentTime += 1.5f;
        }

        yield return GameManager.Instance.StartCoroutine(PlayBattleSimulation(logs));
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
    public string id;
    public string city;
    public string weatherType;
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
public class BattleRecordResponse
{
    public BattleRecordData data;
}

[System.Serializable]
public class BattleRecordData
{
    public List<BattleRecordItem> items;
}
[System.Serializable]
public class BattleRequestDto
{
    public string attackerDeckId;
    public string attackerUserId;
    public string defenderDeckId;
    public string defenderUserId;
    public string weatherLogId;
}