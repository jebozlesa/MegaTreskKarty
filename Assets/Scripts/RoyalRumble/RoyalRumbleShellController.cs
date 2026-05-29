using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Rewrite entrypoint for Royal Rumble.
/// Owns the RR shell and, in M3, shared attack-selection UX only.
/// Battle submit/playback runtime still comes in the next milestone.
/// </summary>
public class RoyalRumbleShellController : MonoBehaviour
{
    private static bool VerboseShellLogs => true;
    private static RoyalRumbleShellController activeInstance;

    [Header("Dependencies")]
    public RoyalRumbleService royalRumbleService;
    public RoyalRumbleTurnAdapter turnAdapter;
    public RoyalRumbleBattlePlayback battlePlayback;
    public Player player;
    public Player enemy;
    public HealthBar playerLifeBar;
    public HealthBar enemyLifeBar;
    public TMP_Text dialogText;
    public AttackDescriptions attackDescriptions;
    public RecordHandler recordHandler;
    public GameObject cardPrefab;

    [Header("Scene References")]
    public GameObject playerBoard;
    public GameObject enemyBoard;

    [Header("Attack UI")]
    public Button attackButton1;
    public Button attackButton2;
    public Button attackButton3;
    public Button attackButton4;
    public Button confirmButton;
    public TMP_Text attackButton1Text;
    public TMP_Text attackButton2Text;
    public TMP_Text attackButton3Text;
    public TMP_Text attackButton4Text;
    public TMP_Text attackButton1CountText;
    public TMP_Text attackButton2CountText;
    public TMP_Text attackButton3CountText;
    public TMP_Text attackButton4CountText;

    [Header("Options")]
    public bool autoStart;
    public float enemyRevealDelaySeconds = 0.05f;
    public float enemyPostRevealDelaySeconds = 1f;

    private RoyalRumbleSessionDto currentSession;
    private string playerId;
    private bool isBusy;
    private bool hasStartedRun;
    private bool isShuttingDown;
    private string lastAutoSubmittedPendingKey;
    private SharedAttackSelectionFlow attackSelectionFlow;
    private readonly List<Kard> renderedPlayerHandCards = new List<Kard>();
    private readonly List<Kard> renderedEnemyHandCards = new List<Kard>();
    private Kard renderedPlayerActiveCard;
    private Kard renderedEnemyActiveCard;
    private string defaultAttackButton1Name;
    private string defaultAttackButton2Name;
    private string defaultAttackButton3Name;
    private string defaultAttackButton4Name;
    private string defaultAttackButton1Count;
    private string defaultAttackButton2Count;
    private string defaultAttackButton3Count;
    private string defaultAttackButton4Count;

    public RoyalRumbleSessionDto CurrentSession => currentSession;
    private string ActivePlayerCardId => currentSession?.active?.playerCardId ?? string.Empty;
    private string ActiveEnemyCardId => currentSession?.active?.enemyCardId ?? string.Empty;

    public bool CanDragCard(Kard card)
    {
        if (card == null || isBusy || currentSession == null)
        {
            return false;
        }

        return IsFighterSelectionStatus()
            && renderedPlayerHandCards.Contains(card);
    }

    private bool IsFighterSelectionStatus()
    {
        return currentSession != null
            && (currentSession.status == "awaiting_player_card"
                || currentSession.status == "awaiting_replacement");
    }

    private void Awake()
    {
        if (activeInstance != null && activeInstance != this)
        {
            Debug.LogWarning("[RoyalRumbleShellController] Duplicate shell detected. Disabling the extra instance.");
            enabled = false;
            return;
        }

        activeInstance = this;
        ResolveDependencies();
        ResolveAttackTextReferences();
        PrepareAttackSelectionFlow();
        UpdateAttackButtons();
    }

    private async void Start()
    {
        if (!autoStart)
        {
            return;
        }

        await StartFreshRunAsync();
    }

    public async Task StartFreshRunAsync()
    {
        if (isBusy || hasStartedRun || isShuttingDown)
        {
            return;
        }

        isBusy = true;
        hasStartedRun = true;
        playerId = PlayFabManagerLogin.Instance != null
            ? PlayFabManagerLogin.Instance.LoggedInPlayerId
            : PlayerPrefs.GetString("LoggedInPlayerId", string.Empty);

        if (string.IsNullOrWhiteSpace(playerId))
        {
            SetStatus("Missing player ID.");
            Debug.LogWarning("[RoyalRumbleShellController] Cannot start RR shell without player ID.");
            isBusy = false;
            hasStartedRun = false;
            return;
        }

        SetStatus("Preparing Royal Rumble...");
        LogVerboseWarning($"[RoyalRumbleShellController] RR shell opening started for player={playerId}.");

        RoyalRumbleSessionEnvelopeDto sessionEnvelope = await royalRumbleService.CreateSessionAsync(playerId);
        if (this == null || isShuttingDown)
        {
            return;
        }

        if (sessionEnvelope?.session == null)
        {
            SetStatus("Failed to create Royal Rumble session.");
            Debug.LogWarning("[RoyalRumbleShellController] RR shell failed to create a session.");
            isBusy = false;
            hasStartedRun = false;
            return;
        }

        currentSession = sessionEnvelope.session;
        SyncRoyalRumbleRecordDisplay(null, "create_session");
        LogVerboseWarning(
            $"[RoyalRumbleShellController] RR shell created session {currentSession.sessionId} with status {currentSession.status}."
        );

        await EnsureDecksLoadedAsync();
        if (this == null || isShuttingDown)
        {
            return;
        }

        isBusy = false;

        if (currentSession?.playerDeck?.cards == null || currentSession.playerDeck.cards.Count == 0)
        {
            SetStatus("Failed to load player deck.");
            UpdateAttackButtons();
            return;
        }

        if (currentSession?.enemyDeck?.cards == null || currentSession.enemyDeck.cards.Count == 0)
        {
            SetStatus("Failed to load enemy deck.");
            UpdateAttackButtons();
            hasStartedRun = false;
            return;
        }

        StartCoroutine(PlayInitialOpeningSequence());
    }

    public void ShowDragSelectionHint()
    {
        SetStatus("Drag a fighter to the battle area.");
    }

    public void AttackButton(int attackType)
    {
        PendingOngoingActionTurnData pendingAction = GetPendingOngoingAction();
        if (pendingAction != null)
        {
            LogVerboseWarning(
                $"[RoyalRumbleShellController] Ignored manual attack click because ongoing action is pending: {DescribePendingAction(pendingAction)}"
            );
            return;
        }

        SelectedCardData selected = FindCard(currentSession?.playerDeck?.cards, ActivePlayerCardId);
        int attackId = GetAttackIdForSlot(selected, attackType);
        int remainingCount = selected != null ? GetRemainingCountForSlot(selected.cardId, attackType) : -1;
        LogVerboseWarning(
            $"[RoyalRumbleShellController] Attack button clicked. slot={attackType}, attackId={attackId}, attackName={GetAttackName(attackId)}, " +
            $"remainingCount={remainingCount}, status={currentSession?.status}, isBusy={isBusy}, selectedCard={ActivePlayerCardId}"
        );
        attackSelectionFlow?.OnAttackButtonClicked(attackType);
    }

