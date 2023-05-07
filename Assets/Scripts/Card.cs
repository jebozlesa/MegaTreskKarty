using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Mono.Data.Sqlite;
using System.Data;

public class Card:MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int cardId;
    public string cardName;
    public int experience;
    public int health;
    public int strength;
    public int speed;
    public int attack;
    public int defense;
    public int knowledge;
    public int charisma;
    public string image;
    public Color32 color;
    public int level;

    int maxHP;

    public string story;

    public Dictionary<int, int> attackCount;

    public int[] attacks = new int[5];
    public int[] countAttacks = new int[5];

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

    void Start()
    {
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

        background.GetComponent<Image>().color = color;
        nameText.color = color;
        levelText.color = color;

        LoadDetails();
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
                UpdateCardInDatabase(currentZoomedCard.cardId, cardId);
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

    private void UpdateCardInDatabase(int newCardId, int oldCardId)
    {
        IDbConnection dbConnection = new SqliteConnection(deckManager.connectionString);
        dbConnection.Open();

        Debug.Log("UpdateCardInDatabase " + newCardId + " -> " + oldCardId);

        // Find the column to update
        string columnName = "";
        for (int i = 1; i <= 5; i++)
        {
            IDbCommand dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText = $"SELECT Card{i} FROM PlayerDecks WHERE DeckID = 1";
            IDataReader reader = dbCommand.ExecuteReader();
            if (reader.Read() && reader.GetInt32(0) == oldCardId)
            {
                columnName = $"Card{i}";
                reader.Close();
                dbCommand.Dispose();
                break;
            }
            reader.Close();
            dbCommand.Dispose();
        }

        // Update the column with the new card ID
        if (!string.IsNullOrEmpty(columnName))
        {
            IDbCommand dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText = $"UPDATE PlayerDecks SET {columnName} = {newCardId} WHERE DeckID = 1";
            int rowsAffected = dbCommand.ExecuteNonQuery();
            Debug.Log($"Rows affected: {rowsAffected}");
            dbCommand.Dispose();
        }
        else
        {
            Debug.LogWarning("Could not find the column to update.");
        }

        dbConnection.Close();
    }

    private void SwapCardsInDatabase(int newCardId, int oldCardId)
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

    public void OnSwipeLeft()
    {
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
        }
    }

    public void OnSwipeRight()
    {
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
        }
        else if (currentZoomedCard == null)
        {
            ZoomIn();
        }
    }

    public void ZoomIn()
    {
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

            // Hide the deck panel
            deckPanel.SetActive(false);
        }
    }

}