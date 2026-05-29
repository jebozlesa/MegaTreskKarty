using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// Rewrite boundary for connecting RR session state to shared multiplayer-style
/// turn runtime.
/// </summary>
public class RoyalRumbleTurnAdapter : MonoBehaviour
{
    private static bool VerboseTurnAdapterLogs => true;

    public RoyalRumbleService royalRumbleService;

    private void Awake()
    {
        royalRumbleService ??= GetComponent<RoyalRumbleService>();
    }

    public Task<RoyalRumbleBattleEnvelopeDto> SubmitAttackAsync(string sessionId, string playerId, int attackSlot)
    {
        if (royalRumbleService == null
            || string.IsNullOrWhiteSpace(sessionId)
            || string.IsNullOrWhiteSpace(playerId)
            || attackSlot <= 0)
        {
            return Task.FromResult<RoyalRumbleBattleEnvelopeDto>(null);
        }

        if (VerboseTurnAdapterLogs)
        {
            Debug.LogWarning(
                $"[RoyalRumbleTurnAdapter] SubmitAttackAsync: session={sessionId}, player={playerId}, slot={attackSlot}"
            );
        }

        return royalRumbleService.SubmitAttackAsync(sessionId, playerId, attackSlot);
    }

    public PendingOngoingActionTurnData GetPendingOngoingAction(SelectedCardData selectedCard)
    {
        return CreatePendingOngoingAction(selectedCard);
    }

    public static PendingOngoingActionTurnData CreatePendingOngoingAction(SelectedCardData selectedCard)
    {
        SelectedCardData.OngoingActionData action = selectedCard?.ongoingActions != null && selectedCard.ongoingActions.Length > 0
            ? selectedCard.ongoingActions[0]
            : null;

        if (action == null || action.sourceAttackId <= 0)
        {
            return null;
        }

        return new PendingOngoingActionTurnData
        {
            actionType = action.type,
            sourceAttackId = action.sourceAttackId,
            targetCardId = action.targetCardId,
            turnsRemaining = action.turnsRemaining
        };
    }
}
