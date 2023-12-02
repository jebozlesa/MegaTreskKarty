using System.Collections.Generic;

[System.Serializable]
public class Deck
{
    public string DeckID;
    public string DeckName;
    public string Card1;
    public string Card2;
    public string Card3;
    public string Card4;
    public string Card5;

    // Metóda na získanie ID všetkých kariet v balíčku
    public List<string> GetCardIDs()
    {
        return new List<string> { Card1, Card2, Card3, Card4, Card5 };
    }
}
