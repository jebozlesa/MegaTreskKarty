using System;
using System.Collections.Generic;

/// <summary>
/// ✅ V11: Server response pre openCardPack endpoint
/// </summary>
[Serializable]
public class OpenCardPackResponse
{
    public bool success;
    public List<GeneratedCard> cards;
    public int packIndex;
    public string error;
}
