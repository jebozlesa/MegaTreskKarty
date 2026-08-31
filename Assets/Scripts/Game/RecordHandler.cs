using TMPro;
using UnityEngine;

public class RecordHandler : MonoBehaviour
{
    public TMP_Text recordText;
    public int bestRecord;
    public PlayFabManager playFabManager;

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
}
