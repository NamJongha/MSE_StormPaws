using UnityEngine;

public class AudioPlay : MonoBehaviour
{
    public AudioSource click;

    public void PlayClick()
    {
        click.Play();
    }
}
