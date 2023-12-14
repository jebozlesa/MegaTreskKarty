using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Data;
using Mono.Data.Sqlite;
using System;

public enum CardState { ATTACK, MAYBE, STAY }

public class Kard : MonoBehaviour, IAttackCount//, IPointerClickHandler 
{
    public string cardId;
    public int styleId;
    public string cardName;

    public string image;
    public Color32 color;
    public int level;

    public int health { get; set; }
    public int strength { get; set; }
    public int speed { get; set; }
    public int attack { get; set; }
    public int defense { get; set; }
    public int knowledge { get; set; }
    public int charisma { get; set; }

    int maxHP;

    public Dictionary<int, int> attackCount;

    public int attack1;
    public int attack2;
    public int attack3;
    public int attack4;

    public int countAttack1;
    public int countAttack2;
    public int countAttack3;
    public int countAttack4;

    public TMP_Text nameText;
    public TMP_Text levelText;

    public Image cardImage;
    public Sprite cardSprite;

    public Image background;

    public GameObject cardPrefab;
    public Transform board; // Referencia na hraciu plochu

    public List<List<int>> effects = new List<List<int>>();

    public CardState state = CardState.ATTACK;

    public GameObject notsureGO;
    public TMP_Text notsureText;

    Color32 color_green = new Color32(0, 255, 0, 255);
    Color32 color_red = new Color32(255, 0, 0, 255);
    Color32 color_blue = new Color32(0, 0, 255, 255);
    Color32 color_purple = new Color32(139, 17, 204, 255);
    Color32 color_yellow = new Color32(255, 181, 24, 255);

    public int experience;
    public TextAsset playerCardDatabase;
    private List<string> kartyHrac;

    private string connectionString;

    public bool isDragable = true;

    public GameObject battleArea;

    public bool priorityAttack;

    private PlayFabCardManager playFabManager = new PlayFabCardManager();



    private void Start()
    {
        //Debug.Log("MegaTresk: " + DateTime.Now.ToString("HH:mm:ss.fff") + "Kard.Start => START");

        connectionString = $"URI=file:{Database.Instance.GetDatabasePath()}";

        nameText.text = cardName;
        levelText.text = "lvl " + level;
        maxHP = health;
        cardImage.sprite = Resources.Load<Sprite>("Cards/" + image);
        background.GetComponent<Image>().color = color;
        nameText.color = color;
        levelText.color = color;

        InitializeAttackCount();

        //LoadPlayerCardData();
    }

    // private void LoadPlayerCardData()
    // {
    //     Debug.Log("MegaTresk: " + DateTime.Now.ToString("HH:mm:ss.fff") + "Kard.LoadPlayerCardData => START");
    //     kartyHrac = LoadPlayerCardFromDatabase(cardId);
    // }

    // private List<string> LoadPlayerCardFromDatabase(int cardId)
    // {
    //     Debug.Log("MegaTresk: " + DateTime.Now.ToString("HH:mm:ss.fff") + "Kard.LoadPlayerCardFromDatabase => START " + cardId);
    //     List<string> kartaData = new List<string>();

    //     IDbConnection dbConnection = new SqliteConnection(connectionString);
    //     dbConnection.Open();

    //     IDbCommand dbCommand = dbConnection.CreateCommand();
    //     dbCommand.CommandText = $"SELECT * FROM PlayerCards WHERE CardID = {cardId}";
    //     IDataReader reader = dbCommand.ExecuteReader();

    //     while (reader.Read())
    //     {
    //         string cardDataString = "";
    //         for (int i = 0; i < reader.FieldCount; i++)
    //         {
    //             cardDataString += reader.GetValue(i).ToString();
    //             if (i < reader.FieldCount - 1) cardDataString += ",";
    //         }
    //         kartaData.Add(cardDataString);
    //     }

    //     reader.Close();
    //     dbCommand.Dispose();
    //     dbConnection.Close();

    //     return kartaData;
    // }


    private void LoadCardData()
    {
        //Debug.Log("MegaTresk: " + DateTime.Now.ToString("HH:mm:ss.fff") + " Kard.LoadCardData => START");

        levelText.text = "lvl " + level;
    }

