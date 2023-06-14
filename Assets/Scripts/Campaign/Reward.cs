using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reward
{
    public int Gold { get; private set; }
    public Card Card { get; private set; } // Predpokladáme, že existuje trieda Card
    public Dictionary<string, int> Attacks { get; private set; } 

    // Konštruktor pre triedu Reward
    public Reward(int gold, Card card, Dictionary<string, int> attacks)
    {
        Gold = gold;
        Card = card;
        Attacks = attacks;
    }
}

