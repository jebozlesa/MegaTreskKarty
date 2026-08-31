using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Mono.Data.Sqlite;
using System.Data;
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
    public CardRecycleService cardRecycleService;
    public ConfirmDialogController recycleConfirmationDialog;
    private bool isRecycleInFlight;

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


    public int currentAttackIndex = 1;
    private int displayedAttack;

    //public GameObject tutorial;

    GameObject cardTutorialObject;


    void Start()
    {
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

    }

    private string BuildDetailLogContext()
    {
        string visibleExpText = expText != null ? expText.text : "<null>";
        return $"id={cardId}, name={cardName}, styleId={styleId}, level={level}, experience={experience}, expText={visibleExpText}, deckCard={deckCard}, isZoomed={isZoomed}, attacks=[{attack1},{attack2},{attack3},{attack4}], counts=[{countAttack1},{countAttack2},{countAttack3},{countAttack4}]";
    }

    // Metoda na odstranenie karty
    public void RequestRecycleCard()
    {
        if (isRecycleInFlight)
        {
            Debug.LogWarning($"[CardRecycle] Recycle ignored while busy: card={cardId}, name={cardName}");
            return;
        }

        if (!CanStartRecycle())
        {
            return;
        }

        if (recycleConfirmationDialog == null)
        {
            Debug.LogError($"[CardRecycle] Cannot recycle card because confirm dialog is not assigned: card={cardId}, name={cardName}");
            return;
        }

        Debug.LogWarning($"[CardRecycle] Confirmation requested: card={cardId}, name={cardName}, level={level}");
        recycleConfirmationDialog.Show("ARE YOU SURE?", () => StartCoroutine(RecycleCard()));
    }
    private bool CanStartRecycle()
    {
        if (string.IsNullOrWhiteSpace(cardId))
        {
            Debug.LogError($"[CardRecycle] Cannot recycle card without cardId: name={cardName}");
            return false;
        }

        if (deckManager == null)
        {
            Debug.LogError($"[CardRecycle] Cannot recycle card because deckManager is not assigned: card={cardId}, name={cardName}");
            return false;
        }

        if (deckManager.TryFindCardDeckUsage(cardId, out LibraryDeckUsageInfo usage))
        {
            Debug.LogWarning($"[CardRecycle] Recycle blocked because card is used in a deck: card={cardId}, name={cardName}, deckUsage={FormatDeckUsage(usage)}");
            ShowRecycleBlocked();
            return false;
        }

        if (cardRecycleService == null)
        {
            Debug.LogError($"[CardRecycle] Cannot recycle card because cardRecycleService is not assigned: card={cardId}, name={cardName}");
            return false;
        }

        return true;
    }

    private IEnumerator RecycleCard()
    {
        if (!CanStartRecycle())
        {
            yield break;
        }

        string playerId = ResolveLoggedInPlayerId();
        if (string.IsNullOrWhiteSpace(playerId))
        {
            Debug.LogError($"[CardRecycle] Cannot recycle card because logged player id is missing: card={cardId}, name={cardName}");
            yield break;
        }

        isRecycleInFlight = true;
        string requestId = Guid.NewGuid().ToString();
        Debug.LogWarning($"[CardRecycle] Server recycle requested: player={playerId}, card={cardId}, requestId={requestId}");

        SceneLoadingOverlay.SetMessage("RECYCLING...");
        SceneLoadingOverlay.Show();

        var recycleTask = cardRecycleService.RecycleCardAsync(playerId, cardId, requestId);
        yield return new WaitUntil(() => recycleTask.IsCompleted);

        if (recycleTask.IsFaulted || recycleTask.Result == null)
        {
            string error = recycleTask.Exception != null ? recycleTask.Exception.GetBaseException().Message : "<no result>";
            Debug.LogError($"[CardRecycle] Server recycle failed: card={cardId}, requestId={requestId}, error={error}");
            FinishRecycleRequest();
            yield break;
        }

        CardRecycleResponse response = recycleTask.Result;
        Debug.LogWarning(
            $"[CardRecycle] Server recycle response: success={response.success}, stage={response.stage}, error={response.error}, card={response.cardId}, requestId={response.requestId}, reward={response.reward} {response.currencyCode}, alreadyProcessed={response.alreadyProcessed}, requiresManualReview={response.requiresManualReview}, deckUsage={FormatDeckUsage(response.deckUsage)}"
        );

        if (!response.success)
        {
            if (response.error == "card_is_in_deck")
            {
                ShowRecycleBlocked();
            }

            FinishRecycleRequest();
            yield break;
        }

        if (response.requiresManualReview)
        {
            Debug.LogError($"[CardRecycle] Card was removed but reward requires manual review: card={cardId}, requestId={requestId}, stage={response.stage}, error={response.error}");
        }

        if (AlbumLoveValue.Instance != null)
        {
            yield return StartCoroutine(AlbumLoveValue.Instance.GetPlayerCurrencyBalance());
        }

        if (deckManager != null && deckManager.libraryDeckController != null)
        {
            yield return StartCoroutine(deckManager.libraryDeckController.LoadForCurrentPlayer(useCardRenderDelay: false));
        }

        CompleteSuccessfulRecycle();
        FinishRecycleRequest();
    }

    private void CompleteSuccessfulRecycle()
    {
        if (currentZoomedCard == this)
        {
            currentZoomedCard = null;
        }

        isZoomed = false;

        if (deckPanel != null)
        {
            deckPanel.SetActive(false);
        }

        Destroy(gameObject);
    }

    private void FinishRecycleRequest()
    {
        isRecycleInFlight = false;
        SceneLoadingOverlay.Hide();
    }

    private void ShowRecycleBlocked()
    {
        if (CardTutorial.instance != null)
        {
            CardTutorial.instance.ShowBlockSellDeckCard();
        }
    }

    private static string ResolveLoggedInPlayerId()
    {
        if (PlayFabManagerLogin.Instance != null && !string.IsNullOrWhiteSpace(PlayFabManagerLogin.Instance.LoggedInPlayerId))
        {
            return PlayFabManagerLogin.Instance.LoggedInPlayerId;
        }

        return PlayerPrefs.GetString("LoggedInPlayerId", string.Empty);
    }

    private static string FormatDeckUsage(LibraryDeckUsageInfo usage)
    {
        if (usage == null)
        {
            return "none";
        }

        return $"context={usage.contextId}, deck={usage.deckId}, deckIndex={usage.deckIndex}";
    }

    private static string FormatDeckUsage(CardRecycleDeckUsage usage)
    {
        if (usage == null)
        {
            return "none";
        }

        return $"context={usage.contextId}, deck={usage.deckId}";
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
        else if (!dragInProgress)
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
            // Hide the deck panel
            deckPanel.SetActive(false);
        }
    }

    public Color32 ChangeAlpha(Color32 inputColor, byte newAlpha)
    {
        return new Color32(inputColor.r, inputColor.g, inputColor.b, newAlpha);
    }


}
