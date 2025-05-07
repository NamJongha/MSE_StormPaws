using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GameManager;

/// <summary>
/// Control Button Scirpt
/// </summary>

public class ButtonManager : MonoBehaviour
{
    public GameManager gameManager;
    public DeckManager deckManager;
    public RecordManager recordManager;

    // For panel activation control
    public GameObject infoPanel;
    public GameObject deckPanel;
    public GameObject recordPanel;
    public GameObject deckCreatePanel;

    // Button Click Sound
    public AudioSource buttonClick;

    // Cached Data
    private List<DeckPreset> cachedDecks = null;
    private List<BattleRecord> cachedBattles = null;

    // Deck Button
    public void DeckButton()
    {
        if (cachedDecks != null)
        {
            deckManager.DisplayDeckList(cachedDecks);

            return;
        }

        gameManager.FetchDeckPresets((deckList) =>
        {
            cachedDecks = deckList;

            deckManager.DisplayDeckList(deckList);
        });

        deckPanel.SetActive(true);
        infoPanel.SetActive(false);
        recordPanel.SetActive(false);
        deckCreatePanel.SetActive(false);

        buttonClick.Play();
    }

    // Personal Info Button
    public void InfoButton()
    {
        infoPanel.SetActive(true);
        deckPanel.SetActive(false);
        recordPanel.SetActive(false);
        deckCreatePanel.SetActive(false);

        buttonClick.Play();
    }

    // Deck Creation Button
    public void deckCreateButton()
    {
        deckCreatePanel.SetActive(true);
        recordPanel.SetActive(false);
        deckManager.InitCardList();

        buttonClick.Play();
    }

    // Record Button
    public void recordButton()
    {
        if (cachedBattles != null)
        {
            recordManager.DisplayBattleRecords(cachedBattles);
        }
        else
        {
            gameManager.FetchBattleRecords((battleList) =>
            {
                cachedBattles = battleList;

                recordManager.DisplayBattleRecords(battleList);
            });
        }

        recordPanel.SetActive(true);
        infoPanel.SetActive(false);
        deckPanel.SetActive(false);
        deckCreatePanel.SetActive(false);

        buttonClick.Play();
    }

    // Home Button
    public void homeButton()
    {
        buttonClick.Play();
        SceneManager.LoadScene("HomeScreen");
    }
}
