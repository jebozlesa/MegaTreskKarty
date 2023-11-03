using UnityEngine;
using UnityEngine.UI;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;
using System;
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

    public GameObject tutorial;

    public GameObject cardTutorial;

    private string connectionString;

    public GameObject deckPanel;

    public static bool IsLoggedIn = false;

    void Start()
    {
        Debug.Log("Album.Start  -- Start");

        if (PlayerPrefs.GetInt("HasCompletedTutorialAlbum", 0) == 0) { tutorial.SetActive(true); }

        connectionString = $"URI=file:{Database.Instance.GetDatabasePath()}";

        deckPanel.SetActive(false);

        if (PlayFabManagerLogin.IsLoggedIn)
        {
            StartCoroutine(VytvorKartyPlayFab());
        }
        else
        {
            Debug.LogError("Hráč nie je prihlásený!");
        }
    }

    void LoginPlayFab()
    {
        Debug.Log("Album.LoginPlayFab  -- Start");

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
    {
        Debug.Log("Album.OnSuccess  -- Start");

        IsLoggedIn = true;
        Debug.Log("Sicko dobre");
        //      loadingImage.SetActive(false);
    }

    void OnError(PlayFabError error)
    {
        Debug.Log("Album.OnError  -- Start");

        Debug.Log("Daco nahovno");
        //      errorImage.SetActive(true);
        Debug.Log(error.GenerateErrorReport());
    }

    string LoadStory(int cardID)
    {
        Debug.Log("Album.LoadStory  -- Start");

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



    private IEnumerator VytvorKartyPlayFab()
    {
        Debug.Log("MegaTresk: " + DateTime.Now.ToString("HH:mm:ss.fff") + " Album.VytvorKartyPlayFab => START ");
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
        },
        error =>
        {
            Debug.LogError(error.GenerateErrorReport());
            if (error.Error == PlayFabErrorCode.ConnectionError)
            {
                // Zobrazte chybové hlásenie alebo vráťte hráča do hlavného menu
            }
        });

        yield return null;
    }


    private IEnumerator SpracujKarty(List<GeneratedCard> data)
    {
        Debug.Log("Album.SpracujKarty  -- Start");

        Debug.Log("Number of cards: " + data.Count); // Pridaný výpis

        foreach (GeneratedCard cardData in data)
        {

            Debug.Log("Hero : " + cardData.PersonName);

            GameObject novaKarta = Instantiate(kartaPrefab, transform);
            novaKarta.GetComponent<Card>().cardId = cardData.CardID;
            novaKarta.GetComponent<Card>().styleId = cardData.StyleID;
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
