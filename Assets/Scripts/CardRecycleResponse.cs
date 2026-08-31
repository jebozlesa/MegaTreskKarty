using System;

[Serializable]
public class CardRecycleResponse
{
    public bool success;
    public string playerId;
    public string cardId;
    public string requestId;
    public string requestedRequestId;
    public string cardName;
    public int reward;
    public string currencyCode;
    public int playerCardsCount;
    public int balanceAfter;
    public bool alreadyProcessed;
    public bool requiresManualReview;
    public string stage;
    public string error;
    public CardRecycleDeckUsage deckUsage;
}

[Serializable]
public class CardRecycleDeckUsage
{
    public string contextId;
    public string deckId;
}
