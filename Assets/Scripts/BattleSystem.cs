using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum BattleState {START,TURN,PLAYERDEATH,ENEMYDEATH,WON,LOST}

public class BattleSystem : MonoBehaviour
{
    public GameObject playerCard;
    public GameObject enemyCard;

    Card playerUnit;
    Card enemyUnit;

    public TMP_Text dialogText;

    public LifeBar playerLifeBar;
    public LifeBar enemyLifeBar;

    public BattleState state;

    // Start is called before the first frame update
    void Start()
    {
        state = BattleState.START;
        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle()
    {
        GameObject playerGO = Instantiate(playerCard);
        playerUnit = playerGO.GetComponent<Card>();

	    yield return new WaitForSeconds(3.5f);

        GameObject enemyGO = Instantiate(enemyCard);
        enemyUnit = enemyGO.GetComponent<Card>();

    //    playerUnit.LoadCard();
    //    enemyUnit.LoadCard();
	yield return new WaitForSeconds(2f);

		Debug.Log(enemyUnit);
		Debug.Log("MENO TYPKA ");
		Debug.Log("MENO TYPKA " + enemyUnit.cardName);

        dialogText.text = "kurva " + enemyUnit.cardName + " !!!";

     //   playerLifeBar.SetBar(playerUnit);
     //   enemyLifeBar.SetBar(enemyUnit);

        yield return new WaitForSeconds(2f);

     //   state = BattleState.TURN;
        //PlayerTurn();

    }


  /*  IEnumerator PlayerAttack()
    {

        bool isDead = enemyUnit.TakeDamage(playerUnit.damage);

		enemyLifeBar.SetHP(enemyUnit.currentHP);
		dialogText.text = "The attack is successful!";

		yield return new WaitForSeconds(2f);

		if(isDead)
		{
			state = BattleState.WON;
			EndBattle();
		} else
		{
			state = BattleState.ENEMYTURN;
			StartCoroutine(EnemyTurn());
		}

    }*/

  /*  IEnumerator EnemyTurn()
	{
		dialogText.text = enemyUnit.cardName + " attacks!";

		yield return new WaitForSeconds(1f);

		bool isDead = playerUnit.TakeDamage(enemyUnit.damage);

		playerLifeBar.SetHP(playerUnit.currentHP);

		yield return new WaitForSeconds(1f);

		if(isDead)
		{
			state = BattleState.LOST;
			EndBattle();
		} else
		{
			state = BattleState.PLAYERTURN;
			PlayerTurn();
		}

	}*/

	/*void EndBattle()
	{
		if(state == BattleState.WON)
		{
			dialogText.text = "You won the battle!";
		} else if (state == BattleState.LOST)
		{
			dialogText.text = "You were defeated.";
		}
	}*/

  /*  IEnumerator PlayerHeal()
	{
		playerUnit.Heal(5);

		playerLifeBar.SetHP(playerUnit.currentHP);
		dialogText.text = "You feel renewed strength!";

		yield return new WaitForSeconds(2f);

		state = BattleState.ENEMYTURN;
		StartCoroutine(EnemyTurn());
	}*/

  /*  public void OnAttackButton()
	{
		if (state != BattleState.PLAYERTURN)
			return;

		StartCoroutine(PlayerAttack());
	}*/

/*	public void OnHealButton()
	{
		if (state != BattleState.PLAYERTURN)
			return;

		StartCoroutine(PlayerHeal());
	}*/


   /* void PlayerTurn()
    {
        dialogText.text = "Choose an attack";
    }*/

  /*  public void AttackButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        StartCoroutine(PlayerAttack());
    }*/
}
