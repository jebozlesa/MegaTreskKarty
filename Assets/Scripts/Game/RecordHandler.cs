using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RecordHandler : MonoBehaviour
{
    public TMP_Text recordText;
    public int bestRecord;
    public PlayerDataHandler playerDataHandler;

    public CardGenerator cardGenerator;

    public PlayFabManager playFabManager;



    // Start is called before the first frame update
    void Start()
    {
        bestRecord = playerDataHandler.GetPlayerIntData("RRrecord");
        recordText.text = bestRecord.ToString();
    }

    public IEnumerator UpdateRecord(int enemyLevel)
    {
        // Počkajte, kým sa dokončí prvá metóda AddRandomCard

        if (enemyLevel > bestRecord)
        {
            //yield return StartCoroutine(cardGenerator.AddRandomCard());
            bestRecord += 1;
            // Počkajte, kým sa dokončí metóda UpdatePlayerData
            yield return StartCoroutine(playerDataHandler.UpdatePlayerData("RRrecord", bestRecord));
            recordText.text = bestRecord.ToString();
            if (enemyLevel > 0)
            {
                // Počkajte, kým sa dokončí druhá metóda AddRandomCard
                yield return StartCoroutine(cardGenerator.AddRandomCardCoroutine());
            }
        }
    }

    public void SendNewRecordToPlayFab(int enemyLevel)
    {
        playFabManager.SendLeaderboard(enemyLevel);
    }

}
