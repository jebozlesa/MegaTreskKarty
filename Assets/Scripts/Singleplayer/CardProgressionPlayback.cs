using System.Collections;
using UnityEngine;

public class CardProgressionPlayback
{
    private static readonly Color32 XpColor = new Color32(140, 89, 255, 255);
    private static readonly Color32 LevelColor = new Color32(255, 209, 46, 255);

    public IEnumerator Play(CardProgressionDto progression, Kard card)
    {
        if (
            progression == null
            || card == null
            || progression.applied == false
            || progression.xpGained <= 0
        )
        {
            yield break;
        }

        card.experience = progression.experienceAfter;
        if (progression.levelAfter > 0)
        {
            card.level = progression.levelAfter;
            if (card.levelText != null)
            {
                card.levelText.text = "lvl " + card.level;
            }
        }

        yield return card.EffectAnimations(progression.xpGained, "XP", XpColor);

        if (progression.leveledUp && progression.levelAfter > 0)
        {
            yield return card.EffectAnimations(progression.levelUps, "LVL", LevelColor);
        }
    }
}
