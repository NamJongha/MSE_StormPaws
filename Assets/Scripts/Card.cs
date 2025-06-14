using System.Collections.Generic;

[System.Serializable]
public class Card
{
    public string id;
    public string name;
    public int attackPower;
    public int attackSpeed;
    public int health;
    public string cardType;
    public float additionalCoefficient;
}

[System.Serializable]
public class CardListData
{
    public List<Card> items;
    public int totalItems;
    public int pageSize;
    public int currentPage;
    public int totalPages;
    public bool hasPrevious;
    public bool hasNext;
}

[System.Serializable]
public class CardListResponse
{
    public bool success;
    public string message;
    public CardListData data;
}