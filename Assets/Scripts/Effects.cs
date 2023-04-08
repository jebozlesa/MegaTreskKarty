using System.Collections;
using System.Collections.Generic;
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
            dialogText.text = "no effects";
            yield break;
        }

    /*    Dictionary<int, int> attackCount = new Dictionary<int, int>();
        List<List<int>> orderedEffects = new List<List<int>>(card.effects);
        orderedEffects.Sort((a, b) => attackCount[a[0]].CompareTo(attackCount[b[0]]));

        foreach (List<int> effect in orderedEffects)
        {
            yield return StartCoroutine(ExecuteEffect(card, effect[1], dialogText, card.effects.IndexOf(effect), target));
        }*/

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
            dialogText.text = card.cardName + " Bleeds no more";
            yield return new WaitForSeconds(2);
            yield break;
        }
        else
        {
            card.effects[iteration][1] -= 1;
            card.TakeDamage(1);
            dialogText.text = card.cardName + " Bleeds, este " + card.effects[iteration][1] + " kola";
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
            dialogText.text = card.cardName + " hurts itself practizing asceticism, este " + card.effects[iteration][1] + " kola";
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
            card.state = CardState.STAY;
            dialogText.text = card.cardName + " sleeps, este " + card.effects[iteration][1] + " kola";
        }
        Debug.Log(card.cardName + " => Sleep");
        yield return new WaitForSeconds(2);
	}
    //4
    public IEnumerator Exposure(Kard card, TMP_Text dialogText, int iteration)
	{
        dialogText.text = card.cardName + " is exposured, uz " + card.effects[iteration][1] + " kola";
        card.effects[iteration][1] -= 1;
        card.TakeDamage(card.effects[iteration][1]);
        Debug.Log(card.cardName + " => Exposure");
        yield return new WaitForSeconds(2);
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
            target.TakeDamage(Random.Range(18, 22));
            dialogText.text = card.cardName + "'s armies attacks!!";
            yield return new WaitForSeconds(2);
            yield break;
        }
        else
        {
            card.effects[iteration][1] -= 1;
            dialogText.text = card.cardName + " keeps a watch, este " + card.effects[iteration][1] + " kola";
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
            if (Random.value <= (card.effects[iteration][1] / 5f))//sanca sa zmensuje od poctu tahov
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
        card.state = CardState.STAY;
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
            card.effects[iteration][1] -= 1;
            dialogText.text = card.cardName + " is Tethered, este " + card.effects[iteration][1] + " kola";
        }
        Debug.Log(card.cardName + " => Tether");
        yield return new WaitForSeconds(2);
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
            dialogText.text = card.cardName + " is Starving, este " + card.effects[iteration][1] + " kola";
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
            target.TakeDamage(Random.Range(1, (card.speed + card.attack + card.strength + card.knowledge - target.defense - target.speed)));
            dialogText.text = card.cardName + " attacks from all sides";
            yield return new WaitForSeconds(2);
            yield break;
        }
        else
        {
            card.effects[iteration][1] -= 1;
        }
        Debug.Log(card.cardName + " => Envelop");
        yield return new WaitForSeconds(2);
	}
    //12
    public IEnumerator Blockade(Kard card, TMP_Text dialogText, int iteration)
	{
        if (card.effects[iteration][1] == 0 || (Random.value <= 0.1f))
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
        card.TakeDamage(1);
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
            card.HandleStrength(Random.Range(0,3));
            card.HandleSpeed(Random.Range(0,3));
            card.HandleAttack(Random.Range(0,3));
            card.HandleDefense(Random.Range(0,3));
            card.HandleCharisma(Random.Range(0,3));
            dialogText.text = card.cardName + " feels better";
            yield return new WaitForSeconds(2);
            yield break;
        }
        else
        {
            card.effects[iteration][1] -= 1;
        }
        Debug.Log(card.cardName + " => Depression");
        yield return new WaitForSeconds(2);
	}
    //14
    public IEnumerator ArtInspiration(Kard card, TMP_Text dialogText, int iteration, Kard target)
	{
        //StartCoroutine(textBubble.ShowForSeconds("surrender!", Resources.Load<Sprite>("oblacik"), 2.5f));
        if (card.effects[iteration][1] == 0)
        {
            card.state = CardState.ATTACK;
            card.RemoveEffect(iteration);
            target.HandleStrength(Random.Range(-3,0));
            target.HandleSpeed(Random.Range(-3,0));
            target.HandleAttack(Random.Range(-3,0));
            target.HandleDefense(Random.Range(-3,0));
            target.HandleKnowledge(Random.Range(0,3));
            dialogText.text = target.cardName + " is impressed by masterpiece";
            yield return new WaitForSeconds(2);
            target.TakeDamage(Random.Range(1, target.defense * 2));
            dialogText.text = card.cardName + " attacks from behind";
            yield break;
        }
        else
        {
            card.state = CardState.STAY;
            dialogText.text = card.cardName + " is making art, este " + card.effects[iteration][1] + " kola";
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
            dialogText.text = card.cardName + " is making art, este " + card.effects[iteration][1] + " kola";
        }
        Debug.Log(card.cardName + " => Starving");
        yield return new WaitForSeconds(2);
	}
}
