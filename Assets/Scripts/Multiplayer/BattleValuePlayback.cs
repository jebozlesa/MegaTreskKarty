using System.Collections;
using UnityEngine;

public static class BattleValuePlayback
{
    public static IEnumerator PlayDamage(
        Kard card,
        int damage,
        bool isMyCard,
        MultiplayerCardAnimator cardAnimator,
        HealthBar playerLifeBar,
        HealthBar enemyLifeBar)
    {
        if (card == null || damage <= 0)
        {
            yield break;
        }

        ApplyDamage(card, damage);

        if (cardAnimator != null)
        {
            yield return cardAnimator.AnimateDamage(card, damage);
        }

        SyncHealthBar(card, isMyCard, playerLifeBar, enemyLifeBar);
    }

    public static IEnumerator PlayHeal(
        Kard card,
        int healAmount,
        bool isMyCard,
        MultiplayerCardAnimator cardAnimator,
        HealthBar playerLifeBar,
        HealthBar enemyLifeBar)
    {
        if (card == null || healAmount <= 0)
        {
            yield break;
        }

        ApplyHeal(card, healAmount);

        if (cardAnimator != null)
        {
            yield return cardAnimator.AnimateHeal(card, healAmount);
        }

        SyncHealthBar(card, isMyCard, playerLifeBar, enemyLifeBar);
    }

    public static void ApplyDamage(Kard card, int damage)
    {
        if (card == null || damage <= 0)
        {
            return;
        }

        card.health = Mathf.Max(0, card.health - damage);
    }

    public static void ApplyHeal(Kard card, int healAmount)
    {
        if (card == null || healAmount <= 0)
        {
            return;
        }

        card.health = Mathf.Min(card.maxHealth, card.health + healAmount);
    }

    public static void SyncHealthBar(
        Kard card,
        bool isMyCard,
        HealthBar playerLifeBar,
        HealthBar enemyLifeBar)
    {
        if (card == null)
        {
            return;
        }

        if (isMyCard)
        {
            playerLifeBar?.SetHP(card.health);
        }
        else
        {
            enemyLifeBar?.SetHP(card.health);
        }
    }
}
