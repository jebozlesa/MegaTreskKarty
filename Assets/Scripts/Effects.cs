using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Effects : MonoBehaviour
{

    public TextBubble textBubble;

    public IEnumerator ExecuteEffects(Kard card, TMP_Text dialogText, Kard target)
    {
        Debug.Log(card.cardName + " card.effects.Count = " + card.effects.Count);
        if (card.effects.Count == 0)
        {
//            dialogText.text = "no effects";
            yield break;
        }

        int dynamickyRozmer = card.effects.Count;
        for (int i = dynamickyRozmer; i > 0; i--)
        {
            Debug.Log(Time.time + " ExecuteEffect -> count: " + card.effects.Count + "  iter: " + (i) + "  efekt: " + card.effects[i-1][0] + "  kola: " + card.effects[i-1][1]);
            yield return StartCoroutine(ExecuteEffect(card, card.effects[i-1][1], dialogText, i-1, target));
        }
        
    }

    IEnumerator ExecuteEffect(Kard card, int param, TMP_Text dialogText, int iteration, Kard target = null)
	{
        switch (card.effects[iteration][0])
        {
            case 1:
                yield return StartCoroutine(Bleed(card, dialogText, iteration));
                break;
            case 2:
                yield return StartCoroutine(Asceticism(card, dialogText, iteration));
                break;
            case 3:
                yield return StartCoroutine(Sleep(card, dialogText, iteration));
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
                yield return StartCoroutine(Tether(card, dialogText, iteration));
                break;
            case 10:
                yield return StartCoroutine(Starving(card, dialogText, iteration));
                break;
            case 11:
                yield return StartCoroutine(Envelop(card, dialogText, iteration, target));
                break;
            case 12:
                yield return StartCoroutine(Blockade(card, dialogText, iteration));
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
                yield return StartCoroutine(DelayedDmg(card, dialogText, iteration, target));
                break;
            case 24:
                yield return StartCoroutine(Poison(card, dialogText, iteration));
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
            card.effects[iteration][1] -= 1;
            card.TakeDamage(1);
            dialogText.text = card.cardName + " Bleeds";
        }
        Debug.Log(card.cardName + " => Bleed");
        yield return new WaitForSeconds(2);
	}
    //2
    public IEnumerator Asceticism(Kard card, TMP_Text dialogText, int iteration)
	{
		card.TakeDamage(1);
        //if (card.state != CardState.STAY) card.state = CardState.MAYBE;
        if (card.effects[iteration][1] == 0)
        {
            card.RemoveEffect(iteration);
            card.state = CardState.ATTACK;
            dialogText.text = card.cardName + " came to his senses";
            yield return new WaitForSeconds(2);
            yield break;
        }
        else
        {
            card.effects[iteration][1] -= 1;
            card.state = CardState.STAY;
            dialogText.text = card.cardName + " hurts itself practizing asceticism";
        }
        Debug.Log(card.cardName + " => Asceticism");
        yield return new WaitForSeconds(2);
	}
    //3
    public IEnumerator Sleep(Kard card, TMP_Text dialogText, int iteration)
	{
        if (card.state != CardState.STAY) card.state = CardState.MAYBE;
        //StartCoroutine(textBubble.ShowForSeconds("ZZZZZ!", Resources.Load<Sprite>("oblacik"), 2.5f));
        if (card.effects[iteration][1] == 0)
        {
            card.RemoveEffect(iteration);
            card.state = CardState.ATTACK;
            dialogText.text = card.cardName + " wokes up";
            yield return new WaitForSeconds(2);
            yield break;
        }
        else
        {
            card.effects[iteration][1] -= 1;
            card.state = CardState.MAYBE;
            dialogText.text = card.cardName + " sleeps";
            yield return new WaitForSeconds(2);
        }
        Debug.Log(card.cardName + " => Sleep");
	}
    //4
    public IEnumerator Exposure(Kard card, TMP_Text dialogText, int iteration)
	{
        if (UnityEngine.Random.value <= 0.2f)
        {
            card.RemoveEffect(iteration);
            dialogText.text = card.cardName + "'s exposure is gone";
            yield return new WaitForSeconds(2);
            yield break;
        }
        else
        {
            dialogText.text = card.cardName + " is exposured";
            card.effects[iteration][1] -= 1;
            card.TakeDamage(System.Math.Abs(card.effects[iteration][1]));
            yield return new WaitForSeconds(2);
        }
        Debug.Log(card.cardName + " => Exposure");
	}
    //5
    public IEnumerator Siege(Kard card, TMP_Text dialogText, int iteration, Kard target)
	{
        card.state = CardState.STAY;
        //StartCoroutine(textBubble.ShowForSeconds("surrender!", Resources.Load<Sprite>("oblacik"), 2.5f));
        if (card.effects[iteration][1] == 0)
        {
            card.RemoveEffect(iteration);
            card.state = CardState.ATTACK;
            card.HandleDefense(-10);
            target.TakeDamage(UnityEngine.Random.Range(10, 22));
            dialogText.text = card.cardName + "'s armies attacks!!";
            yield return new WaitForSeconds(2);
            yield break;
        }
        else
        {
            card.effects[iteration][1] -= 1;
            dialogText.text = card.cardName + " keeps a watch";
        }
        Debug.Log(card.cardName + " => Siege");
        yield return new WaitForSeconds(2);
	}
    //6
    public IEnumerator Fury(Kard card, TMP_Text dialogText, int iteration)
	{
        if (card.effects[iteration][1] == 0)
        {
            card.RemoveEffect(iteration);
            dialogText.text = card.cardName + " has calmed down";
            card.HandleStrength(-5);
            card.HandleAttack(-5);
            card.HandleDefense(3);
            yield return new WaitForSeconds(2);
            yield break;
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
            card.RemoveEffect(iteration);
            dialogText.text = card.cardName + " is starving no more";
            yield return new WaitForSeconds(2);
            yield break;
        }
        else
        {
            card.effects[iteration][1] -= 1;
            card.HandleDefense(1);
            card.Heal(2);
        }
        Debug.Log(card.cardName + " => Famine");
	}
    //8
    public IEnumerator Electicity(Kard card, TMP_Text dialogText, int iteration)
	{
        if (card.effects[iteration][1] == 0)
        {
            card.RemoveEffect(iteration);
            dialogText.text = card.cardName + " is fresh after shock";
            yield return new WaitForSeconds(2);
            yield break;
        }
        else
        {
            if (UnityEngine.Random.value <= (card.effects[iteration][1] / 5f))//sanca sa zmensuje od poctu tahov
            {
                card.state = CardState.MAYBE;
                dialogText.text = card.cardName + " cannot move";
                yield return new WaitForSeconds(2);
            }
            else
            {
                card.state = CardState.ATTACK;
            }
            card.effects[iteration][1] -= 1;
        }
        Debug.Log(card.cardName + " => Electicity");
	}
    //9
    public IEnumerator Tether(Kard card, TMP_Text dialogText, int iteration)
	{
        card.state = CardState.MAYBE;
        //StartCoroutine(textBubble.ShowForSeconds("ZZZZZ!", Resources.Load<Sprite>("oblacik"), 2.5f));
        if (card.effects[iteration][1] == 0)
        {
            card.RemoveEffect(iteration);
            card.state = CardState.ATTACK;
            dialogText.text = card.cardName + " broke loose";
            yield return new WaitForSeconds(2);
            yield break;
        }
        else
        {
            if (UnityEngine.Random.value <= 0.9f) card.effects[iteration][1] -= 1;
            dialogText.text = card.cardName + " is locked";
            yield return new WaitForSeconds(2);
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
        //StartCoroutine(textBubble.ShowForSeconds("surrender!", Resources.Load<Sprite>("oblacik"), 2.5f));
        if (card.effects[iteration][1] == 0)
        {
            card.RemoveEffect(iteration);
            card.state = CardState.ATTACK;
            card.HandleDefense(3);
            target.TakeDamage(UnityEngine.Random.Range(3, Math.Max(4, 3 + card.speed + card.attack + card.strength + card.knowledge - target.defense - target.speed)));
            dialogText.text = card.cardName + " attacks from all sides";
            yield return new WaitForSeconds(2);
            yield break;
        }
        else
        {
            card.effects[iteration][1] -= 1;
        }
        Debug.Log(card.cardName + " => Envelop");
	}
    //12
    public IEnumerator Blockade(Kard card, TMP_Text dialogText, int iteration)
	{
        if (card.effects[iteration][1] == 0 || (UnityEngine.Random.value <= 0.5f))
        {
            card.state = CardState.ATTACK;
            card.RemoveEffect(iteration);
            dialogText.text = card.cardName + " Blockade breached!";
            yield return new WaitForSeconds(2);
            yield break;
        }
        else
        {
            card.state = CardState.MAYBE;
            dialogText.text = "The blockade holds strong";
            card.TakeDamage(2);
            card.HandleDefense(-1);
            card.HandleStrength(-1);
            card.effects[iteration][1] -= 1;
            yield return new WaitForSeconds(2);
        }
        Debug.Log(card.cardName + " => Blockade");
	}
    //13
    public IEnumerator Depression(Kard card, TMP_Text dialogText, int iteration)
	{
        //StartCoroutine(textBubble.ShowForSeconds("surrender!", Resources.Load<Sprite>("oblacik"), 2.5f));
        if (card.effects[iteration][1] == 0)
        {
            card.RemoveEffect(iteration);
            card.HandleStrength(UnityEngine.Random.Range(0,3));
            card.HandleSpeed(UnityEngine.Random.Range(0,3));
            card.HandleAttack(UnityEngine.Random.Range(0,3));
            card.HandleDefense(UnityEngine.Random.Range(0,3));
            card.HandleCharisma(UnityEngine.Random.Range(0,3));
            dialogText.text = card.cardName + " feels better";
            yield return new WaitForSeconds(2);
            yield break;
        }
        else
        {
            card.effects[iteration][1] -= 1;
        }
        Debug.Log(card.cardName + " => Depression");
	}
    //14
    public IEnumerator ArtInspiration(Kard card, TMP_Text dialogText, int iteration, Kard target)
	{
        //StartCoroutine(textBubble.ShowForSeconds("surrender!", Resources.Load<Sprite>("oblacik"), 2.5f));
        if (card.effects[iteration][1] == 0)
        {
            card.state = CardState.ATTACK;
            card.RemoveEffect(iteration);
            target.HandleStrength(UnityEngine.Random.Range(-3,0));
            target.HandleSpeed(UnityEngine.Random.Range(-3,0));
            target.HandleAttack(UnityEngine.Random.Range(-3,0));
            target.HandleDefense(UnityEngine.Random.Range(-3,0));
            target.HandleKnowledge(UnityEngine.Random.Range(0,3));
            dialogText.text = target.cardName + " is impressed by masterpiece";
            yield return new WaitForSeconds(2);
            target.TakeDamage(UnityEngine.Random.Range(1, target.defense * 2));
            dialogText.text = card.cardName + " attacks from behind";
            yield return new WaitForSeconds(2);
            yield break;
        }
        else
        {
            card.state = CardState.STAY;
            dialogText.text = card.cardName + " is making art";
            card.effects[iteration][1] -= 1;
        }
        Debug.Log(card.cardName + " => Depression");
        yield return new WaitForSeconds(2);
	}
    //15
    public IEnumerator Autoportrait(Kard card, TMP_Text dialogText, int iteration)
	{
        //StartCoroutine(textBubble.ShowForSeconds("surrender!", Resources.Load<Sprite>("oblacik"), 2.5f));
        if (card.effects[iteration][1] == 0)
        {
            card.state = CardState.ATTACK;
            card.RemoveEffect(iteration);
            dialogText.text = card.cardName + " finished";
            yield return new WaitForSeconds(2);
            yield break;
        }
        else
        {
            card.state = CardState.STAY;
            card.effects[iteration][1] -= 1;
            card.HandleKnowledge(1);
            card.HandleStrength(1);
            card.HandleDefense(1);
            dialogText.text = card.cardName + " is making art";
        }
        Debug.Log(card.cardName + " => Autoportrait");
        yield return new WaitForSeconds(2);
	}
    //16
    public IEnumerator Burn(Kard card, TMP_Text dialogText, int iteration)
	{
        card.TakeDamage(1);
        dialogText.text = card.cardName + " is on fire";

        Debug.Log(card.cardName + " => Burn");
        yield return new WaitForSeconds(2);
	}
    //17
    public IEnumerator Confusion(Kard card, TMP_Text dialogText, int iteration)
	{
        if (card.effects[iteration][1] == 0)
        {
            card.state = CardState.ATTACK;
            card.RemoveEffect(iteration);
            dialogText.text = card.cardName + " is out of confussion";
            yield return new WaitForSeconds(2);
            yield break;
        }
        else
        {
            if (UnityEngine.Random.value <= 0.33f)
            {
                dialogText.text = card.cardName + " hurt itself in confusion";
                card.state = CardState.MAYBE;
                card.TakeDamage(UnityEngine.Random.Range(1, 3));
                yield return new WaitForSeconds(2);
            }
            else if (UnityEngine.Random.value <= 0.33f)
            {
                dialogText.text = card.cardName + " forget to attack in confusion";
                yield return new WaitForSeconds(2);
                card.state = CardState.MAYBE;
            }
            card.effects[iteration][1] -= 1;
        }
        Debug.Log(card.cardName + " => Confusion");
	}
    //18
    public IEnumerator Satellite(Kard card, TMP_Text dialogText, int iteration)
	{
        card.HandleKnowledge(1);
        Debug.Log(card.cardName + " => Satellite");
        yield return new WaitForSeconds(0.5f);
	}
    //19
    public IEnumerator Fear(Kard card, TMP_Text dialogText, int iteration)
	{
        if (card.effects[iteration][1] == 0)
        {
            card.RemoveEffect(iteration);
            dialogText.text = card.cardName + " fears no men no more";
            card.HandleStrength(5);
            card.HandleAttack(5);
            card.HandleDefense(-3);
            yield return new WaitForSeconds(2);
            yield break;
        }
        else
        {
            card.effects[iteration][1] -= 1;
        }
        Debug.Log(card.cardName + " => Fury");
	}
    //20
    public IEnumerator Horns(Kard card, TMP_Text dialogText, int iteration, Kard target)
	{
        card.state = CardState.STAY;
        //StartCoroutine(textBubble.ShowForSeconds("surrender!", Resources.Load<Sprite>("oblacik"), 2.5f));
        if (card.effects[iteration][1] == 0)
        {
            card.RemoveEffect(iteration);
            card.state = CardState.ATTACK;
            card.HandleDefense(1);
            card.HandleAttack(-2);
            target.TakeDamage(UnityEngine.Random.Range(1, Math.Max(4, 3 + card.speed + card.attack + card.strength + card.knowledge - target.defense - target.speed)));
            dialogText.text = card.cardName + "'s Horns strike";
            yield return new WaitForSeconds(2);
            yield break;
        }
        else
        {
            card.TakeDamage(1);
            target.TakeDamage(1);
            card.effects[iteration][1] -= 1;
            dialogText.text = "buffalo's head keeps strong";
            yield return new WaitForSeconds(2);
        }
        Debug.Log(card.cardName + " => Horns");
	}
    //21
    public IEnumerator Calm(Kard card, TMP_Text dialogText, int iteration)
	{
        if (card.effects[iteration][1] == 0)
        {
            card.RemoveEffect(iteration);
            dialogText.text = card.cardName + " is sharp again";
            card.HandleStrength(3);
            card.HandleAttack(3);
            yield return new WaitForSeconds(2);
            yield break;
        }
        else
        {
            card.effects[iteration][1] -= 1;
        }
        Debug.Log(card.cardName + " => Fury");
	}
    //22
    public IEnumerator Reloading(Kard card, TMP_Text dialogText, int iteration, Kard target)
	{
        card.state = CardState.STAY;
        //StartCoroutine(textBubble.ShowForSeconds("surrender!", Resources.Load<Sprite>("oblacik"), 2.5f));
        if (card.effects[iteration][1] == 0)
        {
            card.RemoveEffect(iteration);
            card.state = CardState.ATTACK;
            card.HandleDefense(1);
            if (UnityEngine.Random.value <= ((20 + card.attack) / 40f))
            {
                dialogText.text = "Bang! " + card.cardName + " shoots";
                target.TakeDamage(UnityEngine.Random.Range(10, 22));
            }
            else
            {
                dialogText.text = "Bang! aaaand miss";
            }
            yield return new WaitForSeconds(2);
            yield break;
        }
        else
        {
            card.effects[iteration][1] -= 1;
            dialogText.text = card.cardName + " is still reloading";
        }
        Debug.Log(card.cardName + " => Reloading");
        yield return new WaitForSeconds(2);
	}
    //23
    public IEnumerator DelayedDmg(Kard card, TMP_Text dialogText, int iteration, Kard target)
	{
        card.state = CardState.STAY;
        //StartCoroutine(textBubble.ShowForSeconds("surrender!", Resources.Load<Sprite>("oblacik"), 2.5f));
        if (card.effects[iteration][1] == 0)
        {
            card.RemoveEffect(iteration);
            card.state = CardState.ATTACK;
            target.TakeDamage(card.attack - target.defense);
            dialogText.text = card.cardName + "attacks with trident";
            yield return new WaitForSeconds(2);
            yield break;
        }
        else
        {
            card.effects[iteration][1] -= 1;
            dialogText.text = card.cardName + " is aiming";
        }
        Debug.Log(card.cardName + " => DelayedDmg");
        yield return new WaitForSeconds(2);
	}
    //24
    public IEnumerator Poison(Kard card, TMP_Text dialogText, int iteration)
	{
        if (UnityEngine.Random.value <= 0.33f) card.HandleStrength(-1);
        card.TakeDamage(1);
        dialogText.text = card.cardName + " is poisoned";

        yield return new WaitForSeconds(2);
        Debug.Log(card.cardName + " => Poison");
	}
}
