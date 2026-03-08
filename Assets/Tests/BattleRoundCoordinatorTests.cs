using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

public class BattleRoundCoordinatorTests
{
    [Test]
    public void TryGetSuccessFlag_ReturnsTrue_ForDictionaryPayload()
    {
        object payload = new Dictionary<string, object> { { "success", true } };

        bool success = InvokeTryGetSuccessFlag(payload, out var flag);

        Assert.IsTrue(success);
        Assert.IsTrue(flag);
    }

    [Test]
    public void TryGetSuccessFlag_ReturnsFalse_ForMissingFlag()
    {
        object payload = new Dictionary<string, object> { { "message", "ok" } };

        bool success = InvokeTryGetSuccessFlag(payload, out var flag);

        Assert.IsFalse(success);
        Assert.IsFalse(flag);
    }

    [Test]
    public void IsCardPresentInSelectedCards_ReturnsTrue_WhenDeadCardStillPresent()
    {
        object payload = new Dictionary<string, object>
        {
            {
                "selectedCards", new Dictionary<string, object>
                {
                    {
                        "p1", new Dictionary<string, object>
                        {
                            { "cardId", "dead-card-id" }
                        }
                    },
                    {
                        "p2", new Dictionary<string, object>
                        {
                            { "cardId", "alive-card-id" }
                        }
                    }
                }
            }
        };

        bool present = InvokeIsCardPresentInSelectedCards(payload, "dead-card-id");

        Assert.IsTrue(present);
    }

    [Test]
    public void IsCardPresentInSelectedCards_ReturnsFalse_WhenDeadCardIsGone()
    {
        object payload = new Dictionary<string, object>
        {
            {
                "selectedCards", new Dictionary<string, object>
                {
                    {
                        "p1", new Dictionary<string, object>
                        {
                            { "cardId", "alive-card-id" }
                        }
                    }
                }
            }
        };

        bool present = InvokeIsCardPresentInSelectedCards(payload, "dead-card-id");

        Assert.IsFalse(present);
    }

    private static bool InvokeTryGetSuccessFlag(object payload, out bool flag)
    {
        Type coordinatorType = Type.GetType("BattleRoundCoordinator, Assembly-CSharp");
        Assert.IsNotNull(coordinatorType, "Type BattleRoundCoordinator was not found in Assembly-CSharp.");

        MethodInfo method = coordinatorType.GetMethod("TryGetSuccessFlag", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(method, "Method TryGetSuccessFlag was not found.");

        object[] args = { payload, false };
        bool ok = (bool)method.Invoke(null, args);
        flag = (bool)args[1];
        return ok;
    }

    private static bool InvokeIsCardPresentInSelectedCards(object payload, string cardId)
    {
        Type coordinatorType = Type.GetType("BattleRoundCoordinator, Assembly-CSharp");
        Assert.IsNotNull(coordinatorType, "Type BattleRoundCoordinator was not found in Assembly-CSharp.");

        MethodInfo method = coordinatorType.GetMethod("IsCardPresentInSelectedCards", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(method, "Method IsCardPresentInSelectedCards was not found.");

        return (bool)method.Invoke(null, new[] { payload, cardId });
    }
}
