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
using System.Linq;

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
    public int maxHealth { get; set; }  // Public property pre max HP
    public int strength { get; set; }
    public int speed { get; set; }
    public int attack { get; set; }
    public int defense { get; set; }
    public int knowledge { get; set; }
    public int charisma { get; set; }

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

    public Transform effectIconContainer;
    public List<GameObject> effectIcons = new List<GameObject>();



    private void Start()
    {
        //Debug.Log("MegaTresk: " + DateTime.Now.ToString("HH:mm:ss.fff") + "Kard.Start => START");

        connectionString = $"URI=file:{Database.Instance.GetDatabasePath()}";

        nameText.text = cardName;
        levelText.text = "lvl " + level;
        maxHealth = health;  // Initialize maxHealth
        cardImage.sprite = Resources.Load<Sprite>("Cards/" + image);
        background.GetComponent<Image>().color = color;
        nameText.color = color;
        levelText.color = color;

        InitializeAttackCount();

        //LoadPlayerCardData();
    }

    private void LoadCardData()
    {
        //Debug.Log("MegaTresk: " + DateTime.Now.ToString("HH:mm:ss.fff") + " Kard.LoadCardData => START");

        levelText.text = "lvl " + level;
    }


    public bool HasAvailableAttacks()
    {
        bool result = attackCount[1] > 0 || attackCount[2] > 0 || attackCount[3] > 0 || attackCount[4] > 0;
        Debug.Log(DateTime.Now.ToString("mm:ss") + " - " + cardName + " - " + result);
        return result;
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
        float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f); // nĂˇhodnĂ˝ Ăşhel v radiĂˇnech
        float radius = 50f; // polomÄ›r kruhu
        Vector2 randomPosition = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius); // vĂ˝poÄŤet nĂˇhodnĂ© pozice
        notsure.transform.position = (Vector2)transform.position + randomPosition; // nastavenĂ­ pozice objektu
        Vector2 direction = (notsure.transform.position - transform.position).normalized; // smÄ›r pohybu objektu
        float distance = 50f; // vzdĂˇlenost, o kterou se objekt posune
        float elapsedTime = 0f; // uplynulĂ˝ ÄŤas
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

        // Pridajte tieto riadky na koniec metĂłdy ShakeCard
        float resetTime = 0.5f;
        float resetElapsed = 0f;
        while (resetElapsed < resetTime)
        {
            transform.position = Vector3.Lerp(transform.position, originalPosition, resetElapsed / resetTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, originalRotation, resetElapsed / resetTime);
            resetElapsed += Time.deltaTime;
            yield return null;
        }

        // Nastavte polohu a rotĂˇciu karty na pĂ´vodnĂ© hodnoty pre istotu
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
                Debug.Log(Time.time + "  " + cardName + " mĂˇ efekt s ID " + id + ".");
                break;
            }
        }

        return idExists;
    }


    public IEnumerator AddEffect(int id, int param)
    {
        Debug.Log(Time.time + "  " + cardName + " pridĂˇva efekt " + id + ", " + param);

        // Pre efekty, ktorĂ© mĂ´Ĺľu byĹĄ na karte viackrĂˇt (ID 1 a 4)
        if (id == 1 || id == 4)
        {
            // VĹľdy pridĂˇme efekt bez kontroly duplicity
            effects.Add(new List<int> { id, param });
            AddEffectIcon(GetEffectNameById(id));
            RepositionEffectIcons();
        }
        else
        {
            // Skontrolujeme, ÄŤi efekt uĹľ existuje
            bool idExists = effects.Any(e => e[0] == id);

            if (!idExists)
            {
                // PridĂˇme efekt a ikonku
                effects.Add(new List<int> { id, param });
                AddEffectIcon(GetEffectNameById(id));
                RepositionEffectIcons();
            }
            else
            {
                Debug.Log(Time.time + "  " + cardName + " uĹľ mĂˇ efekt s ID " + id + ".");
            }
        }

        yield return new WaitForSeconds(0.1f);
    }

    // PomocnĂˇ metĂłda na kontrolu, ÄŤi bola ikonka efektu uĹľ pridanĂˇ
    private bool IsEffectIconAdded(int effectId)
    {
        string effectName = GetEffectNameById(effectId);
        Transform iconTransform = effectIconContainer.Find(effectName + "Icon");
        return iconTransform != null;
    }


    public void RemoveEffect(int index)
    {
        int effectId = effects[index][0];
        string effectName = GetEffectNameById(effectId);

        effects.RemoveAt(index);

        StartCoroutine(RemoveEffectIcon(effectName));

        // Pre efekty, ktorĂ© mĂ´Ĺľu byĹĄ na karte viackrĂˇt (ID 1 a 4)
        if (effectId == 1 || effectId == 4)
        {
            // Skontrolujeme, ÄŤi eĹˇte existujĂş ÄŹalĹˇie inĹˇtancie efektu
            bool effectStillExists = effects.Any(e => e[0] == effectId);
            if (!effectStillExists)
            {
                // OdstrĂˇnime ikonku, ak uĹľ neexistujĂş ÄŹalĹˇie inĹˇtancie
                RemoveEffectIcon(GetEffectNameById(effectId));
            }
        }
        else
        {
            // Pre ostatnĂ© efekty odstrĂˇnime ikonku okamĹľite
            RemoveEffectIcon(GetEffectNameById(effectId));
        }
    }


    public void RemoveEffectById(int id)
    {
        // OdstrĂˇnime vĹˇetky efekty s danĂ˝m ID
        effects.RemoveAll(e => e[0] == id);

        // Pre efekty, ktorĂ© mĂ´Ĺľu byĹĄ na karte viackrĂˇt (ID 1 a 4)
        if (id == 1 || id == 4)
        {
            // Skontrolujeme, ÄŤi eĹˇte existujĂş ÄŹalĹˇie inĹˇtancie efektu
            bool effectStillExists = effects.Any(e => e[0] == id);
            if (!effectStillExists)
            {
                // OdstrĂˇnime ikonku, ak uĹľ neexistujĂş ÄŹalĹˇie inĹˇtancie
                StartCoroutine(RemoveEffectIcon(GetEffectNameById(id)));
            }
        }
        else
        {
            // Pre ostatnĂ© efekty odstrĂˇnime ikonku okamĹľite
            StartCoroutine(RemoveEffectIcon(GetEffectNameById(id)));
        }
    }


    public void RemoveEffectsById(int[] ids)
    {
        foreach (int id in ids)
        {
            RemoveEffectById(id);
        }
    }


    public void AddEffectIcon(string effectName)
{
    // NaÄŤĂ­tanie ikonky z Resources
    Sprite iconSprite = Resources.Load<Sprite>("Game/EffectIcons/" + effectName);
    if (iconSprite == null)
    {
        string placeholderPath = effectName switch
        {
            "Blockade" => "Game/Animations/continentalblocade",
            "Trident" => "Game/Animations/trident",
            _ => null
        };
        if (!string.IsNullOrEmpty(placeholderPath))
        {
            iconSprite = Resources.Load<Sprite>(placeholderPath);
            if (iconSprite != null)
            {
                Debug.LogWarning($"Ikonka efektu nebola nĂˇjdenĂˇ: {effectName}. PouĹľĂ­vam placeholder sprite z {placeholderPath}.");
            }
        }

        if (iconSprite == null)
        {
            Debug.LogError("Ikonka efektu nebola nĂˇjdenĂˇ: " + effectName);
            return;
        }
    }

    // Vytvorenie unikĂˇtneho nĂˇzvu pre ikonku
    string uniqueIconName = effectName + "Icon_" + Guid.NewGuid().ToString();
    GameObject iconGO = new GameObject(uniqueIconName);

    // Pridanie komponentu Image
    Image iconImage = iconGO.AddComponent<Image>();
    iconImage.sprite = iconSprite;

    // Nastavenie rodiÄŤa na effectIconContainer
    iconGO.transform.SetParent(effectIconContainer, false);

    // Nastavenie veÄľkosti ikonky
    RectTransform rectTransform = iconGO.GetComponent<RectTransform>();
    rectTransform.sizeDelta = new Vector2(80, 80); // Nastavte veÄľkosĹĄ podÄľa vaĹˇich ikon

    // Nastavenie pivotu a anchoru ikonky
    rectTransform.anchorMin = new Vector2(1, 1); // UkotvenĂ© k pravĂ©mu hornĂ©mu rohu
    rectTransform.anchorMax = new Vector2(1, 1);
    rectTransform.pivot = new Vector2(0.5f, 1); // Pivot v strede horizontĂˇlne, hore vertikĂˇlne
}



    private int GetEffectIconIndex(string effectName)
    {
        int index = 0;
        foreach (Transform child in effectIconContainer)
        {
            if (child.name == effectName + "Icon")
            {
                // NĂˇjdeme existujĂşcu ikonku a vrĂˇtime jej index
                return index;
            }
            index++;
        }
        // Ak ikonka neexistuje, vrĂˇtime poÄŤet detĂ­ ako novĂ˝ index
        return effectIconContainer.childCount - 1;
    }


    public IEnumerator RemoveEffectIcon(string effectName)
{
    // NĂˇjdeme prvĂş ikonku, ktorĂˇ zodpovedĂˇ danĂ©mu efektu
    foreach (Transform child in effectIconContainer)
    {
        if (child.name.StartsWith(effectName + "Icon"))
        {
            Destroy(child.gameObject);
            break; // OdstrĂˇnime iba jednu ikonku
        }
    }

    // PoÄŤkĂˇme do konca frame-u, aby sa ikonka skutoÄŤne odstrĂˇnila
    yield return null;

    // Po odstrĂˇnenĂ­ ikonky preusporiadame zvyĹˇnĂ© ikonky
    RepositionEffectIcons();
}



    public void RepositionEffectIcons()
{
    int index = 0;
    //float iconWidth = 80f; // Ĺ Ă­rka ikonky
    float iconHeight = 80f; // VĂ˝Ĺˇka ikonky
    float verticalSpacing = 10f; // Medzera medzi ikonkami
    float xOffset = 0f; // HorizontĂˇlny posun ikoniek

    foreach (Transform child in effectIconContainer)
    {
        RectTransform rectTransform = child.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            float xPos = xOffset; // Posun ikonky doprava
            float yPos = -index * (iconHeight + verticalSpacing);

            rectTransform.anchoredPosition = new Vector2(xPos, yPos);
            index++;
        }
    }
}



    private string GetEffectNameById(int id)
    {
        switch (id)
        {
            case 1:
                return "bleed";
            case 2:
                return "asceticism";
            case 3:
                return "sleep";  // Boredom sleep from attack 11
            case 4:
                return "exposure";
            case 5:
                return "siege";
            case 6:
                return "fury";
            case 7:
                return "famine";
            case 8:
                return "electricity"; // OpravenĂ˝ preklep
            case 9:
                return "tether";
            case 10:
                return "starving";
            case 11:
                return "envelop";
            case 12:
                return "blockade";
            case 13:
                return "depression";
            case 14:
                return "art_inspiration"; // Pre lepĹˇiu ÄŤitateÄľnosĹĄ ikonky
            case 15:
                return "autoportrait";
            case 16:
                return "burn";
            case 17:
                return "confusion";
            case 18:
                return "satellite";
            case 19:
                return "fear";
            case 20:
                return "horns";
            case 21:
                return "calm";
            case 22:
                return "reloading";
            case 23:
                return "trident";
            case 24:
                return "poison";
            case 26:
                return "curse";
            case 27:
                return "knockout";  // Boredom sleep from attack 11
            // Pridajte ÄŹalĹˇie efekty podÄľa potreby
            default:
                Debug.LogError("NeznĂˇmy efekt s ID: " + id);
                return "unknown";
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
        if (health > maxHealth)  // âś… Use maxHealth
            health = maxHealth;
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
    //     // Ak na hracej ploche nie je Ĺľiadna karta, pridaj novĂş
    //     if (board.childCount < 2)
    //     {
    //         // Vytvorte novĂş inĹˇtanciu karty z prefabrikĂˇtu
    //         GameObject newCard = Instantiate(cardPrefab);

    //         // Nastavte pozĂ­ciu karty
    //         newCard.transform.position = board.position;

    //         // Priradte kartu k hracej ploche
    //         newCard.transform.SetParent(board);
    //     }
    // }
}
