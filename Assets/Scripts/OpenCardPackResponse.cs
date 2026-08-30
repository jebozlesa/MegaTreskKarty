using System;
using System.Collections.Generic;

[Serializable]
public class OpenCardPackResponse
{
    public bool success;
    public List<GeneratedCard> cards;
    public string playerId;
    public int packIndex;
    public string requestId;
    public int price;
    public string currencyCode;
    public int playerCardsCount;
    public bool firstDeckEnsured;
    public bool deckStateChanged;
    public bool requiresManualReview;
    public string stage;
    public string error;
}
