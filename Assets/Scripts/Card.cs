using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Mono.Data.Sqlite;
using System.Data;
using PlayFab;
using PlayFab.ClientModels;

public class Card : MonoBehaviour, IAttackCount, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int cardId;
    public int styleId;
    public string cardName;
    public int experience;
    // public int health;
    // public int strength;
    // public int speed;
    // public int attack;
    // public int defense;
    // public int knowledge;
    // public int charisma;
    public string image;
    public Color32 color;
    public int level;

    public int health { get; set; }
    public int strength { get; set; }
    public int speed { get; set; }
    public int attack { get; set; }
    public int defense { get; set; }
    public int knowledge { get; set; }
    public int charisma { get; set; }

    int maxHP;

    public string story;

    public Dictionary<int, int> attackCount;

    public int attack1;
    public int attack2;
    public int attack3;
    public int attack4;

    public int countAttack1;
    public int countAttack2;
    public int countAttack3;
    public int countAttack4;

    public TMP_Text nameText;
    public TMP_Text levelText;

    public Image cardImage;
    public Image cardImageAttr;
    public Image cardImageDesc;
    public Image cardImageAttack;
    public Sprite cardSprite;

    public Image background;

    public GameObject cardPrefab;

   // public CardState state = CardState.ATTACK;
    private bool isZoomed = false;
    public GameObject zoomedCardHolder;
    private GameObject originalParent;

    private static Card currentZoomedCard = null;

    private int originalSiblingIndex;

    private Vector2 pointerDownPosition;
    private Vector2 pointerUpPosition;
    private float swipeDistanceThreshold = 50f;

    private ScrollRect parentScrollRect;
    private bool isDragging;
    private bool dragInProgress;
    private Vector2 pointerDragStartPosition;

    public GameObject frontSide;
    public GameObject backSideAttributes;
    public GameObject backSideDescription;
    public GameObject backSideAttack;

    public TMP_Text nameTextAttr;
    public TMP_Text expText;
    public TMP_Text hpText;
    public TMP_Text strText;
    public TMP_Text speText;
    public TMP_Text attText;
    public TMP_Text defText;
    public TMP_Text knoText;
    public TMP_Text chaText;
    
    public TMP_Text storyText;

    public GameObject notsureGO;//tie kokotiny co vyskakuju ked dostane damage
    public TMP_Text notsureText;

    Color32 color_green = new Color32(0, 255, 0, 255);
    Color32 color_red = new Color32(255, 0, 0, 255);

    public GameObject deckPanel;
    public bool deckCard;
    public DeckManager deckManager;

    public string connectionString;

    public TMP_Text attNameText;
    public TMP_Text attDescriptionText;
    public TMP_Text attAttributesText;
    public TMP_Text attSpecialText;

    public Image changeButtonImg;
    public TMP_Text changeButtonText;

    public int currentAttackIndex = 1;

    public GameObject attackPrefab;
    public Transform attackListContainer;
    public Button changeButton;

    public AttackListController attackListController;
    private int displayedAttack;

    private int selectedAttackId = -1;
    private int selectedOriginalAttackId = -1;

    string PlayFabId;

    //public GameObject tutorial;

    GameObject cardTutorialObject;


    void Start()
    {
        //LoginPlayFab();

        connectionString = $"URI=file:{Database.Instance.GetDatabasePath()}";
        backSideAttributes.SetActive(false);
        backSideDescription.SetActive(false);

        parentScrollRect = GetComponentInParent<ScrollRect>();

        nameText.text = cardName;
        levelText.text = "lvl " + level;

        maxHP = health; 

        cardImage.sprite = Resources.Load<Sprite>(image);
        cardImageAttr.sprite = Resources.Load<Sprite>(image + "_back");
        cardImageDesc.sprite = Resources.Load<Sprite>(image + "_back");
        cardImageAttack.sprite = Resources.Load<Sprite>(image + "_back");

        background.GetComponent<Image>().color = color;
        nameText.color = color;
        levelText.color = color;

        LoadDetails();

        changeButton.onClick.AddListener(ShowAttackList);
        //attackListContainer.gameObject.SetActive(false);

        //cardTutorialObject = GameObject.Find("CardTutorial"); // Nájdenie objektu s názvom "CardTutorial" na scéne
    }

    public void Initialize(GameObject deckPanelReference)
    {
        deckPanel = deckPanelReference;
    }

    public void LoadDetails()
    {
        nameTextAttr.text = cardName;
        expText.text = "Experience: " + experience + " / " + level * (10 * level);    //LEVEL   <========== dat potom nadruhu level
        hpText.text = "Health: " + health;
        strText.text = "Strength: " + strength;
        speText.text = "Speed: " + speed;
        attText.text = "Attack: " + attack;
        defText.text = "Defense: " + defense;
        knoText.text = "Knowledge: " + knowledge;
        chaText.text = "Charisma: " + charisma;

        storyText.text = story;

        nameTextAttr.color = color;
        expText.color = color;
        hpText.color = color;
        strText.color = color;
        speText.color = color;
        attText.color = color;
        defText.color = color;
        knoText.color = color;
        chaText.color = color;

        storyText.color = color;

        attNameText.color = color;
        attDescriptionText.color = color;
        attAttributesText.color = color;
        attSpecialText.color = color;

        changeButtonImg.color = ChangeAlpha(color, 80);
        changeButtonText.color = color;

    }

    public bool ContainsAttack(int attackId)
    {
        return attack1 == attackId || attack2 == attackId || attack3 == attackId || attack4 == attackId;
    }

    public void DeselectAllAttacks() 
    {
        foreach (AviableAttack attack in GetComponentsInChildren<AviableAttack>()) 
        {
            attack.SetSelected(false);
        }
    }

    public void OnChangeAttackClick()
    {
        if (selectedAttackId != -1 && selectedOriginalAttackId != -1)
        {
            ChangeAttackInDatabase(selectedAttackId, selectedOriginalAttackId, cardId);
            selectedAttackId = -1;
            selectedOriginalAttackId = -1;
            attackListContainer.gameObject.SetActive(false);
        }
    }

    public void SelectAttack(int newAttackId, int originalAttackId)
    {
        selectedAttackId = newAttackId;
        selectedOriginalAttackId = originalAttackId;
    }

    public void ChangeAttackInDatabase(int newAttackId, int originalAttackId, int cardId)
    {
        string connectionString = $"URI=file:{Database.Instance.GetDatabasePath()}";
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        Debug.Log($"ChangeAttackInDatabase: {newAttackId} -> {originalAttackId}");

        // Nájdite stĺpec, ktorý treba aktualizovať
        string columnName = "";
        for (int i = 1; i <= 4; i++)
        {
            IDbCommand dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText = $"SELECT Attack{i} FROM PlayerCards WHERE CardID = {cardId}";
            IDataReader reader = dbCommand.ExecuteReader();
            if (reader.Read() && reader.GetInt32(0) == originalAttackId)
            {
                columnName = $"Attack{i}";
                reader.Close();
                dbCommand.Dispose();
                break;
            }
            reader.Close();
            dbCommand.Dispose();
        }

        // Aktualizujte stĺpec s novým ID útoku
        if (!string.IsNullOrEmpty(columnName))
        {
            IDbCommand dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText = $"UPDATE PlayerCards SET {columnName} = {newAttackId} WHERE CardID = {cardId}";
            int rowsAffected = dbCommand.ExecuteNonQuery();
            Debug.Log($"Rows affected: {rowsAffected}");
            dbCommand.Dispose();

            // Po úspešnej zmene útoku v databáze aktualizujte útok na karte
            switch (columnName)
            {
                case "Attack1":
                    attack1 = newAttackId;
                    if (currentAttackIndex == 1)
                        LoadAttackData(newAttackId);
                    break;
                case "Attack2":
                    attack2 = newAttackId;
                    if (currentAttackIndex == 2)
                        LoadAttackData(newAttackId);
                    break;
                case "Attack3":
                    attack3 = newAttackId;
                    if (currentAttackIndex == 3)
                        LoadAttackData(newAttackId);
                    break;
                case "Attack4":
                    attack4 = newAttackId;
                    if (currentAttackIndex == 4)
                        LoadAttackData(newAttackId);
                    break;
            }
        }
        else
        {
            Debug.LogWarning("Could not find the column to update.");
        }

        dbConnection.Close();
    }



    public void ShowAttackList()
    {
        attackListController.ClearAttackList(); // Vymaže všetky predchádzajúce útoky
        Debug.Log("CHange kliknuty ");
        attackListController.ShowAttackList(styleId,displayedAttack);
        attackListContainer.gameObject.SetActive(true);
    }

    public void HideAttackList()
    {
        attackListContainer.gameObject.SetActive(false);
    }

    public void ChangeAttack(int direction)
    {
        
        if (direction == 1)
        {
            currentAttackIndex++;
            if (currentAttackIndex > 4)
            {
                currentAttackIndex = 1;
            }
        }
        else if (direction == -1)
        {
            currentAttackIndex--;
            if (currentAttackIndex < 1)
            {
                currentAttackIndex = 4;
            }
        }

        switch (currentAttackIndex)
        {
            case 1:
                LoadAttackData(attack1);
                displayedAttack = attack1;
                break;
            case 2:
                LoadAttackData(attack2);
                displayedAttack = attack2;
                break;
            case 3:
                LoadAttackData(attack3);
                displayedAttack = attack3;
                break;
            case 4:
                LoadAttackData(attack4);
                displayedAttack = attack4;
                break;
        }
    }

    public void LoadAttackData(int attackID)
    {
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = $"SELECT * FROM Attacks WHERE AttackID = {attackID}";
                using (IDataReader reader = dbCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        attNameText.text = reader["AttackName"].ToString();
                        attDescriptionText.text = "Description: " + reader["Description"].ToString();
                        attAttributesText.text = "Attributes: " + reader["Attributes"].ToString();
                        attSpecialText.text = "Special: " + reader["Special"].ToString();
                    }
                    else
                    {
                        Debug.LogError($"No attack data found for AttackID: {attackID}");
                    }
                }
            }
        }
    }


    public void OnClick()
    {
        if (deckCard)
        {
            if (!isZoomed && currentZoomedCard != null)
            {
                if (cardId == currentZoomedCard.cardId)
                {
                    Debug.LogWarning("Card with the same name already exists in the deck.");
                    return;
                }
                if (deckManager.IsCardNameInDeck(currentZoomedCard.cardName) && cardName != currentZoomedCard.cardName)
                {
                    Debug.LogWarning("Card with the same name already exists in the deck.");
                    return;
                }
                SwapCardsInPlayFab(currentZoomedCard.cardId, cardId);
                deckManager.AddCardToHand(currentZoomedCard.cardId);
                if (!currentZoomedCard.deckCard)
                {
                    currentZoomedCard.ZoomOut();
                }
                else
                {
                    Destroy(currentZoomedCard.gameObject);
                }
                ZoomIn();
            }
            else if (isZoomed)
            {
                Destroy(this.gameObject);
                deckPanel.SetActive(false);
            }
        }
    }


    private void SwapCardsInPlayFab(int newCardId, int oldCardId)
    {
        // Získame existujúce údaje o balíčkoch z PlayFab
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            string existingDataJson = GetExistingDeckDataJson(result);
            Dictionary<string, Deck> existingDecks = ConvertJsonToDeckData(existingDataJson);

            // Prejdeme všetky balíčky a zmeníme starú kartu za novú
            foreach (Deck deck in existingDecks.Values)
            {
                if (deck.Card1 == oldCardId) deck.Card1 = newCardId;
                else if (deck.Card2 == oldCardId) deck.Card2 = newCardId;
                else if (deck.Card3 == oldCardId) deck.Card3 = newCardId;
                else if (deck.Card4 == oldCardId) deck.Card4 = newCardId;
                else if (deck.Card5 == oldCardId) deck.Card5 = newCardId;
            }

            // Konvertujeme upravené balíčky späť na JSON a aktualizujeme údaje v PlayFab
            string updatedJson = ConvertDeckDataToJson(existingDecks);
            UpdateDeckDataInPlayFab(updatedJson);
        }, error => Debug.LogError(error.GenerateErrorReport()));
    }

    private string GetExistingDeckDataJson(GetUserDataResult result)
    {
        if (result.Data.ContainsKey("PlayerDecks"))
        {
            return result.Data["PlayerDecks"].Value;
        }
        return "{}";
    }

    private Dictionary<string, Deck> ConvertJsonToDeckData(string existingDataJson)
    {
        Dictionary<string, Deck> data = new Dictionary<string, Deck>();
        if (!string.IsNullOrEmpty(existingDataJson))
        {
            DeckListWrapper existingDecks = JsonUtility.FromJson<DeckListWrapper>(existingDataJson);
            foreach (Deck existingDeck in existingDecks.Decks)
            {
                data.Add(existingDeck.DeckID, existingDeck);
            }
        }
        return data;
    }

    private string ConvertDeckDataToJson(Dictionary<string, Deck> data)
    {
        DeckListWrapper updatedDecks = new DeckListWrapper
        {
            Decks = new List<Deck>(data.Values)
        };
        return JsonUtility.ToJson(updatedDecks);
    }

    private void UpdateDeckDataInPlayFab(string updatedJson)
    {   Debug.Log("UpdateDeckDataInPlayFab ==> Start");

        var updateRequest = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "PlayerDecks", updatedJson }
            }
        };
        Debug.Log("Updated JSON deck: " + updatedJson);
        PlayFabClientAPI.UpdateUserData(updateRequest, updateResult => Debug.Log("User deck data updated successfully"), error => Debug.LogError(error.GenerateErrorReport()));
    }



    void OnDataUpdated(UpdateUserDataResult result)
    {
        Debug.Log("User data updated successfully");
    }

    void OnUpdateError(PlayFabError error)
    {
        Debug.LogError("Error updating user data: " + error.GenerateErrorReport());
    }




    private void SwapCardsInDatabase(int newCardId, int oldCardId)  //POTOM VYMAZAT
    {
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = $"UPDATE PlayerDecks SET Card1 = (CASE WHEN Card1 = {oldCardId} THEN {newCardId} ELSE Card1 END), " +
                                            $"Card2 = (CASE WHEN Card2 = {oldCardId} THEN {newCardId} ELSE Card2 END), " +
                                            $"Card3 = (CASE WHEN Card3 = {oldCardId} THEN {newCardId} ELSE Card3 END), " +
                                            $"Card4 = (CASE WHEN Card4 = {oldCardId} THEN {newCardId} ELSE Card4 END), " +
                                            $"Card5 = (CASE WHEN Card5 = {oldCardId} THEN {newCardId} ELSE Card5 END) " +
                                $"WHERE DeckID = 1";
        dbCommand.ExecuteNonQuery();

        int rowsAffected = dbCommand.ExecuteNonQuery();
        Debug.Log("Rows affected: " + rowsAffected);

        dbCommand.Dispose();
        dbConnection.Close();
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isZoomed && !deckCard)
        {
            pointerDragStartPosition = eventData.position;
            isDragging = true;
            parentScrollRect.OnBeginDrag(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isZoomed && isDragging && !deckCard)
        {
            dragInProgress = true;
            parentScrollRect.OnDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isZoomed && !deckCard)
        {
            isDragging = false;
            parentScrollRect.OnEndDrag(eventData);

            float dragDistance = Vector2.Distance(pointerDragStartPosition, eventData.position);
        }
    }

    public void OnSwipeRight()
    {
        Debug.Log("Swipe Right");
        if (isZoomed)
        {
            if (frontSide.activeSelf)
            {
                frontSide.SetActive(false);
                backSideDescription.SetActive(true);
            }
            else if (backSideDescription.activeSelf)
            {
                backSideAttributes.SetActive(true);
                backSideDescription.SetActive(false);
            }
            else if (backSideAttributes.activeSelf)
            {
                frontSide.SetActive(true);
                backSideAttributes.SetActive(false);
            }
            if (backSideAttack.activeSelf)
            {
                ChangeAttack(-1); // Zmena útoku pri swipovaní doľava
            }
        }
    }

    public void OnSwipeLeft()
    {
        Debug.Log("Swipe Left");
        if (isZoomed)
        {
            if (frontSide.activeSelf)
            {
                frontSide.SetActive(false);
                backSideAttributes.SetActive(true);
            }
            else if (backSideAttributes.activeSelf)
            {
                backSideAttributes.SetActive(false);
                backSideDescription.SetActive(true);
            }
            else if (backSideDescription.activeSelf)
            {
                frontSide.SetActive(true);
                backSideDescription.SetActive(false);
            }
            if (backSideAttack.activeSelf)
            {
                ChangeAttack(1); // Zmena útoku pri swipovaní doprava
            }
        }
    }


    public void OnSwipeUp()
    {
        if (isZoomed)
        {
            LoadAttackData(attack1); // Načítajte údaje o prvom útoku hráča podľa ID útoku
            displayedAttack = attack1;
            backSideAttack.SetActive(true);
            frontSide.SetActive(false);
            backSideAttributes.SetActive(false);
            backSideDescription.SetActive(false);
        }
    }

    public void OnSwipeDown()
    {
        if (isZoomed)
        {
            backSideAttack.SetActive(false);
            frontSide.SetActive(true);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDownPosition = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerUpPosition = eventData.position;
        float distance = Vector2.Distance(pointerDownPosition, pointerUpPosition);

        if (distance > swipeDistanceThreshold && isZoomed)
        {
            Vector2 direction = pointerUpPosition - pointerDownPosition;

            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                // Horizontal swipe
                if (direction.x > 0)
                {
                    OnSwipeRight();
                }
                else
                {
                    OnSwipeLeft();
                }
            }
        }
        else if (!dragInProgress && !attackListContainer.gameObject.activeSelf)
        {
            if (deckCard) OnClick();
            else ToggleZoom();
        }

        dragInProgress = false;
    }

    public void ToggleZoom()
    {
        if (isZoomed)
        {
            ZoomOut();
        }
        else if (currentZoomedCard == null)
        {
            ZoomIn();
        }
    }

    public void ZoomIn()
    {
        Debug.Log("Card.ZoomIn() ===> START");
        if (!isZoomed)
        {
            originalParent = transform.parent.gameObject;
            originalSiblingIndex = transform.GetSiblingIndex();
            transform.SetParent(zoomedCardHolder.transform);
            transform.localPosition = Vector3.zero;
            transform.localScale = new Vector3(3f, 3f, 3f);
            transform.SetSiblingIndex(transform.parent.childCount - 1);
            currentZoomedCard = this;
            isZoomed = true;

            // Show the deck panel
            deckPanel.SetActive(true);

            if (PlayerPrefs.GetInt("HasCompletedTutorialCard", 0) == 0) 
            {
                CardTutorial.instance.gameObject.SetActive(true);
            }
        }
    }

    public void ZoomOut()
    {
        if (isZoomed)
        {
            transform.SetParent(originalParent.transform);
            transform.localScale = new Vector3(1f, 1f, 1f);
            transform.SetSiblingIndex(originalSiblingIndex);
            currentZoomedCard = null;
            isZoomed = false;
            frontSide.SetActive(true);
            backSideAttributes.SetActive(false);
            backSideDescription.SetActive(false);
            backSideAttack.SetActive(false);
            attackListContainer.gameObject.SetActive(false);

            // Hide the deck panel
            deckPanel.SetActive(false);
        }
    }

    public Color32 ChangeAlpha(Color32 inputColor, byte newAlpha)
    {
        return new Color32(inputColor.r, inputColor.g, inputColor.b, newAlpha);
    }

    void LoginPlayFab()
    {
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
    Debug.Log("Successfully logged in to PlayFab!");
    // Store the PlayFabId for later use
    PlayFabId = result.PlayFabId;
}

void OnError(PlayFabError error)
{
    Debug.LogError("Error logging in to PlayFab: " + error.GenerateErrorReport());
}



}