    public void ConfirmAttackButton()
    {
        PendingOngoingActionTurnData pendingAction = GetPendingOngoingAction();
        if (pendingAction != null)
        {
            LogVerboseWarning(
                $"[RoyalRumbleShellController] Ignored manual confirm because ongoing action is pending: {DescribePendingAction(pendingAction)}"
            );
            return;
        }

        SelectedCardData selected = FindCard(currentSession?.playerDeck?.cards, ActivePlayerCardId);
        int selectedSlot = attackSelectionFlow?.SelectedAttackType ?? 0;
        int attackId = GetAttackIdForSlot(selected, selectedSlot);
        int remainingCount = selected != null ? GetRemainingCountForSlot(selected.cardId, selectedSlot) : -1;
        LogVerboseWarning(
            $"[RoyalRumbleShellController] Confirm attack clicked. selectedSlot={selectedSlot}, attackId={attackId}, attackName={GetAttackName(attackId)}, " +
            $"remainingCount={remainingCount}, status={currentSession?.status}, isBusy={isBusy}, selectedCard={ActivePlayerCardId}"
        );
        attackSelectionFlow?.OnConfirmAttackClicked();
    }

    public async void OnCardDropped(Kard card, RoyalRumbleCardDrag dragHandler)
    {
        if (card == null)
        {
            Debug.LogWarning("[RoyalRumbleShellController] OnCardDropped called with null card.");
            dragHandler?.ResetToOriginalPosition();
            return;
        }

        if (!CanDragCard(card))
        {
            Debug.LogWarning(
                $"[RoyalRumbleShellController] Ignored dropped card {card.cardName}. isBusy={isBusy}, status={currentSession?.status}"
            );
            dragHandler?.ResetToOriginalPosition();
            return;
        }

        isBusy = true;
        UpdateAttackButtons();

        try
        {
            SetStatus("Selecting fighter...");
            player.PlayCard(card, playerBoard);
            player.cardInGame.isDragable = false;
            renderedPlayerActiveCard = player.cardInGame;
            playerLifeBar?.SetBar(player.cardInGame);

            RoyalRumbleSessionEnvelopeDto envelope = await royalRumbleService.SelectCardAsync(currentSession.sessionId, playerId, card.cardId);
            if (this == null || isShuttingDown)
            {
                return;
            }

            if (envelope?.session == null)
            {
                Debug.LogWarning($"[RoyalRumbleShellController] Failed to select fighter {card.cardId} after drop.");
                Vector3 returnPosition = dragHandler != null ? dragHandler.GetOriginalLocalPosition() : Vector3.zero;
                player.ReturnCardToHand(card, returnPosition);
                card.isDragable = true;
                SetStatus("Failed to select fighter.");
                return;
            }

            currentSession = envelope.session;
            SyncRoyalRumbleRecordDisplay(null, "select_card");
            renderedPlayerHandCards.Remove(card);
            lastAutoSubmittedPendingKey = null;
            LogVerboseWarning(
                $"[RoyalRumbleShellController] Fighter selected via drag: cardId={card.cardId}, name={card.cardName}, hp={card.health}/{card.maxHealth}, " +
                $"attacks=[{card.attack1},{card.attack2},{card.attack3},{card.attack4}]"
            );
            SetStatus("Choose attack!");
        }
        finally
        {
            isBusy = false;
            UpdateAttackButtons();
        }
    }

    private void ResolveDependencies()
    {
        royalRumbleService ??= GetComponent<RoyalRumbleService>();
        turnAdapter ??= GetComponent<RoyalRumbleTurnAdapter>();
        turnAdapter ??= FindFirstObjectByType<RoyalRumbleTurnAdapter>();
        battlePlayback ??= GetComponent<RoyalRumbleBattlePlayback>();
        battlePlayback ??= FindFirstObjectByType<RoyalRumbleBattlePlayback>();
        recordHandler ??= FindFirstObjectByType<RecordHandler>();
        dialogText ??= FindFirstObjectByType<TMP_Text>();
        attackDescriptions ??= FindFirstObjectByType<AttackDescriptions>();
        player ??= FindPlayers(false).FirstOrDefault();
        enemy ??= FindPlayers(true).FirstOrDefault();

        if (royalRumbleService == null)
        {
            Debug.LogWarning("[RoyalRumbleShellController] RoyalRumbleService reference is missing.");
        }

        if (player != null)
        {
            player.isEnemy = false;
            player.dialogText = dialogText;
        }

        if (enemy != null)
        {
            enemy.isEnemy = true;
            enemy.dialogText = dialogText;
        }
    }

    private async Task EnsureDecksLoadedAsync()
    {
        if (currentSession == null)
        {
            return;
        }

        if (currentSession.playerDeck == null || currentSession.playerDeck.cards == null || currentSession.playerDeck.cards.Count == 0)
        {
            RoyalRumbleSessionEnvelopeDto playerDeckEnvelope = await royalRumbleService.LoadPlayerDeckAsync(currentSession.sessionId, playerId);
            if (playerDeckEnvelope?.session != null)
            {
                currentSession = playerDeckEnvelope.session;
                SyncRoyalRumbleRecordDisplay(null, "load_player_deck");
                LogVerboseWarning(
                    $"[RoyalRumbleShellController] Player deck loaded: cards={currentSession.playerDeck?.cards?.Count ?? 0}, selected={ActivePlayerCardId}"
                );
            }
            else
            {
                Debug.LogWarning("[RoyalRumbleShellController] Failed to load RR player deck.");
            }
        }

        if (currentSession.enemyDeck == null || currentSession.enemyDeck.cards == null || currentSession.enemyDeck.cards.Count == 0)
        {
            RoyalRumbleSessionEnvelopeDto enemyDeckEnvelope = await royalRumbleService.LoadEnemyDeckAsync(currentSession.sessionId, playerId);
            if (enemyDeckEnvelope?.session != null)
            {
                currentSession = enemyDeckEnvelope.session;
                SyncRoyalRumbleRecordDisplay(null, "load_enemy_deck");
                LogVerboseWarning(
                    $"[RoyalRumbleShellController] Enemy deck loaded: cards={currentSession.enemyDeck?.cards?.Count ?? 0}, selected={ActiveEnemyCardId}"
                );
            }
            else
            {
                Debug.LogWarning("[RoyalRumbleShellController] Failed to load RR enemy deck.");
            }
        }
    }

