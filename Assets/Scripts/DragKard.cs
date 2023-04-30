using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragKard : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Vector3 originalPosition;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    public Vector3 originalHandPosition;

    public Kard kard;

    private FightSystem fightManager;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        fightManager = FindObjectOfType<FightSystem>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {

        if (fightManager.state == FightState.PLAYERDEATH)
        {
            originalPosition = rectTransform.position;
            originalHandPosition = rectTransform.localPosition;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (fightManager.state == FightState.PLAYERDEATH)
        {
            rectTransform.position += new Vector3(eventData.delta.x, eventData.delta.y);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (fightManager.state == FightState.PLAYERDEATH)
        {
            // Overenie, či je karta na správnom mieste pre premiestnenie do bojového priestoru
            // Ak nie, vráti sa na pôvodnú pozíciu
            if (!IsOverBattleArea())
            {
                rectTransform.position = originalPosition;
            }
            else
            {
                // Vykonajte akciu pre premiestnenie karty do bojového priestoru
                // Napríklad pomocou metódy v triede FightManager
                fightManager.PlaceCardInBattleArea(gameObject);
            }
        }

        canvasGroup.blocksRaycasts = true;
    }

    private bool IsOverBattleArea()
    {
        // Získajte kolíder karty
        Collider2D cardCollider = GetComponent<Collider2D>();
        
        // Získajte kolíder bojového poľa
        Collider2D battleAreaCollider = kard.battleArea.GetComponent<Collider2D>();

        // Ak sa kolíder karty prekrýva s kolíderom bojového poľa, vráťte true
        return cardCollider.bounds.Intersects(battleAreaCollider.bounds);
    }
    }
