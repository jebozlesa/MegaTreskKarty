using Newtonsoft.Json.Linq;
using UnityEngine;

[System.Serializable]
public class SelectedCardData
{
    public string playerId;
    public string cardId;
    public string name;
    public string image;
    public int level;
    public int health;
    public int maxHealth;
    public int styleId;
    public int strength;
    public int speed;
    public int attack;
    public int defense;
    public int knowledge;
    public int charisma;
    public int experience;
    public int attack1;
    public int attack2;
    public int attack3;
    public int attack4;
    public int[] color;
    public EffectData[] effects;  // ✅ Support pre effects (burn, sleep, atď.)
    
    [System.Serializable]
    public class EffectData
    {
        public string type;        // "burn", "sleep", "stun", atď.
        public int duration;       // Počet turno v
        public int appliedTurn;    // Kedy bol aplikovaný
        public int value;          // Optional value (napr. burn damage per turn)
    }

    public static SelectedCardData FromCard(Kard card, GeneratedCard definition, string ownerPlayerId)
    {
        if (card == null)
        {
            Debug.LogError("[SelectedCardData] Cannot create payload from null Kard instance.");
            return null;
        }

        int resolvedMaxHealth = definition != null && definition.MaxHealth > 0 ? definition.MaxHealth : card.health;
        int resolvedStyleId = definition?.StyleID ?? card.styleId;
        int resolvedStrength = definition?.Strength ?? card.strength;
        int resolvedSpeed = definition?.Speed ?? card.speed;
        int resolvedAttack = definition?.Attack ?? card.attack;
        int resolvedDefense = definition?.Defense ?? card.defense;
        int resolvedKnowledge = definition?.Knowledge ?? card.knowledge;
        int resolvedCharisma = definition?.Charisma ?? card.charisma;
        int resolvedExperience = definition?.Experience ?? card.experience;
        int[] resolvedColor = definition?.Color ?? new int[] { card.color.r, card.color.g, card.color.b };

        SelectedCardData data = new SelectedCardData
        {
            playerId = ownerPlayerId,
            cardId = card.cardId,
            name = card.cardName,
            image = card.image,
            level = card.level,
            health = card.health,
            maxHealth = resolvedMaxHealth,
            styleId = resolvedStyleId,
            strength = resolvedStrength,
            speed = resolvedSpeed,
            attack = resolvedAttack,
            defense = resolvedDefense,
            knowledge = resolvedKnowledge,
            charisma = resolvedCharisma,
            experience = resolvedExperience,
            attack1 = card.attack1,
            attack2 = card.attack2,
            attack3 = card.attack3,
            attack4 = card.attack4,
            color = resolvedColor,
            effects = null  // ✅ Effects budú z DB pri refresh
        };

        return data;
    }

    public static SelectedCardData FromJson(string ownerPlayerId, JObject payload)
    {
        if (payload == null)
        {
            return null;
        }

        return new SelectedCardData
        {
            playerId = ownerPlayerId,
            cardId = payload.Value<string>("cardId"),
            name = payload.Value<string>("name"),
            image = payload.Value<string>("image"),
            level = payload.Value<int?>("level") ?? 1,
            health = payload.Value<int?>("health") ?? 0,
            maxHealth = payload.Value<int?>("maxHealth") ?? payload.Value<int?>("health") ?? 0,
            styleId = payload.Value<int?>("styleId") ?? 0,
            strength = payload.Value<int?>("strength") ?? 0,
            speed = payload.Value<int?>("speed") ?? 0,
            attack = payload.Value<int?>("attack") ?? 0,
            defense = payload.Value<int?>("defense") ?? 0,
            knowledge = payload.Value<int?>("knowledge") ?? 0,
            charisma = payload.Value<int?>("charisma") ?? 0,
            experience = payload.Value<int?>("experience") ?? 0,
            attack1 = payload.Value<int?>("attack1") ?? 0,
            attack2 = payload.Value<int?>("attack2") ?? 0,
            attack3 = payload.Value<int?>("attack3") ?? 0,
            attack4 = payload.Value<int?>("attack4") ?? 0,
            color = payload["color"] is JArray colorArray ? colorArray.ToObject<int[]>() : null,
            effects = ParseEffects(payload["effects"])  // ✅ Parse effects z servera
        };
    }
    
    /// <summary>
    /// Parsuje effects array z JSON
    /// </summary>
    private static EffectData[] ParseEffects(JToken effectsToken)
    {
        if (effectsToken == null || effectsToken.Type != JTokenType.Array)
        {
            return null;
        }
        
        try
        {
            var effectsArray = (JArray)effectsToken;
            var effects = new System.Collections.Generic.List<EffectData>();
            
            foreach (var effectObj in effectsArray)
            {
                if (effectObj is JObject jobj)
                {
                    effects.Add(new EffectData
                    {
                        type = jobj.Value<string>("type"),
                        duration = jobj.Value<int?>("duration") ?? 0,
                        appliedTurn = jobj.Value<int?>("appliedTurn") ?? 0,
                        value = jobj.Value<int?>("value") ?? 0
                    });
                }
            }
            
            return effects.Count > 0 ? effects.ToArray() : null;
        }
        catch
        {
            return null;
        }
    }

    public GeneratedCard ToGeneratedCard()
    {
        return new GeneratedCard
        {
            CardID = cardId,
            StyleID = styleId,
            PersonName = name,
            Level = level,
            Experience = experience,
            Health = health,
            MaxHealth = maxHealth,
            Strength = strength,
            Speed = speed,
            Attack = attack,
            Defense = defense,
            Knowledge = knowledge,
            Charisma = charisma,
            Color = color ?? new int[] { 255, 255, 255 },
            Attack1 = attack1,
            Attack2 = attack2,
            Attack3 = attack3,
            Attack4 = attack4,
            CardPicture = image
        };
    }
}
