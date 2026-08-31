using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LibraryDeckController : MonoBehaviour
{
    [Header("Service")]
    public LibraryDeckService libraryDeckService;

    [Header("Render Targets")]
    public Album album;
    public DeckManager deckManager;

    [Header("Context Visuals")]
    public Image libraryBackgroundImage;
    public List<LibraryContextBackgroundBinding> contextBackgrounds = new List<LibraryContextBackgroundBinding>();

    [Header("Sorting")]
    public TMP_Text sortCriterionLabel;

    public LibraryDeckStateResponse CurrentState { get; private set; }
    public LibraryContextDto CurrentContext { get; private set; }
    public LibraryDeckDto CurrentDeck { get; private set; }

    private readonly Dictionary<string, GeneratedCard> cardsById = new Dictionary<string, GeneratedCard>();
    private int currentContextIndex;
    private int currentDeckIndex;
    private string playerId;
    private LibrarySortCriterion selectedSortCriterion = LibrarySortCriterion.Level;
    private LibrarySortCriterion lastAppliedSortCriterion = LibrarySortCriterion.Level;
    private bool sortApplied;
    private bool sortAscending;

    private void Awake()
    {
        UpdateSortCriterionLabel();
    }

    public IEnumerator LoadForCurrentPlayer(bool useCardRenderDelay = true)
    {
        string resolvedPlayerId = ResolvePlayerId();
        Task<bool> task = LoadAsync(resolvedPlayerId, useCardRenderDelay);
        yield return new WaitUntil(() => task.IsCompleted);
    }

    public void RefreshCurrentPlayer(bool useCardRenderDelay = false)
    {
        StartCoroutine(LoadForCurrentPlayer(useCardRenderDelay));
    }

    public async Task<bool> LoadAsync(string resolvedPlayerId, bool useCardRenderDelay = true)
    {
        playerId = resolvedPlayerId;
        if (string.IsNullOrWhiteSpace(playerId))
        {
            Debug.LogError("[LibraryDeckController] Cannot load library deck state: playerId is empty");
            return false;
        }

        if (!ValidateReferences())
        {
            return false;
        }

        Debug.LogWarning($"[LibraryDeckController] Loading state: playerId={playerId}");
        LibraryDeckStateResponse state = await libraryDeckService.GetLibraryDeckStateAsync(playerId);
        if (state == null || !state.success)
        {
            Debug.LogError(
                $"[LibraryDeckController] Failed to load state: stage={state?.stage}, error={state?.error}"
            );
            return false;
        }

        ApplyState(state);
        RenderCurrentContext(useCardRenderDelay);
        return true;
    }

    public async Task<bool> CreateDeckForCurrentContextAsync()
    {
        if (!CanMutateCurrentContext())
        {
            return false;
        }

        LibraryDeckMutationResponse response = await libraryDeckService.CreateDeckAsync(
            playerId,
            CurrentContext.contextId
        );
        return ApplyMutationResponse(response, renderFullContext: false);
    }

    public async void CreateDeckForCurrentContext()
    {
        await CreateDeckForCurrentContextAsync();
    }

    public async Task<bool> SetActiveDeckAsync(string deckId)
    {
        if (!CanMutateCurrentContext() || string.IsNullOrWhiteSpace(deckId))
        {
            return false;
        }

        LibraryDeckMutationResponse response = await libraryDeckService.SetActiveDeckAsync(
            playerId,
            CurrentContext.contextId,
            deckId
        );
        return ApplyMutationResponse(response, renderFullContext: false);
    }

    public async void SetActiveDeck(string deckId)
    {
        await SetActiveDeckAsync(deckId);
    }

    public async Task<bool> SwapActiveDeckCardAsync(string oldCardId, string newCardId)
    {
        if (!CanMutateCurrentContext() || CurrentDeck == null)
        {
            return false;
        }

        LibraryDeckMutationResponse response = await libraryDeckService.SwapDeckCardAsync(
            playerId,
            CurrentContext.contextId,
            CurrentDeck.deckId,
            oldCardId,
            newCardId
        );
        return ApplyMutationResponse(response, renderFullContext: false);
    }

    public void SelectNextContext()
    {
        MoveContext(1);
    }

    public void SelectPreviousContext()
    {
        MoveContext(-1);
    }

    public async void SelectNextDeck()
    {
        await MoveDeckAsync(1);
    }

    public async void SelectPreviousDeck()
    {
        await MoveDeckAsync(-1);
    }

    public void CycleSortCriterion()
    {
        LibrarySortCriterion previous = selectedSortCriterion;
        selectedSortCriterion = LibraryCardSorter.NextCriterion(selectedSortCriterion);
        UpdateSortCriterionLabel();

        Debug.LogWarning(
            $"[LibraryDeckController] Sort criterion changed: {LibraryCardSorter.LabelFor(previous)}->{LibraryCardSorter.LabelFor(selectedSortCriterion)}, appliedSort={FormatSortState()}"
        );
    }

    public void ApplySort()
    {
        if (CurrentState == null)
        {
            Debug.LogWarning("[LibraryDeckController] Sort ignored: library state is not loaded");
            return;
        }

        if (sortApplied && lastAppliedSortCriterion == selectedSortCriterion)
        {
            sortAscending = !sortAscending;
        }
        else
        {
            sortAscending = LibraryCardSorter.DefaultAscending(selectedSortCriterion);
            lastAppliedSortCriterion = selectedSortCriterion;
            sortApplied = true;
        }

        Debug.LogWarning(
            $"[LibraryDeckController] Sort applied: {FormatSortState()}, context={FormatContext(CurrentContext)}"
        );
        RenderCurrentContext(showTransitionFrame: true);
    }

    public bool IsCardUsedInAnyKnownDeck(string cardId)
    {
        return TryFindCardDeckUsage(cardId, out _);
    }

    public bool TryFindCardDeckUsage(string cardId, out LibraryDeckUsageInfo usage)
    {
        if (CurrentState?.visibleContexts == null || string.IsNullOrWhiteSpace(cardId))
        {
            usage = null;
            return false;
        }

        foreach (LibraryContextDto context in CurrentState.visibleContexts)
        {
            if (context?.decks == null) continue;
            for (int deckIndex = 0; deckIndex < context.decks.Count; deckIndex++)
            {
                LibraryDeckDto deck = context.decks[deckIndex];
                if (deck?.cardIds != null && deck.cardIds.Contains(cardId))
                {
                    usage = new LibraryDeckUsageInfo
                    {
                        contextId = context.contextId,
                        deckId = deck.deckId,
                        deckIndex = deckIndex
                    };
                    return true;
                }
            }
        }

        usage = null;
        return false;
    }

    private bool ApplyMutationResponse(LibraryDeckMutationResponse response, bool renderFullContext)
    {
        if (response == null || !response.success)
        {
            Debug.LogError(
                $"[LibraryDeckController] Deck mutation failed: stage={response?.stage}, error={response?.error}"
            );
            return false;
        }

        ReplaceContext(response.context);
        if (renderFullContext)
        {
            RenderCurrentContext();
        }
        else
        {
            RenderCurrentDeckOnly();
        }

        return true;
    }

    private void ApplyState(LibraryDeckStateResponse state)
    {
        CurrentState = state;
        cardsById.Clear();

        if (CurrentState.cards != null)
        {
            foreach (GeneratedCard card in CurrentState.cards)
            {
                if (!string.IsNullOrWhiteSpace(card.CardID))
                {
                    cardsById[card.CardID] = card;
                }
            }
        }

        currentContextIndex = Mathf.Clamp(currentContextIndex, 0, VisibleContextCount() - 1);
        CurrentContext = GetVisibleContext(currentContextIndex);
        SetCurrentDeckFromActiveDeck();
    }

    private void ReplaceContext(LibraryContextDto updatedContext)
    {
        if (updatedContext == null || CurrentState?.visibleContexts == null)
        {
            return;
        }

        for (int i = 0; i < CurrentState.visibleContexts.Count; i++)
        {
            if (CurrentState.visibleContexts[i].contextId == updatedContext.contextId)
            {
                CurrentState.visibleContexts[i] = updatedContext;
                currentContextIndex = i;
                CurrentContext = updatedContext;
                SetCurrentDeckFromActiveDeck();
                return;
            }
        }
    }

    private void MoveContext(int direction)
    {
        int count = VisibleContextCount();
        if (count <= 1)
        {
            Debug.LogWarning($"[LibraryDeckController] Context swipe ignored: visibleContexts={count}");
            return;
        }

        int previousIndex = currentContextIndex;
        LibraryContextDto previousContext = CurrentContext;
        LibraryDeckDto previousDeck = CurrentDeck;
        currentContextIndex = (currentContextIndex + direction + count) % count;
        CurrentContext = GetVisibleContext(currentContextIndex);
        SetCurrentDeckFromActiveDeck();
        Debug.LogWarning(
            $"[LibraryDeckController] Context changed: direction={FormatDirection(direction)}, index={previousIndex}->{currentContextIndex}, context={FormatContext(previousContext)}->{FormatContext(CurrentContext)}, deck={FormatDeck(previousDeck)}->{FormatDeck(CurrentDeck)}"
        );
        RenderCurrentContext(showTransitionFrame: true);
    }

    private async Task<bool> MoveDeckAsync(int direction)
    {
        if (CurrentContext == null)
        {
            Debug.LogWarning("[LibraryDeckController] Deck swipe ignored: no current context");
            return false;
        }

        int slotCount = GetDeckSlotCount(CurrentContext);
        if (slotCount <= 1)
        {
            Debug.LogWarning(
                $"[LibraryDeckController] Deck swipe ignored: context={FormatContext(CurrentContext)}, slots={slotCount}, decks={CurrentContext.decks?.Count ?? 0}"
            );
            return false;
        }

        int previousIndex = currentDeckIndex;
        LibraryDeckDto previousDeck = CurrentDeck;
        currentDeckIndex = (currentDeckIndex + direction + slotCount) % slotCount;
        CurrentDeck = GetDeckByIndex(CurrentContext, currentDeckIndex);
        Debug.LogWarning(
            $"[LibraryDeckController] Deck changed: direction={FormatDirection(direction)}, context={FormatContext(CurrentContext)}, slot={previousIndex}->{currentDeckIndex}/{slotCount}, deck={FormatDeck(previousDeck)}->{FormatDeck(CurrentDeck)}"
        );

        if (CurrentDeck == null)
        {
            RenderCurrentDeckOnly();
            return true;
        }

        return await SetActiveDeckAsync(CurrentDeck.deckId);
    }

    private void RenderCurrentContext(bool useCardRenderDelay = false, bool showTransitionFrame = false)
    {
        if (CurrentContext == null)
        {
            Debug.LogWarning("[LibraryDeckController] No visible library context to render");
            album.RenderLibraryCards(
                new List<GeneratedCard>(),
                new HashSet<string>(),
                useCardRenderDelay,
                showTransitionFrame
            );
            deckManager.RenderDeck(null, new List<GeneratedCard>(), CurrentContext);
            return;
        }

        HashSet<string> eligibleIds = new HashSet<string>(CurrentContext.eligibleCardIds ?? new List<string>());
        List<GeneratedCard> visibleCards = (CurrentState.cards ?? new List<GeneratedCard>())
            .Where(card => eligibleIds.Contains(card.CardID))
            .ToList();
        visibleCards = ApplyCurrentSort(visibleCards);

        Debug.LogWarning(
            $"[LibraryDeckController] Render context: index={currentContextIndex}/{VisibleContextCount()}, context={FormatContext(CurrentContext)}, cards={visibleCards.Count}, deckSlot={currentDeckIndex}/{GetDeckSlotCount(CurrentContext)}, activeDeck={FormatDeck(CurrentDeck)}, createDeckSlot={CurrentDeck == null}, sort={FormatSortState()}"
        );

        ApplyCurrentContextVisuals();
        album.RenderLibraryCards(visibleCards, eligibleIds, useCardRenderDelay, showTransitionFrame);
        RenderCurrentDeckOnly();
    }

    private void ApplyCurrentContextVisuals()
    {
        if (CurrentContext == null)
        {
            return;
        }

        if (libraryBackgroundImage == null)
        {
            Debug.LogWarning("[LibraryDeckController] Library background image is not assigned");
            return;
        }

        LibraryContextBackgroundBinding binding = contextBackgrounds.FirstOrDefault(entry =>
            entry != null
            && !string.IsNullOrWhiteSpace(entry.contextId)
            && string.Equals(entry.contextId, CurrentContext.contextId, System.StringComparison.OrdinalIgnoreCase)
        );

        if (binding == null || binding.backgroundSprite == null)
        {
            Debug.LogWarning(
                $"[LibraryDeckController] No background binding for context={CurrentContext.contextId}"
            );
            return;
        }

        libraryBackgroundImage.sprite = binding.backgroundSprite;
        Debug.LogWarning(
            $"[LibraryDeckController] Background applied: context={CurrentContext.contextId}, sprite={binding.backgroundSprite.name}"
        );
    }

    private void RenderCurrentDeckOnly()
    {
        Debug.LogWarning(
            $"[LibraryDeckController] Render deck only: index={currentContextIndex}/{VisibleContextCount()}, context={FormatContext(CurrentContext)}, deckSlot={currentDeckIndex}/{GetDeckSlotCount(CurrentContext)}, activeDeck={FormatDeck(CurrentDeck)}, createDeckSlot={CurrentDeck == null}"
        );

        deckManager.RenderDeck(CurrentDeck, CurrentState?.cards ?? new List<GeneratedCard>(), CurrentContext);
    }

    private List<GeneratedCard> ApplyCurrentSort(List<GeneratedCard> visibleCards)
    {
        if (!sortApplied)
        {
            return visibleCards;
        }

        return LibraryCardSorter.SortCards(
            visibleCards,
            lastAppliedSortCriterion,
            sortAscending
        );
    }

    private void UpdateSortCriterionLabel()
    {
        if (sortCriterionLabel == null)
        {
            return;
        }

        sortCriterionLabel.text = LibraryCardSorter.LabelFor(selectedSortCriterion);
    }

    private LibraryContextDto GetVisibleContext(int index)
    {
        if (CurrentState?.visibleContexts == null || CurrentState.visibleContexts.Count == 0)
        {
            return null;
        }

        return CurrentState.visibleContexts[Mathf.Clamp(index, 0, CurrentState.visibleContexts.Count - 1)];
    }

    private LibraryDeckDto GetActiveDeck(LibraryContextDto context)
    {
        if (context?.decks == null || context.decks.Count == 0)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(context.activeDeckId))
        {
            LibraryDeckDto activeDeck = context.decks.FirstOrDefault(deck => deck.deckId == context.activeDeckId);
            if (activeDeck != null)
            {
                return activeDeck;
            }
        }

        return context.decks[0];
    }

    private void SetCurrentDeckFromActiveDeck()
    {
        CurrentDeck = GetActiveDeck(CurrentContext);
        currentDeckIndex = GetDeckIndex(CurrentContext, CurrentDeck);
    }

    private static int GetDeckSlotCount(LibraryContextDto context)
    {
        int deckCount = context?.decks?.Count ?? 0;
        int maxDecks = context?.maxDecks > 0 ? context.maxDecks : 10;
        return deckCount < maxDecks ? deckCount + 1 : deckCount;
    }

    private static LibraryDeckDto GetDeckByIndex(LibraryContextDto context, int index)
    {
        if (context?.decks == null || index < 0 || index >= context.decks.Count)
        {
            return null;
        }

        return context.decks[index];
    }

    private static int GetDeckIndex(LibraryContextDto context, LibraryDeckDto deck)
    {
        if (context?.decks == null || deck == null)
        {
            return 0;
        }

        int index = context.decks.FindIndex(entry => entry.deckId == deck.deckId);
        return Mathf.Max(index, 0);
    }

    private int VisibleContextCount()
    {
        return CurrentState?.visibleContexts?.Count ?? 0;
    }

    private static string FormatDirection(int direction)
    {
        return direction > 0 ? "next" : "previous";
    }

    private static string FormatContext(LibraryContextDto context)
    {
        if (context == null)
        {
            return "none";
        }

        return $"{context.contextId}(decks={context.decks?.Count ?? 0}, eligible={context.eligibleCardIds?.Count ?? 0})";
    }

    private static string FormatDeck(LibraryDeckDto deck)
    {
        if (deck == null)
        {
            return "create_slot";
        }

        return $"{deck.deckId}(cards={deck.cardIds?.Count ?? 0})";
    }

    private string FormatSortState()
    {
        if (!sortApplied)
        {
            return $"not_applied/selected={LibraryCardSorter.LabelFor(selectedSortCriterion)}";
        }

        return $"{LibraryCardSorter.LabelFor(lastAppliedSortCriterion)}/{(sortAscending ? "asc" : "desc")}/selected={LibraryCardSorter.LabelFor(selectedSortCriterion)}";
    }

    private bool CanMutateCurrentContext()
    {
        if (!ValidateReferences())
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(playerId) || CurrentContext == null)
        {
            Debug.LogError("[LibraryDeckController] Cannot mutate deck before library state is loaded");
            return false;
        }

        return true;
    }

    private bool ValidateReferences()
    {
        bool valid = true;
        if (libraryDeckService == null)
        {
            Debug.LogError("[LibraryDeckController] libraryDeckService is not assigned");
            valid = false;
        }

        if (album == null)
        {
            Debug.LogError("[LibraryDeckController] album is not assigned");
            valid = false;
        }

        if (deckManager == null)
        {
            Debug.LogError("[LibraryDeckController] deckManager is not assigned");
            valid = false;
        }

        return valid;
    }

    private static string ResolvePlayerId()
    {
        return PlayFabManagerLogin.Instance != null
            ? PlayFabManagerLogin.Instance.LoggedInPlayerId
            : PlayerPrefs.GetString("LoggedInPlayerId", string.Empty);
    }
}

