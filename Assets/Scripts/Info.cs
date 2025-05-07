using TMPro;
using UnityEngine;

public class Info : MonoBehaviour
{
    public GameManager gameManager;

    // Personal Info UI
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI mainText;
    public TextMeshProUGUI emailText;
    public TextMeshProUGUI idText;

    void Start()
    {
        gameManager.FetchPersonalInfo(info =>
        {
            nameText.text = info.name;
            idText.text = info.id;
            mainText.text = info.name;
            emailText.text = info.email;
        });
    }
}
