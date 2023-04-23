using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
//[System.Serializable]

public class Card:MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string cardName;
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

    public GameObject notsureGO;//tie kokotiny co vyskakuju ked dostane damage
    public TMP_Text notsureText;

    Color32 color_green = new Color32(0, 255, 0, 255);
    Color32 color_red = new Color32(255, 0, 0, 255);

    private void Start()
    {
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

        LoadAttributes();
    }

    public void LoadAttributes()
    {
        nameTextAttr.text = cardName;
        expText.text = "Exp: 1000/450000";
        hpText.text = "Health: " + health;
        strText.text = "Strength: " + strength;
        speText.text = "Speed: " + speed;
        attText.text = "Attack: " + attack;
        defText.text = "Defense: " + defense;
        knoText.text = "Knowledge: " + knowledge;
        chaText.text = "Charisma: " + charisma;

        nameTextAttr.color = color;
        expText.color = color;
        hpText.color = color;
        strText.color = color;
        speText.color = color;
        attText.color = color;
        defText.color = color;
        knoText.color = color;
        chaText.color = color;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isZoomed)
        {
            pointerDragStartPosition = eventData.position;
            isDragging = true;
            parentScrollRect.OnBeginDrag(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isZoomed && isDragging)
        {
            dragInProgress = true;
            parentScrollRect.OnDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isZoomed)
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
            ToggleZoom();
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
        }
    }

}