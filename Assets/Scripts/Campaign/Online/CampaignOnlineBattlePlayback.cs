using System.Collections;
using TMPro;
using UnityEngine;

public class CampaignOnlineBattlePlayback : MonoBehaviour
{
    private static bool VerboseCampaignPlaybackLogs => true;

    [Header("Dependencies")]
    public Attack attackComponent;
    public MultiplayerCardAnimator cardAnimator;
    public HealthBar playerLifeBar;
    public HealthBar enemyLifeBar;
    public TMP_Text dialogText;

    public IEnumerator PlayBattleAsync(
        CampaignOnlineBattleEnvelopeDto envelope,
        Kard playerCard,
        Kard enemyCard,
        string playerCardId)
    {
        if (envelope?.battleResult == null || playerCard == null || enemyCard == null)
        {
            yield break;
        }

        if (!BattleTimelineBuilder.TryBuild(envelope.battleResult, out var timelineSteps, out var timelineError))
        {
            Debug.LogError($"[CampaignOnlineBattlePlayback] TimelineV2 unavailable. Campaign legacy playback is disabled. Error: {timelineError}");
            yield return ShowDialog("Battle timeline unavailable.");
            yield break;
        }

        if (VerboseCampaignPlaybackLogs)
        {
            Debug.LogWarning(
                $"[CampaignOnlineBattlePlayback] Playing TimelineV2: steps={timelineSteps.Count}, " +
                $"playerCard={playerCard.cardId}, enemyCard={enemyCard.cardId}, " +
                $"cardDied={envelope.battleResult.cardDied}, winner={envelope.battleResult.winnerCardId}, loser={envelope.battleResult.loserCardId}"
            );
        }

        var context = new BattleTimelinePlaybackContext
        {
            CoroutineHost = this,
            PlayerCard = playerCard,
            EnemyCard = enemyCard,
            PlayerCardId = playerCardId,
            EnemyCardId = enemyCard.cardId,
            AttackComponent = attackComponent,
            CardAnimator = cardAnimator,
            PlayerLifeBar = playerLifeBar,
            EnemyLifeBar = enemyLifeBar,
            DialogText = dialogText,
            DialogDelaySeconds = 0.75f,
            InterAttackDelaySeconds = 0.25f,
            FinalDelaySeconds = 0f,
            ResetCardsAfterPlayback = false,
        };

        yield return BattleTimelinePlayback.Play(context, timelineSteps);
    }

    private IEnumerator ShowDialog(string message)
    {
        if (dialogText != null)
        {
            dialogText.text = message ?? string.Empty;
        }

        yield return new WaitForSeconds(0.75f);
    }
}
