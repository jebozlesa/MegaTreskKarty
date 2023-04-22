using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CardsData : MonoBehaviour
{
    private List<string> cards = new List<string>();

    // Táto funkcia bude uložiť údaje o kartách do textového súboru
    public void SaveCardData()
    {
        // Vytvor si nový textový súbor alebo prepíš existujúci
        string filePath = Path.Combine(Application.dataPath, "files", "CardsData.txt");

        // Otvor súbor a zapíš údaje
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (string card in cards)
            {
                writer.WriteLine(card);
            }
        }
    }
}
