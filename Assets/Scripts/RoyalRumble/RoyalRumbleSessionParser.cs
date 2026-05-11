using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class RoyalRumbleSessionParser
{
    public static RoyalRumbleSessionEnvelopeDto ParseSessionEnvelope(object functionResult)
    {
        var token = ToToken(functionResult);
        if (token == null || token.Type == JTokenType.Null)
        {
            return null;
        }

        var dto = new RoyalRumbleSessionEnvelopeDto
        {
            success = token.Value<bool?>("success") ?? false,
            message = token.Value<string>("message") ?? string.Empty,
            error = token.Value<string>("error") ?? string.Empty,
        };

        dto.session = ParseSessionToken(token["session"]);
        dto.sessionSummary = ParseSummaryToken(token["sessionSummary"]);
        return dto;
    }

    public static RoyalRumbleBattleEnvelopeDto ParseBattleEnvelope(object functionResult)
    {
        var token = ToToken(functionResult);
        if (token == null || token.Type == JTokenType.Null)
        {
            return null;
        }

        var dto = new RoyalRumbleBattleEnvelopeDto
        {
            success = token.Value<bool?>("success") ?? false,
            message = token.Value<string>("message") ?? string.Empty,
            error = token.Value<string>("error") ?? string.Empty,
            playerNeedsReplacement = token.Value<bool?>("playerNeedsReplacement") ?? false,
            enemyNeedsReplacement = token.Value<bool?>("enemyNeedsReplacement") ?? false,
            runEnded = token.Value<bool?>("runEnded") ?? false,
            runStatus = token.Value<string>("runStatus") ?? string.Empty,
        };

        dto.battleResult = BattleContractMapper.ParseBattleResultObject(token["battleResult"]);
        dto.session = ParseSessionToken(token["session"]);
        dto.sessionSummary = ParseSummaryToken(token["sessionSummary"]);
        dto.botAttack = token["botAttack"]?.ToObject<RoyalRumbleBotAttackDto>() ?? new RoyalRumbleBotAttackDto();
        dto.botAttack.validSlots ??= new List<int>();
        return dto;
    }

    private static RoyalRumbleSessionDto ParseSessionToken(JToken token)
    {
        if (token == null || token.Type == JTokenType.Null)
        {
            return null;
        }

        var dto = new RoyalRumbleSessionDto
        {
            sessionId = token.Value<string>("sessionId") ?? string.Empty,
            schemaVersion = token.Value<int?>("schemaVersion") ?? 0,
            mode = token.Value<string>("mode") ?? string.Empty,
            playerId = token.Value<string>("playerId") ?? string.Empty,
            status = token.Value<string>("status") ?? string.Empty,
            playerSelectedCardId = token.Value<string>("playerSelectedCardId") ?? string.Empty,
            enemySelectedCardId = token.Value<string>("enemySelectedCardId") ?? string.Empty,
            turnNumber = token.Value<int?>("turnNumber") ?? 0,
            battleCount = token.Value<int?>("battleCount") ?? 0,
            createdAt = token.Value<string>("createdAt") ?? string.Empty,
            updatedAt = token.Value<string>("updatedAt") ?? string.Empty,
            abandonedAt = token.Value<string>("abandonedAt") ?? string.Empty,
            playerInfo = token["playerInfo"]?.ToObject<RoyalRumblePlayerInfoDto>() ?? new RoyalRumblePlayerInfoDto(),
            modeConfig = token["modeConfig"]?.ToObject<RoyalRumbleModeConfigDto>() ?? new RoyalRumbleModeConfigDto(),
            resultSummary = token["resultSummary"]?.ToObject<RoyalRumbleResultSummaryDto>() ?? new RoyalRumbleResultSummaryDto(),
        };

        dto.playerInfo.playerId ??= string.Empty;
        dto.playerInfo.username ??= string.Empty;
        dto.modeConfig.botStrategy ??= string.Empty;
        dto.modeConfig.persistenceScope ??= string.Empty;
        dto.resultSummary.lastBattleAt ??= string.Empty;
        dto.attackCounts = ParseAttackCounts(token["attackCounts"]);
        dto.playerDeck = ParseDeckToken(token["playerDeck"], dto.playerId);
        dto.enemyDeck = ParseDeckToken(token["enemyDeck"], "ROYAL_RUMBLE_AI");
        dto.lastBattleResult = BattleContractMapper.ParseBattleResultObject(token["lastBattleResult"]);
        return dto;
    }

    private static RoyalRumbleSessionSummaryDto ParseSummaryToken(JToken token)
    {
        if (token == null || token.Type == JTokenType.Null)
        {
            return null;
        }

        return new RoyalRumbleSessionSummaryDto
        {
            sessionId = token.Value<string>("sessionId") ?? string.Empty,
            schemaVersion = token.Value<int?>("schemaVersion") ?? 0,
            mode = token.Value<string>("mode") ?? string.Empty,
            playerId = token.Value<string>("playerId") ?? string.Empty,
            status = token.Value<string>("status") ?? string.Empty,
            playerSelectedCardId = token.Value<string>("playerSelectedCardId") ?? string.Empty,
            enemySelectedCardId = token.Value<string>("enemySelectedCardId") ?? string.Empty,
            turnNumber = token.Value<int?>("turnNumber") ?? 0,
            battleCount = token.Value<int?>("battleCount") ?? 0,
            playerDeckCount = token.Value<int?>("playerDeckCount") ?? 0,
            enemyDeckCount = token.Value<int?>("enemyDeckCount") ?? 0,
            createdAt = token.Value<string>("createdAt") ?? string.Empty,
            updatedAt = token.Value<string>("updatedAt") ?? string.Empty,
            abandonedAt = token.Value<string>("abandonedAt") ?? string.Empty,
        };
    }

    private static RoyalRumbleDeckDto ParseDeckToken(JToken token, string fallbackOwnerPlayerId)
    {
        if (token == null || token.Type == JTokenType.Null)
        {
            return new RoyalRumbleDeckDto { cards = new List<SelectedCardData>() };
        }

        if (token.Type == JTokenType.Array)
        {
            var arrayToken = (JArray)token;
            return new RoyalRumbleDeckDto
            {
                playerId = fallbackOwnerPlayerId ?? string.Empty,
                deckId = string.Empty,
                deckName = string.Empty,
                deckSize = arrayToken.Count,
                loadedAt = string.Empty,
                generatedAt = string.Empty,
                cards = ParseCards(arrayToken, fallbackOwnerPlayerId),
            };
        }

        var dto = token.ToObject<RoyalRumbleDeckDto>() ?? new RoyalRumbleDeckDto();
        dto.playerId ??= string.Empty;
        dto.deckId ??= string.Empty;
        dto.deckName ??= string.Empty;
        dto.loadedAt ??= string.Empty;
        dto.generatedAt ??= string.Empty;
        dto.cards = ParseCards(token["cards"], fallbackOwnerPlayerId);
        dto.deckSize = Math.Max(dto.deckSize, dto.cards.Count);
        return dto;
    }

    private static RoyalRumbleAttackCountsDto ParseAttackCounts(JToken token)
    {
        var dto = token?.ToObject<RoyalRumbleAttackCountsDto>() ?? new RoyalRumbleAttackCountsDto();
        dto.player ??= new Dictionary<string, RoyalRumbleAttackCountEntryDto>();
        dto.enemy ??= new Dictionary<string, RoyalRumbleAttackCountEntryDto>();
        return dto;
    }

    private static List<SelectedCardData> ParseCards(JToken token, string fallbackOwnerPlayerId)
    {
        var result = new List<SelectedCardData>();
        if (token == null || token.Type != JTokenType.Array)
        {
            return result;
        }

        foreach (var item in token)
        {
            if (item is not JObject rawCard)
            {
                continue;
            }

            SelectedCardData card = ParseCard(rawCard, fallbackOwnerPlayerId);
            if (card != null)
            {
                result.Add(card);
            }
        }

        return result;
    }

    private static SelectedCardData ParseCard(JObject rawCard, string fallbackOwnerPlayerId)
    {
        var payload = new JObject
        {
            ["cardId"] = FirstValue(rawCard, "cardId", "CardID"),
            ["name"] = FirstValue(rawCard, "name", "PersonName"),
            ["image"] = FirstValue(rawCard, "image", "CardPicture"),
            ["level"] = FirstValue(rawCard, "level", "Level") ?? 1,
            ["health"] = FirstValue(rawCard, "health", "Health") ?? 0,
            ["maxHealth"] = FirstValue(rawCard, "maxHealth", "MaxHealth", "Health") ?? 0,
            ["styleId"] = FirstValue(rawCard, "styleId", "StyleID") ?? 0,
            ["strength"] = FirstValue(rawCard, "strength", "Strength") ?? 0,
            ["speed"] = FirstValue(rawCard, "speed", "Speed") ?? 0,
            ["attack"] = FirstValue(rawCard, "attack", "Attack") ?? 0,
            ["defense"] = FirstValue(rawCard, "defense", "Defense") ?? 0,
            ["knowledge"] = FirstValue(rawCard, "knowledge", "Knowledge") ?? 0,
            ["charisma"] = FirstValue(rawCard, "charisma", "Charisma") ?? 0,
            ["experience"] = FirstValue(rawCard, "experience", "Experience") ?? 0,
            ["attack1"] = FirstValue(rawCard, "attack1", "Attack1") ?? 0,
            ["attack2"] = FirstValue(rawCard, "attack2", "Attack2") ?? 0,
            ["attack3"] = FirstValue(rawCard, "attack3", "Attack3") ?? 0,
            ["attack4"] = FirstValue(rawCard, "attack4", "Attack4") ?? 0,
            ["color"] = NormalizeColorToken(FirstValue(rawCard, "color", "Color")),
            ["effects"] = NormalizeEffectsToken(FirstValue(rawCard, "effects", "TurnEffects")),
            ["ongoingActions"] = NormalizeArrayToken(FirstValue(rawCard, "ongoingActions", "OngoingActions"))
        };

        string ownerPlayerId = FirstString(rawCard, "playerId", "OwnerPlayerId") ?? fallbackOwnerPlayerId ?? string.Empty;
        return SelectedCardData.FromJson(ownerPlayerId, payload);
    }

    private static JToken FirstValue(JObject source, params string[] names)
    {
        foreach (string name in names)
        {
            if (source.TryGetValue(name, StringComparison.Ordinal, out var value) && value != null && value.Type != JTokenType.Null)
            {
                return value;
            }
        }

        return null;
    }

    private static string FirstString(JObject source, params string[] names)
    {
        return FirstValue(source, names)?.Value<string>();
    }

    private static JToken NormalizeColorToken(JToken token)
    {
        if (token is JArray)
        {
            return token;
        }

        return new JArray(255, 255, 255);
    }

    private static JArray NormalizeEffectsToken(JToken token)
    {
        if (token is JArray array)
        {
            return array;
        }

        return new JArray();
    }

    private static JArray NormalizeArrayToken(JToken token)
    {
        if (token is JArray array)
        {
            return array;
        }

        return new JArray();
    }

    private static JToken ToToken(object value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is JToken token)
        {
            return token;
        }

        if (value is string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            return JToken.Parse(text);
        }

        return JToken.FromObject(value, JsonSerializer.CreateDefault());
    }
}
