using UnityEngine;

// BGM ���� ��ũ��Ʈ
public class BGMManager : MonoBehaviour
{
    public AudioSource bgm;

    void Awake()
    {
        bgm = GetComponent<AudioSource>();

        if (bgm != null)
        {
            DontDestroyOnLoad(bgm);
        }
        else
        {
            Destroy(bgm);
        }
    }
}
