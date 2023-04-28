using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordHandler : MonoBehaviour
{
    public TMP_Text recordText;
    public int bestRecord;
    public PlayerDataHandler playerDataHandler;



    // Start is called before the first frame update
    void Start()
    {
        bestRecord = playerDataHandler.GetPlayerIntData("RRrecord");
    }

    public void UpdateRecord()
    {
        bestRecord += 1;
        playerDataHandler.UpdatePlayerData("RRrecord", bestRecord);
    }
}