    // public void AddExperience(int increase)
    // {
    //     Debug.Log("MegaTresk: " + DateTime.Now.ToString("HH:mm:ss.fff") + " Kard.AddExperience => START " + increase);

    //     //bool lvlUp = false;
    //     Debug.Log("LoadCardData()");

    //     // Connect to the database
    //     using (IDbConnection dbConnection = new SqliteConnection(connectionString))
    //     {
    //         dbConnection.Open();
    //         Debug.Log("DATABAZA PRIPOJENA");

    //         // Get the current experience and level
    //         using (IDbCommand dbCommand = dbConnection.CreateCommand())
    //         {
    //             dbCommand.CommandText = $"SELECT Level, Experience FROM PlayerCards WHERE CardID = {cardId}";
    //             using (IDataReader reader = dbCommand.ExecuteReader())
    //             {
    //                 if (reader.Read())
    //                 {
    //                     int currentExperience = reader.GetInt32(1);
    //                     int currentLevel = reader.GetInt32(0);
    //                     int newExperience = currentExperience + increase;

    //                     // Update the experience
    //                     UpdateStat("Experience", increase, dbConnection);
    //                     StartCoroutine(EffectAnimations(increase, "XP", color_purple));
    //                     experience = newExperience;

    //                     // Check if player leveled up
    //                     if (currentLevel * (10*(currentLevel)) <= newExperience)                                             //LEVEL   <========== dat potom nadruhu corentlevel
    //                     {
    //                         //lvlUp = true;
    //                         UpdateStat("Level", 1, dbConnection);
    //                         UpdateRandomStat(dbConnection);
    //                         level = currentLevel + 1;
    //                         StartCoroutine(EffectAnimations(level, "LVL", color_yellow));
    //                     }

    //                 }
    //             }
    //             dbCommand.Dispose();
    //             dbConnection.Close();
    //         }
    //     }

    //     LoadCardData();
    // }

    // public void UpdateRandomStat(IDbConnection dbConnection)
    // {

    //     switch ((int)UnityEngine.Random.Range(2, 9))
    //     {
    //         case 2:
    //             Heal(2);
    //             UpdateStat("Health", 2, dbConnection);
    //             StartCoroutine(EffectAnimations(2, "HP", color_blue));
    //             break;
    //         case 3:
    //             HandleStrength(1);
    //             UpdateStat("Strength", 1, dbConnection);
    //             StartCoroutine(EffectAnimations(1, "STR", color_blue));
    //             break;
    //         case 4:
    //             HandleSpeed(1);
    //             UpdateStat("Speed", 1, dbConnection);
    //             StartCoroutine(EffectAnimations(1, "SPE", color_blue));
    //             break;
    //         case 5:
    //             HandleAttack(1);
    //             UpdateStat("Attack", 1, dbConnection);
    //             StartCoroutine(EffectAnimations(1, "ATT", color_blue));
    //             break;
    //         case 6:
    //             HandleDefense(1);
    //             UpdateStat("Defense", 1, dbConnection);
    //             StartCoroutine(EffectAnimations(1, "DEF", color_blue));
    //             break;
    //         case 7:
    //             HandleKnowledge(1);
    //             UpdateStat("Knowledge", 1, dbConnection);
    //             StartCoroutine(EffectAnimations(1, "KNO", color_blue));
    //             break;
    //         case 8:
    //             HandleCharisma(1);
    //             UpdateStat("Charisma", 1, dbConnection);
    //             StartCoroutine(EffectAnimations(1, "CHA", color_blue));
    //             break;
    //         default:
    //             Debug.LogError("Invalid shit fak UpdateRandomStat(int index,int increase)");
    //             break;
    //     }
    // }

    // public void UpdateStat(string parameterName, int difference, IDbConnection dbConnection)
    // {
    //     if (string.IsNullOrEmpty(parameterName)) return;