[System.Serializable]
public class LibraryContextBackgroundBinding
{
    public string contextId;
    public Sprite backgroundSprite;
}

[System.Serializable]
public class LibraryDeckUsageInfo
{
    public string contextId;
    public string deckId;
    public int deckIndex;
}

public enum LibrarySortCriterion
{
    Level,
    Name,
    Experience,
    Health,
    Strength,
    Speed,
    Attack,
    Defense,
    Knowledge,
    Charisma
}

public static class LibraryCardSorter
{
    private static readonly LibrarySortCriterion[] CriterionCycle =
    {
        LibrarySortCriterion.Level,
        LibrarySortCriterion.Name,
        LibrarySortCriterion.Experience,
        LibrarySortCriterion.Health,
        LibrarySortCriterion.Strength,
        LibrarySortCriterion.Speed,
        LibrarySortCriterion.Attack,
        LibrarySortCriterion.Defense,
        LibrarySortCriterion.Knowledge,
        LibrarySortCriterion.Charisma
    };

    public static LibrarySortCriterion NextCriterion(LibrarySortCriterion current)
    {
        int index = System.Array.IndexOf(CriterionCycle, current);
        if (index < 0)
        {
            return CriterionCycle[0];
        }

        return CriterionCycle[(index + 1) % CriterionCycle.Length];
    }