    private IEnumerator PlayInitialOpeningSequence()
    {
        ClearRenderedCards();
        RenderPlayerHand();

        if (currentSession?.enemyDeck?.cards == null || currentSession.enemyDeck.cards.Count == 0)
        {
            Debug.LogWarning("[RoyalRumbleShellController] Opening sequence started without an enemy deck.");
            yield break;
        }

        SelectedCardData selectedEnemy = ResolveSelectedEnemyCard();
        List<SelectedCardData> aliveEnemyCards = currentSession.enemyDeck.cards.Where(IsAlive).ToList();

        LogVerboseWarning($"[RoyalRumbleShellController] Starting RR shell enemy reveal with {aliveEnemyCards.Count} cards.");
        SetStatus("Preparing enemy fighters...");

        foreach (SelectedCardData cardData in aliveEnemyCards)
        {
            Kard createdCard = CreateCardInGame(
                RoyalRumbleCardMapper.ToGeneratedCard(cardData),
                enemy != null ? enemy.gameObject : null,
                enemy,
                addToHand: true,
                battleAreaOverride: enemyBoard
            );

            if (createdCard != null)
            {
                renderedEnemyHandCards.Add(createdCard);
            }

            if (enemyRevealDelaySeconds > 0f)
            {
                yield return new WaitForSeconds(enemyRevealDelaySeconds);
            }
        }

        if (enemyPostRevealDelaySeconds > 0f)
        {
            yield return new WaitForSeconds(enemyPostRevealDelaySeconds);
        }

        PromoteEnemyCardToBoard(selectedEnemy);
        SetStatus("Choose fighter.");
        UpdateAttackButtons();
        LogVerboseWarning("[RoyalRumbleShellController] RR shell enemy reveal finished.");
    }

    private void RenderPlayerHand()
    {
        if (currentSession?.playerDeck?.cards == null || player == null)
        {
            return;
        }

        foreach (SelectedCardData cardData in currentSession.playerDeck.cards.Where(IsAlive))
        {
            Kard createdCard = CreateCardInGame(
                RoyalRumbleCardMapper.ToGeneratedCard(cardData),
                player.gameObject,
                player,
                addToHand: true,
                battleAreaOverride: playerBoard
            );

            if (createdCard != null)
            {
                RoyalRumbleSelectableCard selectable = createdCard.gameObject.GetComponent<RoyalRumbleSelectableCard>();
                if (selectable == null)
                {
                    selectable = createdCard.gameObject.AddComponent<RoyalRumbleSelectableCard>();
                }

                selectable.Initialize(this, createdCard);
                RoyalRumbleCardDrag dragHandler = createdCard.gameObject.GetComponent<RoyalRumbleCardDrag>();
                if (dragHandler == null)
                {
                    dragHandler = createdCard.gameObject.AddComponent<RoyalRumbleCardDrag>();
                }

                dragHandler.Initialize(this);
                createdCard.isDragable = true;
                renderedPlayerHandCards.Add(createdCard);
            }
        }
    }

    private SelectedCardData ResolveSelectedEnemyCard()
    {
        SelectedCardData selected = FindCard(currentSession?.enemyDeck?.cards, ActiveEnemyCardId);
        if (selected != null)
        {
            return selected;
        }

        if (currentSession?.enemyDeck?.cards != null && currentSession.enemyDeck.cards.Any(IsAlive))
        {
            Debug.LogWarning("[RoyalRumbleShellController] Server active enemy card is missing; RR cannot choose one locally.");
        }

        return null;
    }

    private void PromoteEnemyCardToBoard(SelectedCardData selectedEnemy)
    {
        if (selectedEnemy == null || enemy == null || enemyBoard == null)
        {
            return;
        }

        Kard cardInHand = renderedEnemyHandCards.FirstOrDefault(card => card != null && card.cardId == selectedEnemy.cardId);
        if (cardInHand == null)
        {
            cardInHand = CreateCardInGame(
                RoyalRumbleCardMapper.ToGeneratedCard(selectedEnemy),
                enemy.gameObject,
                enemy,
                addToHand: false,
                battleAreaOverride: enemyBoard
            );
        }

        if (cardInHand != null)
        {
            renderedEnemyHandCards.Remove(cardInHand);
            enemy.PlayCard(cardInHand, enemyBoard);
            renderedEnemyActiveCard = cardInHand;
            enemy.cardInGame = cardInHand;
            enemyLifeBar?.SetBar(enemy.cardInGame);
            SyncEffectIconsFromSnapshot(cardInHand, selectedEnemy.effects);
        }
    }

    private void ClearRenderedCards()
    {
        foreach (Kard card in renderedPlayerHandCards)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }

