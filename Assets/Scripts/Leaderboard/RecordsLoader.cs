using System.Collections.Generic;
using TMPro;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;

public class RecordsLoader : MonoBehaviour
{
    public PlayFabManagerLeaderboard playFabManager;
    public GameObject rowPrefab;
    public Transform rowsParent;

    private void Start()
    {
        if (!HasRequiredReferences())
        {
            return;
        }

        playFabManager.OnLeaderboardLoaded += UpdateRecordList;
        Debug.Log("[RecordsLoader] Requesting RoyalRumble leaderboard.");
        playFabManager.GetLeaderboard();
    }

    private void OnDestroy()
    {
        if (playFabManager != null)
        {
            playFabManager.OnLeaderboardLoaded -= UpdateRecordList;
        }
    }

    private void UpdateRecordList(List<PlayerLeaderboardEntry> records)
    {
        if (!HasRequiredReferences())
        {
            return;
        }

        records ??= new List<PlayerLeaderboardEntry>();
        Debug.Log($"[RecordsLoader] Rendering RoyalRumble leaderboard: records={records.Count}.");

        foreach (Transform child in rowsParent)
        {
            Destroy(child.gameObject);
        }

        string loggedInPlayerId =
            PlayFabManagerLogin.Instance != null ? PlayFabManagerLogin.Instance.LoggedInPlayerId : "";

        foreach (var record in records)
        {
            GameObject rowInstance = Instantiate(rowPrefab, rowsParent);
            TextMeshProUGUI[] texts = rowInstance.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length < 3)
            {
                Debug.LogError("[RecordsLoader] Leaderboard row prefab must contain 3 TMP text fields.");
                continue;
            }

            texts[0].text = (record.Position + 1).ToString() + ".";
            texts[1].text = string.IsNullOrEmpty(record.DisplayName) ? "Noname" : record.DisplayName;
            texts[2].text = record.StatValue.ToString();

            Image innerImage =
                rowInstance.transform.childCount > 0
                    ? rowInstance.transform.GetChild(0).GetComponent<Image>()
                    : null;

            if (innerImage != null && record.PlayFabId == loggedInPlayerId)
            {
                innerImage.color = Color.yellow;
            }
        }
    }

    private bool HasRequiredReferences()
    {
        bool valid = true;
        if (playFabManager == null)
        {
            Debug.LogError("[RecordsLoader] Missing PlayFabManagerLeaderboard reference.");
            valid = false;
        }

        if (rowPrefab == null)
        {
            Debug.LogError("[RecordsLoader] Missing leaderboard row prefab reference.");
            valid = false;
        }

        if (rowsParent == null)
        {
            Debug.LogError("[RecordsLoader] Missing leaderboard rows parent reference.");
            valid = false;
        }

        return valid;
    }
}