    //     // Aktualizácia hodnôt karty s rozdielmi
    //     using (IDbCommand dbCommand = dbConnection.CreateCommand())
    //     {
    //         string query = $"UPDATE PlayerCards SET {parameterName} = {parameterName} + @{parameterName} WHERE CardID = @CardID";
    //         Debug.Log("TU JE TO   " + query);

    //         dbCommand.CommandText = query;

    //         // Pridajte hodnoty parametrov pre aktualizáciu do SQL dotazu
    //         IDbDataParameter parameter = dbCommand.CreateParameter();
    //         parameter.ParameterName = $"@{parameterName}";
    //         parameter.Value = difference;
    //         dbCommand.Parameters.Add(parameter);

    //         IDbDataParameter cardIdParameter = dbCommand.CreateParameter();
    //         cardIdParameter.ParameterName = "@CardID";
    //         cardIdParameter.Value = cardId;
    //         dbCommand.Parameters.Add(cardIdParameter);

    //         dbCommand.ExecuteNonQuery();
    //     }

    // }

    public IEnumerator AddExperience(int increase)
    {
        Debug.Log("MegaTresk: " + DateTime.Now.ToString("HH:mm:ss.fff") + " Kard.AddExperience => START " + increase);

        Dictionary<string, object> cardDataDictionary = null;
        bool updateSuccess = false;

        // Získanie údajov o karte
        yield return StartCoroutine(playFabManager.GetCardData(cardId, data =>
        {
            cardDataDictionary = data;
        }));

        if (cardDataDictionary != null)
        {
            // Konvertujte slovník na objekt CardData
            PlayFabCardManager.CardData cardData = ConvertDictionaryToCardData(cardDataDictionary);

            int currentExperience = cardData.Experience;
            int currentLevel = cardData.Level;
            int newExperience = currentExperience + increase;

            Dictionary<string, string> updates = new Dictionary<string, string>
        {
            { "Experience", newExperience.ToString() }
        };

            bool leveledUp = false;
            // Check if player leveled up
            if (currentLevel * (10 * (currentLevel)) <= newExperience)
            {
                updates.Add("Level", (currentLevel + 1).ToString());
                leveledUp = true;
            }

            // Aktualizujte údaje o karte
            yield return StartCoroutine(playFabManager.UpdateCardData(cardId, updates, success =>
            {
                updateSuccess = success;
            }));

            if (updateSuccess)
            {
                StartCoroutine(EffectAnimations(increase, "XP", color_purple));
                experience = newExperience;

                if (leveledUp)
                {
                    UpdateRandomStat();
                    StartCoroutine(EffectAnimations(level, "LVL", color_yellow));
                    level += 1;
                    LoadCardData();
                }
            }
        }
        else
        {
            Debug.LogError("ERROR: cardDataDictionary == null");
        }
    }



    private PlayFabCardManager.CardData ConvertDictionaryToCardData(Dictionary<string, object> cardDataDictionary)
    {
        //Debug.Log("MegaTresk: " + DateTime.Now.ToString("HH:mm:ss.fff") + " Kard.PlayFabCardManager.CardData => START ");

        PlayFabCardManager.CardData cardData = new PlayFabCardManager.CardData();
        cardData.CardID = cardDataDictionary["StyleID"].ToString();
        cardData.Experience = Convert.ToInt32(cardDataDictionary["Experience"]);
        cardData.Level = Convert.ToInt32(cardDataDictionary["Level"]);
        cardData.StyleID = Convert.ToInt32(cardDataDictionary["StyleID"]);
        cardData.PersonName = cardDataDictionary["PersonName"].ToString();
        cardData.Health = Convert.ToInt32(cardDataDictionary["Health"]);
        cardData.Strength = Convert.ToInt32(cardDataDictionary["Strength"]);
        cardData.Speed = Convert.ToInt32(cardDataDictionary["Speed"]);
        cardData.Attack = Convert.ToInt32(cardDataDictionary["Attack"]);
        cardData.Defense = Convert.ToInt32(cardDataDictionary["Defense"]);
        cardData.Knowledge = Convert.ToInt32(cardDataDictionary["Knowledge"]);
        cardData.Charisma = Convert.ToInt32(cardDataDictionary["Charisma"]);
        cardData.Color = (List<int>)cardDataDictionary["Color"];
        cardData.Attack1 = Convert.ToInt32(cardDataDictionary["Attack1"]);
        cardData.Attack2 = Convert.ToInt32(cardDataDictionary["Attack2"]);
        cardData.Attack3 = Convert.ToInt32(cardDataDictionary["Attack3"]);
        cardData.Attack4 = Convert.ToInt32(cardDataDictionary["Attack4"]);
        cardData.CardPicture = cardDataDictionary["CardPicture"].ToString();

        Debug.Log(cardData.PersonName);

        return cardData;
    }

