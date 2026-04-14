using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 80: Niten Ichi-ryu
/// Server stores the total damage in battle state and passes the split beats through attackResult.
/// This handler renders the two SP attack phases separately.
/// </summary>
public class Attack80Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        int damage,
        bool isMyAttack,
        AttackAnimations animations,
        MultiplayerCardAnimator cardAnimator,
        HealthBar playerLifeBar,
        HealthBar enemyLifeBar,
        System.Func<string, IEnumerator> showDialog,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Niten Ichi-Ryu");

        int katanaDamage = 0;
        int wakizashiDamage = 0;
        ParseSplitDamage(attackResult, out katanaDamage, out wakizashiDamage);

        if (katanaDamage <= 0 && wakizashiDamage <= 0 && damage > 0)
        {
            katanaDamage = damage;
        }

        yield return animations.PlayNitenIchiRyuKatanaAnimation(attacker.transform, defender.transform);
        if (katanaDamage > 0)
        {
            yield return BattleValuePlayback.PlayDamage(
                defender,
                katanaDamage,
                !isMyAttack,
                cardAnimator,
                playerLifeBar,
                enemyLifeBar
            );
        }

        yield return animations.PlayNitenIchiRyuWakizashiAnimation(attacker.transform, defender.transform);
        if (wakizashiDamage > 0)
        {
            yield return BattleValuePlayback.PlayDamage(
                defender,
                wakizashiDamage,
                !isMyAttack,
                cardAnimator,
                playerLifeBar,
                enemyLifeBar
            );
        }

        yield return showDialog($"{attacker.cardName} attacks wit Katana and Wakizashi");
    }

    private static void ParseSplitDamage(string attackResult, out int katanaDamage, out int wakizashiDamage)
    {
        katanaDamage = 0;
        wakizashiDamage = 0;

        if (string.IsNullOrEmpty(attackResult))
        {
            return;
        }

        string[] parts = attackResult.Split('|');
        foreach (string part in parts)
        {
            string[] keyValue = part.Split(':');
            if (keyValue.Length != 2)
            {
                continue;
            }

            if (!int.TryParse(keyValue[1], out int parsed))
            {
                continue;
            }

            if (keyValue[0] == "katana")
            {
                katanaDamage = parsed;
            }
            else if (keyValue[0] == "wakizashi")
            {
                wakizashiDamage = parsed;
            }
        }
    }
}
