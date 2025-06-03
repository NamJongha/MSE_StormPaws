using System.Collections.Generic;

[System.Serializable]
public class DeckCard
{
    public string id;
    public Card card;
    public int pos;
    public int cardQuantity;
}

[System.Serializable]
public class DeckPreset
{
    public string id;
    public string deckName;
    public List<DeckCard> decklist;
}

[System.Serializable]
public class DeckListData
{
    public List<DeckPreset> items;
}

[System.Serializable]
public class DeckListResponse
{
    public bool success;
    public string message;
    public DeckListData data;
}

[System.Serializable]
public class SelectedMyDeckResponse
{
    public bool success;
    public string message;
    public DeckPreset data;
}

[System.Serializable]
public class OpponentDeck
{
    public string id;
    public string name;
    public User user;
    public List<DeckCard> decklist;

    public string ownerName => user != null ? user.name : "Unknown";

    [System.Serializable]
    public class User
    {
        public string id;
        public string name;
    }
}

[System.Serializable]
public class SelectedOpponentResponse
{
    public bool success;
    public string message;
    public OpponentDeck data;
}

[System.Serializable]
public class OpponentDeckListResponse
{
    public bool success;
    public string message;
    public OpponentDeckListData data;
}

[System.Serializable]
public class OpponentDeckListData
{
    public List<OpponentDeck> items;
    public int totalItems;
    public int totalPages;
    public int currentPage;
    public int pageSize;
    public bool hasPrevious;
    public bool hasNext;
}