using System.Collections;
using UnityEngine;

public partial class BattleResultProcessor
{
    /// <summary>
    /// Skontroluje vAsledok boja a urATA vALAaza
    /// </summary>
    private void CheckBattleOutcome(Kard myCard, Kard enemyCard)
    {
        if (roundCoordinator == null)
        {
            roundCoordinator = new BattleRoundCoordinator(
                fightSystem,
                multiplayerService,
                killCounterManager,
                dialogText
            );
        }

        bool myCardDead = myCard.health <= 0;
        bool enemyCardDead = enemyCard.health <= 0;

        if (myCardDead && enemyCardDead)
        {
            if (dialogText != null)
            {
                dialogText.text = "Both cards destroyed!";
            }

            Debug.Log("[BattleResultProcessor] Both cards died - checking remaining cards");
            StartCoroutine(roundCoordinator.HandleBothCardsDeath(myCard, enemyCard));
        }
        else if (myCardDead)
        {
            Debug.Log("[BattleResultProcessor] My card died - checking if I have more cards");
            StartCoroutine(roundCoordinator.HandlePlayerCardDeath(myCard));
        }
        else if (enemyCardDead)
        {
            if (dialogText != null)
            {
                dialogText.text = "Enemy card destroyed!";
            }

            Debug.Log("[BattleResultProcessor] Enemy card died");
            StartCoroutine(roundCoordinator.HandleEnemyCardDeath(enemyCard));
        }
        else
        {
            Debug.Log("[BattleResultProcessor] Battle continues - preparing next turn");
            StartCoroutine(roundCoordinator.PrepareNextTurn());
        }
    }

    /// <summary>
    /// PripravA AZalLA turn - ready check systAm + reset UI
    /// </summary>
    private IEnumerator HandleCardDeath(Kard deadCard, bool isMyCard)
    {
        if (deadCard == null)
        {
            Debug.LogWarning("[BattleResultProcessor] HandleCardDeath called with null card!");
            yield break;
        }

        string cardId = deadCard.cardId;
        string cardName = deadCard.cardName;

        Debug.Log(
            $"[BattleResultProcessor] z Card died: {cardName} (ID: {cardId}, isMyCard: {isMyCard})"
        );

        yield return new WaitForSeconds(1f);

        Player owner = isMyCard ? fightSystem.player : fightSystem.enemy;
        if (owner != null)
        {
            Debug.Log(
                $"[BattleResultProcessor] Removing {cardName} from {(isMyCard ? "player" : "enemy")} board"
            );
            owner.RemoveCardFromBoard(deadCard);
        }
        else
        {
            Debug.LogWarning($"[BattleResultProcessor] Owner not found for card {cardName}!");
            Destroy(deadCard.gameObject);
        }

        Debug.Log(
            $"[BattleResultProcessor] Skipping explicit dead-card clear for {cardName} (ID: {cardId}); replacement flow now relies on matchState dead-card inference"
        );

        Debug.Log($"[BattleResultProcessor] Card death handling complete for {cardName}");
    }

    /// <summary>
    /// Handler pre smrLA player karty - skontroluje ATi mA AZalLie karty
    /// </summary>
    private IEnumerator HandlePlayerCardDeath(Kard myCard)
    {
        if (killCounterManager != null)
        {
            Debug.Log("[BattleResultProcessor] z Player card died - incrementing enemy kill count");
            killCounterManager.OnEnemyKilledPlayerCard();
        }
        else
        {
            Debug.LogWarning("[BattleResultProcessor] KillCounterManager not assigned!");
        }

        yield return StartCoroutine(HandleCardDeath(myCard, isMyCard: true));

        Player player = fightSystem?.player;
        if (player == null)
        {
            Debug.LogError("[BattleResultProcessor] Player reference is null!");
            fightSystem.state = FightStateMultiplayer.LOST;
            yield break;
        }

        int remainingCards = player.hand.Count;
        Debug.Log($"[BattleResultProcessor] Player has {remainingCards} cards remaining in hand");

        if (remainingCards > 0)
        {
            dialogText.text = "Choose new fighter!";
            fightSystem.state = FightStateMultiplayer.PLAYERDEATH;

            Debug.Log("[BattleResultProcessor] z Unlocking hand for new card selection");

            var boardManager = fightSystem.multiplayerBoardManager;
            if (boardManager != null)
            {
                boardManager.UnlockPlayerHand();
            }
            else
            {
                Debug.LogError(
                    "[BattleResultProcessor] MultiplayerBoardManager not found - cannot unlock hand!"
                );
            }
        }
        else
        {
            dialogText.text = "You Lost! No cards left!";
            fightSystem.state = FightStateMultiplayer.LOST;
            Debug.Log("[BattleResultProcessor] Player lost - no cards remaining");
        }
    }

