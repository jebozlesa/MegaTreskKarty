using System;
using System.Reflection;
using NUnit.Framework;

public class TemptationRulesTests
{
    [Test]
    public void CanTemptTarget_ReturnsTrue_ForOppositeGroups()
    {
        Assert.IsTrue(InvokeCanTemptTarget(10, 18), "Female style should tempt male style.");
        Assert.IsTrue(InvokeCanTemptTarget(18, 10), "Male style should tempt female style.");
    }

    [Test]
    public void CanTemptTarget_ReturnsFalse_ForSameGroupPairs()
    {
        Assert.IsFalse(InvokeCanTemptTarget(10, 4), "Female style should not tempt female style.");
        Assert.IsFalse(InvokeCanTemptTarget(18, 17), "Male style should not tempt male style.");
    }

    [Test]
    public void CanTemptTarget_ReturnsFalse_WhenEitherStyleHasNoTemptationGroup()
    {
        Assert.IsFalse(InvokeCanTemptTarget(10, 999), "Unknown defender style should fail compatibility.");
        Assert.IsFalse(InvokeCanTemptTarget(999, 18), "Unknown attacker style should fail compatibility.");
    }

    [Test]
    public void UseMaleSuccessAnimationVariant_MatchesConfiguredGroups()
    {
        Assert.IsTrue(InvokeUseMaleSuccessAnimationVariant(18), "Male group should use male animation variant.");
        Assert.IsFalse(InvokeUseMaleSuccessAnimationVariant(10), "Female group should not use male animation variant.");
        Assert.IsFalse(InvokeUseMaleSuccessAnimationVariant(999), "Unknown styles should default to non-male variant.");
    }

    private static bool InvokeCanTemptTarget(int attackerStyleId, int defenderStyleId)
    {
        Type rulesType = GetTemptationRulesType();
        MethodInfo method = rulesType.GetMethod("CanTemptTarget", BindingFlags.Public | BindingFlags.Static);
        Assert.IsNotNull(method, "Method TemptationRules.CanTemptTarget was not found.");
        return (bool)method.Invoke(null, new object[] { attackerStyleId, defenderStyleId });
    }

    private static bool InvokeUseMaleSuccessAnimationVariant(int styleId)
    {
        Type rulesType = GetTemptationRulesType();
        MethodInfo method = rulesType.GetMethod("UseMaleSuccessAnimationVariant", BindingFlags.Public | BindingFlags.Static);
        Assert.IsNotNull(method, "Method TemptationRules.UseMaleSuccessAnimationVariant was not found.");
        return (bool)method.Invoke(null, new object[] { styleId });
    }

    private static Type GetTemptationRulesType()
    {
        Type rulesType = Type.GetType("TemptationRules, Assembly-CSharp");
        Assert.IsNotNull(rulesType, "Type TemptationRules was not found in Assembly-CSharp.");
        return rulesType;
    }
}
