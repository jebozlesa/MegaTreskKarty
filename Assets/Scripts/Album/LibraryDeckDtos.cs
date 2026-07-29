using System;
using System.Collections.Generic;

[Serializable]
public class LibraryDeckStateResponse
{
    public bool success;
    public string playerId;
    public List<GeneratedCard> cards;
    public List<LibraryContextDto> visibleContexts;
    public LibraryDecksDocumentDto document;
    public string error;
    public string stage;
}

[Serializable]
public class LibraryDeckMutationResponse
{
    public bool success;
    public string playerId;
    public LibraryContextDto context;
    public LibraryDeckDto deck;
    public string removedCardId;
    public string addedCardId;
    public string error;
    public string stage;
}

[Serializable]
public class LibraryDecksDocumentDto
{
    public int version;
    public List<LibraryContextDecksDto> contexts;
}

[Serializable]
public class LibraryContextDecksDto
{
    public string contextId;
    public string activeDeckId;
    public List<LibraryDeckDto> decks;
}

[Serializable]
public class LibraryContextDto
{
    public string contextId;
    public string displayName;
    public string mode;
    public string campaignId;
    public int order;
    public int maxDecks;
    public string activeDeckId;
    public List<LibraryDeckDto> decks;
    public List<string> eligibleCardIds;
}

[Serializable]
public class LibraryDeckDto
{
    public string deckId;
    public string deckName;
    public List<string> cardIds;
    public string createdAt;
    public string updatedAt;
}
