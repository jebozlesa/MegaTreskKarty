using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CampaignOnlineShellController : MonoBehaviour
{
    private static bool VerboseCampaignShellLogs => true;

    [Header("Dependencies")]
    public CampaignOnlineService campaignService;
    public CampaignOnlineBattlePlayback battlePlayback;
    public Player player;
    public Player enemy;
    public HealthBar playerLifeBar;
    public HealthBar enemyLifeBar;
    public TMP_Text dialogText;
    public AttackDescriptions attackDescriptions;
    public GameObject cardPrefab;
    public GameObject playerBoard;
    public GameObject enemyBoard;
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
    public string campaignId = "bushido";
    public int missionId = 1;
    public bool autoStart;

    private CampaignOnlineSessionDto currentSession;
    private string playerId;
    private bool isBusy;
    private bool hasStartedRun;
    private bool isShuttingDown;
    private string lastAutoSubmittedPendingKey;
    private SharedAttackSelectionFlow attackSelectionFlow;
    private readonly List<Kard> renderedPlayerHandCards = new List<Kard>();
    private Kard renderedPlayerActiveCard;
    private Kard renderedEnemyActiveCard;
    private bool buttonListenersBound;
    private string defaultAttackButton1Name;
    private string defaultAttackButton2Name;
    private string defaultAttackButton3Name;
    private string defaultAttackButton4Name;
    private string defaultAttackButton1Count;
    private string defaultAttackButton2Count;
    private string defaultAttackButton3Count;
    private string defaultAttackButton4Count;

    public CampaignOnlineSessionDto CurrentSession => currentSession;
    private string ActivePlayerCardId => currentSession?.active?.playerCardId ?? string.Empty;
    private string ActiveEnemyCardId => currentSession?.active?.enemyCardId ?? string.Empty;

    private void Awake()
    {
        ResolveDependencies();
        ResolveAttackTextReferences();
        BindButtonListeners();
        PrepareAttackSelectionFlow();
        UpdateAttackButtons();
    }

    private async void Start()
    {
        if (autoStart)
        {
            await StartFreshRunAsync();
        }
    }

    private void OnDestroy()
    {
        isShuttingDown = true;
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
            Debug.LogWarning("[CampaignOnlineShellController] Cannot start Campaign without player ID.");
            isBusy = false;
            hasStartedRun = false;
            return;
        }

        SetStatus("Preparing Campaign...");
        LogVerboseWarning($"[CampaignOnlineShellController] Campaign opening started for player={playerId}, campaign={campaignId}, mission={missionId}.");

        CampaignOnlineSessionEnvelopeDto sessionEnvelope = await campaignService.CreateSessionAsync(playerId, campaignId, missionId);
        if (this == null || isShuttingDown)
        {
            return;
        }

        if (sessionEnvelope?.session == null)
        {
            SetStatus("Failed to create Campaign session.");
            Debug.LogWarning("[CampaignOnlineShellController] Failed to create Campaign session.");
            isBusy = false;
            hasStartedRun = false;
            return;
        }

        currentSession = sessionEnvelope.session;
        LogVerboseWarning($"[CampaignOnlineShellController] Campaign session created: {currentSession.sessionId}, status={currentSession.status}.");

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
            hasStartedRun = false;
            return;
        }

        if (currentSession.enemyDeck?.cards == null || currentSession.enemyDeck.cards.Count == 0)
        {
            SetStatus("Failed to load enemy deck.");
            UpdateAttackButtons();
            hasStartedRun = false;
            return;
        }

        RenderCampaignView();
        ApplyCurrentSessionStatus();
    }

    public bool CanDragCard(Kard card)
    {
        if (card == null || isBusy || currentSession == null)
        {
            return false;
        }

        return IsFighterSelectionStatus() && renderedPlayerHandCards.Contains(card);
    }

    public void AttackButton(int attackType)
    {
        PendingOngoingActionTurnData pendingAction = GetPendingOngoingAction();
        if (pendingAction != null)
        {
            LogVerboseWarning($"[CampaignOnlineShellController] Ignored manual attack click because ongoing action is pending: {DescribePendingAction(pendingAction)}");
            return;
        }

        attackSelectionFlow?.OnAttackButtonClicked(attackType);
    }

    public void ConfirmAttackButton()
    {
        PendingOngoingActionTurnData pendingAction = GetPendingOngoingAction();
        if (pendingAction != null)
        {
            LogVerboseWarning($"[CampaignOnlineShellController] Ignored manual confirm because ongoing action is pending: {DescribePendingAction(pendingAction)}");
            return;
        }

        attackSelectionFlow?.OnConfirmAttackClicked();
    }

    public async void OnCardDropped(Kard card, CampaignOnlineCardDrag dragHandler)
    {
        if (card == null)
        {
            Debug.LogWarning("[CampaignOnlineShellController] OnCardDropped called with null card.");
            dragHandler?.ResetToOriginalPosition();
            return;
        }

        if (!CanDragCard(card))
        {
            Debug.LogWarning($"[CampaignOnlineShellController] Ignored dropped card {card.cardName}. isBusy={isBusy}, status={currentSession?.status}");
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

            CampaignOnlineSessionEnvelopeDto envelope = await campaignService.SelectCardAsync(currentSession.sessionId, playerId, card.cardId);
            if (this == null || isShuttingDown)
            {
                return;
            }

            if (envelope?.session == null)
            {
                Debug.LogWarning($"[CampaignOnlineShellController] Failed to select fighter {card.cardId} after drop.");
                Vector3 returnPosition = dragHandler != null ? dragHandler.GetOriginalLocalPosition() : Vector3.zero;
                player.ReturnCardToHand(card, returnPosition);
                card.isDragable = true;
                SetStatus("Failed to select fighter.");
                return;
            }

            currentSession = envelope.session;
            lastAutoSubmittedPendingKey = null;
            LogVerboseWarning($"[CampaignOnlineShellController] Fighter selected: cardId={card.cardId}, name={card.cardName}.");
            RenderCampaignView();
            SetStatus("Choose attack!");
        }
        finally
        {
            isBusy = false;
            UpdateAttackButtons();
        }
    }

    public void ShowDragSelectionHint(Kard card = null)
    {
        SetStatus(IsFighterSelectionStatus() ? "Drag a fighter to the battle area." : "Choose attack!");
    }

    public void ReplaceSceneButtonListenersForOnlineCampaign()
    {
        ResolveAttackTextReferences();
        ReplaceButtonClickEvent(attackButton1);
        ReplaceButtonClickEvent(attackButton2);
        ReplaceButtonClickEvent(attackButton3);
        ReplaceButtonClickEvent(attackButton4);
        ReplaceButtonClickEvent(confirmButton);
        buttonListenersBound = false;
        BindButtonListeners();
    }

    private void ResolveDependencies()
    {
        campaignService ??= GetComponent<CampaignOnlineService>();
        if (campaignService == null)
        {
            campaignService = gameObject.AddComponent<CampaignOnlineService>();
        }

        battlePlayback ??= GetComponent<CampaignOnlineBattlePlayback>();
        if (battlePlayback == null)
        {
            battlePlayback = gameObject.AddComponent<CampaignOnlineBattlePlayback>();
        }

        battlePlayback ??= FindFirstObjectByType<CampaignOnlineBattlePlayback>();
        dialogText ??= FindFirstObjectByType<TMP_Text>();
        attackDescriptions ??= FindFirstObjectByType<AttackDescriptions>();
        player ??= FindPlayers(false).FirstOrDefault();
        enemy ??= FindPlayers(true).FirstOrDefault();
        playerBoard ??= GameObject.Find("PlayerSide");
        enemyBoard ??= GameObject.Find("EnemySide");

        if (campaignService == null)
        {
            Debug.LogWarning("[CampaignOnlineShellController] CampaignOnlineService reference is missing.");
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

        if (battlePlayback != null)
        {
            battlePlayback.playerLifeBar ??= playerLifeBar;
            battlePlayback.enemyLifeBar ??= enemyLifeBar;
            battlePlayback.dialogText ??= dialogText;
            battlePlayback.attackComponent ??= FindFirstObjectByType<Attack>();
            battlePlayback.cardAnimator ??= FindFirstObjectByType<MultiplayerCardAnimator>();
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
            CampaignOnlineSessionEnvelopeDto playerDeckEnvelope = await campaignService.LoadPlayerDeckAsync(currentSession.sessionId, playerId);
            if (playerDeckEnvelope?.session != null)
            {
                currentSession = playerDeckEnvelope.session;
                LogVerboseWarning($"[CampaignOnlineShellController] Player deck loaded: cards={currentSession.playerDeck?.cards?.Count ?? 0}, selected={ActivePlayerCardId}");
            }
            else
            {
                Debug.LogWarning("[CampaignOnlineShellController] Failed to load Campaign player deck.");
            }
        }

        if (currentSession.enemyDeck == null || currentSession.enemyDeck.cards == null || currentSession.enemyDeck.cards.Count == 0)
        {
            CampaignOnlineSessionEnvelopeDto enemyDeckEnvelope = await campaignService.LoadEnemyDeckAsync(currentSession.sessionId, playerId);
            if (enemyDeckEnvelope?.session != null)
            {
                currentSession = enemyDeckEnvelope.session;
                LogVerboseWarning($"[CampaignOnlineShellController] Enemy deck loaded: cards={currentSession.enemyDeck?.cards?.Count ?? 0}, selected={ActiveEnemyCardId}");
            }
            else
            {
                Debug.LogWarning("[CampaignOnlineShellController] Failed to load Campaign enemy deck.");
            }
        }
    }

    private void RenderCampaignView()
    {
        ClearRenderedCards();

        SelectedCardData selectedEnemy = ResolveSelectedEnemyCard();
        PromoteEnemyCardToBoard(selectedEnemy);

        SelectedCardData selectedPlayer = FindCard(currentSession?.playerDeck?.cards, ActivePlayerCardId);
        if (selectedPlayer != null)
        {
            PromotePlayerCardToBoard(selectedPlayer);
            RenderPlayerHand(selectedPlayer.cardId);
        }
        else
        {
            RenderPlayerHand(null);
        }

        UpdateAttackButtons();
    }

    private void RenderPlayerHand(string excludeCardId)
    {
        if (currentSession?.playerDeck?.cards == null || player == null)
        {
            return;
        }

        foreach (SelectedCardData cardData in currentSession.playerDeck.cards.Where(IsAlive))
        {
            if (!string.IsNullOrWhiteSpace(excludeCardId) && string.Equals(cardData.cardId, excludeCardId, StringComparison.Ordinal))
            {
                continue;
            }

            Kard createdCard = CreateCardInGame(
                CampaignOnlineCardMapper.ToGeneratedCard(cardData),
                player.gameObject,
                player,
                addToHand: true,
                battleAreaOverride: playerBoard
            );

            if (createdCard == null)
            {
                continue;
            }

            CampaignOnlineSelectableCard selectable = createdCard.gameObject.GetComponent<CampaignOnlineSelectableCard>()
                ?? createdCard.gameObject.AddComponent<CampaignOnlineSelectableCard>();
            selectable.Initialize(this, createdCard);

            CampaignOnlineCardDrag dragHandler = createdCard.gameObject.GetComponent<CampaignOnlineCardDrag>()
                ?? createdCard.gameObject.AddComponent<CampaignOnlineCardDrag>();
            dragHandler.Initialize(this, createdCard);

            renderedPlayerHandCards.Add(createdCard);
            createdCard.isDragable = CanDragCard(createdCard);
        }
    }

    private void PromotePlayerCardToBoard(SelectedCardData selectedPlayer)
    {
        if (selectedPlayer == null || player == null || playerBoard == null)
        {
            return;
        }

        Kard activeCard = CreateCardInGame(
            CampaignOnlineCardMapper.ToGeneratedCard(selectedPlayer),
            player.gameObject,
            player,
            addToHand: false,
            battleAreaOverride: playerBoard
        );

        if (activeCard == null)
        {
            return;
        }

        player.PlayCard(activeCard, playerBoard);
        player.cardInGame = activeCard;
        renderedPlayerActiveCard = activeCard;
        activeCard.isDragable = false;
        playerLifeBar?.SetBar(activeCard);
        SyncEffectIconsFromSnapshot(activeCard, selectedPlayer.effects);
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
            Debug.LogWarning("[CampaignOnlineShellController] Server active enemy card is missing; Campaign cannot choose one locally.");
        }

        return null;
    }

    private void PromoteEnemyCardToBoard(SelectedCardData selectedEnemy)
    {
        if (selectedEnemy == null || enemy == null || enemyBoard == null)
        {
            return;
        }

        Kard activeCard = CreateCardInGame(
            CampaignOnlineCardMapper.ToGeneratedCard(selectedEnemy),
            enemy.gameObject,
            enemy,
            addToHand: false,
            battleAreaOverride: enemyBoard
        );

        if (activeCard == null)
        {
            return;
        }

        enemy.PlayCard(activeCard, enemyBoard);
        enemy.cardInGame = activeCard;
        renderedEnemyActiveCard = activeCard;
        activeCard.isDragable = false;
        enemyLifeBar?.SetBar(activeCard);
        SyncEffectIconsFromSnapshot(activeCard, selectedEnemy.effects);
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

        if (renderedPlayerActiveCard != null)
        {
            Destroy(renderedPlayerActiveCard.gameObject);
        }

        if (renderedEnemyActiveCard != null)
        {
            Destroy(renderedEnemyActiveCard.gameObject);
        }

        renderedPlayerHandCards.Clear();
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
        SelectedCardData.OngoingActionData action = selected?.ongoingActions != null && selected.ongoingActions.Length > 0
            ? selected.ongoingActions[0]
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
                LogVerboseWarning($"[CampaignOnlineShellController] Ignored duplicate ongoing auto-submit for key={pendingKey}");
                return;
            }

            lastAutoSubmittedPendingKey = pendingKey;
            LogVerboseWarning($"[CampaignOnlineShellController] Auto-submitting ongoing action: {DescribePendingAction(pendingAction)}, key={pendingKey}");
        }
        else
        {
            lastAutoSubmittedPendingKey = null;
        }

        StartCoroutine(SubmitAttackRoutine(attackData.attackType));
    }

    private IEnumerator SubmitAttackRoutine(int attackSlot)
    {
        isBusy = true;
        UpdateAttackButtons();
        SetStatus("Resolving battle...");

        string previousPlayerSelectedCardId = ActivePlayerCardId;
        string previousEnemySelectedCardId = ActiveEnemyCardId;

        Task<CampaignOnlineBattleEnvelopeDto> submitTask = campaignService.SubmitAttackAsync(currentSession.sessionId, playerId, attackSlot);
        yield return new WaitUntil(() => submitTask.IsCompleted);

        if (this == null || isShuttingDown)
        {
            yield break;
        }

        CampaignOnlineBattleEnvelopeDto envelope = submitTask.Status == TaskStatus.RanToCompletion
            ? submitTask.Result
            : null;

        if (envelope?.battleResult == null)
        {
            SetStatus("Battle failed.");
            isBusy = false;
            UpdateAttackButtons();
            yield break;
        }

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

        if (envelope.session == null)
        {
            SetStatus("Battle failed.");
            isBusy = false;
            UpdateAttackButtons();
            yield break;
        }

        currentSession = envelope.session;

        PendingOngoingActionTurnData pendingAfter = GetPendingOngoingAction();
        string nextPendingKey = BuildPendingActionKey(ActivePlayerCardId, pendingAfter);
        if (pendingAfter == null)
        {
            lastAutoSubmittedPendingKey = null;
        }
        else if (!string.Equals(nextPendingKey, lastAutoSubmittedPendingKey, StringComparison.Ordinal))
        {
            LogVerboseWarning($"[CampaignOnlineShellController] Ongoing action state advanced. previousKey={lastAutoSubmittedPendingKey ?? "none"}, nextKey={nextPendingKey}");
        }

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

    private void RefreshPostBattleView(
        CampaignOnlineBattleEnvelopeDto envelope,
        string previousPlayerSelectedCardId,
        string previousEnemySelectedCardId)
    {
        if (envelope?.runEnded == true || envelope?.playerNeedsReplacement == true)
        {
            ClearPendingOngoingState();
        }

        if (ShouldRebuildViewAfterBattle(envelope, previousPlayerSelectedCardId, previousEnemySelectedCardId))
        {
            RenderCampaignView();
        }
        else
        {
            SyncActiveCardsFromSession();
        }

        ApplyPostBattleStatus(envelope);
    }

    private bool ShouldRebuildViewAfterBattle(
        CampaignOnlineBattleEnvelopeDto envelope,
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

        lifeBar?.SetBar(card);
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

    private void ApplyCurrentSessionStatus()
    {
        if (currentSession == null)
        {
            return;
        }

        switch (currentSession.status)
        {
            case "awaiting_player_card":
                SetStatus("Choose fighter.");
                break;
            case "awaiting_replacement":
                SetStatus("Choose your next fighter!");
                break;
            case "awaiting_attack":
                SetStatus(string.IsNullOrWhiteSpace(ActivePlayerCardId) ? "Choose fighter." : "Choose attack!");
                break;
            case "won":
                SetStatus("Campaign mission won!");
                break;
            case "lost":
                SetStatus("Campaign mission lost.");
                break;
            default:
                SetStatus(currentSession.status ?? string.Empty);
                break;
        }
    }

    private void ApplyPostBattleStatus(CampaignOnlineBattleEnvelopeDto envelope)
    {
        if (envelope == null)
        {
            return;
        }

        if (envelope.runEnded)
        {
            ClearPendingOngoingState();
            SetStatus(envelope.runStatus == "won"
                ? "Campaign mission won!"
                : envelope.runStatus == "lost"
                    ? "Campaign mission lost."
                    : "Campaign mission ended.");
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

        int attackId = GetAttackIdForSlot(selected, attackSlot);
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
            || !currentSession.attackCounts.player.TryGetValue(cardId, out CampaignOnlineAttackCountEntryDto counts)
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

        CacheDefaultAttackButtonTexts();
    }

    private void BindButtonListeners()
    {
        if (buttonListenersBound)
        {
            return;
        }

        if (attackButton1 != null) attackButton1.onClick.AddListener(() => AttackButton(1));
        if (attackButton2 != null) attackButton2.onClick.AddListener(() => AttackButton(2));
        if (attackButton3 != null) attackButton3.onClick.AddListener(() => AttackButton(3));
        if (attackButton4 != null) attackButton4.onClick.AddListener(() => AttackButton(4));
        if (confirmButton != null) confirmButton.onClick.AddListener(ConfirmAttackButton);
        buttonListenersBound = true;
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

    private void ClearPendingOngoingState()
    {
        lastAutoSubmittedPendingKey = null;
    }

    private bool IsFighterSelectionStatus()
    {
        return currentSession != null
            && (currentSession.status == "awaiting_player_card"
                || currentSession.status == "awaiting_replacement");
    }

    private string GetAttackName(int attackId)
    {
        return attackDescriptions != null ? attackDescriptions.GetAttackName(attackId) : $"Attack {attackId}";
    }

    private void SetStatus(string message)
    {
        if (dialogText != null)
        {
            dialogText.text = message ?? string.Empty;
        }
    }

    private void LogVerboseWarning(string message)
    {
        if (VerboseCampaignShellLogs)
        {
            Debug.LogWarning(message);
        }
    }

    private static Button FindButtonByName(string objectName)
    {
        GameObject buttonObject = GameObject.Find(objectName);
        return buttonObject != null ? buttonObject.GetComponent<Button>() : null;
    }

    private static void ReplaceButtonClickEvent(Button button)
    {
        if (button != null)
        {
            button.onClick = new Button.ButtonClickedEvent();
        }
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

            if (string.Equals(text.gameObject.name, "AttackCountText", StringComparison.Ordinal))
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
}
