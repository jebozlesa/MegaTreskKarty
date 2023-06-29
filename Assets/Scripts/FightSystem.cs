using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;


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

    public RecordHandler recordHandler;
    int enemyLevel = 0;

    private string connectionString;


    void Start()
    {
        connectionString = $"URI=file:{Database.Instance.GetDatabasePath()}";

        state = FightState.START;

        player.isEnemy = false;
        enemy.isEnemy = true;

        StartCoroutine(SetupBattle(GameParameters.MissionID));
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator SetupBattle(int missionID = 0)
    {
        dialogText.text = "lets get ready for rumble!";

        yield return StartCoroutine(VytvorKartyZBalicka(hrac, player));
        yield return new WaitForSeconds(0.5f);

        if(missionID > 0)
            yield return StartCoroutine(VytvorKartyAIMission(nepriatel, enemy, missionID));
        else
            yield return StartCoroutine(VytvorKartyAI(nepriatel, enemy));

        yield return new WaitForSeconds(1);
        // player.PlayCard(player.hand[0],playerBoard);
        // playerLifeBar.SetBar(player.cardInGame);
        // player.cardInGame.isDragable = false;

        enemy.PlayCard(enemy.hand[0],enemyBoard);
        enemyLifeBar.SetBar(enemy.cardInGame);

        dialogText.text = "Ohhh s@!t " + enemy.cardInGame.cardName + " !!!";

        dialogText.text = "Choose fighter";
        yield return new WaitUntil(() => state == FightState.TURN);
        playerLifeBar.SetBar(player.cardInGame);
        player.cardInGame.isDragable = false;

       // yield return new WaitForSeconds(1f);
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

    public Kard ChooseAttacker(Kard player, Kard enemy, int playerAttack, int enemyAttack)
    {
        bool playerPriority = attack.priorityAttacks.Contains(playerAttack);
        bool enemyPriority = attack.priorityAttacks.Contains(enemyAttack);

        if (playerPriority && !enemyPriority)
        {
            return player;
        }
        else if (!playerPriority && enemyPriority)
        {
            return enemy;
        }
        else if (player.speed > enemy.speed)
        {
            return player;
        }
        else if (enemy.speed > player.speed)
        {
            return enemy;
        }
        else // Rovnaká rýchlosť
        {
            if (Random.value < 0.5f) // 50% šanca pre každú kartu
            {
                return player;
            }
            else
            {
                return enemy;
            }
        }
    }



    IEnumerator Fight()
    {

        int playerAttackNumber = attack.GetAttackNumber(player.cardInGame, playerAttack);
        int enemyAttackNumber = attack.GetAttackNumber(enemy.cardInGame, enemyAttack);

        Debug.Log("PLAYER "+playerAttackNumber+" ENEMY "+enemyAttackNumber);

        bool playerPriority = attack.IsPriorityAttack(playerAttackNumber);
        bool enemyPriority = attack.IsPriorityAttack(enemyAttackNumber);

        Debug.Log("PLAYER "+playerPriority+" ENEMY "+enemyPriority);

        bool playerGoesFirst = (playerPriority && !enemyPriority) ||
                            (!enemyPriority && player.cardInGame.speed > enemy.cardInGame.speed) ||
                            (player.cardInGame.speed == enemy.cardInGame.speed && UnityEngine.Random.Range(0, 2) == 0);

        if (playerGoesFirst)
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

        if (enemy.cardInGame.health <= 0)
        {
            player.cardInGame.AddExperience(enemy.cardInGame.level);//exp
            enemyLevel += 1;
            yield return StartCoroutine(recordHandler.UpdateRecord(enemyLevel));

            enemy.RemoveCardFromBoard(enemy.cardInGame);
            state = FightState.ENEMYDEATH;
            yield return new WaitForSeconds(0.5f);

            if (enemy.hand.Count == 0)
            {
                state = FightState.WON;
                dialogText.text = "You won the game!";
                yield break;
            }
            enemy.PlayCard(enemy.hand[0],enemyBoard);
            enemyLifeBar.SetBar(enemy.cardInGame);
        }

        if (player.cardInGame.health <= 0)
        {
            player.RemoveCardFromBoard(player.cardInGame);
            state = FightState.PLAYERDEATH;
            yield return new WaitForSeconds(0.5f);
            if (player.hand.Count == 0)
            {
                state = FightState.LOST;
                dialogText.text = "Loser you are!";
                yield break;
            }
            else
            {
                dialogText.text = "Choose new fighter";
            }
            yield return new WaitUntil(() => state == FightState.TURN);
        }
        
        

        Turn();

    }

    public void PlaceCardInBattleArea(GameObject cardObject)
    {
        // Získajte referenciu na komponenty Kard a Player
        Kard kard = cardObject.GetComponent<Kard>();
        DragKard dragKard = cardObject.GetComponent<DragKard>();

        // Vykonajte akciu pre premiestnenie karty do bojového priestoru
        if (state == FightState.PLAYERDEATH || state == FightState.START)
        {
            player.PlayCard(kard, playerBoard);
            player.cardInGame.isDragable = false;
            state = FightState.TURN;
            playerLifeBar.SetBar(player.cardInGame);
            
        }
        else
        {
            // Vráťte kartu do ruky, ak nie je splnená podmienka
            player.ReturnCardToHand(kard, dragKard.originalHandPosition);
        }
    }

    private IEnumerator VytvorKartyZBalicka(GameObject playerGO, Player player)
    {
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "SELECT * FROM PlayerDecks WHERE DeckID = 1";
        IDataReader reader = dbCommand.ExecuteReader();

        List<int> cardIDs = new List<int>();
        if (reader.Read())
        {
            for (int i = 0; i < 5; i++)
            {
                cardIDs.Add(reader.GetInt32(i + 2));
            }
        }
        reader.Close();
        dbCommand.Dispose();

        foreach (int cardID in cardIDs)
        {
            dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText = $"SELECT * FROM PlayerCards WHERE CardID = {cardID}";
            reader = dbCommand.ExecuteReader();

            if (reader.Read())
            {
                string[] farbaKarty = reader.GetString(13).Split(';');
                Color32 cardColor = new Color32(byte.Parse(farbaKarty[0]), byte.Parse(farbaKarty[1]), byte.Parse(farbaKarty[2]), 255);

                GameObject novaKarta = Instantiate(kartaPrefab, playerGO.transform);
                novaKarta.GetComponent<Kard>().cardId = reader.GetInt32(0);
                novaKarta.GetComponent<Kard>().cardName = reader.GetString(2);
                novaKarta.GetComponent<Kard>().level = reader.GetInt32(3);
                novaKarta.GetComponent<Kard>().experience = reader.GetInt32(4);
                novaKarta.GetComponent<Kard>().health = reader.GetInt32(5);
                novaKarta.GetComponent<Kard>().strength = reader.GetInt32(6);
                novaKarta.GetComponent<Kard>().speed = reader.GetInt32(7);
                novaKarta.GetComponent<Kard>().attack = reader.GetInt32(8);
                novaKarta.GetComponent<Kard>().defense = reader.GetInt32(9);
                novaKarta.GetComponent<Kard>().knowledge = reader.GetInt32(10);
                novaKarta.GetComponent<Kard>().charisma = reader.GetInt32(11);
                novaKarta.GetComponent<Kard>().color = cardColor;
                novaKarta.GetComponent<Kard>().attack1 = reader.GetInt32(14);
                novaKarta.GetComponent<Kard>().countAttack1 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(), reader.GetInt32(14));
                novaKarta.GetComponent<Kard>().attack2 = reader.GetInt32(15);
                novaKarta.GetComponent<Kard>().countAttack2 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(), reader.GetInt32(15));
                novaKarta.GetComponent<Kard>().attack3 = reader.GetInt32(16);
                novaKarta.GetComponent<Kard>().countAttack3 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(), reader.GetInt32(16));
                novaKarta.GetComponent<Kard>().attack4 = reader.GetInt32(17);
                novaKarta.GetComponent<Kard>().countAttack4 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(), reader.GetInt32(17));
                novaKarta.GetComponent<Kard>().image = reader.GetString(18);

                novaKarta.GetComponent<Kard>().battleArea = playerBoard;

                player.AddCardToHand(novaKarta.GetComponent<Kard>());

                yield return new WaitForSeconds(0.1f);
            }
            reader.Close();
            dbCommand.Dispose();
        }

        dbConnection.Close();
    }


    private IEnumerator VytvorJedinecneKarty(int pocet, GameObject playerGO, Player player)
    {
        List<int> usedIndexes = new List<int>();
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "SELECT * FROM PlayerCards";
        IDataReader reader = dbCommand.ExecuteReader();

        List<string[]> kartyHrac = new List<string[]>();
        while (reader.Read())
        {
            string[] rowValues = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
            {
                rowValues[i] = reader[i].ToString();
            }
            kartyHrac.Add(rowValues);
        }

        reader.Close();
        dbCommand.Dispose();
        dbConnection.Close();

        int maxIndex = kartyHrac.Count;

        for (int i = 0; i < pocet; i++)
        {
            int index;
            do
            {
                index = Random.Range(0, maxIndex);
            } while (usedIndexes.Contains(index));

            usedIndexes.Add(index);

            string[] kartaHodnoty = kartyHrac[index];

            string[] farbaKarty = kartaHodnoty[13].Split(';');
            Color32 cardColor = new Color32(byte.Parse(farbaKarty[0]), byte.Parse(farbaKarty[1]), byte.Parse(farbaKarty[2]), 255);

            GameObject novaKarta = Instantiate(kartaPrefab, playerGO.transform);
            novaKarta.GetComponent<Kard>().cardId = int.Parse(kartaHodnoty[0]);
            novaKarta.GetComponent<Kard>().cardName = kartaHodnoty[2];
            novaKarta.GetComponent<Kard>().level = int.Parse(kartaHodnoty[3]);
            novaKarta.GetComponent<Kard>().experience = int.Parse(kartaHodnoty[4]);
            novaKarta.GetComponent<Kard>().health = int.Parse(kartaHodnoty[5]);
            novaKarta.GetComponent<Kard>().strength = int.Parse(kartaHodnoty[6]);
            novaKarta.GetComponent<Kard>().speed = int.Parse(kartaHodnoty[7]);
            novaKarta.GetComponent<Kard>().attack = int.Parse(kartaHodnoty[8]);
            novaKarta.GetComponent<Kard>().defense = int.Parse(kartaHodnoty[9]);
            novaKarta.GetComponent<Kard>().knowledge = int.Parse(kartaHodnoty[10]);
            novaKarta.GetComponent<Kard>().charisma = int.Parse(kartaHodnoty[11]);
            novaKarta.GetComponent<Kard>().color = cardColor;
            novaKarta.GetComponent<Kard>().attack1 = int.Parse(kartaHodnoty[14]);
            novaKarta.GetComponent<Kard>().countAttack1 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(), int.Parse(kartaHodnoty[14]));
            novaKarta.GetComponent<Kard>().attack2 = int.Parse(kartaHodnoty[15]);
            novaKarta.GetComponent<Kard>().countAttack2 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(), int.Parse(kartaHodnoty[15]));
            novaKarta.GetComponent<Kard>().attack3 = int.Parse(kartaHodnoty[16]);
            novaKarta.GetComponent<Kard>().countAttack3 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(), int.Parse(kartaHodnoty[16]));
            novaKarta.GetComponent<Kard>().attack4 = int.Parse(kartaHodnoty[17]);
            novaKarta.GetComponent<Kard>().countAttack4 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(), int.Parse(kartaHodnoty[17]));
            novaKarta.GetComponent<Kard>().image = kartaHodnoty[18];

            novaKarta.GetComponent<Kard>().battleArea = playerBoard;

            player.AddCardToHand(novaKarta.GetComponent<Kard>());

            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator VytvorKartyAIMission(GameObject playerGO, Player player, int missionID)
    {
        int[] boost = new int[6];
        int iter = missionID - 1;
        
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = $"SELECT Card1ID, Card2ID, Card3ID, Card4ID, Card5ID FROM Missions WHERE MissionID = {missionID}";
        IDataReader reader = dbCommand.ExecuteReader();

        List<int> cardIDs = new List<int>();
        while (reader.Read())
        {
            for (int i = 0; i < 5; i++)
            {
                cardIDs.Add(reader.GetInt32(i));
            }
        }
        reader.Close();
        dbCommand.Dispose();

        // Načítanie kariet podľa ich ID
        foreach (int cardID in cardIDs)
        {
            dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText = $"SELECT * FROM CardDatabase WHERE StyleID = {cardID}";
            reader = dbCommand.ExecuteReader();

            while (reader.Read())
            {
                boost = DistributeRandomly(iter);
                string[] kartaHodnoty = new string[reader.FieldCount];

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    kartaHodnoty[i] = reader[i].ToString();
                }

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
                novaKarta.GetComponent<Kard>().image = kartaHodnoty[15];
                novaKarta.GetComponent<Kard>().color = cardColor;
                novaKarta.GetComponent<Kard>().level = 1  + iter;
                novaKarta.GetComponent<Kard>().attack1 = int.Parse(kartaHodnoty[11]);
                novaKarta.GetComponent<Kard>().countAttack1 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(),int.Parse(kartaHodnoty[11]));
                novaKarta.GetComponent<Kard>().attack2 = int.Parse(kartaHodnoty[12]);
                novaKarta.GetComponent<Kard>().countAttack2 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(),int.Parse(kartaHodnoty[12]));
                novaKarta.GetComponent<Kard>().attack3 = int.Parse(kartaHodnoty[13]);
                novaKarta.GetComponent<Kard>().countAttack3 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(),int.Parse(kartaHodnoty[13]));
                novaKarta.GetComponent<Kard>().attack4 = int.Parse(kartaHodnoty[14]);
                novaKarta.GetComponent<Kard>().countAttack4 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(),int.Parse(kartaHodnoty[14]));

                novaKarta.GetComponent<Kard>().isDragable = false;
                novaKarta.GetComponent<Kard>().battleArea = enemyBoard;

                player.AddCardToHand(novaKarta.GetComponent<Kard>());
            }

            reader.Close();
            dbCommand.Dispose();
        }

        dbConnection.Close();

        yield return new WaitForSeconds(0.1f);
    }



    private IEnumerator VytvorKartyAI(GameObject playerGO, Player player)
    {
        int iter = 0;
        int[] boost = new int[6];

        // Load cards from the database
        List<string> kartyData = LoadCardsFromDatabase();

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
            novaKarta.GetComponent<Kard>().image = kartaHodnoty[15];
            novaKarta.GetComponent<Kard>().color = cardColor;
            novaKarta.GetComponent<Kard>().level = 1 + iter;
            novaKarta.GetComponent<Kard>().attack1 = int.Parse(kartaHodnoty[11]);
            novaKarta.GetComponent<Kard>().countAttack1 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(),int.Parse(kartaHodnoty[11]));
            novaKarta.GetComponent<Kard>().attack2 = int.Parse(kartaHodnoty[12]);
            novaKarta.GetComponent<Kard>().countAttack2 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(),int.Parse(kartaHodnoty[12]));
            novaKarta.GetComponent<Kard>().attack3 = int.Parse(kartaHodnoty[13]);
            novaKarta.GetComponent<Kard>().countAttack3 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(),int.Parse(kartaHodnoty[13]));
            novaKarta.GetComponent<Kard>().attack4 = int.Parse(kartaHodnoty[14]);
            novaKarta.GetComponent<Kard>().countAttack4 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(),int.Parse(kartaHodnoty[14]));

            novaKarta.GetComponent<Kard>().isDragable = false;
            novaKarta.GetComponent<Kard>().battleArea = enemyBoard;

            player.AddCardToHand(novaKarta.GetComponent<Kard>());

            iter += 1;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private List<string> LoadCardsFromDatabase()
    {
        List<string> kartyData = new List<string>();

        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "SELECT * FROM CardDatabase";
        IDataReader reader = dbCommand.ExecuteReader();

        while (reader.Read())
        {
            string cardDataString = "";
            for (int i = 0; i < reader.FieldCount; i++)
            {
                cardDataString += reader.GetValue(i).ToString();
                if (i < reader.FieldCount - 1) cardDataString += ",";
            }
            kartyData.Add(cardDataString);
        }

        reader.Close();
        dbCommand.Dispose();
        dbConnection.Close();

        return kartyData;
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