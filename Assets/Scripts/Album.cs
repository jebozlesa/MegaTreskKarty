using UnityEngine;
using UnityEngine.UI;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;

public class Album : MonoBehaviour
{

    public GameObject kartaPrefab;
    public GameObject album;
    public GameObject zoomedCardHolder;
    public AttackDescriptions attackDescriptions;
    public GameObject content;

    private string connectionString;

    public GameObject deckPanel;

    void Start()
    {   Debug.Log("Album.Start  -- Start");

        connectionString = $"URI=file:{Database.Instance.GetDatabasePath()}";
        LoginPlayFab();

        deckPanel.SetActive(false);

        //StartCoroutine(VytvorKarty());
    }

    void LoginPlayFab()
    {   Debug.Log("Album.LoginPlayFab  -- Start");

//        loadingImage.SetActive(true);
        string username = PlayerPrefs.GetString("username");
        string email = PlayerPrefs.GetString("email");
        string password = PlayerPrefs.GetString("password");

        var request = new LoginWithEmailAddressRequest 
            {
                Email = email,
                Password = password
            };
            PlayFabClientAPI.LoginWithEmailAddress(request, OnSuccess, OnError);
//        loadingImage.SetActive(false);
    }

    void OnSuccess(LoginResult result)
    {   Debug.Log("Album.OnSuccess  -- Start");

        Debug.Log("Sicko dobre");
  //      loadingImage.SetActive(false);
        StartCoroutine(VytvorKartyPlayFab());
    }

    void OnError(PlayFabError error)
    {   Debug.Log("Album.OnError  -- Start");

        Debug.Log("Daco nahovno");
  //      errorImage.SetActive(true);
        Debug.Log(error.GenerateErrorReport());
    }

    string LoadStory(int cardID)
    {   Debug.Log("Album.LoadStory  -- Start");

        string cardStory = "";
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "SELECT * FROM CardStories WHERE StyleID = " + cardID;
        IDataReader reader = dbCommand.ExecuteReader();

        if (reader.Read())
        {
            cardStory = reader.GetString(2);
        }

        reader.Close();
        dbCommand.Dispose();
        dbConnection.Close();

        return cardStory;
    }

