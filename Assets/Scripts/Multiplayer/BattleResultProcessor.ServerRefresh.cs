using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BattleResultProcessor
{
    private IEnumerator RefreshCardsFromServer()
    {
        if (multiplayerService == null)
        {
            Debug.LogWarning(
                "[BattleResultProcessor] MultiplayerService not set - cannot refresh cards"
            );
            yield break;
        }

        LogVerboseBattle("[BattleResultProcessor] Refreshing selectedCards from matchState...");

        var task = multiplayerService.GetSelectedCardsFromMatchStateAsync(fightSystem.roomCode, fightSystem.myPlayerId);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Result == null || task.Result.Count <= 0)
        {
            yield break;
        }

        LogVerboseBattle(
            $"[BattleResultProcessor] Received {task.Result.Count} updated cards from server"
        );

        foreach (var kvp in task.Result)
        {
            string playerId = kvp.Key;
            var cardData = kvp.Value;

            LogVerboseBattle(
                $"[RefreshCards] Processing playerId={playerId}, cardName={cardData.name}, HP={cardData.health}/{cardData.maxHealth}"
            );
            LogVerboseBattle($"[RefreshCards] myPlayerId={fightSystem.myPlayerId}");
            LogVerboseBattle(
                $"[RefreshCards] player.cardInGame={fightSystem.player?.cardInGame?.cardName}, enemy.cardInGame={fightSystem.enemy?.cardInGame?.cardName}"
            );

            Kard card = null;
            if (playerId == fightSystem.myPlayerId && fightSystem.player?.cardInGame != null)
            {
                card = fightSystem.player.cardInGame;
                LogVerboseBattle($"[RefreshCards] Mapped to MY card: {card.cardName}");
            }
            else if (
                playerId != fightSystem.myPlayerId
                && fightSystem.enemy?.cardInGame != null
            )
            {
                card = fightSystem.enemy.cardInGame;
                LogVerboseBattle($"[RefreshCards] Mapped to ENEMY card: {card.cardName}");
            }

            if (card == null)
            {
                continue;
            }

            LogVerboseBattleWarning($"[REFRESH] Updating {card.cardName} from server:");
            LogVerboseBattleWarning($"[REFRESH]   - Current Kard.health: {card.health}/{card.maxHealth}");
            LogVerboseBattleWarning(
                $"[REFRESH]   - Server cardData.health: {cardData.health}/{cardData.maxHealth}"
            );

            int oldHealth = card.health;

            card.health = cardData.health;
            card.maxHealth = cardData.maxHealth;
            card.strength = cardData.strength;
            card.defense = cardData.defense;
            card.speed = cardData.speed;
            card.knowledge = cardData.knowledge;

            LogVerboseBattleWarning(
                $"[REFRESH]   - After update Kard.health: {card.health}/{card.maxHealth} (change: {card.health - oldHealth})"
            );

            if (playerId == fightSystem.myPlayerId)
            {
                playerLifeBar.SetHP(card.health);
                LogVerboseBattleWarning($"[REFRESH] playerLifeBar.SetHP({card.health}) - MY card synced");
                fightSystem.UpdatePendingOngoingAction(cardData);
            }
            else
            {
                enemyLifeBar.SetHP(card.health);
                LogVerboseBattleWarning(
                    $"[REFRESH] enemyLifeBar.SetHP({card.health}) - ENEMY card synced"
                );
            }

            if (cardData.effects != null && cardData.effects.Length > 0)
            {
                foreach (var effect in cardData.effects)
                {
                    Debug.Log(
                        $"[BattleResultProcessor] {card.cardName} has effect: {effect.type} (duration: {effect.duration})"
                    );

                    int effectType = int.Parse(effect.type.ToString());
                    string effectName = BattleEffectPlayback.GetEffectName(effectType);
                    if (string.IsNullOrEmpty(effectName))
                    {
                        continue;
                    }

                    bool hasIcon = false;
                    foreach (Transform child in card.effectIconContainer)
                    {
                        if (!child.name.StartsWith(effectName + "Icon"))
                        {
                            continue;
                        }

                        hasIcon = true;
                        break;
                    }

                    if (hasIcon)
                    {
                        continue;
                    }

                    LogVerboseBattleWarning(
                        $"[REFRESH] Adding missing {effectName} icon to {card.cardName}"
                    );
                    card.AddEffectIcon(effectName);
                }
            }
            else
            {
                LogVerboseBattleWarning(
                    $"[REFRESH] {card.cardName} has NO effects in DB - removing all effect icons"
                );

                List<Transform> iconsToRemove = new List<Transform>();
                foreach (Transform child in card.effectIconContainer)
                {
                    iconsToRemove.Add(child);
                }

                foreach (Transform icon in iconsToRemove)
                {
                    LogVerboseBattleWarning($"[REFRESH] Removing orphaned icon: {icon.name}");
                    Destroy(icon.gameObject);
                }

                if (iconsToRemove.Count > 0)
                {
                    card.RepositionEffectIcons();
                }
            }

            Debug.Log(
                $"[BattleResultProcessor] Updated {card.cardName}: HP={card.health}/{card.maxHealth}, STR={card.strength}, DEF={card.defense}"
            );
        }
    }
}
