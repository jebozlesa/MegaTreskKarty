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
            ApplyDisplayedRecord(Mathf.Max(bestRecord, score));
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
            yield return new WaitForSeconds(0.5f);
            if (enemyLevel > 0)
            {
                // Počkajte, kým sa dokončí druhá metóda AddRandomCard
                yield return StartCoroutine(cardGenerator.AddRandomCardCoroutine());
            }
        }
    }

    public void ApplyRoyalRumbleRecordSnapshot(
        RoyalRumbleSessionDto session,
        RoyalRumbleRecordSyncDto recordSync = null)
    {
        ApplyDisplayedRecord(RoyalRumbleRecordDisplay.ResolveVisibleRecord(bestRecord, session, recordSync));
    }

    public void ApplyDisplayedRecord(int score)
    {
        bestRecord = Mathf.Max(0, score);
        if (recordText != null)
        {
            recordText.text = bestRecord.ToString();
        }
    }

    public IEnumerator SendNewRecordToPlayFab(int enemyLevel)
    {
        playFabManager.SendLeaderboard(enemyLevel);
        yield break;
    }

}
