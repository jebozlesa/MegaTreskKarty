using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoyalRumbleBattleCoordinator : MonoBehaviour
{
    [Header("Dependencies")]
    public RoyalRumbleService royalRumbleService;
    public RoyalRumbleBattlePlayback battlePlayback;
    public Player player;
    public Player enemy;
    public HealthBar playerLifeBar;
    public HealthBar enemyLifeBar;
    public TMP_Text dialogText;
    public AttackDescriptions attackDescriptions;
    public GameObject cardPrefab;

    [Header("Scene References")]
    public GameObject playerHandRoot;
    public GameObject enemyHandRoot;
    public GameObject playerBoard;
    public GameObject enemyBoard;

    [Header("Optional Attack UI")]
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
    public bool autoStart = true;
    public float enemyRevealDelaySeconds = 0.05f;
    public float enemyPostRevealDelaySeconds = 1f;

    private RoyalRumbleSessionDto currentSession;
    private string playerId;
    private bool isBusy;
    private bool openingSequenceRunning;
    private bool sessionAbandonRequested;
    private SharedAttackSelectionFlow attackSelectionFlow;
    private readonly List<Kard> renderedPlayerHandCards = new List<Kard>();
    private readonly List<Kard> renderedEnemyHandCards = new List<Kard>();
    private Kard renderedPlayerActiveCard;
    private Kard renderedEnemyActiveCard;
    public string CurrentSessionId => currentSession?.sessionId ?? string.Empty;
    public RoyalRumbleSessionDto CurrentSession => currentSession;
    private string ActivePlayerCardId => currentSession?.active?.playerCardId ?? string.Empty;
    private string ActiveEnemyCardId => currentSession?.active?.enemyCardId ?? string.Empty;

    public bool CanDragCard(Kard card)
    {
        if (card == null || isBusy || openingSequenceRunning || currentSession == null)
        {
            return false;
        }

        return IsFighterSelectionStatus() && renderedPlayerHandCards.Contains(card);
    }

    private async void Start()
    {
        ResolveDependencies();
        WireAttackButtons();

        if (!autoStart)
        {
            return;
        }

        await StartRoyalRumbleAsync();
    }

    private void OnDestroy()
    {
        TryAbandonSessionOnExit();
    }

    private void OnApplicationQuit()
    {
        TryAbandonSessionOnExit();
    }

    public async Task StartRoyalRumbleAsync()
    {
        if (isBusy)
        {
            return;
        }

        isBusy = true;
        try
        {
            playerId =
                PlayFabManagerLogin.Instance != null
                    ? PlayFabManagerLogin.Instance.LoggedInPlayerId
                    : PlayerPrefs.GetString("LoggedInPlayerId", string.Empty);

            if (string.IsNullOrWhiteSpace(playerId))
            {
                SetStatus("Missing player ID.");
                return;
            }

            SetStatus("Preparing Royal Rumble...");

            RoyalRumbleSessionEnvelopeDto sessionEnvelope =
                await royalRumbleService.CreateSessionAsync(playerId);
            if (sessionEnvelope?.session == null)
            {
                SetStatus("Failed to create Royal Rumble session.");
                return;
            }

            currentSession = sessionEnvelope.session;
            sessionAbandonRequested = false;
            await EnsureDecksLoadedAsync();
            StartCoroutine(PlayInitialOpeningSequence());
        }
        finally
        {
            isBusy = false;
            UpdateAttackButtons();
        }
    }

    public async Task RefreshSessionAsync()
    {
        if (isBusy || string.IsNullOrWhiteSpace(CurrentSessionId))
        {
            return;
        }

        isBusy = true;
        try
        {
            RoyalRumbleSessionEnvelopeDto envelope = await royalRumbleService.GetSessionAsync(
                CurrentSessionId,
                playerId
            );
            if (envelope?.session != null)
            {
                currentSession = envelope.session;
                RenderSession();
            }
        }
        finally
        {
            isBusy = false;
            UpdateAttackButtons();
        }
    }

    public void OnHandCardClicked(Kard card)
    {
        ShowDragSelectionHint();
    }

    public void OnAttackSlotClicked(int attackSlot)
    {
        AttackButton(attackSlot);
    }

    public void AttackButton(int attackType)
    {
        PendingOngoingActionTurnData pendingTurn = GetPendingOngoingAction();
        Debug.LogWarning(
            $"[RoyalRumbleBattleCoordinator] Attack button clicked. slot={attackType}, status={currentSession?.status}, isBusy={isBusy}, "
                + $"pendingOngoing={(pendingTurn != null ? $"{pendingTurn.actionType}/{pendingTurn.sourceAttackId}/turns={pendingTurn.turnsRemaining}" : "none")}"
        );
        attackSelectionFlow?.OnAttackButtonClicked(attackType);
    }

    public void ConfirmAttackButton()
    {
        PendingOngoingActionTurnData pendingTurn = GetPendingOngoingAction();
        Debug.LogWarning(
            $"[RoyalRumbleBattleCoordinator] Confirm attack clicked. selectedSlot={attackSelectionFlow?.SelectedAttackType ?? 0}, status={currentSession?.status}, isBusy={isBusy}, "
                + $"pendingOngoing={(pendingTurn != null ? $"{pendingTurn.actionType}/{pendingTurn.sourceAttackId}/turns={pendingTurn.turnsRemaining}" : "none")}"
        );
        attackSelectionFlow?.OnConfirmAttackClicked();
    }

    public void RestartGame()
    {
        TryAbandonSessionOnExit();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ShowDragSelectionHint()
    {
        SetStatus("Drag a fighter to the battle area.");
    }

    private async Task EnsureDecksLoadedAsync()
    {
        if (currentSession == null)
        {
            return;
        }

        if (
            currentSession.playerDeck == null
            || currentSession.playerDeck.cards == null
            || currentSession.playerDeck.cards.Count == 0
        )
        {
            RoyalRumbleSessionEnvelopeDto playerDeckEnvelope =
                await royalRumbleService.LoadPlayerDeckAsync(currentSession.sessionId, playerId);
            if (playerDeckEnvelope?.session != null)
            {
                currentSession = playerDeckEnvelope.session;
            }
            else
            {
                Debug.LogWarning("[RoyalRumbleBattleCoordinator] Failed to load RR player deck.");
                SetStatus("Failed to load player deck.");
            }
        }

        if (
            currentSession.enemyDeck == null
            || currentSession.enemyDeck.cards == null
            || currentSession.enemyDeck.cards.Count == 0
        )
        {
            RoyalRumbleSessionEnvelopeDto enemyDeckEnvelope =
                await royalRumbleService.LoadEnemyDeckAsync(currentSession.sessionId, playerId);
            if (enemyDeckEnvelope?.session != null)
            {
                currentSession = enemyDeckEnvelope.session;
            }
            else
            {
                Debug.LogWarning("[RoyalRumbleBattleCoordinator] Failed to load RR enemy deck.");
                SetStatus("Failed to load enemy deck.");
            }
        }

        if (currentSession.playerDeck?.cards == null || currentSession.playerDeck.cards.Count == 0)
        {
            Debug.LogWarning(
                "[RoyalRumbleBattleCoordinator] RR player deck is still empty after hydration."
            );
        }

        if (currentSession.enemyDeck?.cards == null || currentSession.enemyDeck.cards.Count == 0)
        {
            Debug.LogWarning(
                "[RoyalRumbleBattleCoordinator] RR enemy deck is still empty after hydration."
            );
        }
        else if (string.IsNullOrWhiteSpace(ActiveEnemyCardId))
        {
            Debug.LogWarning(
                "[RoyalRumbleBattleCoordinator] Server active enemy card is missing after hydration; RR cannot choose one locally."
            );
        }
    }

    private IEnumerator PlayInitialOpeningSequence()
    {
        openingSequenceRunning = true;
        ClearRenderedCards();
        RenderPlayerHand();
        SetPlayerHandInteractivity(false);
        UpdateAttackButtons();

        if (currentSession?.enemyDeck?.cards == null || currentSession.enemyDeck.cards.Count == 0)
        {
            Debug.LogWarning(
                "[RoyalRumbleBattleCoordinator] Opening sequence started without an enemy deck."
            );
            openingSequenceRunning = false;
            RenderSession();
            yield break;
        }

        SelectedCardData selectedEnemy = ResolveSelectedEnemyCard();
        List<SelectedCardData> aliveEnemyCards = currentSession
            .enemyDeck.cards.Where(IsAlive)
            .ToList();

        Debug.LogWarning(
            $"[RoyalRumbleBattleCoordinator] Starting RR enemy reveal with {aliveEnemyCards.Count} cards."
        );
        SetStatus("Preparing enemy fighters...");

        foreach (SelectedCardData cardData in aliveEnemyCards)
        {
            Kard createdCard = CreateCardInGame(
                RoyalRumbleCardMapper.ToGeneratedCard(cardData),
                enemy.gameObject,
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
        openingSequenceRunning = false;
        SetPlayerHandInteractivity(CanSelectFighter());
        UpdateAttackButtons();
        UpdateStatusFromSession();
        Debug.LogWarning(
            "[RoyalRumbleBattleCoordinator] RR enemy reveal finished. Player may choose fighter."
        );
    }

    public async void OnCardDropped(Kard card, RoyalRumbleCardDrag dragHandler)
    {
        if (card == null)
        {
            Debug.LogWarning("[RoyalRumbleBattleCoordinator] OnCardDropped called with null card.");
            dragHandler?.ResetToOriginalPosition();
            return;
        }

        if (!CanDragCard(card))
        {
            Debug.LogWarning(
                $"[RoyalRumbleBattleCoordinator] Ignored dropped card {card.cardName}. isBusy={isBusy}, status={currentSession?.status}"
            );
            dragHandler?.ResetToOriginalPosition();
            return;
        }

        isBusy = true;
        try
        {
            SetStatus("Selecting fighter...");
            player.PlayCard(card, playerBoard);
            player.cardInGame.isDragable = false;
            renderedPlayerActiveCard = player.cardInGame;
            playerLifeBar?.SetBar(player.cardInGame);
            SetPlayerHandInteractivity(false);

            RoyalRumbleSessionEnvelopeDto envelope = await royalRumbleService.SelectCardAsync(
                CurrentSessionId,
                playerId,
                card.cardId
            );
            if (envelope?.session == null)
            {
                Debug.LogWarning(
                    $"[RoyalRumbleBattleCoordinator] Failed to select fighter {card.cardId} after drop."
                );
                Vector3 returnPosition =
                    dragHandler != null ? dragHandler.GetOriginalLocalPosition() : Vector3.zero;
                player.ReturnCardToHand(card, returnPosition);
                card.isDragable = true;
                SetPlayerHandInteractivity(true);
                SetStatus("Failed to select fighter.");
                return;
            }

            currentSession = envelope.session;
            Debug.LogWarning(
                $"[RoyalRumbleBattleCoordinator] Fighter selected via drag: {card.cardId}"
            );
            renderedPlayerHandCards.Remove(card);
            UpdateAttackButtons();
            UpdateStatusFromSession();
        }
        finally
        {
            isBusy = false;
            UpdateAttackButtons();
        }
    }

    private void RenderSession()
    {
        ClearRenderedCards();
        RenderPlayerHand();
        RenderEnemyHand();
        if (!string.IsNullOrWhiteSpace(ActivePlayerCardId))
        {
            RenderPlayerActiveCard();
        }

        if (ShouldRenderEnemyActiveCard())
        {
            RenderEnemyActiveCard();
        }

        SetPlayerHandInteractivity(CanSelectFighter());
        UpdateAttackButtons();
        UpdateStatusFromSession();
    }

    private void RenderPlayerHand()
    {
        if (currentSession?.playerDeck?.cards == null || player == null)
        {
            return;
        }

        string selectedCardId = ActivePlayerCardId;
        foreach (SelectedCardData cardData in currentSession.playerDeck.cards.Where(IsAlive))
        {
            if (!string.IsNullOrEmpty(selectedCardId) && cardData.cardId == selectedCardId)
            {
                continue;
            }

            Kard createdCard = CreateCardInGame(
                RoyalRumbleCardMapper.ToGeneratedCard(cardData),
                player.gameObject,
                player,
                addToHand: true,
                battleAreaOverride: playerBoard
            );

            if (createdCard == null)
            {
                continue;
            }

            RoyalRumbleSelectableCard selectable =
                createdCard.gameObject.GetComponent<RoyalRumbleSelectableCard>();
            if (selectable == null)
            {
                selectable = createdCard.gameObject.AddComponent<RoyalRumbleSelectableCard>();
            }

            selectable.Initialize(this, createdCard);
            RoyalRumbleCardDrag dragHandler =
                createdCard.gameObject.GetComponent<RoyalRumbleCardDrag>();
            if (dragHandler == null)
            {
                dragHandler = createdCard.gameObject.AddComponent<RoyalRumbleCardDrag>();
            }
            dragHandler.Initialize(this);
            createdCard.isDragable = CanSelectFighter();
            renderedPlayerHandCards.Add(createdCard);
        }
    }

    private void RenderEnemyHand()
    {
        if (currentSession?.enemyDeck?.cards == null || enemy == null)
        {
            return;
        }

        string selectedCardId = ActiveEnemyCardId;
        int aliveEnemyCards = 0;

        foreach (SelectedCardData cardData in currentSession.enemyDeck.cards.Where(IsAlive))
        {
            aliveEnemyCards++;

            if (!string.IsNullOrEmpty(selectedCardId) && cardData.cardId == selectedCardId)
            {
                continue;
            }

            Kard createdCard = CreateCardInGame(
                RoyalRumbleCardMapper.ToGeneratedCard(cardData),
                enemy.gameObject,
                enemy,
                addToHand: true,
                battleAreaOverride: enemyBoard
            );

            if (createdCard == null)
            {
                continue;
            }

            renderedEnemyHandCards.Add(createdCard);
        }

        if (aliveEnemyCards > 1 && renderedEnemyHandCards.Count == 0)
        {
            Debug.LogWarning(
                $"[RoyalRumbleBattleCoordinator] Enemy deck has {aliveEnemyCards} alive cards but rendered enemy hand is empty. selectedCardId={selectedCardId}"
            );
        }
    }

    private void RenderPlayerActiveCard()
    {
        SelectedCardData selected = FindCard(currentSession?.playerDeck?.cards, ActivePlayerCardId);
        if (selected == null || player == null)
        {
            return;
        }

        renderedPlayerActiveCard = CreateCardInGame(
            RoyalRumbleCardMapper.ToGeneratedCard(selected),
            playerBoard,
            player,
            addToHand: false,
            battleAreaOverride: playerBoard
        );

        if (renderedPlayerActiveCard != null)
        {
            player.cardInGame = renderedPlayerActiveCard;
            playerLifeBar?.SetBar(renderedPlayerActiveCard);
        }
    }

    private void RenderEnemyActiveCard()
    {
        SelectedCardData selected = ResolveSelectedEnemyCard();
        if (selected == null || enemy == null)
        {
            if (selected == null && !string.IsNullOrEmpty(ActiveEnemyCardId))
            {
                Debug.LogWarning(
                    $"[RoyalRumbleBattleCoordinator] Active enemy card '{ActiveEnemyCardId}' was not found in the loaded enemy deck."
                );
            }

            if (selected == null || enemy == null)
            {
                return;
            }
        }

        renderedEnemyActiveCard = CreateCardInGame(
            RoyalRumbleCardMapper.ToGeneratedCard(selected),
            enemyBoard,
            enemy,
            addToHand: false,
            battleAreaOverride: enemyBoard
        );

        if (renderedEnemyActiveCard != null)
        {
            enemy.cardInGame = renderedEnemyActiveCard;
            enemyLifeBar?.SetBar(renderedEnemyActiveCard);
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
        renderedPlayerHandCards.Clear();

        foreach (Kard card in renderedEnemyHandCards)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }
        renderedEnemyHandCards.Clear();

        if (renderedPlayerActiveCard != null)
        {
            Destroy(renderedPlayerActiveCard.gameObject);
            renderedPlayerActiveCard = null;
        }

        if (renderedEnemyActiveCard != null)
        {
            Destroy(renderedEnemyActiveCard.gameObject);
            renderedEnemyActiveCard = null;
        }

        if (player != null)
        {
            player.hand.Clear();
            player.cardInGame = null;
        }

        if (enemy != null)
        {
            enemy.hand.Clear();
            enemy.cardInGame = null;
        }
    }

    private void UpdateStatusFromSession()
    {
        if (currentSession == null)
        {
            SetStatus("No Royal Rumble session.");
            return;
        }

        switch (currentSession.status)
        {
            case "awaiting_player_card":
            case "awaiting_replacement":
                SetStatus("Choose fighter!");
                break;
            case "awaiting_attack":
                SetStatus(
                    string.IsNullOrEmpty(ActivePlayerCardId) ? "Choose fighter!" : "Choose attack!"
                );
                break;
            case "won":
                SetStatus("You won the Royal Rumble!");
                break;
            case "lost":
                SetStatus("You lost the Royal Rumble!");
                break;
            case "abandoned":
                SetStatus("Royal Rumble abandoned.");
                break;
            default:
                SetStatus(currentSession.status);
                break;
        }
    }

    private void SetStatus(string message)
    {
        if (dialogText != null)
        {
            dialogText.text = message ?? string.Empty;
        }
    }

    private void WireAttackButtons()
    {
        attackButton1 ??= FindButtonByName("AttackButton (1)");
        attackButton2 ??= FindButtonByName("AttackButton (2)");
        attackButton3 ??= FindButtonByName("AttackButton (3)");
        attackButton4 ??= FindButtonByName("AttackButton (4)");
        confirmButton ??= FindButtonByName("DialogButton2");
        ResolveAttackTextReferences();

        if (
            attackButton1 == null
            || attackButton2 == null
            || attackButton3 == null
            || attackButton4 == null
            || confirmButton == null
        )
        {
            Debug.LogWarning(
                "[RoyalRumbleBattleCoordinator] RR attack UI references are incomplete. Verify button wiring in the editor."
            );
        }
    }

    private IEnumerator SubmitAttackRoutine(int attackSlot)
    {
        isBusy = true;
        SetAttackButtonsInteractable(false);
        SetStatus($"Using attack {attackSlot}...");
        PendingOngoingActionTurnData pendingTurnBeforeSubmit = GetPendingOngoingAction();
        Debug.LogWarning(
            $"[RoyalRumbleBattleCoordinator] Submitting RR attack. slot={attackSlot}, playerCard={ActivePlayerCardId}, enemyCard={ActiveEnemyCardId}, "
                + $"pendingOngoingBeforeSubmit={(pendingTurnBeforeSubmit != null ? $"{pendingTurnBeforeSubmit.actionType}/{pendingTurnBeforeSubmit.sourceAttackId}/turns={pendingTurnBeforeSubmit.turnsRemaining}" : "none")}"
        );
        string previousPlayerSelectedCardId = ActivePlayerCardId;
        string previousEnemySelectedCardId = ActiveEnemyCardId;

        Task<RoyalRumbleBattleEnvelopeDto> submitTask = royalRumbleService.SubmitAttackAsync(
            CurrentSessionId,
            playerId,
            attackSlot
        );
        yield return new WaitUntil(() => submitTask.IsCompleted);

        RoyalRumbleBattleEnvelopeDto envelope =
            submitTask.Status == TaskStatus.RanToCompletion ? submitTask.Result : null;

        if (envelope?.battleResult == null)
        {
            SetStatus("Battle failed.");
            isBusy = false;
            UpdateAttackButtons();
            yield break;
        }

        if (
            battlePlayback != null
            && renderedPlayerActiveCard != null
            && renderedEnemyActiveCard != null
        )
        {
            yield return StartCoroutine(
                battlePlayback.PlayBattleAsync(
                    envelope,
                    renderedPlayerActiveCard,
                    renderedEnemyActiveCard,
                    previousPlayerSelectedCardId
                )
            );
        }
        else
        {
            Debug.LogWarning(
                $"[RoyalRumbleBattleCoordinator] Skipping RR playback. battlePlaybackNull={battlePlayback == null}, playerCardNull={renderedPlayerActiveCard == null}, enemyCardNull={renderedEnemyActiveCard == null}"
            );
        }

        yield return StartCoroutine(ResetActiveCardPositionsAfterPlayback());

        if (envelope.session != null)
        {
            currentSession = envelope.session;
        }
        else
        {
            Task<RoyalRumbleSessionEnvelopeDto> refreshTask = royalRumbleService.GetSessionAsync(
                CurrentSessionId,
                playerId
            );
            yield return new WaitUntil(() => refreshTask.IsCompleted);

            RoyalRumbleSessionEnvelopeDto refreshEnvelope =
                refreshTask.Status == TaskStatus.RanToCompletion ? refreshTask.Result : null;
            if (refreshEnvelope?.session != null)
            {
                currentSession = refreshEnvelope.session;
            }
        }

        PendingOngoingActionTurnData pendingTurnAfterSubmit = GetPendingOngoingAction();
        Debug.LogWarning(
            $"[RoyalRumbleBattleCoordinator] RR submit finished. runStatus={envelope.runStatus}, playerNeedsReplacement={envelope.playerNeedsReplacement}, enemyNeedsReplacement={envelope.enemyNeedsReplacement}, "
                + $"pendingOngoingAfterSubmit={(pendingTurnAfterSubmit != null ? $"{pendingTurnAfterSubmit.actionType}/{pendingTurnAfterSubmit.sourceAttackId}/turns={pendingTurnAfterSubmit.turnsRemaining}" : "none")}"
        );
        RefreshPostBattleView(envelope, previousPlayerSelectedCardId, previousEnemySelectedCardId);
        isBusy = false;
        UpdateAttackButtons();
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

    private void UpdateAttackButtons()
    {
        bool attackPhaseActive =
            currentSession != null
            && currentSession.status == "awaiting_attack"
            && !string.IsNullOrWhiteSpace(ActivePlayerCardId)
            && !isBusy;

        SetAttackButtonsInteractable(false);

        UpdateAttackButtonDisplay(attackButton1, attackButton1Text, attackButton1CountText, 1);
        UpdateAttackButtonDisplay(attackButton2, attackButton2Text, attackButton2CountText, 2);
        UpdateAttackButtonDisplay(attackButton3, attackButton3Text, attackButton3CountText, 3);
        UpdateAttackButtonDisplay(attackButton4, attackButton4Text, attackButton4CountText, 4);

        Kard activeCard = renderedPlayerActiveCard ?? player?.cardInGame;
        PendingOngoingActionTurnData pendingTurn = GetPendingOngoingAction();
        if (pendingTurn != null)
        {
            Debug.LogWarning(
                $"[RoyalRumbleBattleCoordinator] Preparing attack UI with pending ongoing action. status={currentSession?.status}, activeCard={activeCard?.cardId}, "
                    + $"action={pendingTurn.actionType}, sourceAttackId={pendingTurn.sourceAttackId}, turnsRemaining={pendingTurn.turnsRemaining}, attackPhaseActive={attackPhaseActive}"
            );
        }
        attackSelectionFlow?.PrepareAttackSelection(
            activeCard,
            BuildCurrentAttackCountsState(),
            attackPhaseActive
        );
    }

    private void SetAttackButtonsInteractable(bool enabled)
    {
        if (attackButton1 != null)
            attackButton1.interactable = enabled;
        if (attackButton2 != null)
            attackButton2.interactable = enabled;
        if (attackButton3 != null)
            attackButton3.interactable = enabled;
        if (attackButton4 != null)
            attackButton4.interactable = enabled;
    }

    private void SetAttackButtonsInteractable(
        bool button1Enabled,
        bool button2Enabled,
        bool button3Enabled,
        bool button4Enabled
    )
    {
        if (attackButton1 != null)
            attackButton1.interactable = button1Enabled;
        if (attackButton2 != null)
            attackButton2.interactable = button2Enabled;
        if (attackButton3 != null)
            attackButton3.interactable = button3Enabled;
        if (attackButton4 != null)
            attackButton4.interactable = button4Enabled;
    }

    private void SetConfirmButtonState(bool enabled)
    {
        if (confirmButton != null)
        {
            confirmButton.interactable = enabled;
        }
    }

    private void UpdateAttackButtonDisplay(
        Button button,
        TMP_Text nameText,
        TMP_Text countText,
        int attackSlot
    )
    {
        if (button == null)
        {
            return;
        }

        SelectedCardData selected = FindCard(currentSession?.playerDeck?.cards, ActivePlayerCardId);
        if (selected == null)
        {
            SetAttackCountText(countText, -1);
            button.gameObject.SetActive(true);
            return;
        }

        int attackId = attackSlot switch
        {
            1 => selected.attack1,
            2 => selected.attack2,
            3 => selected.attack3,
            4 => selected.attack4,
            _ => 0,
        };

        if (attackId <= 0)
        {
            SetAttackNameText(nameText, string.Empty);
            SetAttackCountText(countText, -1);
            button.gameObject.SetActive(false);
            return;
        }

        string attackName = GetAttackName(attackId);
        int remainingCount = GetRemainingCountForSlot(selected.cardId, attackSlot);
        SetAttackNameText(nameText, attackName);
        SetAttackCountText(countText, remainingCount);
        button.gameObject.SetActive(true);
    }

    private string GetAttackDisplayText(int attackSlot)
    {
        SelectedCardData selected = FindCard(currentSession?.playerDeck?.cards, ActivePlayerCardId);
        if (selected == null)
        {
            return $"Attack {attackSlot}";
        }

        int attackId = attackSlot switch
        {
            1 => selected.attack1,
            2 => selected.attack2,
            3 => selected.attack3,
            4 => selected.attack4,
            _ => 0,
        };

        return attackId > 0 ? GetAttackName(attackId) : $"Attack {attackSlot}";
    }

    private bool CanUseAttackSlot(int attackSlot)
    {
        SelectedCardData selected = FindCard(currentSession?.playerDeck?.cards, ActivePlayerCardId);
        if (selected == null)
        {
            return false;
        }

        int attackId = attackSlot switch
        {
            1 => selected.attack1,
            2 => selected.attack2,
            3 => selected.attack3,
            4 => selected.attack4,
            _ => 0,
        };

        if (attackId <= 0)
        {
            return false;
        }

        if (
            currentSession?.attackCounts?.player == null
            || !currentSession.attackCounts.player.TryGetValue(
                selected.cardId,
                out RoyalRumbleAttackCountEntryDto counts
            )
            || counts == null
        )
        {
            return true;
        }

        int remainingCount = attackSlot switch
        {
            1 => counts.count1,
            2 => counts.count2,
            3 => counts.count3,
            4 => counts.count4,
            _ => 0,
        };

        return remainingCount > 0;
    }

    private SharedAttackSelectionFlow.AttackCountsState BuildCurrentAttackCountsState()
    {
        SelectedCardData selected = FindCard(currentSession?.playerDeck?.cards, ActivePlayerCardId);
        if (selected == null)
        {
            return null;
        }

        int count1 = GetRemainingCountForSlot(selected.cardId, 1);
        int count2 = GetRemainingCountForSlot(selected.cardId, 2);
        int count3 = GetRemainingCountForSlot(selected.cardId, 3);
        int count4 = GetRemainingCountForSlot(selected.cardId, 4);

        return new SharedAttackSelectionFlow.AttackCountsState
        {
            count1 = Mathf.Max(0, count1),
            count2 = Mathf.Max(0, count2),
            count3 = Mathf.Max(0, count3),
            count4 = Mathf.Max(0, count4),
        };
    }

    private PendingOngoingActionTurnData GetPendingOngoingAction()
    {
        SelectedCardData selected = FindCard(currentSession?.playerDeck?.cards, ActivePlayerCardId);
        PendingOngoingActionTurnData pendingAction =
            RoyalRumbleTurnAdapter.CreatePendingOngoingAction(selected);

        if (pendingAction == null)
        {
            return null;
        }

        Debug.LogWarning(
            $"[RoyalRumbleBattleCoordinator] Pending RR ongoing action detected on card {selected?.cardId}. type={pendingAction.actionType}, sourceAttackId={pendingAction.sourceAttackId}, turnsRemaining={pendingAction.turnsRemaining}, target={pendingAction.targetCardId}"
        );
        return pendingAction;
    }

    private void SubmitSelectedAttack(SelectedAttackData attackData)
    {
        if (attackData == null || attackData.attackType <= 0)
        {
            return;
        }

        StartCoroutine(SubmitAttackRoutine(attackData.attackType));
    }

    private int GetRemainingCountForSlot(string cardId, int attackSlot)
    {
        if (
            string.IsNullOrWhiteSpace(cardId)
            || currentSession?.attackCounts?.player == null
            || !currentSession.attackCounts.player.TryGetValue(
                cardId,
                out RoyalRumbleAttackCountEntryDto counts
            )
            || counts == null
        )
        {
            return -1;
        }

        return attackSlot switch
        {
            1 => counts.count1,
            2 => counts.count2,
            3 => counts.count3,
            4 => counts.count4,
            _ => -1,
        };
    }

    private void ApplyPostBattleStatus(RoyalRumbleBattleEnvelopeDto envelope)
    {
        if (envelope == null)
        {
            UpdateStatusFromSession();
            return;
        }

        if (envelope.runEnded)
        {
            SetStatus(
                envelope.runStatus == "won" ? "You won the Royal Rumble!"
                : envelope.runStatus == "lost" ? "You lost the Royal Rumble!"
                : "Royal Rumble ended."
            );
            return;
        }

        if (envelope.playerNeedsReplacement)
        {
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

    private void RefreshPostBattleView(
        RoyalRumbleBattleEnvelopeDto envelope,
        string previousPlayerSelectedCardId,
        string previousEnemySelectedCardId
    )
    {
        bool shouldRebuildView = ShouldRebuildViewAfterBattle(
            envelope,
            previousPlayerSelectedCardId,
            previousEnemySelectedCardId
        );

        if (shouldRebuildView)
        {
            Debug.LogWarning("[RoyalRumbleBattleCoordinator] Rebuilding RR view after battle.");
            RenderSession();
        }
        else
        {
            Debug.LogWarning(
                "[RoyalRumbleBattleCoordinator] Applying lightweight RR board sync after battle."
            );
            SyncActiveCardsFromSession();
            SetPlayerHandInteractivity(CanSelectFighter());
        }

        ApplyPostBattleStatus(envelope);
        UpdateAttackButtons();
    }

    private bool ShouldRebuildViewAfterBattle(
        RoyalRumbleBattleEnvelopeDto envelope,
        string previousPlayerSelectedCardId,
        string previousEnemySelectedCardId
    )
    {
        if (currentSession == null)
        {
            return true;
        }

        if (
            envelope?.runEnded == true
            || envelope.playerNeedsReplacement
            || envelope.enemyNeedsReplacement
        )
        {
            return true;
        }

        if (
            currentSession.status == "awaiting_player_card"
            || currentSession.status == "awaiting_replacement"
            || currentSession.status == "won"
            || currentSession.status == "lost"
            || currentSession.status == "abandoned"
        )
        {
            return true;
        }

        if (renderedPlayerActiveCard == null || renderedEnemyActiveCard == null)
        {
            return true;
        }

        return false;
    }

    private void SyncActiveCardsFromSession()
    {
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

    private void ApplyCardSnapshotToRenderedCard(
        SelectedCardData snapshot,
        Kard card,
        HealthBar lifeBar
    )
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
                if (
                    child != null
                    && child.name.StartsWith(effectName + "Icon", StringComparison.Ordinal)
                )
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

    private void ResolveDependencies()
    {
        royalRumbleService ??= GetComponent<RoyalRumbleService>();
        if (royalRumbleService == null)
        {
            royalRumbleService = gameObject.AddComponent<RoyalRumbleService>();
        }

        battlePlayback ??= GetComponent<RoyalRumbleBattlePlayback>();
        if (battlePlayback == null)
        {
            battlePlayback = gameObject.AddComponent<RoyalRumbleBattlePlayback>();
        }

        player ??= FindPlayerByEnemyFlag(false);
        enemy ??= FindPlayerByEnemyFlag(true);
        playerLifeBar ??= FindHealthBar("Player");
        enemyLifeBar ??= FindHealthBar("Enemy");
        dialogText ??= FindFirstObjectByType<TMP_Text>();
        attackDescriptions ??= FindFirstObjectByType<AttackDescriptions>();
        playerHandRoot ??= player != null ? player.gameObject : playerHandRoot;
        enemyHandRoot ??= enemy != null ? enemy.gameObject : enemyHandRoot;

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

        battlePlayback.attackComponent ??= FindFirstObjectByType<Attack>();
        battlePlayback.playerLifeBar ??= playerLifeBar;
        battlePlayback.enemyLifeBar ??= enemyLifeBar;
        battlePlayback.dialogText ??= dialogText;
        battlePlayback.cardAnimator ??= FindFirstObjectByType<MultiplayerCardAnimator>();
        ResolveAttackTextReferences();
        confirmButton ??= FindButtonByName("DialogButton2");

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

    private void TryAbandonSessionOnExit()
    {
        if (sessionAbandonRequested || royalRumbleService == null || currentSession == null)
        {
            return;
        }

        if (
            string.IsNullOrWhiteSpace(currentSession.sessionId)
            || string.IsNullOrWhiteSpace(playerId)
        )
        {
            return;
        }

        sessionAbandonRequested = true;
        Debug.LogWarning(
            $"[RoyalRumbleBattleCoordinator] Abandoning RR session on exit: {currentSession.sessionId}"
        );
        royalRumbleService.AbandonSessionFireAndForget(currentSession.sessionId, playerId);
    }

    private bool CanSelectFighter()
    {
        return currentSession != null
            && !isBusy
            && !openingSequenceRunning
            && IsFighterSelectionStatus();
    }

    private bool IsFighterSelectionStatus()
    {
        return currentSession != null
            && (
                currentSession.status == "awaiting_player_card"
                || currentSession.status == "awaiting_replacement"
            );
    }

    private bool ShouldRenderEnemyActiveCard()
    {
        if (
            openingSequenceRunning
            || currentSession == null
            || string.IsNullOrWhiteSpace(ActiveEnemyCardId)
        )
        {
            return false;
        }

        return true;
    }

    private void SetPlayerHandInteractivity(bool isEnabled)
    {
        foreach (Kard kard in renderedPlayerHandCards)
        {
            if (kard == null)
            {
                continue;
            }

            kard.isDragable = isEnabled;
            RoyalRumbleCardDrag dragHandler = kard.GetComponent<RoyalRumbleCardDrag>();
            if (dragHandler != null)
            {
                dragHandler.enabled = isEnabled;
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

        if (currentSession?.enemyDeck?.cards == null)
        {
            return null;
        }

        Debug.LogWarning(
            "[RoyalRumbleBattleCoordinator] Enemy deck loaded but server active enemy card is missing."
        );
        return null;
    }

    private void PromoteEnemyCardToBoard(SelectedCardData selectedEnemy)
    {
        if (selectedEnemy == null || enemy == null)
        {
            return;
        }

        Kard cardInHand = renderedEnemyHandCards.FirstOrDefault(card =>
            card != null && card.cardId == selectedEnemy.cardId
        );
        if (cardInHand != null)
        {
            renderedEnemyHandCards.Remove(cardInHand);
            enemy.PlayCard(cardInHand, enemyBoard);
            renderedEnemyActiveCard = cardInHand;
            enemy.cardInGame = cardInHand;
            enemyLifeBar?.SetBar(enemy.cardInGame);
            return;
        }

        RenderEnemyActiveCard();
    }

    private Kard CreateCardInGame(
        GeneratedCard cardData,
        GameObject parentOverride,
        Player owner,
        bool addToHand,
        GameObject battleAreaOverride
    )
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
        kardComponent.battleArea = battleAreaOverride != null ? battleAreaOverride : playerBoard;

        if (cardData.Color != null && cardData.Color.Length >= 3)
        {
            kardComponent.color = new Color32(
                (byte)Mathf.Clamp(cardData.Color[0], 0, 255),
                (byte)Mathf.Clamp(cardData.Color[1], 0, 255),
                (byte)Mathf.Clamp(cardData.Color[2], 0, 255),
                255
            );
        }

        if (attackDescriptions != null)
        {
            kardComponent.countAttack1 = attackDescriptions.LoadAttackCount(
                kardComponent,
                cardData.Attack1
            );
            kardComponent.countAttack2 = attackDescriptions.LoadAttackCount(
                kardComponent,
                cardData.Attack2
            );
            kardComponent.countAttack3 = attackDescriptions.LoadAttackCount(
                kardComponent,
                cardData.Attack3
            );
            kardComponent.countAttack4 = attackDescriptions.LoadAttackCount(
                kardComponent,
                cardData.Attack4
            );
        }

        DragKard legacyDrag = cardObject.GetComponent<DragKard>();
        if (legacyDrag != null)
        {
            legacyDrag.enabled = false;
        }

        MultiplayerCardDrag multiplayerDrag = cardObject.GetComponent<MultiplayerCardDrag>();
        if (multiplayerDrag != null)
        {
            Destroy(multiplayerDrag);
        }

        RoyalRumbleCardDrag royalRumbleDrag = cardObject.GetComponent<RoyalRumbleCardDrag>();
        if (royalRumbleDrag != null && (!addToHand || owner != player))
        {
            Destroy(royalRumbleDrag);
        }

        if (addToHand)
        {
            owner.AddCardToHand(kardComponent);
        }

        return kardComponent;
    }

    private static Button FindButtonByName(string objectName)
    {
        GameObject buttonObject = GameObject.Find(objectName);
        return buttonObject != null ? buttonObject.GetComponent<Button>() : null;
    }

    private void ResolveAttackTextReferences()
    {
        attackButton1Text ??= FindAttackNameText(attackButton1);
        attackButton2Text ??= FindAttackNameText(attackButton2);
        attackButton3Text ??= FindAttackNameText(attackButton3);
        attackButton4Text ??= FindAttackNameText(attackButton4);

        attackButton1CountText ??= FindAttackCountText(attackButton1);
        attackButton2CountText ??= FindAttackCountText(attackButton2);
        attackButton3CountText ??= FindAttackCountText(attackButton3);
        attackButton4CountText ??= FindAttackCountText(attackButton4);
    }

    private static TMP_Text FindAttackNameText(Button button)
    {
        if (button == null)
        {
            return null;
        }

        foreach (TMP_Text text in button.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text == null)
            {
                continue;
            }

            if (
                !string.Equals(
                    text.gameObject.name,
                    "AttackCountText",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return text;
            }
        }

        return null;
    }

    private static TMP_Text FindAttackCountText(Button button)
    {
        if (button == null)
        {
            return null;
        }

        foreach (TMP_Text text in button.GetComponentsInChildren<TMP_Text>(true))
        {
            if (
                text != null
                && string.Equals(
                    text.gameObject.name,
                    "AttackCountText",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return text;
            }
        }

        return null;
    }

    private string GetAttackName(int attackId)
    {
        if (attackDescriptions != null)
        {
            string attackName = attackDescriptions.GetAttackName(attackId);
            if (!string.IsNullOrWhiteSpace(attackName))
            {
                return attackName;
            }
        }

        return AttackRegistry.GetAttackName(attackId);
    }

    private static void SetAttackNameText(TMP_Text label, string value)
    {
        if (label != null)
        {
            label.text = value ?? string.Empty;
        }
    }

    private static void SetAttackCountText(TMP_Text label, int remainingCount)
    {
        if (label != null)
        {
            label.text = remainingCount >= 0 ? remainingCount.ToString() : string.Empty;
        }
    }

    private static Player FindPlayerByEnemyFlag(bool isEnemy)
    {
        foreach (Player candidate in FindObjectsByType<Player>(FindObjectsSortMode.None))
        {
            if (candidate != null && candidate.isEnemy == isEnemy)
            {
                return candidate;
            }
        }

        return null;
    }

    private static HealthBar FindHealthBar(string nameFragment)
    {
        foreach (HealthBar bar in FindObjectsByType<HealthBar>(FindObjectsSortMode.None))
        {
            if (
                bar != null
                && bar.name.IndexOf(nameFragment, StringComparison.OrdinalIgnoreCase) >= 0
            )
            {
                return bar;
            }
        }

        return null;
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
}
