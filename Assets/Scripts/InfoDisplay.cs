using TMPro;
using UnityEngine;

/// <summary>
/// Display User Information (MyPage)
/// </summary>

public class InfoDisplay : MonoBehaviour
{
    [Header("Personal Info UI")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI mainText;
    public TextMeshProUGUI emailText;
    public TextMeshProUGUI idText;

    private GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.Instance;

        gameManager.UserService.FetchPersonalInfo(OnInfoLoaded);
    }

    private void OnInfoLoaded(PersonalInfo info)
    {
        if (info == null)
        {
            Debug.LogWarning("Failed to load user info.");
            return;
        }

        nameText.text = info.name ?? "-";
        mainText.text = info.name ?? "-";
        emailText.text = info.email ?? "-";
        idText.text = info.id ?? "-";
    }
}