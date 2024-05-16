using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Effects : MonoBehaviour
{

    public AttackAnimations attackAnimations;
    public TextBubble textBubble;

    private IEnumerator ShowDialog(TMP_Text dialogText, string message)
    {
        dialogText.text = "";
        yield return new WaitForSeconds(0.2f);
        dialogText.text = message;
        yield return new WaitForSeconds(1.8f);
    }

    private IEnumerator ShowAttackDialog(TMP_Text dialogText, string message)
    {
        dialogText.text = "";
        yield return new WaitForSeconds(0.2f);
        dialogText.text = message;
        yield return new WaitForSeconds(0.8f);
    }

    public IEnumerator ExecuteEffects(Kard card, TMP_Text dialogText, int attackType, Kard target)
    {
        Debug.Log(card.cardName + " card.effects.Count = " + card.effects.Count);
        if (card.effects.Count == 0)
        {
            yield break;
        }

        // Získanie a zoradenie efektov podľa ich trvania od najkratšieho po najdlhšie
        var sortedEffects = card.effects.OrderBy(effect => effect[1]).ToList();

        for (int i = 0; i < sortedEffects.Count; i++)
        {
            Debug.Log(Time.time + " ExecuteEffect -> count: " + card.effects.Count + "  iter: " + (i) + "  efekt: " + sortedEffects[i][0] + "  kola: " + sortedEffects[i][1]);
            yield return StartCoroutine(ExecuteEffect(card, sortedEffects[i][1], dialogText, i, attackType, target));
        }
    }

    public bool CanMove(Kard card)
    {
        bool isSleeping = false;
        bool isTethered = false;
        bool isBlocked = false;

        // Prechádzajte všetky efekty a aktualizujte stav na základe ich trvania a typu
        for (int i = 0; i < card.effects.Count; i++)
        {
            int effectType = card.effects[i][0]; // Typ efektu
            int effectDuration = card.effects[i][1]; // Trvanie efektu

            if (effectDuration > 0)
            {
                switch (effectType)
                {
                    case 3: // Sleep
                        isSleeping = true;
                        break;
                    case 9: // Tether
                        isTethered = true;
                        break;
                    case 12: // Blockade
                        isBlocked = true;
                        break;
                }
            }
        }

        // Nastavte stav karty na základe kombinácie efektov
        if (isSleeping || isTethered || isBlocked)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    

    IEnumerator ExecuteEffect(Kard card, int param, TMP_Text dialogText, int iteration, int attackType, Kard target = null)
    {
        switch (attackType)
        {
            case 1:
                attackType = card.attack1;
                break;
            case 2:
                attackType = card.attack2;
                break;
            case 3:
                attackType = card.attack3;
                break;
            case 4:
                attackType = card.attack4;
                break;
            default:
                Debug.LogError("Invalid attack type from button.");
                break;
        }

        switch (card.effects[iteration][0])
        {
            case 1:
                yield return StartCoroutine(Bleed(card, dialogText, iteration));
                break;
            case 2:
                yield return StartCoroutine(Asceticism(card, dialogText, iteration));
                break;
            case 3:
                yield return StartCoroutine(Sleep(card, dialogText, iteration, attackType));
                break;
            case 4:
                yield return StartCoroutine(Exposure(card, dialogText, iteration));
                break;
            case 5:
                yield return StartCoroutine(Siege(card, dialogText, iteration, target));
                break;
            case 6:
                yield return StartCoroutine(Fury(card, dialogText, iteration));
                break;
            case 7:
                yield return StartCoroutine(Famine(card, dialogText, iteration));
                break;
            case 8:
                yield return StartCoroutine(Electicity(card, dialogText, iteration));
                break;
            case 9:
                yield return StartCoroutine(Tether(card, dialogText, iteration, attackType));
                break;
            case 10:
                yield return StartCoroutine(Starving(card, dialogText, iteration));
                break;
            case 11:
                yield return StartCoroutine(Envelop(card, dialogText, iteration, target));
                break;
            case 12:
                yield return StartCoroutine(Blockade(card, dialogText, iteration, attackType));
                break;
            case 13:
                yield return StartCoroutine(Depression(card, dialogText, iteration));
                break;
            case 14:
                yield return StartCoroutine(ArtInspiration(card, dialogText, iteration, target));
                break;
            case 15:
                yield return StartCoroutine(Autoportrait(card, dialogText, iteration));
                break;
            case 16:
                yield return StartCoroutine(Burn(card, dialogText, iteration));
                break;
            case 17:
                yield return StartCoroutine(Confusion(card, dialogText, iteration));
                break;
            case 18:
                yield return StartCoroutine(Satellite(card, dialogText, iteration));
                break;
            case 19:
                yield return StartCoroutine(Fear(card, dialogText, iteration));
                break;
            case 20:
                yield return StartCoroutine(Horns(card, dialogText, iteration, target));
                break;
            case 21:
                yield return StartCoroutine(Calm(card, dialogText, iteration));
                break;
            case 22:
                yield return StartCoroutine(Reloading(card, dialogText, iteration, target));
                break;
            case 23:
                yield return StartCoroutine(Trident(card, dialogText, iteration, target));
                break;
            case 24:
                yield return StartCoroutine(Poison(card, dialogText, iteration));
                break;
            case 26:
                yield return StartCoroutine(Curse(card, dialogText, iteration));
                break;
            default:
                Debug.LogError("Invalid effect type.");
                break;
        }
    }
    //1
    public IEnumerator Bleed(Kard card, TMP_Text dialogText, int iteration)
    {
        if (card.effects[iteration][1] == 0)
        {
            card.RemoveEffect(iteration);
            yield break;
        }
        else
        {
            StartCoroutine(attackAnimations.PlayBleedContinueAnimation(card.transform, card.effects[iteration][1]));        //ANIMACIA
            card.TakeDamage(card.effects[iteration][1]);
            card.effects[iteration][1] -= 1;
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " Bleeds"));
        }
        Debug.Log(card.cardName + " => Bleed");
    }
    //2
    public IEnumerator Asceticism(Kard card, TMP_Text dialogText, int iteration)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText, card.cardName + " is affected by Asceticism"));
        //if (card.state != CardState.STAY) card.state = CardState.MAYBE;
        if (card.effects[iteration][1] == 0)
        {
            yield return StartCoroutine(attackAnimations.PlayAscetismEndAnimation(card.transform));        //ANIMACIA
            card.RemoveEffect(iteration);
            if (CanMove(card)) { card.state = CardState.ATTACK; }
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " feels blessed again"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayConfusionHurtItselfAnimation(card.transform));        //ANIMACIA
            card.TakeDamage(1);
            card.effects[iteration][1] -= 1;
            card.state = CardState.STAY;
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " hurts itself practizing asceticism"));
        }
        Debug.Log(card.cardName + " => Asceticism");
    }
    public IEnumerator Sleep(Kard card, TMP_Text dialogText, int iteration, int attackType)
    {

        yield return StartCoroutine(ShowAttackDialog(dialogText, card.cardName + " is affected by Sleep"));

        // Množina čísel útokov, ktoré resetujú trvanie efektu Sleep
        HashSet<int> resetSleepAttacks = new HashSet<int> {26}; 
        // Kontrola, či sa útok nachádza v množine určených čísel
        if (resetSleepAttacks.Contains(attackType)) {  card.effects[iteration][1] = 0; }// Resetovanie trvanie efektu na 0

        if (card.state != CardState.STAY) card.state = CardState.MAYBE;
        if (card.effects[iteration][1] == 0)
        {
            yield return StartCoroutine(attackAnimations.PlaySleepEndAnimation(card.transform)); // ANIMÁCIA
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " wakes up"));
            card.RemoveEffect(iteration);
            if (CanMove(card)) { if (CanMove(card)) { card.state = CardState.ATTACK; } }
            yield break;
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlaySleepAnimation(card.transform)); // ANIMÁCIA
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " sleeps"));
            card.effects[iteration][1] -= 1;
            card.state = CardState.MAYBE;
        }
        Debug.Log(card.cardName + " => Sleep");
    }
    //4
    public IEnumerator Exposure(Kard card, TMP_Text dialogText, int iteration)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText, card.cardName + " is affected by Irradiation"));
        if (UnityEngine.Random.value <= 0.2f)
        {
            yield return StartCoroutine(attackAnimations.PlayExposureEndAnimation(card.transform));        //ANIMACIA
            card.RemoveEffect(iteration);
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + "'s irradiation is gone"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayExposureAnimation(card.transform));        //ANIMACIA
            card.effects[iteration][1] -= 1;
            card.TakeDamage(System.Math.Abs(card.effects[iteration][1]));
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " is irradiated"));
        }
        Debug.Log(card.cardName + " => Exposure");
    }
    //5
    public IEnumerator Siege(Kard card, TMP_Text dialogText, int iteration, Kard target)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText, card.cardName + " using siege"));
        card.state = CardState.STAY;
        if (card.effects[iteration][1] == 0)
        {
            yield return StartCoroutine(attackAnimations.PlaySiegeEndAnimation(target.transform));        //ANIMACIA
            card.RemoveEffect(iteration);
            if (CanMove(card)) { card.state = CardState.ATTACK; }
            card.HandleDefense(-10);
            target.TakeDamage(UnityEngine.Random.Range(8, 15));
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " attacking gates"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlaySiegeContinueAnimation(card.transform, target.transform));        //ANIMACIA
            card.effects[iteration][1] -= 1;
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " is building siege equipement"));
        }
        Debug.Log(card.cardName + " => Siege");
    }
    //6
    public IEnumerator Fury(Kard card, TMP_Text dialogText, int iteration)
    {
        if (card.effects[iteration][1] == 0)
        {
            yield return StartCoroutine(attackAnimations.PlayFuryEndAnimation(card.transform));        //ANIMACIA
            card.RemoveEffect(iteration);
            card.HandleStrength(-5);
            card.HandleAttack(-5);
            card.HandleDefense(3);
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " has calmed down"));
        }
        else
        {
            card.effects[iteration][1] -= 1;
        }
        Debug.Log(card.cardName + " => Fury");
    }
    //7
    public IEnumerator Famine(Kard card, TMP_Text dialogText, int iteration)
    {
        if (card.effects[iteration][1] == 0)
        {
            yield return StartCoroutine(attackAnimations.PlayFamineEndAnimation(card.transform));        //ANIMACIA
            card.RemoveEffect(iteration);
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " is starving no more"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayFamineContinueAnimation(card.transform));        //ANIMACIA
            card.effects[iteration][1] -= 1;
            card.Heal(2);
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " is harvesting"));
        }
        Debug.Log(card.cardName + " => Famine");
    }
    //8
    public IEnumerator Electicity(Kard card, TMP_Text dialogText, int iteration)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText, card.cardName + " is affected by Electicity"));
        if (card.effects[iteration][1] == 0)
        {
            yield return StartCoroutine(attackAnimations.PlayElectricityEndAnimation(card.transform));        //ANIMACIA
            card.RemoveEffect(iteration);
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " is fresh after shock"));
        }
        else
        {
            if (UnityEngine.Random.value <= (card.effects[iteration][1] / 5f))//sanca sa zmensuje od poctu tahov
            {
                yield return StartCoroutine(attackAnimations.PlayElectricityAnimation(card.transform));        //ANIMACIA
                card.state = CardState.MAYBE;
                yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " cannot move"));
            }
            else
            {
                if (CanMove(card)) { card.state = CardState.ATTACK; }
            }
            card.effects[iteration][1] -= 1;
        }
        Debug.Log(card.cardName + " => Electicity");
    }
    //9
    public IEnumerator Tether(Kard card, TMP_Text dialogText, int iteration, int attackType)
    {
        // Množina čísel útokov, ktoré resetujú trvanie efektu Sleep
        HashSet<int> resetSleepAttacks = new HashSet<int> {64}; 
        // Kontrola, či sa útok nachádza v množine určených čísel
        if (resetSleepAttacks.Contains(attackType)) {  card.effects[iteration][1] = 0; }// Resetovanie trvanie efektu na 0

        card.state = CardState.MAYBE;
        if (card.effects[iteration][1] == 0)
        {
            yield return StartCoroutine(attackAnimations.PlayTetherEndAnimation(card.transform));        //ANIMACIA
            card.RemoveEffect(iteration);
            if (CanMove(card)) { card.state = CardState.ATTACK; }
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " broke loose"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayTetherAnimation(card.transform));        //ANIMACIA
            if (UnityEngine.Random.value <= 0.9f) card.effects[iteration][1] -= 1;
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " is locked"));
        }
        Debug.Log(card.cardName + " => Tether");
    }
    //10
    public IEnumerator Starving(Kard card, TMP_Text dialogText, int iteration)
    {
        //StartCoroutine(textBubble.ShowForSeconds("surrender!", Resources.Load<Sprite>("oblacik"), 2.5f));
        if (card.effects[iteration][1] == 0)
        {
            card.RemoveEffect(iteration);
            dialogText.text = card.cardName + " found some food";
            yield return new WaitForSeconds(2);
            yield break;
        }
        else
        {
            card.effects[iteration][1] -= 1;
            card.TakeDamage(1);
            card.HandleDefense(1);
            dialogText.text = card.cardName + " is Starving";
        }
        Debug.Log(card.cardName + " => Starving");
        yield return new WaitForSeconds(2);
    }
    //11
    public IEnumerator Envelop(Kard card, TMP_Text dialogText, int iteration, Kard target)
    {
        card.state = CardState.STAY;
        yield return StartCoroutine(ShowAttackDialog(dialogText, card.cardName + " using Double Envelopment"));

        if (card.effects[iteration][1] == 0)
        {
            yield return StartCoroutine(attackAnimations.PlayDoubleEnvelopAttackAnimation(target.transform));        //ANIMACIA
            card.RemoveEffect(iteration);
            if (CanMove(card)) { card.state = CardState.ATTACK; }
            card.HandleDefense(3);
            target.TakeDamage(UnityEngine.Random.Range(12, 16) + (card.attack / 2) - (target.defense / 4));
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " attacks from all sides"));
            yield break;
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayDoubleEnvelopmentWaitAnimation(card.transform));        //ANIMACIA
            card.effects[iteration][1] -= 1;
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " is turning direction"));
        }
        Debug.Log(card.cardName + " => Envelop");
    }
    //12
    public IEnumerator Blockade(Kard card, TMP_Text dialogText, int iteration, int attackType)
    {
        // Množina čísel útokov, ktoré resetujú trvanie efektu Sleep
        HashSet<int> resetSleepAttacks = new HashSet<int> {64}; 
        // Kontrola, či sa útok nachádza v množine určených čísel
        if (resetSleepAttacks.Contains(attackType)) {  card.effects[iteration][1] = 0; }// Resetovanie trvanie efektu na 0

        yield return StartCoroutine(ShowAttackDialog(dialogText, card.cardName + " is affected by Continental Blockade"));
        if (card.effects[iteration][1] == 0 || (UnityEngine.Random.value <= 0.5f))
        {
            if (CanMove(card)) { card.state = CardState.ATTACK; }
            yield return StartCoroutine(attackAnimations.PlayBlocadeEndAnimation(card.transform));        //ANIMACIA
            card.RemoveEffect(iteration);
            yield return StartCoroutine(ShowDialog(dialogText, "Blockade breached!"));
            yield break;
        }
        else
        {
            card.state = CardState.MAYBE;
            yield return StartCoroutine(attackAnimations.PlayBlocadeWaitAnimation(card.transform));        //ANIMACIA
            card.TakeDamage(2);
            card.HandleStrength(-1);
            card.effects[iteration][1] -= 1;
            yield return StartCoroutine(ShowDialog(dialogText, "The blockade holds strong"));
        }
        Debug.Log(card.cardName + " => Blockade");
    }
    //13
    public IEnumerator Depression(Kard card, TMP_Text dialogText, int iteration)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText, card.cardName + " is affected by Depression"));
        //StartCoroutine(textBubble.ShowForSeconds("surrender!", Resources.Load<Sprite>("oblacik"), 2.5f));
        if (card.effects[iteration][1] == 0)
        {
            yield return StartCoroutine(attackAnimations.PlayDepressionEndAnimation(card.transform));        //ANIMACIA
            card.RemoveEffect(iteration);
            card.HandleStrength(3);
            card.HandleSpeed(3);
            card.HandleAttack(3);
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " feels better"));
            yield break;
        }
        else
        {
            card.effects[iteration][1] -= 1;
            //yield return StartCoroutine(attackAnimations.PlayDepressionStartAnimation(card.transform));        //ANIMACIA
        }
        Debug.Log(card.cardName + " => Depression");
    }
    //14
    public IEnumerator ArtInspiration(Kard card, TMP_Text dialogText, int iteration, Kard target)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText, card.cardName + " is affected by Art Inspiration"));
        //StartCoroutine(textBubble.ShowForSeconds("surrender!", Resources.Load<Sprite>("oblacik"), 2.5f));
        if (card.effects[iteration][1] == 0)
        {
            if (CanMove(card)) { card.state = CardState.ATTACK; }
            yield return StartCoroutine(attackAnimations.PlayArtInspirationEndAnimation(card.transform));        //ANIMACIA
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " finished his creation"));
            card.RemoveEffect(iteration);
            yield return StartCoroutine(attackAnimations.PlayArtInspirationEndEnemyAnimation(target.transform));        //ANIMACIA
            target.HandleStrength(-1);
            target.HandleSpeed(-1);
            target.HandleAttack(-1);
            target.HandleDefense(-1);
            target.HandleKnowledge(2);
            yield return StartCoroutine(ShowDialog(dialogText, target.cardName + " is impressed by masterpiece"));
            yield return StartCoroutine(attackAnimations.PlayArtInspirationEndAttackAnimation(card.transform, target.transform));        //ANIMACIA
            target.TakeDamage(10 - (target.defense / 4));
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " attacks in distruption"));
            //    yield break;
        }
        else
        {
            card.state = CardState.STAY;
            yield return StartCoroutine(attackAnimations.PlayArtInspirationWaitAnimation(card.transform));        //ANIMACIA
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " is making art"));
            card.effects[iteration][1] -= 1;
        }
        Debug.Log(card.cardName + " => ArtInspiration");
    }
    //15
    public IEnumerator Autoportrait(Kard card, TMP_Text dialogText, int iteration)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText, card.cardName + " is painting Autoportrait"));
        if (card.effects[iteration][1] == 0)
        {
            if (CanMove(card)) { card.state = CardState.ATTACK; }
            yield return StartCoroutine(attackAnimations.PlayAutoportraitFinishAnimation(card.transform));        //ANIMACIA
            card.RemoveEffect(iteration);
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " finished"));
            //    yield break;
        }
        else
        {
            card.state = CardState.STAY;
            yield return StartCoroutine(attackAnimations.PlayAutoportraitAnimation(card.transform));        //ANIMACIA
            card.effects[iteration][1] -= 1;
            card.HandleKnowledge(1);
            card.HandleStrength(1);
            card.HandleDefense(1);
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " is still painting"));
        }
        Debug.Log(card.cardName + " => Autoportrait");
    }
    //16
    public IEnumerator Burn(Kard card, TMP_Text dialogText, int iteration)
    {
        yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " is on fire"));
        yield return StartCoroutine(attackAnimations.PlayBurnContinueAnimation(card.transform));        //ANIMACIA
        card.TakeDamage(1);
        Debug.Log(card.cardName + " => Burn");
    }
    //17
    public IEnumerator Confusion(Kard card, TMP_Text dialogText, int iteration)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText, card.cardName + " is confused"));
        if (card.effects[iteration][1] == 0)
        {
            if (CanMove(card)) { card.state = CardState.ATTACK; }
            yield return StartCoroutine(attackAnimations.PlayConfusionEndAnimation(card.transform));        //ANIMACIA
            card.RemoveEffect(iteration);
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " is out of confussion"));
            yield break;
        }
        else
        {
            if (UnityEngine.Random.value <= 0.33f)
            {
                card.state = CardState.MAYBE;
                yield return StartCoroutine(attackAnimations.PlayConfusionHurtItselfAnimation(card.transform));        //ANIMACIA
                card.TakeDamage(UnityEngine.Random.Range(1, 3));
                yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " hurt itself in confusion"));
            }
            else if (UnityEngine.Random.value <= 0.33f)
            {
                yield return StartCoroutine(attackAnimations.PlayConfusionForgetAnimation(card.transform));        //ANIMACIA
                card.state = CardState.MAYBE;
                yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " forget to attack in confusion"));
            }
            card.effects[iteration][1] -= 1;
        }
        Debug.Log(card.cardName + " => Confusion");
    }
    //18
    public IEnumerator Satellite(Kard card, TMP_Text dialogText, int iteration)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText, card.cardName + " has satellite"));
        yield return StartCoroutine(attackAnimations.PlaySatelliteAnimation(card.transform));        //ANIMACIA
        card.HandleKnowledge(1);
        yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " gets informations"));
        Debug.Log(card.cardName + " => Satellite");
    }
    //19
    public IEnumerator Fear(Kard card, TMP_Text dialogText, int iteration)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText, card.cardName + " is affected by fear"));
        if (card.effects[iteration][1] == 0)
        {
            yield return StartCoroutine(attackAnimations.PlayFearEndAnimation(card.transform));        //ANIMACIA
            card.RemoveEffect(iteration);
            card.HandleStrength(5);
            card.HandleAttack(5);
            card.HandleDefense(-3);
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + "'s bravery is back"));
        }
        else
        {
            card.effects[iteration][1] -= 1;
        }
        Debug.Log(card.cardName + " => Fear");
    }
    //20
    public IEnumerator Horns(Kard card, TMP_Text dialogText, int iteration, Kard target)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText, card.cardName + " uses horns"));
        card.state = CardState.STAY;
        if (card.effects[iteration][1] == 0)
        {
            yield return StartCoroutine(attackAnimations.PlayBuffaloHornsEndAnimation(card.transform, target.transform));        //ANIMACIA
            card.RemoveEffect(iteration);
            if (CanMove(card)) { card.state = CardState.ATTACK; }
            card.HandleDefense(1);
            target.HandleAttack(-2);
            target.TakeDamage(UnityEngine.Random.Range(1, Math.Max(4, 10 + (card.knowledge / 4) - (target.defense / 4))));
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + "'s Horns strike"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayBuffaloHornsContinueAnimation(card.transform));        //ANIMACIA
            card.TakeDamage(2);
            target.TakeDamage(5);
            card.effects[iteration][1] -= 1;
            yield return StartCoroutine(ShowDialog(dialogText, "buffalo's head keeps strong"));
        }
        Debug.Log(card.cardName + " => Horns");
    }
    //21
    public IEnumerator Calm(Kard card, TMP_Text dialogText, int iteration)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText, card.cardName + " is affected by Calm"));
        if (card.effects[iteration][1] == 0)
        {
            yield return StartCoroutine(attackAnimations.PlayCalmEndAnimation(card.transform));        //ANIMACIA
            card.RemoveEffect(iteration);
            card.HandleStrength(3);
            card.HandleAttack(3);
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " is sharp again"));
        }
        else
        {
            card.effects[iteration][1] -= 1;
        }
        Debug.Log(card.cardName + " => Calm");
    }
    //22
    public IEnumerator Reloading(Kard card, TMP_Text dialogText, int iteration, Kard target)
    {
        card.state = CardState.STAY;
        if (card.effects[iteration][1] == 0)
        {
            card.RemoveEffect(iteration);
            if (CanMove(card)) { card.state = CardState.ATTACK; }
            card.HandleDefense(1);
            if (UnityEngine.Random.value <= ((20 + card.attack) / 40f))
            {
                yield return StartCoroutine(attackAnimations.PlayFlintlockPistolShotAnimation(card.transform, target.transform, true));        //ANIMACIA
                target.TakeDamage(UnityEngine.Random.Range(18, 22));
                yield return StartCoroutine(ShowDialog(dialogText, "Bang! " + card.cardName + " shoots"));
            }
            else
            {
                yield return StartCoroutine(attackAnimations.PlayFlintlockPistolShotAnimation(card.transform, target.transform, false));        //ANIMACIA
                yield return StartCoroutine(ShowDialog(dialogText, "Bang! aaaand miss"));
            }
        }
        else
        {

            card.effects[iteration][1] -= 1;
            yield return StartCoroutine(attackAnimations.PlayFlintlockPistolLoadingAnimation(card.transform));        //ANIMACIA
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " is still reloading"));
        }
        Debug.Log(card.cardName + " => Reloading");
    }
    //23
    public IEnumerator Trident(Kard card, TMP_Text dialogText, int iteration, Kard target)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText, card.cardName + " uses Retiarius"));
        card.state = CardState.STAY;
        if (card.effects[iteration][1] == 0)
        {
            yield return StartCoroutine(attackAnimations.PlayTridentHitAnimation(card.transform, target.transform));        //ANIMACIA
            card.RemoveEffect(iteration);
            if (CanMove(card)) { card.state = CardState.ATTACK; }
            target.TakeDamage(8);
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + "attacks with trident"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayTridentAimAnimation(card.transform));        //ANIMACIA
            card.effects[iteration][1] -= 1;
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " is aiming"));
        }
        Debug.Log(card.cardName + " => DelayedDmg");
    }
    //24
    public IEnumerator Poison(Kard card, TMP_Text dialogText, int iteration)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText, card.cardName + " is poisoned"));
        yield return StartCoroutine(attackAnimations.PlayPoisonContinueAnimation(card.transform));        //ANIMACIA
        if (UnityEngine.Random.value <= 0.33f) card.HandleStrength(-1);
        card.TakeDamage(1);
        dialogText.text = card.cardName + " is suffering of poison";

        yield return new WaitForSeconds(2);
        Debug.Log(card.cardName + " => Poison");
    }
    //25
    public IEnumerator KnockOut(Kard card, TMP_Text dialogText, int iteration)
    {
        if (card.state != CardState.STAY) card.state = CardState.MAYBE;
        //StartCoroutine(textBubble.ShowForSeconds("ZZZZZ!", Resources.Load<Sprite>("oblacik"), 2.5f));
        if (card.effects[iteration][1] == 0)
        {
            card.RemoveEffect(iteration);
            if (CanMove(card)) { card.state = CardState.ATTACK; }
            dialogText.text = card.cardName + " wokes up";
            yield return new WaitForSeconds(2);
            yield break;
        }
        else
        {
            card.effects[iteration][1] -= 1;
            card.state = CardState.MAYBE;
            dialogText.text = card.cardName + " is Knocked Out";
            yield return new WaitForSeconds(2);
        }
        Debug.Log(card.cardName + " => KnockOut");
    }

    //26
    public IEnumerator Curse(Kard card, TMP_Text dialogText, int iteration)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText, card.cardName + " is cursed"));
        if (card.effects[iteration][1] == 0)
        {
            if (CanMove(card)) { card.state = CardState.ATTACK; }
            yield return StartCoroutine(attackAnimations.PlayCurseEndAnimation(card.transform));        //ANIMACIA
            card.RemoveEffect(iteration);
            yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " is out of curse"));
            yield break;
        }
        else
        {
            if (UnityEngine.Random.value <= 0.25f)
            {
                card.state = CardState.MAYBE;
                yield return StartCoroutine(attackAnimations.PlayCurseStartAnimation(card.transform));        //ANIMACIA
                yield return StartCoroutine(ShowDialog(dialogText, card.cardName + " has bad luck attacking"));
            }
            else
            {
                if (CanMove(card)) { card.state = CardState.ATTACK; }
            }
            card.effects[iteration][1] -= 1;
        }
        Debug.Log(card.cardName + " => Confusion");
    }
}
