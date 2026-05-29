using System.Collections;
using UnityEngine;

public partial class BattleResultProcessor
{
    private IEnumerator ShowDialog(string message)
    {
        if (dialogText != null)
        {
            dialogText.text = message;
        }

        yield return new WaitForSeconds(1.5f);
    }
}