    public bool HasAvailableAttacks()
    {
        return countAttack1 > 0 || countAttack2 > 0 || countAttack3 > 0 || countAttack4 > 0;
    }

    public void UpdateRandomStat()
    {
        Debug.Log("MegaTresk: " + DateTime.Now.ToString("HH:mm:ss.fff") + " Kard.UpdateRandomStat => START ");

        playFabManager.GetCardData(cardId, cardDataDictionary =>
        {
            if (cardDataDictionary != null)
            {
                // Konvertujte slovník na objekt CardData
                PlayFabCardManager.CardData cardData = ConvertDictionaryToCardData(cardDataDictionary);

                string statName = "";
                int increaseValue = 0;

                switch ((int)UnityEngine.Random.Range(2, 9))
                {
                    case 2:
                        cardData.Health += 2;
                        statName = "Health";
                        increaseValue = cardData.Health; // Aktualizujte na novú celkovú hodnotu
                        StartCoroutine(EffectAnimations(2, "HP", color_blue));
                        break;
                    case 3:
                        cardData.Strength += 1;
                        statName = "Strength";
                        increaseValue = cardData.Strength;
                        StartCoroutine(EffectAnimations(1, "STR", color_blue));
                        break;
                    case 4:
                        cardData.Speed += 1;
                        statName = "Speed";
                        increaseValue = cardData.Speed;
                        StartCoroutine(EffectAnimations(1, "SPE", color_blue));
                        break;
                    case 5:
                        cardData.Attack += 1;
                        statName = "Attack";
                        increaseValue = cardData.Attack;
                        StartCoroutine(EffectAnimations(1, "ATT", color_blue));
                        break;
                    case 6:
                        cardData.Defense += 1;
                        statName = "Defense";
                        increaseValue = cardData.Defense;
                        StartCoroutine(EffectAnimations(1, "DEF", color_blue));
                        break;
                    case 7:
                        cardData.Knowledge += 1;
                        statName = "Knowledge";
                        increaseValue = cardData.Knowledge;
                        StartCoroutine(EffectAnimations(1, "KNO", color_blue));
                        break;
                    case 8:
                        cardData.Charisma += 1;
                        statName = "Charisma";
                        increaseValue = cardData.Charisma;
                        StartCoroutine(EffectAnimations(1, "CHA", color_blue));
                        break;
                    default:
                        Debug.LogError("Invalid value in UpdateRandomStat");
                        break;
                }

                if (!string.IsNullOrEmpty(statName))
                {
                    Dictionary<string, string> updates = new Dictionary<string, string>
                    {
                        { statName, increaseValue.ToString() }
                    };

                    playFabManager.UpdateCardData(cardId, updates, success =>
                    {
                        if (!success)
                        {
                            Debug.LogError("Failed to update stat in PlayFab");
                        }
                    });
                }
            }
            else
            {
                Debug.Log("ERROR: cardDataDictionary == null");
            }
        });
    }




    private void InitializeAttackCount()
    {
        attackCount = new Dictionary<int, int>
        {
            { 1, countAttack1 },
            { 2, countAttack2 },
            { 3, countAttack3 },
            { 4, countAttack4 }
        };
    }

    public IEnumerator EffectAnimations(int iter, string animationText, Color32 color)
    {
        for (int i = 0; i < System.Math.Abs(iter); i++)
        {
            yield return new WaitForSeconds(0.1f);
            StartCoroutine(EffectAnimation(animationText, color));
        }
    }

