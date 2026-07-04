using System.Collections.Generic;
using UnityEngine;

public class PlayFabCardManager : MonoBehaviour
{
    [System.Serializable]
    public class CardsContainer
    {
        public List<CardData> cards;
    }

    [System.Serializable]
    public class CardData
    {
        public string CardID;
        public int StyleID;
        public string PersonName;
        public int Level;
        public int Experience;
        public int Health;
        public int Strength;
        public int Speed;
        public int Attack;
        public int Defense;
        public int Knowledge;
        public int Charisma;
        public List<int> Color;
        public int Attack1;
        public int Attack2;
        public int Attack3;
        public int Attack4;
        public string CardPicture;
    }
}
