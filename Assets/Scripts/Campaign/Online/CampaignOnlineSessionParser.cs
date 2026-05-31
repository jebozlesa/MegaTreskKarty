using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class CampaignOnlineSessionParser
{
    public static CampaignOnlineSessionEnvelopeDto ParseSessionEnvelope(object functionResult)
    {
        JToken token = ToToken(functionResult);
        if (token == null || token.Type == JTokenType.Null)
        {
            return null;
        }

        var dto = new CampaignOnlineSessionEnvelopeDto
        {
            success = token.Value<bool?>("success") ?? false,
            message = token.Value<string>("message") ?? string.Empty,
            error = token.Value<string>("error") ?? string.Empty,
            session = ParseSessionToken(token["session"]),
            sessionSummary = ParseSummaryToken(token["sessionSummary"])
        };

        return dto;
    }

    public static CampaignOnlineBattleEnvelopeDto ParseBattleEnvelope(object functionResult)
    {
        JToken token = ToToken(functionResult);
        if (token == null || token.Type == JTokenType.Null)
        {
            return null;
        }

        var dto = new CampaignOnlineBattleEnvelopeDto
        {
            success = token.Value<bool?>("success") ?? false,
            message = token.Value<string>("message") ?? string.Empty,
            error = token.Value<string>("error") ?? string.Empty,
            battleResult = BattleContractMapper.ParseBattleResultObject(token["battleResult"]),
            session = ParseSessionToken(token["session"]),
            sessionSummary = ParseSummaryToken(token["sessionSummary"]),
            playerNeedsReplacement = token.Value<bool?>("playerNeedsReplacement") ?? false,
            enemyNeedsReplacement = token.Value<bool?>("enemyNeedsReplacement") ?? false,
            runEnded = token.Value<bool?>("runEnded") ?? false,
            runStatus = token.Value<string>("runStatus") ?? string.Empty,
            botAttack = token["botAttack"]?.ToObject<CampaignOnlineBotAttackDto>() ?? new CampaignOnlineBotAttackDto()
        };

        dto.botAttack.validSlots ??= new List<int>();
        return dto;
    }

    private static CampaignOnlineSessionDto ParseSessionToken(JToken token)
    {
        if (token == null || token.Type == JTokenType.Null)
        {
            return null;
        }

        var dto = new CampaignOnlineSessionDto
        {
            sessionId = token.Value<string>("sessionId") ?? string.Empty,
            schemaVersion = token.Value<int?>("schemaVersion") ?? 0,
            mode = token.Value<string>("mode") ?? string.Empty,
            playerId = token.Value<string>("playerId") ?? string.Empty,
            status = token.Value<string>("status") ?? string.Empty,
            active = ParseActiveToken(token["active"]),
            progress = ParseProgressToken(token["progress"]),
            modeConfig = ParseModeConfigToken(token["modeConfig"]),
            resultSummary = token["resultSummary"]?.ToObject<CampaignOnlineResultSummaryDto>() ?? new CampaignOnlineResultSummaryDto(),
            lastBattleResult = BattleContractMapper.ParseBattleResultObject(token["lastBattleResult"]),
            createdAt = token.Value<string>("createdAt") ?? string.Empty,
            updatedAt = token.Value<string>("updatedAt") ?? string.Empty,
            abandonedAt = token.Value<string>("abandonedAt") ?? string.Empty
        };

        dto.resultSummary.lastBattleAt ??= string.Empty;
        dto.attackCounts = ParseAttackCounts(token["attackCounts"]);
        dto.playerDeck = ParseDeckToken(token["playerDeck"], dto.playerId);
        dto.enemyDeck = ParseDeckToken(token["enemyDeck"], "CAMPAIGN_AI");
        return dto;
    }

    private static CampaignOnlineSessionSummaryDto ParseSummaryToken(JToken token)
    {
        if (token == null || token.Type == JTokenType.Null)
        {
            return null;
        }

        var dto = new CampaignOnlineSessionSummaryDto
        {
            sessionId = token.Value<string>("sessionId") ?? string.Empty,
            schemaVersion = token.Value<int?>("schemaVersion") ?? 0,
            mode = token.Value<string>("mode") ?? string.Empty,
            playerId = token.Value<string>("playerId") ?? string.Empty,
            status = token.Value<string>("status") ?? string.Empty,
            playerDeck = ParseDeckSummaryToken(token["playerDeck"]),
            enemyDeck = ParseDeckSummaryToken(token["enemyDeck"]),
            attackCounts = ParseAttackCounts(token["attackCounts"]),
            active = ParseActiveToken(token["active"]),
            progress = ParseProgressToken(token["progress"]),
            modeConfig = ParseModeConfigToken(token["modeConfig"]),
            lastBattleResult = BattleContractMapper.ParseBattleResultObject(token["lastBattleResult"]),
            resultSummary = token["resultSummary"]?.ToObject<CampaignOnlineResultSummaryDto>() ?? new CampaignOnlineResultSummaryDto(),
            createdAt = token.Value<string>("createdAt") ?? string.Empty,
            updatedAt = token.Value<string>("updatedAt") ?? string.Empty,
            abandonedAt = token.Value<string>("abandonedAt") ?? string.Empty
        };

        dto.resultSummary.lastBattleAt ??= string.Empty;
        return dto;
    }

    private static CampaignOnlineActiveDto ParseActiveToken(JToken token)
    {
        return new CampaignOnlineActiveDto
        {
            playerCardId = token?.Value<string>("playerCardId") ?? string.Empty,
            enemyCardId = token?.Value<string>("enemyCardId") ?? string.Empty
        };
    }

    private static CampaignOnlineProgressDto ParseProgressToken(JToken token)
    {
        return new CampaignOnlineProgressDto
        {
            turnNumber = token?.Value<int?>("turnNumber") ?? 0,
            battleCount = token?.Value<int?>("battleCount") ?? 0,
            defeatedEnemyCount = token?.Value<int?>("defeatedEnemyCount") ?? 0,
            playerDeaths = token?.Value<int?>("playerDeaths") ?? 0,
            campaignId = token?.Value<string>("campaignId") ?? string.Empty,
            levelId = token?.Value<int?>("levelId")
        };
    }

    private static CampaignOnlineModeConfigDto ParseModeConfigToken(JToken token)
    {
        var dto = new CampaignOnlineModeConfigDto
        {
            playerDeckSize = token?.Value<int?>("playerDeckSize") ?? 0,
            enemyDeckSize = token?.Value<int?>("enemyDeckSize") ?? 0,
            campaignId = token?.Value<string>("campaignId") ?? string.Empty,
            missionId = token?.Value<int?>("missionId") ?? 0,
            rewardFlowEnabled = token?.Value<bool?>("rewardFlowEnabled") ?? false,
            persistenceScope = token?.Value<string>("persistenceScope") ?? string.Empty
        };

        JToken botStrategyToken = token?["botStrategy"];
        dto.botStrategy = botStrategyToken?.Type == JTokenType.Object
            ? botStrategyToken.Value<string>("type") ?? string.Empty
            : botStrategyToken?.Value<string>() ?? string.Empty;

        return dto;
    }

    private static CampaignOnlineDeckSummaryDto ParseDeckSummaryToken(JToken token)
    {
        return new CampaignOnlineDeckSummaryDto
        {
            deckId = token?.Value<string>("deckId") ?? string.Empty,
            deckName = token?.Value<string>("deckName") ?? string.Empty,
            deckSize = token?.Value<int?>("deckSize") ?? 0,
            loaded = token?.Value<bool?>("loaded") ?? false
        };
    }

    private static CampaignOnlineDeckDto ParseDeckToken(JToken token, string fallbackOwnerPlayerId)
    {
        if (token == null || token.Type == JTokenType.Null)
        {
            return new CampaignOnlineDeckDto { cards = new List<SelectedCardData>() };
        }

        if (token.Type == JTokenType.Array)
        {
            var arrayToken = (JArray)token;
            return new CampaignOnlineDeckDto
            {
                playerId = fallbackOwnerPlayerId ?? string.Empty,
                deckId = string.Empty,
                deckName = string.Empty,
                deckSize = arrayToken.Count,
                loadedAt = string.Empty,
                generatedAt = string.Empty,
                cards = ParseCards(arrayToken, fallbackOwnerPlayerId)
            };
        }

        var dto = token.ToObject<CampaignOnlineDeckDto>() ?? new CampaignOnlineDeckDto();
        dto.playerId ??= string.Empty;
        dto.deckId ??= string.Empty;
        dto.deckName ??= string.Empty;
        dto.loadedAt ??= string.Empty;
        dto.generatedAt ??= string.Empty;
        dto.cards = ParseCards(token["cards"], fallbackOwnerPlayerId);
        dto.deckSize = Math.Max(dto.deckSize, dto.cards.Count);
        return dto;
    }

    private static CampaignOnlineAttackCountsDto ParseAttackCounts(JToken token)
    {
        var dto = token?.ToObject<CampaignOnlineAttackCountsDto>() ?? new CampaignOnlineAttackCountsDto();
        dto.player ??= new Dictionary<string, CampaignOnlineAttackCountEntryDto>();
        dto.enemy ??= new Dictionary<string, CampaignOnlineAttackCountEntryDto>();
        return dto;
    }

    private static List<SelectedCardData> ParseCards(JToken token, string fallbackOwnerPlayerId)
    {
        var result = new List<SelectedCardData>();
        if (token == null || token.Type != JTokenType.Array)
        {
            return result;
        }

        foreach (JToken item in token)
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
            ["effects"] = NormalizeArrayToken(FirstValue(rawCard, "effects", "TurnEffects")),
            ["ongoingActions"] = NormalizeArrayToken(FirstValue(rawCard, "ongoingActions", "OngoingActions"))
        };

        string ownerPlayerId = FirstString(rawCard, "playerId", "OwnerPlayerId") ?? fallbackOwnerPlayerId ?? string.Empty;
        return SelectedCardData.FromJson(ownerPlayerId, payload);
    }

    private static JToken FirstValue(JObject source, params string[] names)
    {
        foreach (string name in names)
        {
            if (source.TryGetValue(name, StringComparison.Ordinal, out JToken value)
                && value != null
                && value.Type != JTokenType.Null)
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
        return token is JArray ? token : new JArray(255, 255, 255);
    }

    private static JArray NormalizeArrayToken(JToken token)
    {
        return token is JArray array ? array : new JArray();
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
            return string.IsNullOrWhiteSpace(text) ? null : JToken.Parse(text);
        }

        return JToken.FromObject(value, JsonSerializer.CreateDefault());
    }
}