        foreach (Kard card in renderedEnemyHandCards)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }

        if (renderedPlayerActiveCard != null)
        {
            Destroy(renderedPlayerActiveCard.gameObject);
        }

        if (renderedEnemyActiveCard != null)
        {
            Destroy(renderedEnemyActiveCard.gameObject);
        }

        renderedPlayerHandCards.Clear();
        renderedEnemyHandCards.Clear();
        renderedPlayerActiveCard = null;
        renderedEnemyActiveCard = null;
        
        if (player != null)
        {
            player.hand.Clear();
            player.cardsInGame.Clear();
            player.cardInGame = null;
        }

        if (enemy != null)
        {
            enemy.hand.Clear();
            enemy.cardsInGame.Clear();
            enemy.cardInGame = null;
        }

        UpdateAttackButtons();
    }

    private Kard CreateCardInGame(
        GeneratedCard cardData,
        GameObject parentOverride,
        Player owner,
        bool addToHand,
        GameObject battleAreaOverride)
    {
        if (cardData == null || cardPrefab == null || owner == null)
        {
            return null;
        }

        GameObject parent = parentOverride != null ? parentOverride : owner.gameObject;
        GameObject cardObject = Instantiate(cardPrefab, parent.transform);
        Kard kardComponent = cardObject.GetComponent<Kard>();
        if (kardComponent == null)
        {
            Destroy(cardObject);
            return null;
        }

        DisableLegacyCardRuntime(cardObject);

        kardComponent.cardId = cardData.CardID;
        kardComponent.styleId = cardData.StyleID;
        kardComponent.cardName = cardData.PersonName;
        kardComponent.health = cardData.Health;
        kardComponent.maxHealth = cardData.MaxHealth;
        kardComponent.strength = cardData.Strength;
        kardComponent.speed = cardData.Speed;
        kardComponent.attack = cardData.Attack;
        kardComponent.defense = cardData.Defense;
        kardComponent.knowledge = cardData.Knowledge;
        kardComponent.charisma = cardData.Charisma;
        kardComponent.level = cardData.Level;
        kardComponent.experience = cardData.Experience;
        kardComponent.attack1 = cardData.Attack1;
        kardComponent.attack2 = cardData.Attack2;
        kardComponent.attack3 = cardData.Attack3;
        kardComponent.attack4 = cardData.Attack4;
        kardComponent.image = cardData.CardPicture;
        kardComponent.battleArea = battleAreaOverride;

        if (cardData.Color != null && cardData.Color.Length >= 3)
        {
            kardComponent.color = new Color32(
                (byte)Mathf.Clamp(cardData.Color[0], 0, 255),
                (byte)Mathf.Clamp(cardData.Color[1], 0, 255),
                (byte)Mathf.Clamp(cardData.Color[2], 0, 255),
                255
            );
        }

        if (addToHand)
        {
            owner.AddCardToHand(kardComponent);
        }

        return kardComponent;
    }

    private static void DisableLegacyCardRuntime(GameObject cardObject)
    {
        if (cardObject == null)
        {
            return;
        }

        DragKard legacyDragKard = cardObject.GetComponent<DragKard>();
        if (legacyDragKard != null)
        {
            legacyDragKard.enabled = false;
        }

        Drag legacyDrag = cardObject.GetComponent<Drag>();
        if (legacyDrag != null)
        {
            legacyDrag.enabled = false;
        }

        PlayerCard legacyPlayerCard = cardObject.GetComponent<PlayerCard>();
        if (legacyPlayerCard != null)
        {
            legacyPlayerCard.enabled = false;
        }

        EnemyCard legacyEnemyCard = cardObject.GetComponent<EnemyCard>();
        if (legacyEnemyCard != null)
        {
            legacyEnemyCard.enabled = false;
        }
    }

    private void PrepareAttackSelectionFlow()
    {
        attackSelectionFlow ??= new SharedAttackSelectionFlow(
            attackDescriptions,
            dialogText,
            "Resolving battle...",
            GetPendingOngoingAction,
            SubmitSelectedAttack,
            SetAttackButtonsInteractable,
            SetConfirmButtonState
        );
    }

    private PendingOngoingActionTurnData GetPendingOngoingAction()
    {
        SelectedCardData selected = FindCard(currentSession?.playerDeck?.cards, ActivePlayerCardId);
        return RoyalRumbleTurnAdapter.CreatePendingOngoingAction(selected);
    }

    private void SubmitSelectedAttack(SelectedAttackData attackData)
    {
        if (attackData == null || attackData.attackType <= 0 || isBusy || currentSession == null)
        {
            return;
        }

        PendingOngoingActionTurnData pendingAction = GetPendingOngoingAction();
        string pendingKey = BuildPendingActionKey(ActivePlayerCardId, pendingAction);
        if (pendingAction != null)
        {
            if (!string.IsNullOrWhiteSpace(pendingKey) && string.Equals(lastAutoSubmittedPendingKey, pendingKey, StringComparison.Ordinal))
            {
                LogVerboseWarning(
                    $"[RoyalRumbleShellController] Ignored duplicate ongoing auto-submit for key={pendingKey}"
                );
                return;
            }

            lastAutoSubmittedPendingKey = pendingKey;
            LogVerboseWarning(
                $"[RoyalRumbleShellController] Auto-submitting ongoing action: {DescribePendingAction(pendingAction)}, key={pendingKey}"
            );
        }
        else
        {
            lastAutoSubmittedPendingKey = null;
        }

        LogVerboseWarning(
            $"[RoyalRumbleShellController] SubmitSelectedAttack accepted: slot={attackData.attackType}, attackId={attackData.attackId}, " +
            $"attackName={GetAttackName(attackData.attackId)}, displayedCount={attackData.attackCount}, cardId={attackData.cardId}"
        );
        StartCoroutine(SubmitAttackRoutine(attackData.attackType));
    }

    private IEnumerator SubmitAttackRoutine(int attackSlot)
    {
        isBusy = true;
        UpdateAttackButtons();
        SetStatus("Resolving battle...");
        SelectedCardData playerSnapshotBefore = FindCard(currentSession?.playerDeck?.cards, ActivePlayerCardId);
        SelectedCardData enemySnapshotBefore = FindCard(currentSession?.enemyDeck?.cards, ActiveEnemyCardId);
        int attackId = GetAttackIdForSlot(playerSnapshotBefore, attackSlot);
        int countBefore = playerSnapshotBefore != null ? GetRemainingCountForSlot(playerSnapshotBefore.cardId, attackSlot) : -1;
        PendingOngoingActionTurnData pendingBefore = GetPendingOngoingAction();
        LogVerboseWarning(
            $"[RoyalRumbleShellController] SubmitAttackRoutine -> server: session={currentSession?.sessionId}, slot={attackSlot}, attackId={attackId}, " +
            $"attackName={GetAttackName(attackId)}, countBefore={countBefore}, playerCard={DescribeCard(playerSnapshotBefore)}, " +
            $"enemyCard={DescribeCard(enemySnapshotBefore)}, pendingBefore={DescribePendingAction(pendingBefore)}"
        );

        Task<RoyalRumbleBattleEnvelopeDto> submitTask = turnAdapter != null
            ? turnAdapter.SubmitAttackAsync(currentSession.sessionId, playerId, attackSlot)
            : royalRumbleService.SubmitAttackAsync(currentSession.sessionId, playerId, attackSlot);
        yield return new WaitUntil(() => submitTask.IsCompleted);

        if (this == null || isShuttingDown)
        {
            yield break;
        }

        RoyalRumbleBattleEnvelopeDto envelope = submitTask.Status == TaskStatus.RanToCompletion
            ? submitTask.Result
            : null;

        if (envelope?.battleResult == null)
        {
            SetStatus("Battle failed.");
            isBusy = false;
            UpdateAttackButtons();
            yield break;
        }

        string previousPlayerSelectedCardId = ActivePlayerCardId;
        string previousEnemySelectedCardId = ActiveEnemyCardId;

        if (battlePlayback != null && renderedPlayerActiveCard != null && renderedEnemyActiveCard != null)
        {
            yield return StartCoroutine(
                battlePlayback.PlayBattleAsync(
                    envelope,
                    renderedPlayerActiveCard,
                    renderedEnemyActiveCard,
                    previousPlayerSelectedCardId
                )
            );

            yield return StartCoroutine(ResetActiveCardPositionsAfterPlayback());
        }

        if (envelope.session != null)
        {
            currentSession = envelope.session;
        }
        else
        {
            Task<RoyalRumbleSessionEnvelopeDto> refreshTask = royalRumbleService.GetSessionAsync(currentSession.sessionId, playerId);
            yield return new WaitUntil(() => refreshTask.IsCompleted);

            RoyalRumbleSessionEnvelopeDto refreshEnvelope = refreshTask.Status == TaskStatus.RanToCompletion
                ? refreshTask.Result
                : null;
            if (refreshEnvelope?.session != null)
            {
                currentSession = refreshEnvelope.session;
            }
        }

        SyncRoyalRumbleRecordDisplay(envelope.recordSync, "post_battle");

        if (this == null || isShuttingDown)
        {
            yield break;
        }

        SelectedCardData playerSnapshotAfter = FindCard(currentSession?.playerDeck?.cards, ActivePlayerCardId);
        SelectedCardData enemySnapshotAfter = FindCard(currentSession?.enemyDeck?.cards, ActiveEnemyCardId);
        int countAfter = playerSnapshotAfter != null ? GetRemainingCountForSlot(playerSnapshotAfter.cardId, attackSlot) : -1;
        PendingOngoingActionTurnData pendingAfter = GetPendingOngoingAction();
        LogVerboseWarning(
            $"[RoyalRumbleShellController] Server response applied: runStatus={envelope.runStatus}, runEnded={envelope.runEnded}, " +
            $"playerNeedsReplacement={envelope.playerNeedsReplacement}, enemyNeedsReplacement={envelope.enemyNeedsReplacement}, " +
            $"botAttack={envelope.botAttack?.attackSlot}/{envelope.botAttack?.attackId}, countAfter={countAfter}, " +
            $"playerAfter={DescribeCard(playerSnapshotAfter)}, enemyAfter={DescribeCard(enemySnapshotAfter)}, pendingAfter={DescribePendingAction(pendingAfter)}"
        );
        LogVerboseWarning(
            $"[RoyalRumbleShellController] Battle summary: {DescribeBattleSummary(envelope.battleResult, previousPlayerSelectedCardId)}"
        );

        string nextPendingKey = BuildPendingActionKey(ActivePlayerCardId, pendingAfter);
        if (pendingAfter == null)
        {
            lastAutoSubmittedPendingKey = null;
        }
        else if (!string.Equals(nextPendingKey, lastAutoSubmittedPendingKey, StringComparison.Ordinal))
        {
            LogVerboseWarning(
                $"[RoyalRumbleShellController] Ongoing action state advanced. previousKey={lastAutoSubmittedPendingKey ?? "none"}, nextKey={nextPendingKey}"
            );
        }

        RefreshPostBattleView(envelope, previousPlayerSelectedCardId, previousEnemySelectedCardId);
        isBusy = false;
        UpdateAttackButtons();
    }

    private void UpdateAttackButtons()
    {
        bool attackPhaseActive = currentSession != null
            && currentSession.status == "awaiting_attack"
            && !string.IsNullOrWhiteSpace(ActivePlayerCardId)
            && !isBusy
            && player?.cardInGame != null;

        SetAttackButtonsInteractable(false);

        UpdateAttackButtonDisplay(attackButton1, attackButton1Text, attackButton1CountText, 1);
        UpdateAttackButtonDisplay(attackButton2, attackButton2Text, attackButton2CountText, 2);
        UpdateAttackButtonDisplay(attackButton3, attackButton3Text, attackButton3CountText, 3);
        UpdateAttackButtonDisplay(attackButton4, attackButton4Text, attackButton4CountText, 4);

        attackSelectionFlow?.PrepareAttackSelection(
            renderedPlayerActiveCard ?? player?.cardInGame,
            BuildCurrentAttackCountsState(),
            attackPhaseActive
        );
    }

    private IEnumerator ResetActiveCardPositionsAfterPlayback()
    {
        if (battlePlayback?.cardAnimator != null)
        {
            if (renderedPlayerActiveCard != null && playerBoard != null)
            {
                yield return StartCoroutine(
                    battlePlayback.cardAnimator.ResetCardPosition(
                        renderedPlayerActiveCard,
                        playerBoard.transform.position,
                        Quaternion.identity
                    )
                );
            }

            if (renderedEnemyActiveCard != null && enemyBoard != null)
            {
                yield return StartCoroutine(
                    battlePlayback.cardAnimator.ResetCardPosition(
                        renderedEnemyActiveCard,
                        enemyBoard.transform.position,
                        Quaternion.identity
                    )
                );
            }

            yield break;
        }

        if (renderedPlayerActiveCard != null && playerBoard != null)
        {
            renderedPlayerActiveCard.transform.position = playerBoard.transform.position;
            renderedPlayerActiveCard.transform.rotation = Quaternion.identity;
        }

        if (renderedEnemyActiveCard != null && enemyBoard != null)
        {
            renderedEnemyActiveCard.transform.position = enemyBoard.transform.position;
            renderedEnemyActiveCard.transform.rotation = Quaternion.identity;
        }
    }

    private void RefreshPostBattleView(
        RoyalRumbleBattleEnvelopeDto envelope,
        string previousPlayerSelectedCardId,
        string previousEnemySelectedCardId)
    {
        if (envelope?.runEnded == true || envelope?.playerNeedsReplacement == true)
        {
            ClearPendingOngoingState();
        }

        bool needsRebuild = ShouldRebuildViewAfterBattle(envelope, previousPlayerSelectedCardId, previousEnemySelectedCardId);
        LogVerboseWarning(
            $"[RoyalRumbleShellController] RefreshPostBattleView: rebuild={needsRebuild}, status={currentSession?.status}, previousPlayer={previousPlayerSelectedCardId}, " +
            $"currentPlayer={ActivePlayerCardId}, previousEnemy={previousEnemySelectedCardId}, currentEnemy={ActiveEnemyCardId}"
        );
        if (needsRebuild)
        {
            RebuildShellView();
        }
        else
        {
            SyncActiveCardsFromSession();
        }

        ApplyPostBattleStatus(envelope);
    }

    private void SyncRoyalRumbleRecordDisplay(RoyalRumbleRecordSyncDto recordSync, string source)
    {
        if (recordHandler == null)
        {
            return;
        }

        int before = recordHandler.bestRecord;
        recordHandler.ApplyRoyalRumbleRecordSnapshot(currentSession, recordSync);
        int after = recordHandler.bestRecord;

        if (before != after || recordSync != null)
        {
            LogVerboseWarning(
                $"[RoyalRumbleShellController] RR record display sync: source={source}, before={before}, after={after}, " +
                $"defeated={currentSession?.progress?.defeatedEnemyCount ?? 0}, bestSubmitted={currentSession?.progress?.bestSubmittedScore ?? 0}, " +
                $"pending={currentSession?.progress?.pendingRecordScore ?? 0}, recordSync={recordSync?.score ?? 0}/pending={recordSync?.pending ?? false}"
            );
        }
    }

    private bool ShouldRebuildViewAfterBattle(
        RoyalRumbleBattleEnvelopeDto envelope,
        string previousPlayerSelectedCardId,
        string previousEnemySelectedCardId)
    {
        if (currentSession == null)
        {
            return true;
        }

        if (envelope?.runEnded == true || envelope.playerNeedsReplacement || envelope.enemyNeedsReplacement)
        {
            return true;
        }

        if (currentSession.status == "awaiting_player_card"
            || currentSession.status == "awaiting_replacement"
            || currentSession.status == "won"
            || currentSession.status == "lost"
            || currentSession.status == "abandoned")
        {
            return true;
        }

        if (renderedPlayerActiveCard == null || renderedEnemyActiveCard == null)
        {
            return true;
        }

        return !string.Equals(ActivePlayerCardId, previousPlayerSelectedCardId, StringComparison.Ordinal)
            || !string.Equals(ActiveEnemyCardId, previousEnemySelectedCardId, StringComparison.Ordinal);
    }

    private void RebuildShellView()
    {
        LogVerboseWarning("[RoyalRumbleShellController] Rebuilding RR shell view after battle.");
        ResetAttackSelectionForShellState();
        ClearRenderedCards();

        SelectedCardData currentEnemy = ResolveSelectedEnemyCard();
        List<SelectedCardData> enemyHandCards = currentSession?.enemyDeck?.cards?
            .Where(IsAlive)
            .Where(card => !string.Equals(card.cardId, currentEnemy?.cardId, StringComparison.Ordinal))
            .ToList() ?? new List<SelectedCardData>();

        foreach (SelectedCardData cardData in enemyHandCards)
        {
            Kard createdCard = CreateCardInGame(
                RoyalRumbleCardMapper.ToGeneratedCard(cardData),
                enemy != null ? enemy.gameObject : null,
                enemy,
                addToHand: true,
                battleAreaOverride: enemyBoard
            );

            if (createdCard != null)
            {
                renderedEnemyHandCards.Add(createdCard);
            }
        }

        PromoteEnemyCardToBoard(currentEnemy);
        RestorePlayerActiveOrHand();
    }

    private void RestorePlayerActiveOrHand()
    {
        SelectedCardData selectedPlayer = FindCard(currentSession?.playerDeck?.cards, ActivePlayerCardId);
        if (selectedPlayer == null)
        {
            RenderPlayerHand();
            return;
        }

        Kard activeCard = CreateCardInGame(
            RoyalRumbleCardMapper.ToGeneratedCard(selectedPlayer),
            player != null ? player.gameObject : null,
            player,
            addToHand: false,
            battleAreaOverride: playerBoard
        );

        if (activeCard == null || player == null || playerBoard == null)
        {
            return;
        }

        player.PlayCard(activeCard, playerBoard);
        player.cardInGame = activeCard;
        renderedPlayerActiveCard = activeCard;
        activeCard.isDragable = false;
        playerLifeBar?.SetBar(activeCard);
        SyncEffectIconsFromSnapshot(activeCard, selectedPlayer.effects);

        foreach (SelectedCardData cardData in currentSession.playerDeck.cards.Where(IsAlive))
        {
            if (string.Equals(cardData.cardId, selectedPlayer.cardId, StringComparison.Ordinal))
            {
                continue;
            }

            Kard handCard = CreateCardInGame(
                RoyalRumbleCardMapper.ToGeneratedCard(cardData),
                player.gameObject,
                player,
                addToHand: true,
                battleAreaOverride: playerBoard
            );

            if (handCard == null)
            {
                continue;
            }

            RoyalRumbleSelectableCard selectable = handCard.gameObject.GetComponent<RoyalRumbleSelectableCard>() ?? handCard.gameObject.AddComponent<RoyalRumbleSelectableCard>();
            selectable.Initialize(this, handCard);
            RoyalRumbleCardDrag dragHandler = handCard.gameObject.GetComponent<RoyalRumbleCardDrag>() ?? handCard.gameObject.AddComponent<RoyalRumbleCardDrag>();
            dragHandler.Initialize(this);
            handCard.isDragable = CanDragCard(handCard);
            renderedPlayerHandCards.Add(handCard);
        }
    }

    private void SyncActiveCardsFromSession()
    {
        LogVerboseWarning(
            $"[RoyalRumbleShellController] Applying lightweight RR sync: player={DescribeCard(FindCard(currentSession?.playerDeck?.cards, ActivePlayerCardId))}, " +
            $"enemy={DescribeCard(FindCard(currentSession?.enemyDeck?.cards, ActiveEnemyCardId))}"
        );
        ApplyCardSnapshotToRenderedCard(
            FindCard(currentSession?.playerDeck?.cards, ActivePlayerCardId),
            renderedPlayerActiveCard,
            playerLifeBar
        );

        ApplyCardSnapshotToRenderedCard(
            FindCard(currentSession?.enemyDeck?.cards, ActiveEnemyCardId),
            renderedEnemyActiveCard,
            enemyLifeBar
        );
    }

    private void ApplyCardSnapshotToRenderedCard(SelectedCardData snapshot, Kard card, HealthBar lifeBar)
    {
        if (snapshot == null || card == null)
        {
            return;
        }

        card.cardId = snapshot.cardId;
        card.styleId = snapshot.styleId;
        card.cardName = snapshot.name;
        card.image = snapshot.image;
        card.level = snapshot.level;
        card.health = snapshot.health;
        card.maxHealth = snapshot.maxHealth;
        card.strength = snapshot.strength;
        card.speed = snapshot.speed;
        card.attack = snapshot.attack;
        card.defense = snapshot.defense;
        card.knowledge = snapshot.knowledge;
        card.charisma = snapshot.charisma;
        card.experience = snapshot.experience;
        card.attack1 = snapshot.attack1;
        card.attack2 = snapshot.attack2;
        card.attack3 = snapshot.attack3;
        card.attack4 = snapshot.attack4;

        if (lifeBar != null)
        {
            lifeBar.SetBar(card);
        }

        SyncEffectIconsFromSnapshot(card, snapshot.effects);
    }

    private void SyncEffectIconsFromSnapshot(Kard card, SelectedCardData.EffectData[] effects)
    {
        if (card?.effectIconContainer == null)
        {
            return;
        }

        List<string> expectedEffectNames = new List<string>();
        if (effects != null)
        {
            foreach (SelectedCardData.EffectData effect in effects)
            {
                if (effect == null || string.IsNullOrWhiteSpace(effect.type))
                {
                    continue;
                }

                if (!int.TryParse(effect.type, out int effectType))
                {
                    continue;
                }

                string effectName = BattleEffectPlayback.GetEffectName(effectType);
                if (!string.IsNullOrWhiteSpace(effectName))
                {
                    expectedEffectNames.Add(effectName);
                }
            }
        }

        List<Transform> iconsToRemove = new List<Transform>();
        foreach (Transform child in card.effectIconContainer)
        {
            if (child == null)
            {
                continue;
            }

            bool keepIcon = expectedEffectNames.Any(effectName =>
                child.name.StartsWith(effectName + "Icon", StringComparison.Ordinal)
            );

            if (!keepIcon)
            {
                iconsToRemove.Add(child);
            }
        }

        foreach (Transform icon in iconsToRemove)
        {
            DestroyEffectIconBeforeReposition(icon);
        }

        foreach (string effectName in expectedEffectNames)
        {
            bool hasIcon = false;
            foreach (Transform child in card.effectIconContainer)
            {
                if (child != null && child.name.StartsWith(effectName + "Icon", StringComparison.Ordinal))
                {
                    hasIcon = true;
                    break;
                }
            }

            if (!hasIcon)
            {
                card.AddEffectIcon(effectName);
            }
        }

        if (iconsToRemove.Count > 0 || expectedEffectNames.Count > 0)
        {
            card.RepositionEffectIcons();
        }
    }

    private void DestroyEffectIconBeforeReposition(Transform icon)
    {
        if (icon == null)
        {
            return;
        }

        GameObject iconObject = icon.gameObject;
        icon.SetParent(null, false);
        Destroy(iconObject);
    }

    private void ApplyPostBattleStatus(RoyalRumbleBattleEnvelopeDto envelope)
    {
        if (envelope == null)
        {
            return;
        }

        if (envelope.runEnded)
        {
            ClearPendingOngoingState();
            SetStatus(envelope.runStatus == "won"
                ? "You won the Royal Rumble!"
                : envelope.runStatus == "lost"
                    ? "You lost the Royal Rumble!"
                    : "Royal Rumble ended.");
            return;
        }

        if (envelope.playerNeedsReplacement)
        {
            ClearPendingOngoingState();
            SetStatus("Choose your next fighter!");
            return;
        }

        if (envelope.enemyNeedsReplacement)
        {
            SetStatus("Enemy sends the next fighter...");
            return;
        }

        SetStatus("Choose attack!");
    }

    private void UpdateAttackButtonDisplay(Button button, TMP_Text nameText, TMP_Text countText, int attackSlot)
    {
        if (button == null)
        {
            return;
        }

        SelectedCardData selected = FindCard(currentSession?.playerDeck?.cards, ActivePlayerCardId);
        if (selected == null)
        {
            RestoreAttackButtonDefaults(attackSlot, button, nameText, countText);
            return;
        }

        int attackId = attackSlot switch
        {
            1 => selected.attack1,
            2 => selected.attack2,
            3 => selected.attack3,
            4 => selected.attack4,
            _ => 0
        };

        if (attackId <= 0)
        {
            SetAttackNameText(nameText, string.Empty);
            SetAttackCountText(countText, -1);
            button.gameObject.SetActive(false);
            return;
        }

        SetAttackNameText(nameText, GetAttackName(attackId));
        SetAttackCountText(countText, GetRemainingCountForSlot(selected.cardId, attackSlot));
        button.gameObject.SetActive(true);
    }

    private SharedAttackSelectionFlow.AttackCountsState BuildCurrentAttackCountsState()
    {
        SelectedCardData selected = FindCard(currentSession?.playerDeck?.cards, ActivePlayerCardId);
        if (selected == null)
        {
            return null;
        }

        return new SharedAttackSelectionFlow.AttackCountsState
        {
            count1 = Mathf.Max(0, GetRemainingCountForSlot(selected.cardId, 1)),
            count2 = Mathf.Max(0, GetRemainingCountForSlot(selected.cardId, 2)),
            count3 = Mathf.Max(0, GetRemainingCountForSlot(selected.cardId, 3)),
            count4 = Mathf.Max(0, GetRemainingCountForSlot(selected.cardId, 4))
        };
    }

    private int GetRemainingCountForSlot(string cardId, int attackSlot)
    {
        if (string.IsNullOrWhiteSpace(cardId)
            || currentSession?.attackCounts?.player == null
            || !currentSession.attackCounts.player.TryGetValue(cardId, out RoyalRumbleAttackCountEntryDto counts)
            || counts == null)
        {
            return -1;
        }

        return attackSlot switch
        {
            1 => counts.count1,
            2 => counts.count2,
            3 => counts.count3,
            4 => counts.count4,
            _ => -1
        };
    }

    private void SetAttackButtonsInteractable(bool enabled)
    {
        if (attackButton1 != null) attackButton1.interactable = enabled;
        if (attackButton2 != null) attackButton2.interactable = enabled;
        if (attackButton3 != null) attackButton3.interactable = enabled;
        if (attackButton4 != null) attackButton4.interactable = enabled;
    }

    private void SetAttackButtonsInteractable(bool button1Enabled, bool button2Enabled, bool button3Enabled, bool button4Enabled)
    {
        if (attackButton1 != null) attackButton1.interactable = button1Enabled;
        if (attackButton2 != null) attackButton2.interactable = button2Enabled;
        if (attackButton3 != null) attackButton3.interactable = button3Enabled;
        if (attackButton4 != null) attackButton4.interactable = button4Enabled;
    }

    private void SetConfirmButtonState(bool enabled)
    {
        if (confirmButton != null)
        {
            confirmButton.interactable = enabled;
        }
    }

    private void ResolveAttackTextReferences()
    {
        attackButton1 ??= FindButtonByName("AttackButton (1)");
        attackButton2 ??= FindButtonByName("AttackButton (2)");
        attackButton3 ??= FindButtonByName("AttackButton (3)");
        attackButton4 ??= FindButtonByName("AttackButton (4)");
        confirmButton ??= FindButtonByName("DialogButton2");

        attackButton1Text ??= FindButtonText(attackButton1);
        attackButton2Text ??= FindButtonText(attackButton2);
        attackButton3Text ??= FindButtonText(attackButton3);
        attackButton4Text ??= FindButtonText(attackButton4);
        attackButton1CountText ??= FindAttackCountText(attackButton1);
        attackButton2CountText ??= FindAttackCountText(attackButton2);
        attackButton3CountText ??= FindAttackCountText(attackButton3);
        attackButton4CountText ??= FindAttackCountText(attackButton4);

        if (attackButton1Text == null || attackButton2Text == null || attackButton3Text == null || attackButton4Text == null)
        {
            Debug.LogWarning("[RoyalRumbleShellController] Some RR attack name text references were not resolved automatically.");
        }

        CacheDefaultAttackButtonTexts();
    }

    private void CacheDefaultAttackButtonTexts()
    {
        defaultAttackButton1Name ??= attackButton1Text != null ? attackButton1Text.text : string.Empty;
        defaultAttackButton2Name ??= attackButton2Text != null ? attackButton2Text.text : string.Empty;
        defaultAttackButton3Name ??= attackButton3Text != null ? attackButton3Text.text : string.Empty;
        defaultAttackButton4Name ??= attackButton4Text != null ? attackButton4Text.text : string.Empty;
        defaultAttackButton1Count ??= attackButton1CountText != null ? attackButton1CountText.text : string.Empty;
        defaultAttackButton2Count ??= attackButton2CountText != null ? attackButton2CountText.text : string.Empty;
        defaultAttackButton3Count ??= attackButton3CountText != null ? attackButton3CountText.text : string.Empty;
        defaultAttackButton4Count ??= attackButton4CountText != null ? attackButton4CountText.text : string.Empty;
    }

    private void RestoreAttackButtonDefaults(int attackSlot, Button button, TMP_Text nameText, TMP_Text countText)
    {
        string defaultName = attackSlot switch
        {
            1 => defaultAttackButton1Name,
            2 => defaultAttackButton2Name,
            3 => defaultAttackButton3Name,
            4 => defaultAttackButton4Name,
            _ => string.Empty
        };

        string defaultCount = attackSlot switch
        {
            1 => defaultAttackButton1Count,
            2 => defaultAttackButton2Count,
            3 => defaultAttackButton3Count,
            4 => defaultAttackButton4Count,
            _ => string.Empty
        };

        SetAttackNameText(nameText, defaultName);
        if (countText != null)
        {
            countText.text = defaultCount ?? string.Empty;
        }

        button.gameObject.SetActive(true);
    }

    private void ResetAttackSelectionForShellState()
    {
        attackSelectionFlow?.ResetSelection(clearDialog: false);
        SetConfirmButtonState(false);
    }

    private void ClearPendingOngoingState()
    {
        lastAutoSubmittedPendingKey = null;
    }

    private string GetAttackName(int attackId)
    {
        return attackDescriptions != null ? attackDescriptions.GetAttackName(attackId) : $"Attack {attackId}";
    }

    private static Button FindButtonByName(string objectName)
    {
        GameObject buttonObject = GameObject.Find(objectName);
        return buttonObject != null ? buttonObject.GetComponent<Button>() : null;
    }

    private static TMP_Text FindButtonText(Button button)
    {
        if (button == null)
        {
            return null;
        }

        Transform labelTransform = button.transform.Find("Text (TMP)");
        if (labelTransform != null)
        {
            TMP_Text directMatch = labelTransform.GetComponent<TMP_Text>();
            if (directMatch != null)
            {
                return directMatch;
            }
        }

        TMP_Text[] texts = button.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text text in texts)
        {
            if (text == null)
            {
                continue;
            }

            if (string.Equals(text.gameObject.name, "AttackCountText", System.StringComparison.Ordinal))
            {
                continue;
            }

            return text;
        }

        return null;
    }

    private static TMP_Text FindAttackCountText(Button button)
    {
        if (button == null)
        {
            return null;
        }

        Transform countTransform = button.transform.Find("AttackCountText");
        return countTransform != null ? countTransform.GetComponent<TMP_Text>() : null;
    }

    private static void SetAttackNameText(TMP_Text textComponent, string value)
    {
        if (textComponent != null)
        {
            textComponent.text = value ?? string.Empty;
        }
    }

    private static void SetAttackCountText(TMP_Text textComponent, int value)
    {
        if (textComponent != null)
        {
            textComponent.text = value >= 0 ? value.ToString() : string.Empty;
        }
    }

    private static SelectedCardData FindCard(List<SelectedCardData> cards, string cardId)
    {
        if (cards == null || string.IsNullOrWhiteSpace(cardId))
        {
            return null;
        }

        return cards.FirstOrDefault(card => card != null && card.cardId == cardId);
    }

    private static bool IsAlive(SelectedCardData card)
    {
        return card != null && card.health > 0;
    }

    private static IEnumerable<Player> FindPlayers(bool wantEnemy)
    {
        return FindObjectsByType<Player>(FindObjectsSortMode.None).Where(player => player != null && player.isEnemy == wantEnemy);
    }

    private void SetStatus(string message)
    {
        if (dialogText != null)
        {
            dialogText.text = message ?? string.Empty;
        }
    }

    private static string DescribePendingAction(PendingOngoingActionTurnData pendingAction)
    {
        if (pendingAction == null)
        {
            return "none";
        }

        return $"{pendingAction.actionType}/attackId={pendingAction.sourceAttackId}/turns={pendingAction.turnsRemaining}/target={pendingAction.targetCardId}";
    }

    private static string BuildPendingActionKey(string playerCardId, PendingOngoingActionTurnData pendingAction)
    {
        if (pendingAction == null)
        {
            return null;
        }

        return $"{playerCardId ?? string.Empty}|{pendingAction.actionType}|{pendingAction.sourceAttackId}|{pendingAction.turnsRemaining}|{pendingAction.targetCardId ?? string.Empty}";
    }

    private static string DescribeCard(SelectedCardData card)
    {
        if (card == null)
        {
            return "none";
        }

        return $"{card.name}({card.cardId}) HP={card.health}/{card.maxHealth} ATK_IDS=[{card.attack1},{card.attack2},{card.attack3},{card.attack4}] " +
               $"EFFECTS={DescribeEffects(card.effects)} ONGOING={DescribeOngoingActions(card.ongoingActions)}";
    }

    private static string DescribeEffects(SelectedCardData.EffectData[] effects)
    {
        if (effects == null || effects.Length == 0)
        {
            return "[]";
        }

        return "[" + string.Join(", ", effects
            .Where(effect => effect != null)
            .Select(effect => $"{effect.type}(dur={effect.duration},turn={effect.appliedTurn},value={effect.value})")) + "]";
    }

    private static string DescribeOngoingActions(SelectedCardData.OngoingActionData[] actions)
    {
        if (actions == null || actions.Length == 0)
        {
            return "[]";
        }

        return "[" + string.Join(", ", actions
            .Where(action => action != null)
            .Select(action =>
                $"{action.type}(attackId={action.sourceAttackId},phase={action.phase},turns={action.turnsRemaining},target={action.targetCardId ?? "none"})")) + "]";
    }

    private static int GetAttackIdForSlot(SelectedCardData selected, int attackSlot)
    {
        if (selected == null)
        {
            return 0;
        }

        return attackSlot switch
        {
            1 => selected.attack1,
            2 => selected.attack2,
            3 => selected.attack3,
            4 => selected.attack4,
            _ => 0
        };
    }

    private static string DescribeBattleSummary(BattleResultDto battleResult, string playerCardId)
    {
        if (battleResult == null)
        {
            return "no battle result";
        }

        int playerDamageDealt = 0;
        int enemyDamageDealt = 0;
        int playerHeal = 0;
        int enemyHeal = 0;

        void Accumulate(BattleAttackerDto attacker)
        {
            if (attacker == null)
            {
                return;
            }

            bool isPlayerAttack = string.Equals(attacker.cardId, playerCardId, StringComparison.Ordinal);
            if (isPlayerAttack)
            {
                playerDamageDealt += attacker.damageDealt;
                playerHeal += attacker.healAmount;
            }
            else
            {
                enemyDamageDealt += attacker.damageDealt;
                enemyHeal += attacker.healAmount;
            }
        }

        Accumulate(battleResult.firstAttacker);
        Accumulate(battleResult.secondAttacker);

        return $"playerDamage={playerDamageDealt}, enemyDamage={enemyDamageDealt}, playerHeal={playerHeal}, enemyHeal={enemyHeal}, cardDied={battleResult.cardDied}, winner={battleResult.winnerCardId}, loser={battleResult.loserCardId}";
    }

    private static void LogVerboseWarning(string message)
    {
        if (VerboseShellLogs)
        {
            Debug.LogWarning(message);
        }
    }

    private void OnDestroy()
    {
        isShuttingDown = true;

        if (activeInstance == this)
        {
            activeInstance = null;
        }
    }
}