    /// <summary>
    /// Handler pre smrLA enemy karty - ATakAme na vAber novej enemy karty
    /// </summary>
    private IEnumerator HandleEnemyCardDeath(Kard enemyCard)
    {
        if (killCounterManager != null)
        {
            Debug.Log("[BattleResultProcessor] z Enemy card died - incrementing player kill count");
            killCounterManager.OnPlayerKilledEnemyCard();
        }
        else
        {
            Debug.LogWarning("[BattleResultProcessor] KillCounterManager not assigned!");
        }

        yield return StartCoroutine(HandleCardDeath(enemyCard, isMyCard: false));

        dialogText.text = "Opponent choosing new fighter...";
        Debug.Log("[BattleResultProcessor] Waiting for opponent to select new card...");

        var boardManager = fightSystem.multiplayerBoardManager;
        if (boardManager == null)
        {
            Debug.LogError("[BattleResultProcessor] MultiplayerBoardManager not found!");
            dialogText.text = "You Won!";
            fightSystem.state = FightStateMultiplayer.WON;
            yield break;
        }

        boardManager.opponentCardRevealed = false;

        var waitTask = boardManager.WaitForOpponentSelectionAsync();
        yield return new WaitUntil(() => waitTask.IsCompleted);

        boardManager.RevealCards();

        Debug.Log("[BattleResultProcessor] Enemy card revealed! Battle continues.");

        yield return new WaitForSeconds(0.5f);
        Debug.Log(
            "[BattleResultProcessor] Replacement selection completed - matchState is authoritative for next flow"
        );

        fightSystem.state = FightStateMultiplayer.TURN;
        Debug.Log($"[BattleResultProcessor] State set to TURN. Current state: {fightSystem.state}");

        var myCard = fightSystem.player.cardInGame;
        if (myCard != null && fightSystem.attackCountLoader != null)
        {
            Debug.Log($"[BattleResultProcessor] Reloading attack counts for {myCard.cardName}");
            fightSystem.LoadAttackCounts(myCard);
        }
    }

    /// <summary>
    /// Handler pre smrLA oboch kariet simultAnne
    /// </summary>
    private IEnumerator HandleBothCardsDeath(Kard myCard, Kard enemyCard)
    {
        if (killCounterManager != null)
        {
            Debug.Log("[BattleResultProcessor] zz Both cards died - incrementing both kill counts");
            killCounterManager.OnEnemyKilledPlayerCard();
            killCounterManager.OnPlayerKilledEnemyCard();
        }
        else
        {
            Debug.LogWarning("[BattleResultProcessor] KillCounterManager not assigned!");
        }

        yield return StartCoroutine(HandleCardDeath(myCard, isMyCard: true));
        yield return StartCoroutine(HandleCardDeath(enemyCard, isMyCard: false));

        Player player = fightSystem?.player;
        if (player == null)
        {
            dialogText.text = "Draw!";
            fightSystem.state = FightStateMultiplayer.WON;
            yield break;
        }

        int remainingCards = player.hand.Count;
        Debug.Log(
            $"[BattleResultProcessor] Both died - Player has {remainingCards} cards remaining"
        );

        if (remainingCards > 0)
        {
            dialogText.text = "Both destroyed! Choose new fighter!";
            fightSystem.state = FightStateMultiplayer.PLAYERDEATH;

            Debug.Log("[BattleResultProcessor] z Unlocking hand after mutual destruction");

            var boardManager = fightSystem.multiplayerBoardManager;
            if (boardManager != null)
            {
                boardManager.UnlockPlayerHand();
            }
            else
            {
                Debug.LogError(
                    "[BattleResultProcessor] MultiplayerBoardManager not found in both cards death!"
                );
            }
        }
        else
        {
            dialogText.text = "Draw! No cards left!";
            fightSystem.state = FightStateMultiplayer.WON;
            Debug.Log("[BattleResultProcessor] Draw - both players out of cards");
        }
    }
}
