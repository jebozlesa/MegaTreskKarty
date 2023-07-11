using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;

public class RecordsLoader : MonoBehaviour
{
    public PlayFabManager playFabManager;
    public GameObject rowPrefab; // Prefab pre záznam
    public Transform rowsParent; // Zoznam záznamov

    void Start()
    {
        // Sledujeme udalosť
        playFabManager.OnLeaderboardLoaded += UpdateRecordList;

        // Načítame záznamy
        //playFabManager.GetLeaderboard();
    }

    void UpdateRecordList(List<PlayerLeaderboardEntry> records)
    {
        // Odstránime existujúce záznamy
        foreach (Transform child in rowsParent)
        {
            Destroy(child.gameObject);
        }
        // Prejdeme všetky záznamy
        foreach (var record in records)
        {
            // Vytvoríme novú inštanciu prefabu pre záznam
            GameObject rowInstance = Instantiate(rowPrefab, rowsParent);

            // Nastavíme textové polia v zázname
            TextMeshProUGUI[] texts = rowInstance.GetComponentsInChildren<TextMeshProUGUI>();
            texts[0].text = (record.Position + 1).ToString() + ".";
            texts[1].text = record.PlayFabId;
            texts[2].text = record.StatValue.ToString();
        }
    }
}