    private IEnumerator VytvorKarty()
    {Debug.Log("Album.VytvorKarty  -- Start");

        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "SELECT * FROM PlayerCards";
        IDataReader reader = dbCommand.ExecuteReader();

        while (reader.Read())
        {
            GameObject novaKarta = Instantiate(kartaPrefab, transform);

            novaKarta.GetComponent<Card>().cardId = reader.GetInt32(0);
            novaKarta.GetComponent<Card>().styleId = reader.GetInt32(1);
            novaKarta.GetComponent<Card>().level = reader.GetInt32(3);
            novaKarta.GetComponent<Card>().experience = reader.GetInt32(4);
            novaKarta.GetComponent<Card>().cardName = reader.GetString(2);
            novaKarta.GetComponent<Card>().health = reader.GetInt32(5);
            novaKarta.GetComponent<Card>().strength = reader.GetInt32(6);
            novaKarta.GetComponent<Card>().speed = reader.GetInt32(7);
            novaKarta.GetComponent<Card>().attack = reader.GetInt32(8);
            novaKarta.GetComponent<Card>().defense = reader.GetInt32(9);
            novaKarta.GetComponent<Card>().knowledge = reader.GetInt32(10);
            novaKarta.GetComponent<Card>().charisma = reader.GetInt32(11);
            
            novaKarta.GetComponent<Card>().image = reader.GetString(18);

            string[] farbaKarty = reader.GetString(13).Split(';');
            Color32 cardColor = new Color32(byte.Parse(farbaKarty[0]), byte.Parse(farbaKarty[1]), byte.Parse(farbaKarty[2]), 255);
            novaKarta.GetComponent<Card>().color = cardColor;

            // novaKarta.GetComponent<Card>().attack1 = reader.GetInt32(14);
            // novaKarta.GetComponent<Card>().attack2 = reader.GetInt32(15);
            // novaKarta.GetComponent<Card>().attack3 = reader.GetInt32(16);
            // novaKarta.GetComponent<Card>().attack4 = reader.GetInt32(17);

            novaKarta.GetComponent<Card>().attack1 = reader.GetInt32(14);
            novaKarta.GetComponent<Card>().countAttack1 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Card>(), reader.GetInt32(14));
            novaKarta.GetComponent<Card>().attack2 = reader.GetInt32(15);
            novaKarta.GetComponent<Card>().countAttack2 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Card>(), reader.GetInt32(15));
            novaKarta.GetComponent<Card>().attack3 = reader.GetInt32(16);
            novaKarta.GetComponent<Card>().countAttack3 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Card>(), reader.GetInt32(16));
            novaKarta.GetComponent<Card>().attack4 = reader.GetInt32(17);
            novaKarta.GetComponent<Card>().countAttack4 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Card>(), reader.GetInt32(17));


            novaKarta.GetComponent<Card>().story = LoadStory(reader.GetInt32(1));

            novaKarta.GetComponent<Card>().zoomedCardHolder = zoomedCardHolder;

            novaKarta.GetComponent<Card>().transform.SetParent(content.transform);
            novaKarta.GetComponent<Card>().transform.localScale = Vector3.one;

            novaKarta.GetComponent<Card>().Initialize(deckPanel);

            yield return new WaitForSeconds(0.05f);
        }

        reader.Close();
        dbCommand.Dispose();
        dbConnection.Close();
    }

    private IEnumerator VytvorKartyPlayFab()
    {
        // Získanie údajov z PlayFab
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            if (result.Data.ContainsKey("PlayerCards"))
            {
                Debug.Log("PlayerCards data: " + result.Data["PlayerCards"].Value); // Pridaný výpis

                // Prevod údajov z formátu JSON do objektov PlayerCardsData
                PlayerCardsData data = JsonUtility.FromJson<PlayerCardsData>(result.Data["PlayerCards"].Value);

                // Spracovanie údajov o kartách
                StartCoroutine(SpracujKarty(data.cards));
            }
        }, error => Debug.LogError(error.GenerateErrorReport()));

        yield return null;
    }


    private IEnumerator SpracujKarty(List<GeneratedCard> data)
    {Debug.Log("Album.SpracujKarty  -- Start");

        Debug.Log("Number of cards: " + data.Count); // Pridaný výpis

        foreach (GeneratedCard cardData in data)
        {

            Debug.Log("Hero : " + cardData.PersonName);

            GameObject novaKarta = Instantiate(kartaPrefab, transform);
            novaKarta.GetComponent<Card>().cardId = cardData.StyleID;
            novaKarta.GetComponent<Card>().styleId = cardData.StyleID;
            novaKarta.GetComponent<Card>().level = cardData.Level;
            novaKarta.GetComponent<Card>().experience = cardData.Experience;
            novaKarta.GetComponent<Card>().cardName = cardData.PersonName;
            novaKarta.GetComponent<Card>().health = cardData.Health;
            novaKarta.GetComponent<Card>().strength = cardData.Strength;
            novaKarta.GetComponent<Card>().speed = cardData.Speed;
            novaKarta.GetComponent<Card>().attack = cardData.Attack;
            novaKarta.GetComponent<Card>().defense = cardData.Defense;
            novaKarta.GetComponent<Card>().knowledge = cardData.Knowledge;
            novaKarta.GetComponent<Card>().charisma = cardData.Charisma;
            novaKarta.GetComponent<Card>().image = cardData.CardPicture;
            Color32 cardColor = new Color32((byte)cardData.Color[0], (byte)cardData.Color[1], (byte)cardData.Color[2], 255);
            novaKarta.GetComponent<Card>().color = cardColor;
            novaKarta.GetComponent<Card>().attack1 = cardData.Attack1;
            novaKarta.GetComponent<Card>().countAttack1 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Card>(), cardData.Attack1);
            novaKarta.GetComponent<Card>().attack2 = cardData.Attack2;
            novaKarta.GetComponent<Card>().countAttack2 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Card>(), cardData.Attack2);
            novaKarta.GetComponent<Card>().attack3 = cardData.Attack3;
            novaKarta.GetComponent<Card>().countAttack3 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Card>(), cardData.Attack3);
            novaKarta.GetComponent<Card>().attack4 = cardData.Attack4;
            novaKarta.GetComponent<Card>().countAttack4 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Card>(), cardData.Attack4);
            novaKarta.GetComponent<Card>().story = LoadStory(cardData.StyleID);
            novaKarta.GetComponent<Card>().zoomedCardHolder = zoomedCardHolder;
            novaKarta.GetComponent<Card>().transform.SetParent(content.transform);
            novaKarta.GetComponent<Card>().transform.localScale = Vector3.one;
            novaKarta.GetComponent<Card>().Initialize(deckPanel);

            yield return new WaitForSeconds(0.05f);
        }
    }
        

}
