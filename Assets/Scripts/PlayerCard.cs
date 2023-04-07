using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCard : MonoBehaviour
{
    public GameObject battlefield;
    public GameObject playerCardInGame;

    public int damage;
    public int maxHP;
    public int currentHP;

    // Start is called before the first frame update
    void Start()
    {
       // GameObject playerGO = Instantiate(playerPrefab, playerBattleStation);


    }

    // Update is called once per frame
    void Update()
    {
        battlefield = GameObject.Find("PlayerSide");
        playerCardInGame.transform.SetParent(battlefield.transform);
        playerCardInGame.transform.localScale = Vector3.one;
        playerCardInGame.transform.position = new Vector3(transform.position.x,transform.position.y,-48);
        //HandCard.transform.eulerAngles = new Vector3(25,0,1);
    }
}
