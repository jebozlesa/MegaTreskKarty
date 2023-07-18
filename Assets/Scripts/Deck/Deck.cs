using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Deck
{
    public string DeckID { get; set; }
    public string DeckName { get; set; }
    public List<int> CardIDs { get; set; }
}
