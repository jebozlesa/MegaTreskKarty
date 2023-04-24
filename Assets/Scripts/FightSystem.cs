using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public enum FightState {START,TURN,ENDTURN,PLAYERDEATH,ENEMYDEATH,WON,LOST}

public class FightSystem : MonoBehaviour
{

    public Player player;
    public Player enemy;

    public GameObject playerBoard;
    public GameObject enemyBoard;

    public TMP_Text dialogText;
    public Image dialogButtonBorder;

    public TMP_Text button1Text;
    public TMP_Text button2Text;
    public TMP_Text button3Text;
    public TMP_Text button4Text;

    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;

    public HealthBar playerLifeBar;
    public HealthBar enemyLifeBar;

    public FightState state;

    public GameObject kartaPrefab;
    public GameObject hrac;
    public GameObject nepriatel;

    public TextAsset cardDatabase;
    private List<string> kartyData;
    public TextAsset playerCardDatabase;
    private List<string> kartyHrac;

    public Attack attack;
    public AttackDescriptions attackDescriptions;

    public Effects effects;

    int playerAttack;
    int enemyAttack;

    void Start()
    {
        kartyData = new List<string>(cardDatabase.text.TrimEnd().Split('\n'));
        kartyHrac = new List<string>(playerCardDatabase.text.TrimEnd().Split('\n'));

        state = FightState.START;
        StartCoroutine(SetupBattle());
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator SetupBattle()
    {

        dialogText.text = "lets get ready for rumble!";

        yield return StartCoroutine(VytvorJedinecneKarty(5,hrac,player));
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(VytvorKartyAI(nepriatel,enemy));
        yield return new WaitForSeconds(1);

        player.PlayCard(player.hand[0],playerBoard);
        playerLifeBar.SetBar(player.cardInGame);

        enemy.PlayCard(enemy.hand[0],enemyBoard);
        enemyLifeBar.SetBar(enemy.cardInGame);

        dialogText.text = "Ohhh s@!t " + enemy.cardInGame.cardName + " !!!";

        yield return new WaitForSeconds(1f);
        Turn();
    }

    void Turn()
    {
        state = FightState.TURN;
        playerAttack = 0;
        float timeout = Time.time + 5f; // nastavíme timeout na 5 sekund od začátku cyklu
        do
        {
            enemyAttack = Random.Range(1, 5);//vyberie nahodny utok pre nepriatela
            if (Time.time > timeout)
            {
                Debug.Log("Timeout - nepodařilo se generovat náhodnou hodnotu, použije se výchozí hodnota");
                dialogText.text = "Daco sa dojebalo";
                break;
            }
        } while (enemy.cardInGame.attackCount[enemyAttack] == 0);

        if (enemy.cardInGame.state == CardState.STAY) enemyAttack = 0;//ak enemy stoji tak nech stoji

        if (player.cardInGame.state != CardState.STAY)
        {
            dialogText.text = "Choose an attack";
            attackDescriptions.ShowDescription(player.cardInGame);
        }
        else
        {
            dialogText.text = "no attack";
            state = FightState.ENDTURN;
            StartCoroutine(Fight());
        }
    }

    public void AttackButton(int attackType)
    {
        if (state != FightState.TURN)
            return;
        playerAttack = attackType;
        attackDescriptions.DisplayAttack(player.cardInGame,attackType,dialogText);
    }

    public void ConfirmAttackButton()
    {
        if (state != FightState.TURN || (playerAttack == 0 && player.cardInGame.state == CardState.ATTACK) || player.cardInGame.attackCount[playerAttack] == 0)
            return;
        state = FightState.ENDTURN;
        StartCoroutine(Fight());
    }

    IEnumerator Fight()
    {

        if (player.cardInGame.speed > enemy.cardInGame.speed || (player.cardInGame.speed == enemy.cardInGame.speed && UnityEngine.Random.Range(0, 2) == 0))
        {
            dialogText.color = Color.blue;
            yield return StartCoroutine(effects.ExecuteEffects(player.cardInGame, dialogText, enemy.cardInGame));
            playerLifeBar.SetHP(player.cardInGame.health);

            if ((player.cardInGame.state == CardState.ATTACK || (player.cardInGame.state == CardState.MAYBE && attack.exceptionAttacks.Contains(playerAttack))) 
                && player.cardInGame.health > 0 && enemy.cardInGame.health > 0)  
            {
                yield return StartCoroutine(attack.ExecuteAttack(player.cardInGame, enemy.cardInGame, playerAttack, dialogText));
                playerLifeBar.SetHP(player.cardInGame.health);
                enemyLifeBar.SetHP(enemy.cardInGame.health);
            }

            if (player.cardInGame.health > 0 && enemy.cardInGame.health > 0) 
            {
                dialogText.color = Color.red;
                yield return StartCoroutine(effects.ExecuteEffects(enemy.cardInGame, dialogText, player.cardInGame));
                enemyLifeBar.SetHP(enemy.cardInGame.health);

                if ((enemy.cardInGame.state == CardState.ATTACK || (enemy.cardInGame.state == CardState.MAYBE && attack.exceptionAttacks.Contains(enemyAttack))) 
                    && player.cardInGame.health > 0 && enemy.cardInGame.health > 0) 
                {
                    yield return StartCoroutine(attack.ExecuteAttack(enemy.cardInGame, player.cardInGame, enemyAttack, dialogText));
                    playerLifeBar.SetHP(player.cardInGame.health);
                    enemyLifeBar.SetHP(enemy.cardInGame.health);
                }
            }
    
        }
        else
        {
            dialogText.color = Color.red;
            yield return StartCoroutine(effects.ExecuteEffects(enemy.cardInGame, dialogText, player.cardInGame));
            enemyLifeBar.SetHP(enemy.cardInGame.health);

            if ((enemy.cardInGame.state == CardState.ATTACK || (enemy.cardInGame.state == CardState.MAYBE && attack.exceptionAttacks.Contains(enemyAttack))) 
                && player.cardInGame.health > 0 && enemy.cardInGame.health > 0) 
            {
                yield return StartCoroutine(attack.ExecuteAttack(enemy.cardInGame, player.cardInGame, enemyAttack, dialogText));
                playerLifeBar.SetHP(player.cardInGame.health);
                enemyLifeBar.SetHP(enemy.cardInGame.health);
            }

            if (player.cardInGame.health > 0 && enemy.cardInGame.health > 0)
            {
                dialogText.color = Color.blue;
                yield return StartCoroutine(effects.ExecuteEffects(player.cardInGame, dialogText, enemy.cardInGame));
                playerLifeBar.SetHP(player.cardInGame.health);

                if ((player.cardInGame.state == CardState.ATTACK || (player.cardInGame.state == CardState.MAYBE && attack.exceptionAttacks.Contains(playerAttack))) 
                    && player.cardInGame.health > 0 && enemy.cardInGame.health > 0) 
                {
                    yield return StartCoroutine(attack.ExecuteAttack(player.cardInGame, enemy.cardInGame, playerAttack, dialogText));
                    playerLifeBar.SetHP(player.cardInGame.health);
                    enemyLifeBar.SetHP(enemy.cardInGame.health);
                }
            }
            
        }

        dialogText.color = Color.black;

        if (player.cardInGame.health <= 0)
			{
                player.RemoveCardFromBoard(player.cardInGame);
                yield return new WaitForSeconds(2.5f);
                if (player.hand.Count == 0)
                {
                    state = FightState.PLAYERDEATH;
                    dialogText.text = "Loser you are!";
                    yield break;
                }
                player.PlayCard(player.hand[0],playerBoard);
                playerLifeBar.SetBar(player.cardInGame);
            }
        
		if (enemy.cardInGame.health <= 0)
			{
                player.cardInGame.AddExperience(enemy.cardInGame.level);//exp

                enemy.RemoveCardFromBoard(enemy.cardInGame);//tu som robil daco, vymazat ak nic zle
                yield return new WaitForSeconds(2.5f);
                if (enemy.hand.Count == 0)
                {
                    state = FightState.ENEMYDEATH;
                    dialogText.text = "You won the game!";
                    yield break;
                }
                enemy.PlayCard(enemy.hand[0],enemyBoard);
                enemyLifeBar.SetBar(enemy.cardInGame);
            }

        Turn();

    }
    

    private IEnumerator VytvorJedinecneKarty(int pocet, GameObject playerGO,Player player)
    {
    
        List<int> usedIndexes = new List<int>(); // List to keep track of used indexes
        int maxIndex = kartyHrac.Count; // Maximum index value to generate random numbers within range

        for (int i = 0; i < pocet; i++)
        {
            int index;
            do
            {
                index = Random.Range(0, maxIndex);
            } while (usedIndexes.Contains(index)); // Keep generating random numbers until a unique index is found

            usedIndexes.Add(index); // Add the index to the list of used indexes

            string kartaString = kartyHrac[index];
            string[] kartaHodnoty = kartaString.Split(',');

            string[] farbaKarty = kartaHodnoty[10].Split(';');
            Color32 cardColor = new Color32(byte.Parse(farbaKarty[0]), byte.Parse(farbaKarty[1]), byte.Parse(farbaKarty[2]), 255);

            GameObject novaKarta = Instantiate(kartaPrefab, playerGO.transform);
            novaKarta.GetComponent<Kard>().cardId = int.Parse(kartaHodnoty[0]);
            novaKarta.GetComponent<Kard>().cardName = kartaHodnoty[1];
            novaKarta.GetComponent<Kard>().health = int.Parse(kartaHodnoty[2]);
            novaKarta.GetComponent<Kard>().strength = int.Parse(kartaHodnoty[3]);
            novaKarta.GetComponent<Kard>().speed = int.Parse(kartaHodnoty[4]);
            novaKarta.GetComponent<Kard>().attack = int.Parse(kartaHodnoty[5]);
            novaKarta.GetComponent<Kard>().defense = int.Parse(kartaHodnoty[6]);
            novaKarta.GetComponent<Kard>().knowledge = int.Parse(kartaHodnoty[7]);
            novaKarta.GetComponent<Kard>().charisma = int.Parse(kartaHodnoty[8]);
            novaKarta.GetComponent<Kard>().image = kartaHodnoty[9];
            novaKarta.GetComponent<Kard>().color = cardColor;
            novaKarta.GetComponent<Kard>().level = int.Parse(kartaHodnoty[11]);
            novaKarta.GetComponent<Kard>().attack1 = int.Parse(kartaHodnoty[12]);
            novaKarta.GetComponent<Kard>().countAttack1 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(),int.Parse(kartaHodnoty[12]));
            novaKarta.GetComponent<Kard>().attack2 = int.Parse(kartaHodnoty[13]);
            novaKarta.GetComponent<Kard>().countAttack2 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(),int.Parse(kartaHodnoty[13]));
            novaKarta.GetComponent<Kard>().attack3 = int.Parse(kartaHodnoty[14]);
            novaKarta.GetComponent<Kard>().countAttack3 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(),int.Parse(kartaHodnoty[14]));
            novaKarta.GetComponent<Kard>().attack4 = int.Parse(kartaHodnoty[15]);
            novaKarta.GetComponent<Kard>().countAttack4 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(),int.Parse(kartaHodnoty[15]));
            novaKarta.GetComponent<Kard>().experience = int.Parse(kartaHodnoty[16]);

            player.AddCardToHand(novaKarta.GetComponent<Kard>());

            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator VytvorKartyAI(GameObject playerGO, Player player)
    {
        int iter = 0;
        int[] boost = new int[6];

        // Shuffle the list of cards
        for (int i = kartyData.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            string temp = kartyData[i];
            kartyData[i] = kartyData[j];
            kartyData[j] = temp;
        }

        // Create all cards in the shuffled order
        foreach (string kartaString in kartyData)
        {
            boost = DistributeRandomly(iter);

            string[] kartaHodnoty = kartaString.Split(',');

            string[] farbaKarty = kartaHodnoty[10].Split(';');
            Color32 cardColor = new Color32(byte.Parse(farbaKarty[0]), byte.Parse(farbaKarty[1]), byte.Parse(farbaKarty[2]), 255);

            GameObject novaKarta = Instantiate(kartaPrefab, playerGO.transform);
            novaKarta.GetComponent<Kard>().cardName = kartaHodnoty[1];
            novaKarta.GetComponent<Kard>().health = int.Parse(kartaHodnoty[2]) + iter;
            novaKarta.GetComponent<Kard>().strength = int.Parse(kartaHodnoty[3]) + boost[0];
            novaKarta.GetComponent<Kard>().speed = int.Parse(kartaHodnoty[4]) + boost[1];
            novaKarta.GetComponent<Kard>().attack = int.Parse(kartaHodnoty[5]) + boost[2];
            novaKarta.GetComponent<Kard>().defense = int.Parse(kartaHodnoty[6]) + boost[3];
            novaKarta.GetComponent<Kard>().knowledge = int.Parse(kartaHodnoty[7]) + boost[4];
            novaKarta.GetComponent<Kard>().charisma = int.Parse(kartaHodnoty[8]) + boost[5];
            novaKarta.GetComponent<Kard>().image = kartaHodnoty[9];
            novaKarta.GetComponent<Kard>().color = cardColor;
            novaKarta.GetComponent<Kard>().level = int.Parse(kartaHodnoty[11]) + iter;
            novaKarta.GetComponent<Kard>().attack1 = int.Parse(kartaHodnoty[12]);
            novaKarta.GetComponent<Kard>().countAttack1 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(),int.Parse(kartaHodnoty[12]));
            novaKarta.GetComponent<Kard>().attack2 = int.Parse(kartaHodnoty[13]);
            novaKarta.GetComponent<Kard>().countAttack2 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(),int.Parse(kartaHodnoty[13]));
            novaKarta.GetComponent<Kard>().attack3 = int.Parse(kartaHodnoty[14]);
            novaKarta.GetComponent<Kard>().countAttack3 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(),int.Parse(kartaHodnoty[14]));
            novaKarta.GetComponent<Kard>().attack4 = int.Parse(kartaHodnoty[15]);
            novaKarta.GetComponent<Kard>().countAttack4 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(),int.Parse(kartaHodnoty[15]));

            player.AddCardToHand(novaKarta.GetComponent<Kard>());

            iter += 1;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public int[] DistributeRandomly(int value)
    {
        int[] result = new int[6];

        // Nastaviť všetky prvky poľa na hodnotu 0
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = 0;
        }

        // Pridať zvyšok hodnoty do pola náhodne
        int remainingValue = value;

        while (remainingValue > 0)
        {
            for (int j = 0; j < result.Length && remainingValue > 0; j++)
            {
                if (UnityEngine.Random.Range(0f, 1f) < 0.2f)
                {
                    result[j] += 1;
                    remainingValue--;
                }
            }
        }

        return result;
    }
}