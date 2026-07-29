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
using System.Linq;
using System;

public class Card : MonoBehaviour, IAttackCount, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string cardId;
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
    public Image cardImageInfo;
    public Image cardImageAttack;
    public Sprite cardSprite;

    public Image background;

    public GameObject cardPrefab;

    // public CardState state = CardState.ATTACK;
    private bool isZoomed = false;
    public GameObject zoomedCardHolder;
    private GameObject originalParent;

    private static Card currentZoomedCard = null;
    public static bool IsAnyCardDetailOpen => currentZoomedCard != null;

    private int originalSiblingIndex;

    private Vector2 pointerDownPosition;
    private Vector2 pointerUpPosition;
    private float swipeDistanceThreshold = 50f;

    private ScrollRect parentScrollRect;
    private LibrarySwipeInput parentLibrarySwipeInput;
    private bool isDragging;
    private bool dragInProgress;
    private Vector2 pointerDragStartPosition;

    public GameObject frontSide;
    public GameObject backSideAttributes;
    public GameObject backSideDescription;
    public GameObject backSideAttack;
    public GameObject backSideInfo;

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

    public TMP_Text cardIdText;
    public TMP_Text seriesIdText;
    public TMP_Text styleIdText;
    public TMP_Text creationDateText;

    public Image recycleButtonImg;
    public TMP_Text recycleButtonText;

    Color32 color_green = new Color32(0, 255, 0, 255);
    Color32 color_red = new Color32(255, 0, 0, 255);

    public GameObject deckPanel;
    public bool deckCard;
    public DeckManager deckManager;

    public string connectionString;

    public TMP_Text attNameText;
    // public TMP_Text attDescriptionTextOld;
    // public TMP_Text attAttributesText;
    // public TMP_Text attSpecialText;
    public TMP_Text attPlayerText;
    public TMP_Text attPlayerSpecialText;
    public TMP_Text attEnemyText;
    public TMP_Text attEnemySpecialText;
    public TMP_Text attAccurancyText;
    public TMP_Text attDurationText;
    public TMP_Text attPlayerAttributesText;
    public TMP_Text attEnemyAttributesText;
    public TMP_Text attDescriptionText;
    public Image attackIcon;


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
    public PlayFabAlbumCardManager playFabAlbumCardManager;


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
        parentLibrarySwipeInput = GetComponentInParent<LibrarySwipeInput>();

        nameText.text = cardName;
        levelText.text = "lvl " + level;

        maxHP = health;

        string imagePath = "Cards/" + image;

        cardImage.sprite = Resources.Load<Sprite>(imagePath);
        cardImageAttr.sprite = Resources.Load<Sprite>(imagePath + "_back");
        cardImageDesc.sprite = Resources.Load<Sprite>(imagePath + "_back");
        cardImageInfo.sprite = Resources.Load<Sprite>(imagePath + "_back");
        cardImageAttack.sprite = Resources.Load<Sprite>(imagePath + "_back");

        background.GetComponent<Image>().color = color;
        nameText.color = color;
        levelText.color = color;

        LoadDetails();

        changeButton.onClick.AddListener(ShowAttackList);
        //attackListContainer.gameObject.SetActive(false);

    }

    public void Initialize(GameObject deckPanelReference)
    {
        deckPanel = deckPanelReference;
    }

    private int CalculateExpForLevel(int level)
    {
        // Použite optimalizovaný vzorec pre výpočet EXP pre daný level
        return OnlineCardExperienceCurve.RequiredXpForLevel(level);
    }


    public void LoadDetails()
    {
        nameTextAttr.text = cardName;
        int nextLevelExperience = OnlineCardExperienceCurve.RequiredXpForLevel(level + 1);
        expText.text = "Experience: " + experience + " / " + nextLevelExperience;
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

        cardIdText.text = cardId;
        styleIdText.text = "Style: " + styleId;


        cardIdText.color = color;
        seriesIdText.color = color;
        styleIdText.color = color;
        creationDateText.color = color;

        recycleButtonImg.color = ChangeAlpha(color, 80);
        recycleButtonText.color = color;

        storyText.color = color;

        attNameText.color = color;
        // attDescriptionTextOld.color = color;
        // attAttributesText.color = color;
        // attSpecialText.color = color;
        attPlayerText.color = color;
        attPlayerSpecialText.color = color;
        attEnemyText.color = color;
        attEnemySpecialText.color = color;
        attAccurancyText.color = color;
        attDurationText.color = color;
        attPlayerAttributesText.color = color;
        attEnemyAttributesText.color = color;
        attDescriptionText.color = color;

        changeButtonImg.color = ChangeAlpha(color, 80);
        changeButtonText.color = color;

    }

    private string BuildDetailLogContext()
    {
        string visibleExpText = expText != null ? expText.text : "<null>";
        return $"id={cardId}, name={cardName}, styleId={styleId}, level={level}, experience={experience}, expText={visibleExpText}, deckCard={deckCard}, isZoomed={isZoomed}, attacks=[{attack1},{attack2},{attack3},{attack4}], counts=[{countAttack1},{countAttack2},{countAttack3},{countAttack4}]";
    }

    // Metoda na odstranenie karty
    public void RemoveCard()
    {
        if (deckManager == null)
        {
            Debug.LogWarning("[Card] Cannot recycle card because deckManager is not assigned.");
            CardTutorial.instance.ShowBlockSellDeckCard();
            return;
        }

        if (deckManager.IsCardUsedInAnyKnownDeck(cardId))
        {
            Debug.LogWarning("[Card] Cannot recycle card because it is used in a deck.");
            CardTutorial.instance.ShowBlockSellDeckCard();
            return;
        }

        GetPlayerCards(cardsData =>
        {
            if (cardsData != null)
            {
                cardsData.cards.RemoveAll(card => card.CardID == cardId);
                SavePlayerCards(cardsData);
            }
        });
        StartCoroutine(AddCurrency(1));
        StartCoroutine(AlbumLoveValue.Instance.GetPlayerCurrencyBalance());
        Destroy(gameObject);
    }
    private void GetPlayerCards(System.Action<PlayerCardsData> callback)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            if (result.Data != null && result.Data.ContainsKey("PlayerCards"))
            {
                string jsonData = result.Data["PlayerCards"].Value;
                PlayerCardsData cardsData = JsonUtility.FromJson<PlayerCardsData>(jsonData);
                callback(cardsData);
            }
        }, error => Debug.LogError(error.GenerateErrorReport()));
    }

    // Uloženie dát hráča
    private void SavePlayerCards(PlayerCardsData cardsData)
    {
        string jsonData = JsonUtility.ToJson(cardsData);
        var updateData = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                {"PlayerCards", jsonData}
            }
        };
        PlayFabClientAPI.UpdateUserData(updateData, result => Debug.Log("Card data updated successfully."), error => Debug.LogError(error.GenerateErrorReport()));
    }

    private IEnumerator AddCurrency(int amount)
    {
        var request = new AddUserVirtualCurrencyRequest
        {
            Amount = amount,
            VirtualCurrency = "SK"
        };
        bool isCompleted = false;

        PlayFabClientAPI.AddUserVirtualCurrency(request, result =>
        {
            Debug.Log("Úspešne pridaná mena. Nový zostatok: " + result.Balance);
            isCompleted = true;
        }, error =>
        {
            Debug.LogError("Chyba pri pridávaní meny: " + error.GenerateErrorReport());
            isCompleted = true;
        });


        yield return new WaitUntil(() => isCompleted);

        yield return StartCoroutine(AlbumLoveValue.Instance.GetPlayerCurrencyBalance());
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
            playFabAlbumCardManager.UpdateCardAttack(cardId, selectedOriginalAttackId, selectedAttackId, (success) =>
            {
                if (success)
                {
                    Debug.Log("Útok bol zmenený");
                    UpdateCardUI(selectedOriginalAttackId, selectedAttackId);
                    selectedAttackId = -1;
                    selectedOriginalAttackId = -1;
                }
                else
                {
                    Debug.LogError("Nepodarilo sa zmeniť útok");
                }
            });
            attackListContainer.gameObject.SetActive(false);
        }
    }

    private void UpdateCardUI(int originalAttackID, int newAttackValue)
    {
        Debug.Log("UpdateCardUI " + originalAttackID + " " + newAttackValue);
        // Zistite, ktorý útok sa má aktualizovať na základe ID útoku
        if (attack1 == originalAttackID)
        {
            attack1 = newAttackValue;
            if (currentAttackIndex == 1)
                LoadAttackData(newAttackValue);
            Debug.Log("Bingo 1");
        }
        else if (attack2 == originalAttackID)
        {
            attack2 = newAttackValue;
            if (currentAttackIndex == 2)
                LoadAttackData(newAttackValue);
            Debug.Log("Bingo 2");
        }
        else if (attack3 == originalAttackID)
        {
            attack3 = newAttackValue;
            if (currentAttackIndex == 3)
                LoadAttackData(newAttackValue);
            Debug.Log("Bingo 3");
        }
        else if (attack4 == originalAttackID)
        {
            attack4 = newAttackValue;
            if (currentAttackIndex == 4)
                LoadAttackData(newAttackValue);
            Debug.Log("Bingo 4");
        }
        else
        {
            Debug.LogWarning("Nebolo možné nájsť útok s daným ID na karte pre aktualizáciu");
        }

    }

    public void SelectAttack(int newAttackId, int originalAttackId)
    {
        selectedAttackId = newAttackId;
        selectedOriginalAttackId = originalAttackId;
        Debug.Log($"SelectAttack: {newAttackId} -> {originalAttackId}");
    }

    public void ShowAttackList()
    {
        attackListController.ClearAttackList(); // Vymaže všetky predchádzajúce útoky
        Debug.Log("CHange kliknuty ");
        attackListController.ShowAttackList(styleId, displayedAttack);
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
                        // attDescriptionTextOld.text = "Description: " + reader["Description"].ToString();
                        // attAttributesText.text = "Attributes: " + reader["Attributes"].ToString();
                        // attSpecialText.text = "Special: " + reader["Special"].ToString();
                        attPlayerText.text = "Player: " + reader["Player"].ToString();
                        attPlayerSpecialText.text = "Special: " + reader["PlayerSpecial"].ToString();
                        attEnemyText.text = "Enemy: " + reader["Enemy"].ToString();
                        attEnemySpecialText.text = "Special: " + reader["EnemySpecial"].ToString();
                        attAccurancyText.text = "Accurancy: " + reader["Accurancy"].ToString();
                        attDurationText.text = "Duration: " + reader["Duration"].ToString() + " R";
                        attPlayerAttributesText.text = "Player Attributes: " + reader["PlayerAttributes"].ToString();
                        attEnemyAttributesText.text = "Enemy Attributes: " + reader["EnemyAttributes"].ToString();
                        attDescriptionText.text = "Description: " + reader["Description"].ToString();

                        attackIcon.sprite = Resources.Load<Sprite>("Game/Animations/" + reader["Icon"].ToString());
                    }
                    else
                    {
                        Debug.LogError($"No attack data found for AttackID: {attackID}");
                    }
                }
            }
        }
    }


    private async void OnClick()
    {
        if (deckCard)
        {
            if (!isZoomed && currentZoomedCard != null)
            {
                if (deckManager == null)
                {
                    Debug.LogError("[Card] Cannot swap deck card because deckManager is not assigned.");
                    return;
                }

                await deckManager.SwapWithSelectedCardAsync(this, currentZoomedCard);
            }
            else if (isZoomed)
            {
                Destroy(this.gameObject);
                deckPanel.SetActive(false);
            }
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isZoomed && !deckCard)
        {
            pointerDragStartPosition = eventData.position;
            isDragging = true;
            if (parentScrollRect != null)
            {
                parentScrollRect.OnBeginDrag(eventData);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isZoomed && isDragging && !deckCard)
        {
            dragInProgress = true;
            if (parentScrollRect != null)
            {
                parentScrollRect.OnDrag(eventData);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isZoomed && !deckCard)
        {
            isDragging = false;
            if (parentScrollRect != null)
            {
                parentScrollRect.OnEndDrag(eventData);
            }

            float dragDistance = Vector2.Distance(pointerDragStartPosition, eventData.position);
        }
    }

    public void OnSwipeRight()
    {
        Debug.Log("Swipe Right");
        if (isZoomed)
        {
            AudioManager.Instance.PlayCardZoomInSound();
            if (frontSide.activeSelf)
            {
                frontSide.SetActive(false);
                backSideInfo.SetActive(true);
            }
            else if (backSideInfo.activeSelf)
            {
                backSideInfo.SetActive(false);
                backSideDescription.SetActive(true);
            }
            else if (backSideDescription.activeSelf)
            {
                backSideDescription.SetActive(false);
                backSideAttributes.SetActive(true);
            }
            else if (backSideAttributes.activeSelf)
            {
                backSideAttributes.SetActive(false);
                frontSide.SetActive(true);
            }
            if (backSideAttack.activeSelf)
            {
                ChangeAttack(-1);
            }
        }
    }

    public void OnSwipeLeft()
    {
        Debug.Log("Swipe Left");
        if (isZoomed)
        {
            AudioManager.Instance.PlayCardZoomInSound();
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
                backSideDescription.SetActive(false);
                backSideInfo.SetActive(true);
            }
            else if (backSideInfo.activeSelf)
            {
                backSideInfo.SetActive(false);
                frontSide.SetActive(true);
            }
            if (backSideAttack.activeSelf)
            {
                ChangeAttack(1);
            }
        }
    }



    public void OnSwipeUp()
    {
        if (isZoomed)
        {
            AudioManager.Instance.PlayCardZoomOutSound();
            LoadAttackData(attack1); // Načítajte údaje o prvom útoku hráča podľa ID útoku
            displayedAttack = attack1;
            backSideAttack.SetActive(true);
            frontSide.SetActive(false);
            backSideAttributes.SetActive(false);
            backSideDescription.SetActive(false);
            backSideInfo.SetActive(false);
        }
    }

    public void OnSwipeDown()
    {
        if (isZoomed)
        {
            AudioManager.Instance.PlayCardZoomOutSound();
            backSideAttack.SetActive(false);
            frontSide.SetActive(true);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDownPosition = eventData.position;
        if (!isZoomed && parentLibrarySwipeInput != null)
        {
            parentLibrarySwipeInput.CapturePointerDown(eventData);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerUpPosition = eventData.position;
        float distance = Vector2.Distance(pointerDownPosition, pointerUpPosition);

        if (!isZoomed && parentLibrarySwipeInput != null && parentLibrarySwipeInput.TryHandlePointerUp(eventData))
        {
            dragInProgress = false;
            return;
        }

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
            AudioManager.Instance.PlayCardZoomOutSound();
        }
        else if (currentZoomedCard == null)
        {
            ZoomIn();
            AudioManager.Instance.PlayCardZoomInSound();
        }
    }

    public void ZoomIn()
    {
        Debug.LogWarning($"[CardDetail] OpenDetail requested: {BuildDetailLogContext()}");
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
            Debug.LogWarning($"[CardDetail] OpenDetail: {BuildDetailLogContext()}, originalParent={originalParent.name}, siblingIndex={originalSiblingIndex}");

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
            Debug.LogWarning($"[CardDetail] CloseDetail: {BuildDetailLogContext()}");
            transform.SetParent(originalParent.transform);
            transform.localScale = new Vector3(1f, 1f, 1f);
            transform.SetSiblingIndex(originalSiblingIndex);
            currentZoomedCard = null;
            isZoomed = false;
            frontSide.SetActive(true);
            backSideAttributes.SetActive(false);
            backSideDescription.SetActive(false);
            backSideAttack.SetActive(false);
            backSideInfo.SetActive(false);
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
