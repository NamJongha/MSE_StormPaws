using System.Collections;
using TMPro;
using UnityEngine;

public class BattleUIHelper : MonoBehaviour
{
    public static BattleUIHelper Instance;

    public TextMeshProUGUI battleLogText;

    private void Awake()
    {
        Instance = this;
    }

    public void Log(string message)
    {
        if (battleLogText != null)
        {
            battleLogText.text += $"{message}\n";
            StartCoroutine(ClearAfterDelay(3f));
        }
    }

    private IEnumerator ClearAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ClearLog();
    }


    public void ClearLog()
    {
        if (battleLogText != null)
        {
            battleLogText.text = "";
        }
    }
}