    public static string LabelFor(LibrarySortCriterion criterion)
    {
        switch (criterion)
        {
            case LibrarySortCriterion.Level:
                return "LVL";
            case LibrarySortCriterion.Name:
                return "ABC";
            case LibrarySortCriterion.Experience:
                return "XP";
            case LibrarySortCriterion.Health:
                return "HP";
            case LibrarySortCriterion.Strength:
                return "STR";
            case LibrarySortCriterion.Speed:
                return "SPD";
            case LibrarySortCriterion.Attack:
                return "ATT";
            case LibrarySortCriterion.Defense:
                return "DEF";
            case LibrarySortCriterion.Knowledge:
                return "KNO";
            case LibrarySortCriterion.Charisma:
                return "CHA";
            default:
                return "LVL";
        }
    }

    public static bool DefaultAscending(LibrarySortCriterion criterion)
    {
        return criterion == LibrarySortCriterion.Name;
    }

    public static List<GeneratedCard> SortCards(
        IEnumerable<GeneratedCard> cards,
        LibrarySortCriterion criterion,
        bool ascending
    )
    {
        IEnumerable<GeneratedCard> source = (cards ?? Enumerable.Empty<GeneratedCard>())
            .Where(card => card != null);

        IOrderedEnumerable<GeneratedCard> orderedCards;
        if (criterion == LibrarySortCriterion.Name)
        {
            orderedCards = ascending
                ? source.OrderBy(NameKey, System.StringComparer.OrdinalIgnoreCase)
                : source.OrderByDescending(NameKey, System.StringComparer.OrdinalIgnoreCase);
        }
        else
        {
            orderedCards = ascending
                ? source.OrderBy(card => NumericKey(card, criterion))
                : source.OrderByDescending(card => NumericKey(card, criterion));
        }

        return orderedCards
            .ThenBy(NameKey, System.StringComparer.OrdinalIgnoreCase)
            .ThenBy(card => card.CardID ?? string.Empty, System.StringComparer.Ordinal)
            .ToList();
    }

    private static string NameKey(GeneratedCard card)
    {
        return card?.PersonName ?? string.Empty;
    }

    private static int NumericKey(GeneratedCard card, LibrarySortCriterion criterion)
    {
        if (card == null)
        {
            return 0;
        }

        switch (criterion)
        {
            case LibrarySortCriterion.Level:
                return card.Level;
            case LibrarySortCriterion.Experience:
                return card.Experience;
            case LibrarySortCriterion.Health:
                return card.MaxHealth > 0 ? card.MaxHealth : card.Health;
            case LibrarySortCriterion.Strength:
                return card.Strength;
            case LibrarySortCriterion.Speed:
                return card.Speed;
            case LibrarySortCriterion.Attack:
                return card.Attack;
            case LibrarySortCriterion.Defense:
                return card.Defense;
            case LibrarySortCriterion.Knowledge:
                return card.Knowledge;
            case LibrarySortCriterion.Charisma:
                return card.Charisma;
            default:
                return card.Level;
        }
    }
}
