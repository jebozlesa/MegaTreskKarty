using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public enum CardState {ATTACK,MAYBE,STAY}

public class Kard : MonoBehaviour//, IPointerClickHandler 
{
    public string cardName;
    public int health;
    public int strength;
    public int speed;
    public int attack;
    public int defense;
    public int knowledge;
    public int charisma;
    public string image;
    public Color32 color;
    public int level;

    int maxHP;

    public Dictionary<int, int> attackCount;

    public int[] attacks = new int[5];
    public int[] countAttacks = new int[5];

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

    private void Start()
    {
        nameText.text = cardName;
        levelText.text = "lvl " + level;

        maxHP = health; 

        cardImage.sprite = Resources.Load<Sprite>(image);

        background.GetComponent<Image>().color = color;
        nameText.color = color;
        levelText.color = color;
        
        InitializeAttackCount();
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
        float angle = Random.Range(0f, Mathf.PI * 2f); // náhodný úhel v radiánech
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
            transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-maxAngle, maxAngle));
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
        yield return new WaitForSeconds(1f);
        Debug.Log(Time.time + "  " + cardName + " pridava efekt " + id + ", " + param);

        bool idExists = false;

        // Kontrola, či efekt s daným ID existuje a toto ID nie je 1 alebo 4
        for (int i = 0; i < effects.Count; i++)
        {
            int effectId = effects[i][0];
            if (effectId == id && id != 1 && id != 4)
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
            StartCoroutine(ShakeCard(0.2f));
            Debug.Log(Time.time + "  " + cardName + " pridal efekt " + id + ", " + param);
            yield return new WaitForSeconds(1f);
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

    public void RemoveEffectsById(int id)
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

    public void TakeDamage(int dmg)
	{
        if (dmg <= 0)
			dmg = 1;
		health -= dmg;
        StartCoroutine(ShakeCard((float)dmg));
        StartCoroutine(EffectAnimations(dmg, "HP", color_red));
        Debug.Log(Time.time + "  " + cardName+" dostal za "+dmg);
	}

    public void Heal(int amount)
	{
        Debug.Log(Time.time + "  " + cardName+" sa healuje za "+amount);
		health += amount;
        StartCoroutine(EffectAnimations(amount, "HP", color_green));
		if (health > maxHP)
			health = maxHP;
	}

    public void HandleStrength(int amount)
	{
        Debug.Log(Time.time + "  " + cardName+" nemi silu o "+amount);
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
        Debug.Log(Time.time + "  " + cardName+" meni rychlost o "+amount);
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
        Debug.Log(Time.time + "  " + cardName+" meni utok o "+amount);
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
        Debug.Log(Time.time + "  " + cardName+" meni obranu o "+amount);
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
        Debug.Log(Time.time + "  " + cardName+" meni vedomosti o "+amount);
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
        Debug.Log(Time.time + "  " + cardName+" meni charizmu o "+amount);
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
