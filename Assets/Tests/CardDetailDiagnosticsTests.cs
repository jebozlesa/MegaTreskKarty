using System.IO;
using NUnit.Framework;
using UnityEngine;

public class CardDetailDiagnosticsTests
{
    [Test]
    public void CardDetailLifecycle_HasXpDiagnostics()
    {
        string cardSourcePath = Path.Combine(Application.dataPath, "Scripts", "Card.cs");
        Assert.IsTrue(File.Exists(cardSourcePath), "Card.cs was not found.");

        string source = File.ReadAllText(cardSourcePath);

        StringAssert.Contains("Debug.LogWarning($\"[CardDetail] LoadDetails", source);
        StringAssert.Contains("Debug.LogWarning($\"[CardDetail] OpenDetail", source);
        StringAssert.Contains("Debug.LogWarning($\"[CardDetail] CloseDetail", source);
        StringAssert.DoesNotContain("Debug.Log($\"[CardDetail]", source);
    }
}
