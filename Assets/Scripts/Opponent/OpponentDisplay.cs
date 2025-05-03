using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Show Opponent Deck Script
/// </summary>

public class OpponentDisplay : MonoBehaviour
{
    public OpponentSelect opponentSelect;

    void Start()
    {
        opponentSelect.ShowSelectedOpponentDeck();
    }
}