    public IEnumerator EffectAnimation(string animationText, Color32 color)
    {
        notsureText.text = animationText;
        notsureText.color = color;
        GameObject notsure = Instantiate(notsureGO, transform);
        float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f); // náhodný úhel v radiánech
        float radius = 50f; // poloměr kruhu
        Vector2 randomPosition = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius); // výpočet náhodné pozice
        notsure.transform.position = (Vector2)transform.position + randomPosition; // nastavení pozice objektu
        Vector2 direction = (notsure.transform.position - transform.position).normalized; // směr pohybu objektu
        float distance = 50f; // vzdálenost, o kterou se objekt posune
        float elapsedTime = 0f; // uplynulý čas
        while (elapsedTime < 3f)
        {
            notsure.transform.position += (Vector3)(direction * distance * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Destroy(notsure);
    }


    public IEnumerator ShakeCard(float dmg)
    {
        Vector3 originalPosition = transform.position;
        Quaternion originalRotation = transform.rotation;

        float radius = 20f;
        float angle = 0f;
        float maxAngle = 15f;
        float increment = 0.02f;

        float shakeTime = dmg / 20;
        float currentTime = 0f;

        while (currentTime < shakeTime)
        {
            float x = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float y = radius * Mathf.Sin(angle * Mathf.Deg2Rad);
            Vector3 newPosition = originalPosition + new Vector3(x, y, 0f);
            transform.position = newPosition;
            transform.rotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(-maxAngle, maxAngle));
            angle += increment;
            currentTime += increment;
            yield return new WaitForSeconds(increment);
        }

        // Pridajte tieto riadky na koniec metódy ShakeCard
        float resetTime = 0.5f;
        float resetElapsed = 0f;
        while (resetElapsed < resetTime)
        {
            transform.position = Vector3.Lerp(transform.position, originalPosition, resetElapsed / resetTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, originalRotation, resetElapsed / resetTime);
            resetElapsed += Time.deltaTime;
            yield return null;
        }

        // Nastavte polohu a rotáciu karty na pôvodné hodnoty pre istotu
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }

    public bool CheckEffect(int id)
    {
        bool idExists = false;

        for (int i = 0; i < effects.Count; i++)
        {
            int effectId = effects[i][0];
            if (effectId == id)
            {
                idExists = true;
                Debug.Log(Time.time + "  " + cardName + " má efekt s ID " + id + ".");
                break;
            }
        }

        return idExists;
    }


    public IEnumerator AddEffect(int id, int param)
    {
        //    yield return new WaitForSeconds(1f);
        Debug.Log(Time.time + "  " + cardName + " pridava efekt " + id + ", " + param);

        bool idExists = false;

        // Kontrola, či efekt s daným ID existuje a toto ID nie je 1 alebo 4
        for (int i = 0; i < effects.Count; i++)
        {
            int effectId = effects[i][0];
            if (effectId == id && id != 1 && id != 4 && id != 16)
            {
                idExists = true;
                Debug.Log(Time.time + "  " + cardName + " má již efekt s ID " + id + ".");
                break;
            }
        }

        if (!idExists)
        {
            // ID sa v zozname nevyskytuje alebo ide o ID 1 alebo 4, efekt pridáme
            effects.Add(new List<int>());
            effects[effects.Count - 1].Add(id);
            effects[effects.Count - 1].Add(param);
            attackCount[id] = param;
            Debug.Log(Time.time + "  " + cardName + " pridal efekt " + id + ", " + param);
            yield return new WaitForSeconds(0.1f);
        }
    }
    public void RemoveEffect(int index)
    {
        Debug.Log(Time.time + "  " + cardName + " odobera efekt " + index);
        if (index < effects.Count)
        {
            effects.RemoveAt(index);
        }
    }

    public void RemoveEffectById(int id)
    {
        Debug.Log(Time.time + "  " + cardName + " odobera vsetky efekty s ID " + id);
        for (int i = effects.Count - 1; i >= 0; i--)
        {
            int effectId = effects[i][0];
            if (effectId == id)
            {
                effects.RemoveAt(i);
            }
        }
    }

    public void RemoveEffectsById(int[] id)
    {
        for (int j = 0; j < id.Length - 1; j++)
        {
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                int effectId = effects[i][0];
                if (effectId == id[j])
                {
                    Debug.Log(Time.time + "  " + cardName + " odobera vsetky efekty s ID " + id);
                    effects[i][0] = 0;
                }
            }
        }
    }

    public void TakeDamage(int dmg)
    {
        if (dmg <= 0)
            dmg = 1;
        health -= dmg;
        StartCoroutine(ShakeCard((float)dmg));
        StartCoroutine(EffectAnimations(dmg, "HP", color_red));
        Debug.Log(Time.time + "  " + cardName + " dostal za " + dmg);
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
            amount = 1;
        health += amount;
        StartCoroutine(EffectAnimations(amount, "HP", color_green));
        if (health > maxHP)
            health = maxHP;
        Debug.Log(Time.time + "  " + cardName + " sa healuje za " + amount);
    }

    public void HandleStrength(int amount)
    {
        Debug.Log(Time.time + "  " + cardName + " nemi silu o " + amount);
        if (amount < 0)
        {
            StartCoroutine(EffectAnimations(amount, "STR", color_red));
        }
        else
        {
            StartCoroutine(EffectAnimations(amount, "STR", color_green));
        }
        strength += amount;
        if (strength <= 1)
            strength = 1;
    }

    public void HandleSpeed(int amount)
    {
        Debug.Log(Time.time + "  " + cardName + " meni rychlost o " + amount);
        if (amount < 0)
        {
            StartCoroutine(EffectAnimations(amount, "SPD", color_red));
        }
        else
        {
            StartCoroutine(EffectAnimations(amount, "SPD", color_green));
        }
        speed += amount;
        if (speed <= 1)
            speed = 1;
    }

    public void HandleAttack(int amount)
    {
        Debug.Log(Time.time + "  " + cardName + " meni utok o " + amount);
        attack += amount;
        if (amount < 0)
        {
            StartCoroutine(EffectAnimations(amount, "ATT", color_red));
        }
        else
        {
            StartCoroutine(EffectAnimations(amount, "ATT", color_green));
        }
        if (attack <= 1)
            attack = 1;
    }

    public void HandleDefense(int amount)
    {
        Debug.Log(Time.time + "  " + cardName + " meni obranu o " + amount);
        defense += amount;
        if (amount < 0)
        {
            StartCoroutine(EffectAnimations(amount, "DEF", color_red));
        }
        else
        {
            StartCoroutine(EffectAnimations(amount, "DEF", color_green));
        }
        if (defense <= 1)
            defense = 1;
    }

    public void HandleKnowledge(int amount)
    {
        Debug.Log(Time.time + "  " + cardName + " meni vedomosti o " + amount);
        knowledge += amount;
        if (amount < 0)
        {
            StartCoroutine(EffectAnimations(amount, "KNW", color_red));
        }
        else
        {
            StartCoroutine(EffectAnimations(amount, "KNW", color_green));
        }
        if (knowledge <= 1)
            knowledge = 1;
    }

    public void HandleCharisma(int amount)
    {
        Debug.Log(Time.time + "  " + cardName + " meni charizmu o " + amount);
        charisma += amount;
        if (amount < 0)
        {
            StartCoroutine(EffectAnimations(amount, "CHA", color_red));
        }
        else
        {
            StartCoroutine(EffectAnimations(amount, "CHA", color_green));
        }
        if (charisma <= 1)
            charisma = 1;
    }

    // public void OnPointerClick(PointerEventData eventData)
    // {
    //     // Ak na hracej ploche nie je žiadna karta, pridaj novú
    //     if (board.childCount < 2)
    //     {
    //         // Vytvorte novú inštanciu karty z prefabrikátu
    //         GameObject newCard = Instantiate(cardPrefab);

    //         // Nastavte pozíciu karty
    //         newCard.transform.position = board.position;

    //         // Priradte kartu k hracej ploche
    //         newCard.transform.SetParent(board);
    //     }
    // }
}
