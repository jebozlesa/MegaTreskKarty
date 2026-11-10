using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class GeneratedCard
{
    public string CardID;
    public int StyleID;
    public string PersonName;
    public int Level;
    public int Experience;
    public int Health; // Permanent HP in the collection; remaining HP in a battle copy.
    public int MaxHealth; // Battle transport only; not stored in PlayerCards.
    public int Strength;
    public int Speed;
    public int Attack;
    public int Defense;
    public int Knowledge;
    public int Charisma;
    public int[] Color;
    public int Attack1;
    public int Attack2;
    public int Attack3;
    public int Attack4;
    public string CardPicture;

}
