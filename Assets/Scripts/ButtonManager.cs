using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject infoPanel;
    public GameObject deckPanel;
    public GameObject recordPanel;
    public GameObject deckCreatePanel;
    public AudioSource buttonClick;

    void Start()
    {
        deckPanel.SetActive(false);
        recordPanel.SetActive(false);
    }

    public void DeckButton()
    {
        deckPanel.SetActive(true);
        infoPanel.SetActive(false);
        recordPanel.SetActive(false);

        buttonClick.Play();
    }

    public void InfoButton()
    {
        infoPanel.SetActive(true);
        deckPanel.SetActive(false);
        recordPanel.SetActive(false);

        buttonClick.Play();
    }

    public void recordButton()
    {
        recordPanel.SetActive(true);
        infoPanel.SetActive(false);
        deckPanel.SetActive(false);

        buttonClick.Play();
    }

    public void deckCreateButton()
    {
        deckCreatePanel.SetActive(true);

        buttonClick.Play();
    }

    public void confirmButton()
    {
        deckCreatePanel.SetActive(false);

        buttonClick.Play();
    }
}
