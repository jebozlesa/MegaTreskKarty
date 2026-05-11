using UnityEngine;

public static class RoyalRumbleCardMapper
{
    public static GeneratedCard ToGeneratedCard(SelectedCardData card)
    {
        if (card == null)
        {
            return null;
        }

        return new GeneratedCard
        {
            CardID = card.cardId,
            StyleID = card.styleId,
            PersonName = card.name,
            Level = card.level,
            Experience = card.experience,
            Health = card.health,
            MaxHealth = card.maxHealth > 0 ? card.maxHealth : card.health,
            Strength = card.strength,
            Speed = card.speed,
            Attack = card.attack,
            Defense = card.defense,
            Knowledge = card.knowledge,
            Charisma = card.charisma,
            Color = card.color ?? new[] { 255, 255, 255 },
            Attack1 = card.attack1,
            Attack2 = card.attack2,
            Attack3 = card.attack3,
            Attack4 = card.attack4,
            CardPicture = card.image
        };
    }
}
