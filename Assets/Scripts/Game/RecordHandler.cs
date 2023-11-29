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
        GetMyBestScoreFromPlayFab("RoyalRumble", score =>
        {
            bestRecord = score;
            recordText.text = bestRecord.ToString();
        });
    }

    private void GetMyBestScoreFromPlayFab(string statisticName, System.Action<int> onScoreReceived)
    {
        playFabManager.GetMyBestScore(statisticName, score =>
        {
            onScoreReceived?.Invoke(score);
        });
    }

    public IEnumerator UpdateRecord(int enemyLevel)
    {
        // Počkajte, kým sa dokončí prvá metóda AddRandomCard

        if (enemyLevel > bestRecord)
        {
            //yield return StartCoroutine(cardGenerator.AddRandomCard());
            bestRecord += 1;
            // Počkajte, kým sa dokončí metóda UpdatePlayerData
            yield return SendNewRecordToPlayFab(enemyLevel);
            recordText.text = bestRecord.ToString();
            if (enemyLevel > 0)
            {
                // Počkajte, kým sa dokončí druhá metóda AddRandomCard
                yield return StartCoroutine(cardGenerator.AddRandomCardCoroutine());
            }
        }
    }

    public IEnumerator SendNewRecordToPlayFab(int enemyLevel)
    {
        playFabManager.SendLeaderboard(enemyLevel);
        yield break;
    }

}
