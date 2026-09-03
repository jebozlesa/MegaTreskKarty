using System;
using System.Collections.Generic;

[Serializable]
public class TutorialStateResponse
{
    public bool success;
    public string playerId;
    public TutorialProgressDto progress;
    public TutorialGatesDto gates;
    public string recommendedRoute;
    public bool blockNavigation;
    public TutorialStarterCurrencyDto starterCurrency;
    public string tutorialId;
    public string stepId;
    public bool alreadyCompleted;
    public string stage;
    public string error;

    public bool IsFlowCompleted(string flowId)
    {
        if (progress?.flows == null || string.IsNullOrWhiteSpace(flowId))
        {
            return false;
        }

        return progress.flows.TryGetValue(flowId, out TutorialFlowProgressDto flow)
            && flow != null
            && flow.completed;
    }

    public bool HasCompletedStep(string flowId, string stepIdToCheck)
    {
        if (
            progress?.flows == null
            || string.IsNullOrWhiteSpace(flowId)
            || string.IsNullOrWhiteSpace(stepIdToCheck)
        )
        {
            return false;
        }

        return progress.flows.TryGetValue(flowId, out TutorialFlowProgressDto flow)
            && flow?.completedStepIds != null
            && flow.completedStepIds.Contains(stepIdToCheck);
    }
}

[Serializable]
public class TutorialProgressDto
{
    public int version;
    public TutorialStarterCurrencyGrantDto starterCurrencyGrant;
    public Dictionary<string, TutorialFlowProgressDto> flows;
}

[Serializable]
public class TutorialStarterCurrencyGrantDto
{
    public bool completed;
    public string currencyCode;
    public int amount;
    public string completedAt;
}

[Serializable]
public class TutorialFlowProgressDto
{
    public bool completed;
    public List<string> completedStepIds;
    public string completedAt;
    public string updatedAt;
}

[Serializable]
public class TutorialGatesDto
{
    public bool safeCardDeckState;
    public bool needsFirstPack;
    public bool needsLibraryDeckSwap;
    public bool criticalOnboardingComplete;
}

[Serializable]
public class TutorialStarterCurrencyDto
{
    public bool ensured;
    public string currencyCode;
    public int amount;
    public int grantedAmount;
}
