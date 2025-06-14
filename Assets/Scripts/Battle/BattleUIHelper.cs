using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Singleton helper to manage displaying temporary battle log messages in UI.
/// </summary>

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

    // Waits for a delay before clearing the log.
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