using System.IO;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public int level = 1;
    public int money = 0;
    public int experience = 0;

    void Start()
    {
        LoadPlayerData();
    }

    // Táto funkcia bude uložiť údaje hráča do textového súboru
    public void SavePlayerData()
    {
        // Pripoj textový súbor k objektu s týmto skriptom
        string filePath = Path.Combine(Application.dataPath, "files", "PlayerData.txt");

        // Otvor súbor a zapíš údaje
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("lvl=" + level);
            writer.WriteLine("money=" + money);
            writer.WriteLine("experience=" + experience);
        }
    }

    // Táto funkcia bude načítať údaje hráča zo textového súboru
    public void LoadPlayerData()
    {
        // Pripoj textový súbor k objektu s týmto skriptom
        string filePath = Path.Combine(Application.dataPath, "files", "PlayerData.txt");

        // Načítaj údaje zo súboru
        if (File.Exists(filePath))
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] data = line.Split('=');
                    if (data.Length == 2)
                    {
                        if (data[0] == "lvl")
                            level = int.Parse(data[1]);
                        else if (data[0] == "money")
                            money = int.Parse(data[1]);
                        else if (data[0] == "experience")
                            experience = int.Parse(data[1]);
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("Súbor neexistuje.");
        }
    }
}
