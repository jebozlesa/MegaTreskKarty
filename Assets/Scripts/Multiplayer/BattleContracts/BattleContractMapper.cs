using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class BattleContractMapper
{
    public static ExecuteBattleEnvelopeDto ParseExecuteBattleResult(object functionResult)
    {
        var token = ToToken(functionResult);
        if (token == null || token.Type == JTokenType.Null)
        {
            return null;
        }

        var dto = token.ToObject<ExecuteBattleEnvelopeDto>();
        if (dto == null)
        {
            return null;
        }

        if (dto.message == null)
        {
            dto.message = string.Empty;
        }

        dto.playersReadyCount = Math.Max(0, dto.playersReadyCount);
        return dto;
    }

    public static BattleResultDto ParseBattleResultObject(object battleResultObject)
    {
        var token = ToToken(battleResultObject);
        if (token == null || token.Type == JTokenType.Null)
        {
            return null;
        }

        var dto = token.ToObject<BattleResultDto>();
        Normalize(dto);
        return dto;
    }

    public static MatchStateDto ParseMatchStateResult(object functionResult)
    {
        var token = ToToken(functionResult);
        if (token == null || token.Type == JTokenType.Null)
        {
            return null;
        }

        var envelope = token.ToObject<MatchStateEnvelopeDto>();
        if (envelope?.matchState == null)
        {
            return null;
        }

        Normalize(envelope.matchState);
        return envelope.matchState;
    }

    private static void Normalize(BattleResultDto dto)
    {
        if (dto == null)
        {
            return;
        }

        NormalizeAttacker(dto.firstAttacker);
        NormalizeAttacker(dto.secondAttacker);
    }

    private static void NormalizeAttacker(BattleAttackerDto attacker)
    {
        if (attacker == null)
        {
            return;
        }

        if (attacker.bleedDamages == null)
        {
            attacker.bleedDamages = new List<int>();
        }

        if (attacker.effectsRemoved == null)
        {
            attacker.effectsRemoved = new List<int>();
        }

        if (attacker.effectsApplied == null)
        {
            attacker.effectsApplied = new List<BattleEffectDto>();
        }

        if (attacker.attackerEffectsApplied == null)
        {
            attacker.attackerEffectsApplied = new List<BattleEffectDto>();
        }

        if (attacker.effects == null)
        {
            attacker.effects = new List<BattleEffectDto>();
        }

        if (attacker.attackResult == null)
        {
            attacker.attackResult = string.Empty;
        }

        if (attacker.ongoingActionSteps == null)
        {
            attacker.ongoingActionSteps = new List<BattleOngoingActionStepDto>();
        }
    }

    private static void Normalize(MatchStateDto dto)
    {
        if (dto == null)
        {
            return;
        }

        dto.phase ??= string.Empty;
        dto.inferredPhase ??= string.Empty;
        dto.status ??= string.Empty;
        dto.seats ??= new List<MatchSeatDto>();
        dto.decks ??= new MatchDeckStateDto();
        dto.selectedCards ??= new Dictionary<string, SelectedCardData>();
        dto.battle ??= new MatchBattleStateDto();
        dto.nextTurnReady ??= new MatchNextTurnReadyDto();
        dto.replacementRequiredPlayerIds ??= new List<string>();
        dto.warnings ??= new List<string>();

        dto.decks.loadedPlayerIds ??= new List<string>();
        dto.battle.submittedByPlayerId ??= new Dictionary<string, bool>();
        dto.nextTurnReady.readyByPlayerId ??= new Dictionary<string, bool>();
        dto.nextTurnReady.readyPlayerIds ??= new List<string>();

        foreach (var kvp in dto.selectedCards)
        {
            if (kvp.Value != null && string.IsNullOrEmpty(kvp.Value.playerId))
            {
                kvp.Value.playerId = kvp.Key;
            }
        }
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

        // Handles Dictionary<string, object>, anonymous objects, etc.
        return JToken.FromObject(value, JsonSerializer.CreateDefault());
    }
}
