using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ServerFunctionsManagerLifecycleTests
{
    private GameObject root;
    private GameObject indicator;
    private Component manager;
    private Type managerType;
    private Type resultType;
    private Delegate callback;
    private int completions;
    private object received;

    [SetUp]
    public void SetUp()
    {
        completions = 0;
        received = null;
        managerType = Type.GetType("ServerFunctionsManager, Assembly-CSharp", true);
        resultType = Type.GetType("PlayFab.CloudScriptModels.ExecuteFunctionResult, PlayFab", true);
        root = new GameObject("ServerFunctionsLifecycleTest");
        manager = root.AddComponent(managerType);
        indicator = new GameObject("NetworkIndicator");
        indicator.SetActive(false);
        managerType.GetField("networkErrorIndicator").SetValue(manager, indicator);
        Type callbackType = typeof(Action<>).MakeGenericType(resultType);
        MethodInfo capture = GetType().GetMethod("Capture", BindingFlags.Instance | BindingFlags.NonPublic);
        callback = Delegate.CreateDelegate(callbackType, this, capture.MakeGenericMethod(resultType));
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(root);
        UnityEngine.Object.DestroyImmediate(indicator);
    }

    [TestCase("destroyed", "success")]
    [TestCase("destroyed", "error")]
    [TestCase("destroyed", "matchError")]
    [TestCase("inactive", "success")]
    [TestCase("inactive", "error")]
    [TestCase("inactive", "matchError")]
    [TestCase("disabled", "success")]
    [TestCase("disabled", "error")]
    [TestCase("disabled", "matchError")]
    public void LateCallbackSettlesWithoutChangingUiOrStartingRetry(string ownerState, string response)
    {
        indicator.SetActive(response == "success");
        bool wasVisible = indicator.activeSelf;
        AbandonOwner(ownerState);

        Assert.DoesNotThrow(() => Deliver(response, 3));

        Assert.AreEqual(1, completions, "Abandoned requests must still settle their awaiting caller.");
        Assert.IsNull(received, "A stale success must not be published as a live response.");
        Assert.AreEqual(wasVisible, indicator.activeSelf, "Old callbacks must not modify a surviving indicator.");
    }

    [TestCase("CallFunction")]
    [TestCase("CallMatchStateFunction")]
    [TestCase("CallFunctionWithRetry")]
    public void DestroyedOwnerRejectsDispatchBeforeContactingSdk(string method)
    {
        AbandonOwner("destroyed");
        Assert.DoesNotThrow(() =>
        {
            if (method == "CallFunction") Invoke(method, "getMatchState", new object(), callback);
            else if (method == "CallMatchStateFunction") Invoke(method, new object(), callback, 3);
            else Invoke(method, "getMatchState", new object(), callback, 3);
        });
        Assert.AreEqual(1, completions);
        Assert.IsNull(received);
    }

    [TestCase("RetryAfterDelay")]
    [TestCase("RetryMatchStateAfterDelay")]
    public void RetryIteratorRechecksOwnerAfterDelay(string method)
    {
        IEnumerator retry = method == "RetryAfterDelay"
            ? (IEnumerator)Invoke(method, "getMatchState", new object(), callback, 2)
            : (IEnumerator)Invoke(method, new object(), callback, 2);
        Assert.IsTrue(retry.MoveNext());
        AbandonOwner("destroyed");
        Assert.IsFalse(retry.MoveNext());
        Assert.AreEqual(1, completions);
        Assert.IsNull(received);
    }

    [Test]
    public void LiveSuccessForwardsOriginalResultAndHidesIndicator()
    {
        indicator.SetActive(true);
        object result = Activator.CreateInstance(resultType);
        Invoke("HandleFunctionSuccess", "getMatchState", result, callback);
        Assert.AreEqual(1, completions);
        Assert.AreSame(result, received);
        Assert.IsFalse(indicator.activeSelf);
    }

    [Test]
    public void LiveCommonErrorShowsIndicatorAndSettlesNull()
    {
        ExpectFunctionError();
        Deliver("error", 0);
        Assert.IsTrue(indicator.activeSelf);
        Assert.AreEqual(1, completions);
        Assert.IsNull(received);
    }

    [Test]
    public void LiveMatchErrorWithBudgetDefersCompletionForRetry()
    {
        managerType.GetField("retryDelay").SetValue(manager, 60f);
        ExpectFunctionError();
        Deliver("matchError", 3);
        Assert.IsTrue(indicator.activeSelf);
        Assert.AreEqual(0, completions, "A live retry must not prematurely settle the caller.");
        ((MonoBehaviour)manager).StopAllCoroutines();
    }

    [Test]
    public void LiveMatchErrorWithoutBudgetSettlesNull()
    {
        ExpectFunctionError();
        LogAssert.Expect(LogType.Error, "[ServerFunctionsManager] Failed: getMatchState failed after 3 retries!");
        Deliver("matchError", 0);
        Assert.IsTrue(indicator.activeSelf);
        Assert.AreEqual(1, completions);
        Assert.IsNull(received);
    }

    private void Capture<T>(T result)
    {
        completions++;
        received = result;
    }

    private void AbandonOwner(string state)
    {
        if (state == "destroyed") UnityEngine.Object.DestroyImmediate(root);
        else if (state == "disabled") ((Behaviour)manager).enabled = false;
        else root.SetActive(false);
    }

    private void Deliver(string response, int retriesLeft)
    {
        if (response == "success")
        {
            Invoke("HandleFunctionSuccess", "getMatchState", Activator.CreateInstance(resultType), callback);
            return;
        }
        Type errorType = Type.GetType("PlayFab.PlayFabError, PlayFab", true);
        object error = Activator.CreateInstance(errorType);
        errorType.GetField("ErrorMessage").SetValue(error, "Lifecycle test timeout");
        if (response == "error") Invoke("HandleFunctionError", "getMatchState", error, callback);
        else Invoke("HandleMatchStateError", new object(), callback, retriesLeft, error);
    }

    private object Invoke(string name, params object[] args)
    {
        MethodInfo method = managerType.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.IsNotNull(method, name);
        return method.Invoke(manager, args);
    }

    private static void ExpectFunctionError()
    {
        // Expected failure input; assertions above verify completion and UI behavior, not logging.
        LogAssert.Expect(LogType.Error, new Regex("ExecuteFunction error: getMatchState, error="));
    }
}
