using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BattleRecord
{
    public string battleId;
    public string weather;
    public string opponent;
    public string myDeck;
    public string opponentDeck;
    public string result;
}

[System.Serializable]
public class BattleRecordItem
{
    public string result;
    public string weather;
    public DeckPreset myDeck;
    public OpponentDeck opponentDeck;
}
