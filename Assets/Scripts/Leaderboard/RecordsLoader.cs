using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;

public class RecordsLoader : MonoBehaviour
{
    public PlayFabManagerLeaderboard playFabManager;
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
        foreach (Transform child in rowsParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var record in records)
        {
            GameObject rowInstance = Instantiate(rowPrefab, rowsParent);
            TextMeshProUGUI[] texts = rowInstance.GetComponentsInChildren<TextMeshProUGUI>();
            texts[0].text = (record.Position + 1).ToString() + ".";
            texts[1].text = string.IsNullOrEmpty(record.DisplayName) ? "Noname" : record.DisplayName;
            texts[2].text = record.StatValue.ToString();

            // Získame vnútorný Image komponent, ktorý chceme zafarbiť
            Image innerImage = rowInstance.transform.GetChild(0).GetComponent<Image>(); // Predpokladáme, že vnútorný Image je druhým dieťaťom

            // Ak sa jedná o záznam prihláseného hráča, zmeníme farbu vnútorného obrázka
            if (record.PlayFabId == PlayFabManagerLeaderboard.Instance.loggedInPlayerId)
            {
                innerImage.color = Color.yellow; // Zmena farby na žltú
            }
        }
    }


}