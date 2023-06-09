using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class Attack : MonoBehaviour
{

    public List<int> exceptionAttacks = new List<int> { 64 };  //je tu utek z retazov od houdiniho co rusi efekty, exception preto ze ide aj ked nieje v stave attack

    public List<int> priorityAttacks;

    private void Start()
    {
        priorityAttacks = new List<int> { 82 };
    }

    public int GetAttackNumber(Kard attacker, int attackIndex)
    {
        switch (attackIndex)
        {
            case 0:
                return 0;
            case 1:
                return attacker.attack1;
            case 2:
                return attacker.attack2;
            case 3:
                return attacker.attack3;
            case 4:
                return attacker.attack4;
            default:
                Debug.LogError("Invalid attack index.");
                return 0;
        }
    }

    public bool IsPriorityAttack(int attackNumber)
{
    bool isPriority = priorityAttacks.Contains(attackNumber);
    Debug.Log("Attack number: " + attackNumber + ", is priority: " + isPriority);
    return isPriority;
}


    public IEnumerator ExecuteAttack(Kard attacker, Kard receiver, int attackType, TMP_Text dialogText)
    {
        if (attackType == 0) yield break;

        if (attacker.attackCount[attackType] > 0)
        {
            attacker.attackCount[attackType]--;
        }
        else
        {
            Debug.Log("No more attacks remaining.");
            yield break;
        }

        switch (attackType)
        {
            case 1:
                attackType = attacker.attack1;
                break;
            case 2:
                attackType = attacker.attack2;
                break;
            case 3:
                attackType = attacker.attack3;
                break;
            case 4:
                attackType = attacker.attack4;
                break;
            default:
                Debug.LogError("Invalid attack type from button.");
                break;
        }

        switch (attackType)
        {
            case 0:
                break;
            case 1:
                yield return StartCoroutine(Punch(attacker, receiver, dialogText));
                break;
            case 2:
                yield return StartCoroutine(Kick(attacker, receiver, dialogText));
                break;
            case 3:
                yield return StartCoroutine(Heal(attacker, receiver, dialogText));
                break;
            case 4:
                yield return StartCoroutine(Forgiveness(attacker, receiver, dialogText));
                break;
            case 5:
                yield return StartCoroutine(Crusade(attacker, receiver, dialogText));
                break;
            case 6:
                yield return StartCoroutine(WaterToWine(attacker, receiver, dialogText));
                break;
            case 7:
                yield return StartCoroutine(CarHit(attacker, receiver, dialogText));
                break;
            case 8:
                yield return StartCoroutine(SteamGun(attacker, receiver, dialogText));
                break;
            case 9:
                yield return StartCoroutine(Radiation(attacker, receiver, dialogText));
                break;
            case 10:
                yield return StartCoroutine(Scratch(attacker, receiver, dialogText));
                break;
            case 11:
                yield return StartCoroutine(ScientificLecture(attacker, receiver, dialogText));
                break;
            case 12:
                yield return StartCoroutine(ChiSau(attacker, receiver, dialogText));
                break;
            case 13:
                yield return StartCoroutine(OneInchPunch(attacker, receiver, dialogText));
                break;
            case 14:
                yield return StartCoroutine(UpInSmoke(attacker, receiver, dialogText));
                break;
            case 15:
                yield return StartCoroutine(Sing(attacker, receiver, dialogText));
                break;
            case 16:
                yield return StartCoroutine(Revolver(attacker, receiver, dialogText));
                break;
            case 17:
                yield return StartCoroutine(ArtilleryRegiment(attacker, receiver, dialogText));
                break;
            case 18:
                yield return StartCoroutine(BloodSucking(attacker, receiver, dialogText));
                break;
            case 19:
                yield return StartCoroutine(Sword(attacker, receiver, dialogText));
                break;
            case 20:
                yield return StartCoroutine(Pike(attacker, receiver, dialogText));
                break;
            case 21:
                yield return StartCoroutine(Terrify(attacker, receiver, dialogText));
                break;
            case 22:
                yield return StartCoroutine(DrinkWine(attacker, receiver, dialogText));
                break;
            case 23:
                yield return StartCoroutine(FlamingGun(attacker, receiver, dialogText));
                break;
            case 24:
                yield return StartCoroutine(Cleaver(attacker, receiver, dialogText));
                break;
            case 25:
                yield return StartCoroutine(Pan(attacker, receiver, dialogText));
                break;
            case 26:
                yield return StartCoroutine(Boost(attacker, receiver, dialogText));
                break;
            case 27:
                yield return StartCoroutine(Temptation(attacker, receiver, dialogText));
                break;
            case 28:
                yield return StartCoroutine(Shamshir(attacker, receiver, dialogText));
                break;
            case 29:
                yield return StartCoroutine(Diplomacy(attacker, receiver, dialogText));
                break;
            case 30:
                yield return StartCoroutine(Siege(attacker, receiver, dialogText));
                break;
            case 31:
                yield return StartCoroutine(TreeStratagem(attacker, receiver, dialogText));
                break;
            case 32:
                yield return StartCoroutine(Tomahawk(attacker, receiver, dialogText));
                break;
            case 33:
                yield return StartCoroutine(PeacePipe(attacker, receiver, dialogText));
                break;
            case 34:
                yield return StartCoroutine(RecurveBow(attacker, receiver, dialogText));
                break;
            case 35:
                yield return StartCoroutine(Fury(attacker, receiver, dialogText));
                break;
            case 36:
                yield return StartCoroutine(Guerilla(attacker, receiver, dialogText));
                break;
            case 37:
                yield return StartCoroutine(Famine(attacker, receiver, dialogText));
                break;
            case 38:
                yield return StartCoroutine(Marxism(attacker, receiver, dialogText));
                break;
            case 39:
                yield return StartCoroutine(TeslaCoil(attacker, receiver, dialogText));
                break;
            case 40:
                yield return StartCoroutine(WirelessCharger(attacker, receiver, dialogText));
                break;
            case 41:
                yield return StartCoroutine(Experiment(attacker, receiver, dialogText));
                break;
            case 42:
                yield return StartCoroutine(TommyGun(attacker, receiver, dialogText));
                break;
            case 43:
                yield return StartCoroutine(TieUp(attacker, receiver, dialogText));
                break;
            case 44:
                yield return StartCoroutine(Corruption(attacker, receiver, dialogText));
                break;
            case 45:
                yield return StartCoroutine(Colt1911(attacker, receiver, dialogText));
                break;
            case 46:
                yield return StartCoroutine(Mortar(attacker, receiver, dialogText));
                break;
            case 47:
                yield return StartCoroutine(GreatArmy(attacker, receiver, dialogText));
                break;
            case 48:
                yield return StartCoroutine(ScorchedEarth(attacker, receiver, dialogText));
                break;
            case 49:
                yield return StartCoroutine(DoubleEnvelopment(attacker, receiver, dialogText));
                break;
            case 50:
                yield return StartCoroutine(ContinentalBlockade(attacker, receiver, dialogText));
                break;
            case 51:
                yield return StartCoroutine(Depression(attacker, receiver, dialogText));
                break;
            case 52:
                yield return StartCoroutine(SelfIsolation(attacker, receiver, dialogText));
                break;
            case 53:
                yield return StartCoroutine(Knife(attacker, receiver, dialogText));
                break;
            case 54:
                yield return StartCoroutine(Autoportrait(attacker, receiver, dialogText));
                break;
            case 55:
                yield return StartCoroutine(GravityPull(attacker, receiver, dialogText));
                break;
            case 56:
                yield return StartCoroutine(Kamikaze(attacker, receiver, dialogText));
                break;
            case 57:
                yield return StartCoroutine(TookOff(attacker, receiver, dialogText));
                break;
            case 58:
                yield return StartCoroutine(AirStrike(attacker, receiver, dialogText));
                break;
            case 59:
                yield return StartCoroutine(JusticeCrusade(attacker, receiver, dialogText));
                break;
            case 60:
                yield return StartCoroutine(Rapier(attacker, receiver, dialogText));
                break;
            case 61:
                yield return StartCoroutine(ExpeditionaryAssault(attacker, receiver, dialogText));
                break;
            case 62:
                yield return StartCoroutine(Culverin(attacker, receiver, dialogText));
                break;
            case 63:
                yield return StartCoroutine(FireShip(attacker, receiver, dialogText));
                break;
            case 64:
                yield return StartCoroutine(HandcuffEscape(attacker, receiver, dialogText));
                break;
            case 65:
                yield return StartCoroutine(Illusion(attacker, receiver, dialogText));
                break;
            case 66:
                yield return StartCoroutine(CarcanoM91(attacker, receiver, dialogText));
                break;
            case 67:
                yield return StartCoroutine(Winchester(attacker, receiver, dialogText));
                break;
            case 68:
                yield return StartCoroutine(Ambush(attacker, receiver, dialogText));
                break;
            case 69:
                yield return StartCoroutine(JupiterC(attacker, receiver, dialogText));
                break;
            case 70:
                yield return StartCoroutine(V2(attacker, receiver, dialogText));
                break;
            case 71:
                yield return StartCoroutine(BattleCry(attacker, receiver, dialogText));
                break;
            case 72:
                yield return StartCoroutine(Revelation(attacker, receiver, dialogText));
                break;
            case 73:
                yield return StartCoroutine(Standard(attacker, receiver, dialogText));
                break;
            case 74:
                yield return StartCoroutine(Pen(attacker, receiver, dialogText));
                break;
            case 75:
                yield return StartCoroutine(IambicPentameter(attacker, receiver, dialogText));
                break;
            case 76:
                yield return StartCoroutine(Ghost(attacker, receiver, dialogText));
                break;
            case 77:
                yield return StartCoroutine(BuffaloHorns(attacker, receiver, dialogText));
                break;
            case 78:
                yield return StartCoroutine(Iklwa(attacker, receiver, dialogText));
                break;
            case 79:
                yield return StartCoroutine(Iwisa(attacker, receiver, dialogText));
                break;
            case 80:
                yield return StartCoroutine(NitenIchiRyu(attacker, receiver, dialogText));
                break;
            case 81:
                yield return StartCoroutine(Tessenjutsu(attacker, receiver, dialogText));
                break;
            case 82:
                yield return StartCoroutine(Iaijutsu(attacker, receiver, dialogText));
                break;
            case 83:
                yield return StartCoroutine(Katana(attacker, receiver, dialogText));
                break;
            case 84:
                yield return StartCoroutine(Nodachi(attacker, receiver, dialogText));
                break;
            case 85:
                yield return StartCoroutine(Yumi(attacker, receiver, dialogText));
                break;
            case 86:
                yield return StartCoroutine(Jujutsu(attacker, receiver, dialogText));
                break;
            case 87:
                yield return StartCoroutine(Espionage(attacker, receiver, dialogText));
                break;
            case 88:
                yield return StartCoroutine(Sabre(attacker, receiver, dialogText));
                break;
            case 89:
                yield return StartCoroutine(Gamble(attacker, receiver, dialogText));
                break;
            case 90:
                yield return StartCoroutine(Philosophy(attacker, receiver, dialogText));
                break;
            case 91:
                yield return StartCoroutine(Calm(attacker, receiver, dialogText));
                break;
            case 92:
                yield return StartCoroutine(Honesty(attacker, receiver, dialogText));
                break;
            case 93:
                yield return StartCoroutine(Valaska(attacker, receiver, dialogText));
                break;
            case 94:
                yield return StartCoroutine(Moonshine(attacker, receiver, dialogText));
                break;
            case 95:
                yield return StartCoroutine(OutlawBand(attacker, receiver, dialogText));
                break;
            case 96:
                yield return StartCoroutine(FlintlockPistol(attacker, receiver, dialogText));
                break;
            case 97:
                yield return StartCoroutine(PassiveResistance(attacker, receiver, dialogText));
                break;
            case 98:
                yield return StartCoroutine(HungerStrike(attacker, receiver, dialogText));
                break;
            case 99:
                yield return StartCoroutine(Gladius(attacker, receiver, dialogText));
                break;
            case 100:
                yield return StartCoroutine(ShieldBash(attacker, receiver, dialogText));
                break;
            case 101:
                yield return StartCoroutine(Yperit(attacker, receiver, dialogText));
                break;
            case 102:
                yield return StartCoroutine(Blitzkrieg(attacker, receiver, dialogText));
                break;
            case 103:
                yield return StartCoroutine(Propaganda(attacker, receiver, dialogText));
                break;
            case 104:
                yield return StartCoroutine(Retiarius(attacker, receiver, dialogText));
                break;
            case 105:
                yield return StartCoroutine(Shuriken(attacker, receiver, dialogText));
                break;
            case 106:
                yield return StartCoroutine(Kusarigama(attacker, receiver, dialogText));
                break;
            case 107:
                yield return StartCoroutine(Ninjutsu(attacker, receiver, dialogText));
                break;
            case 108:
                yield return StartCoroutine(OrientalSpice(attacker, receiver, dialogText));
                break;
            case 109:
                yield return StartCoroutine(FieryArquebus(attacker, receiver, dialogText));
                break;
            case 110:
                yield return StartCoroutine(PirateRaid(attacker, receiver, dialogText));
                break;
            case 111:
                yield return StartCoroutine(Axe(attacker, receiver, dialogText));
                break;
            case 112:
                yield return StartCoroutine(JaguarWarriors(attacker, receiver, dialogText));
                break;
            case 113:
                yield return StartCoroutine(Atlatl(attacker, receiver, dialogText));
                break;
            case 114:
                yield return StartCoroutine(Macuahuitl(attacker, receiver, dialogText));
                break;
            default:
                Debug.LogError("Invalid attack type.");
                break;
        }
    }

    public delegate IEnumerator AttackDelegate(Kard attacker, Kard receiver, TMP_Text dialogText);
    
    //1
    public IEnumerator Punch(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = "Puf! punch from " + attacker.cardName;
		receiver.TakeDamage(((attacker.strength + attacker.attack)/2) - ((receiver.defense + receiver.strength)/2));
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.1f)
        {
            dialogText.text = receiver.cardName + " falls asleep";
            yield return StartCoroutine(receiver.AddEffect(3,Random.Range(1, 3)));//sleep
            yield return new WaitForSeconds(2);
        }
        Debug.Log(attacker.cardName + " -> Punch => " + receiver.cardName);
	}
    //2
    public IEnumerator Kick(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = "Plesk! kick from " + attacker.cardName;
		receiver.TakeDamage(((attacker.speed + attacker.attack)/2) - ((receiver.defense + receiver.strength)/2));
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.2f) receiver.TakeDamage(4);//extra dmg
        Debug.Log(attacker.cardName + " -> Kick => " + receiver.cardName);
	}
    //3
    public IEnumerator Heal(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName+ " Heals himself";
		attacker.Heal(attacker.knowledge/2);
        Debug.Log(attacker.cardName + " -> Heal => " + attacker.cardName);
        yield return new WaitForSeconds(2);
	}
    //4
    public IEnumerator Forgiveness(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " forgives your heressy";
		receiver.HandleAttack(-(int)System.Math.Ceiling((double)attacker.charisma / 5));
        receiver.HandleAttack(-(int)System.Math.Ceiling((double)attacker.charisma / 5));
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.5f)
        {
            dialogText.text = receiver.cardName + " began to repent";
            yield return StartCoroutine(receiver.AddEffect(2,Random.Range(1, 3)));//Asceticism
        }
        Debug.Log(attacker.cardName+" -> Forgiveness => "+receiver.cardName);
	}
    //5
    public IEnumerator Crusade(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = "In the name of crist!!! damage was done";
		receiver.TakeDamage((attacker.strength + attacker.charisma) - ((receiver.defense + receiver.charisma)/2));
        Debug.Log(attacker.cardName+" -> Crusade => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //6
    public IEnumerator WaterToWine(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " changes his water to wine";
		attacker.HandleAttack(2);
        attacker.HandleStrength(2);
        attacker.HandleDefense(-1);
        Debug.Log(attacker.cardName+" -> WaterToWine => "+attacker.cardName);
        yield return new WaitForSeconds(2);
	}
    //7
    public IEnumerator CarHit(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = "Tresk! hit by " + attacker.cardName + "'s car";
		receiver.TakeDamage((attacker.speed*3) - receiver.strength - receiver.defense);
        Debug.Log(attacker.cardName+" -> CarHit => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //8
    public IEnumerator SteamGun(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = "Bum! Steam gun explosed";
		receiver.TakeDamage(Random.Range(1,attacker.knowledge));
        if (Random.value <= 0.5f) attacker.TakeDamage(attacker.knowledge/5);
        yield return new WaitForSeconds(2);
        Debug.Log(attacker.cardName+" -> SteamGun => "+receiver.cardName);
	}
    //9
    public IEnumerator Radiation(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
		if (Random.value <= 0.8f) 
        {
            dialogText.text = receiver.cardName + " is hit by radiation";
            yield return StartCoroutine(receiver.AddEffect(4,0));//exposure
        }
        if (Random.value <= 0.3f)
        {
            dialogText.text = attacker.cardName + " is hit by radiation";
            yield return StartCoroutine(attacker.AddEffect(4,0));//exposure
        }
        Debug.Log(attacker.cardName+" -> Radiation => "+receiver.cardName);
	}
    //10
    public IEnumerator Scratch(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " scratches opponent!";
		receiver.TakeDamage(attacker.attack - receiver.defense);
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.1f) 
        {
            dialogText.text = receiver.cardName + " is wounded";
            yield return StartCoroutine(receiver.AddEffect(1,Random.Range(1, 3)));//bleed
        }
        Debug.Log(attacker.cardName+" -> Scratch => "+receiver.cardName);
	}
    //11
    public IEnumerator ScientificLecture(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " explaining sciience!!!";
		receiver.TakeDamage(attacker.knowledge - receiver.knowledge);
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.3f) 
        {
            dialogText.text = receiver.cardName + " falls asleep";
            yield return StartCoroutine(receiver.AddEffect(3,Random.Range(1, 3)));//sleep
        }
        Debug.Log(attacker.cardName+" -> ScientificLecture => "+receiver.cardName);
	}
    //12
    public IEnumerator ChiSau(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " attacks with his sticky hands";
        receiver.TakeDamage(Random.Range(1, 5) * (attacker.speed  - receiver.defense));
        Debug.Log(attacker.cardName+" -> ChiSau => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //13
    public IEnumerator OneInchPunch(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " pokes enemy with finger";
		receiver.TakeDamage(attacker.attack + attacker.knowledge - receiver.defense);
        if (Random.value <= 0.2f) receiver.TakeDamage(attacker.strength);//critical hit
        Debug.Log(attacker.cardName+" -> OneInchPunch => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //14
    public IEnumerator UpInSmoke(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " smoke some s#!t and feels good";
        attacker.Heal((int)System.Math.Ceiling((double)attacker.strength / 4));
        attacker.HandleStrength(1);
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.1f) 
        {
            dialogText.text = receiver.cardName + " falls asleep";
            yield return StartCoroutine(attacker.AddEffect(3,Random.Range(1, 3)));//sleep
        }
        Debug.Log(attacker.cardName+" -> UpInSmoke => "+receiver.cardName);
	}
    //15
    public IEnumerator Sing(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " sings a banger";
		if (Random.value <= 0.5f) receiver.HandleAttack(-(attacker.charisma / 3));
        if (Random.value <= 0.5f) receiver.HandleDefense(-(attacker.charisma / 3));
        Debug.Log(attacker.cardName+" -> Sing => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}

    //16
    public IEnumerator Revolver(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        if (Random.value <= (attacker.attack / 15f))
        {
        dialogText.text = "Bang! " + attacker.cardName + " shoots";
		receiver.TakeDamage(7);
        }
        else
        {
            dialogText.text = "Bang! aaaand miss";
        }
        Debug.Log(attacker.cardName+" -> Revolver => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}

    //17
    public IEnumerator ArtilleryRegiment(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
		if (Random.value <= 0.7f)
        {
        dialogText.text = receiver.cardName + " is heavily bombarded";
		receiver.TakeDamage(15);
        }
        else
        {
            dialogText.text = "Bang! aaaand miss";
        }
        Debug.Log(attacker.cardName+" -> ArtilleryRegiment => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}

    //18
    public IEnumerator BloodSucking(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = receiver.cardName + " is being drained";
        int a = Random.Range(1, attacker.attack);
		receiver.TakeDamage(a);
        attacker.Heal(a);
        Debug.Log(attacker.cardName+" -> BloodSucking => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}

    //19
    public IEnumerator Sword(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " cuts enemy with swort";
		receiver.TakeDamage(3 + ((attacker.strength + attacker.speed + attacker.attack) / 3) - ((receiver.defense - receiver.strength) / 2));
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.4f) 
        {
            dialogText.text = receiver.cardName + " is wounded";
            yield return StartCoroutine(receiver.AddEffect(1,Random.Range(1, 6)));//bleed
        }
        Debug.Log(attacker.cardName+" -> Sword => "+receiver.cardName);
	}

    //20
    public IEnumerator Pike(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = receiver.cardName + " is impaled on a pike";
		receiver.TakeDamage(((attacker.strength + attacker.speed + attacker.attack) / 2) - receiver.defense);
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.2f) 
        {
            dialogText.text = receiver.cardName + " is wounded";
            yield return StartCoroutine(receiver.AddEffect(1,Random.Range(1, 4)));//bleed
        }
        Debug.Log(attacker.cardName+" -> Pike => "+receiver.cardName);
	}
    //21
    public IEnumerator Terrify(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        if (Random.value <= (int)System.Math.Ceiling((double)receiver.strength / 20))
        {
            dialogText.text = receiver.cardName + " is terribly frightened";
            receiver.HandleStrength((-1) - (attacker.attack/7));
            receiver.HandleAttack((-1) - (attacker.attack/7));
            receiver.HandleDefense(1);
            receiver.HandleCharisma((-(attacker.attack/3)));
        }
        else
        {
            dialogText.text = receiver.cardName + " does not fear you";
        }
        Debug.Log(attacker.cardName+" -> Terrify => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //22
    public IEnumerator DrinkWine(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " is getting slightlty drunk";
        attacker.HandleStrength(2);
		attacker.Heal(attacker.charisma/3);
        Debug.Log(attacker.cardName+" -> drink wine => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //23
    public IEnumerator FlamingGun(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = receiver.cardName + " is being caramelized";
		receiver.TakeDamage(4);
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.1f) 
        {
            dialogText.text = receiver.cardName + " is burning";
            yield return StartCoroutine(receiver.AddEffect(16,1));//burn
            yield return new WaitForSeconds(2);
        }
        Debug.Log(attacker.cardName+" -> flaming gun => "+receiver.cardName);
	}
    //24
    public IEnumerator Cleaver(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = receiver.cardName + " gets chopped by cleaver ";
		receiver.TakeDamage(1 + ((attacker.strength + attacker.attack) / 2) - ((receiver.defense - receiver.strength) / 2));
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.3f) 
        {
            dialogText.text = receiver.cardName + " is wounded";
            yield return StartCoroutine(receiver.AddEffect(1,Random.Range(2, 5)));//bleed
        }
        Debug.Log(attacker.cardName+" -> cleaver => "+receiver.cardName);
    }
    //25
    public IEnumerator Pan(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = "Tongggg!!! "+ receiver.cardName + " gets hit by the pan ";
		receiver.TakeDamage(1 + attacker.strength - receiver.defense);
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.3f)
        {
            dialogText.text = receiver.cardName + " falls asleep";
            yield return StartCoroutine(receiver.AddEffect(3,Random.Range(2, 4)));//sleep
        }
        Debug.Log(attacker.cardName+" -> pan => "+receiver.cardName);
    }
    //26
    public IEnumerator Boost(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " gets some good boost";
		attacker.HandleStrength(2);
        attacker.HandleAttack(2);
        attacker.HandleDefense(2);
        attacker.HandleCharisma(-1);
        attacker.HandleKnowledge(-2);
        attacker.RemoveEffectById(3);
        Debug.Log(attacker.cardName+" -> Boost => "+attacker.cardName);
        yield return new WaitForSeconds(2);

    }
    //27
    public IEnumerator Temptation(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = "ohhh!!! "+ receiver.cardName + " falls in love <3 ";
        receiver.HandleAttack(- (attacker.charisma - receiver.charisma));
        receiver.HandleDefense(- (attacker.charisma - receiver.charisma));
        receiver.HandleCharisma(1);
        receiver.HandleKnowledge(- ((attacker.charisma - receiver.charisma) / 2));
        Debug.Log(attacker.cardName+" -> Temptation => "+receiver.cardName);
        yield return new WaitForSeconds(2);
    }
    //28
    public IEnumerator Shamshir(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " cuts wit shamshir";
		receiver.TakeDamage(3 + ((attacker.strength + attacker.speed + attacker.attack) / 3) - ((receiver.defense - receiver.strength) / 2));
        if (Random.value <= 0.2f) receiver.TakeDamage(3);//critical hit
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.5f) 
        {
            dialogText.text = receiver.cardName + " is wounded";
            yield return StartCoroutine(receiver.AddEffect(1,Random.Range(2, 6)));//bleed
        }
        Debug.Log(attacker.cardName+" -> Shamshir => "+receiver.cardName);
    }
    //29
    public IEnumerator Diplomacy(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " made diplomatic gesture";
        receiver.HandleDefense(-(int)System.Math.Ceiling((double)attacker.charisma / 10));
        receiver.HandleKnowledge(-(int)System.Math.Ceiling((double)attacker.charisma / 10));
        Debug.Log(attacker.cardName+" -> Diplomacy => "+receiver.cardName);
        yield return new WaitForSeconds(2);
    }
    //30
    public IEnumerator Siege(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " is building siege equipment";
        StartCoroutine(attacker.AddEffect(5,3));//siege
        attacker.state = CardState.STAY;
        attacker.HandleDefense(10);
        Debug.Log(attacker.cardName+" -> Siege => "+receiver.cardName);
        yield return new WaitForSeconds(2);
    }
    //31
    public IEnumerator TreeStratagem(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        if (attacker.knowledge >= receiver.knowledge)
        {
        dialogText.text = "Trees attacks!";
		receiver.TakeDamage(Random.Range(1, 3));
        }
        else
        {
            dialogText.text = receiver.cardName + ": Yo think am stupid?!";
        }
        Debug.Log(attacker.cardName+" -> TreeStratagem => "+receiver.cardName);
        yield return new WaitForSeconds(2);
    }
    //32
    public IEnumerator Tomahawk(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " attacks wit tomahawk";
		receiver.TakeDamage(2 + ((attacker.strength + attacker.speed + attacker.attack) / 3) - receiver.defense );
        if (Random.value <= 0.2f) receiver.TakeDamage(5);//critical hit
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.2f) 
        {
            dialogText.text = receiver.cardName + " is wounded";
            yield return StartCoroutine(receiver.AddEffect(1,Random.Range(2, 6)));//bleed
            yield return new WaitForSeconds(2);
        }
        Debug.Log(attacker.cardName+" -> Tomahawk => "+receiver.cardName);
    }
    //33
    public IEnumerator PeacePipe(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " offers peace pipe";
        receiver.HandleStrength(Random.Range(-3, 3));
        receiver.HandleAttack(Random.Range(-3, 3));
        receiver.HandleDefense(Random.Range(-3, 3));
        receiver.HandleCharisma(Random.Range(-3, 3));
        receiver.HandleKnowledge(Random.Range(-3, 3));

        attacker.HandleStrength(Random.Range(-1, 1));
        attacker.HandleAttack(Random.Range(-1, 1));
        attacker.HandleDefense(Random.Range(-1, 1));
        attacker.HandleCharisma(Random.Range(-1, 1));
        attacker.HandleKnowledge(Random.Range(-1, 1));
        Debug.Log(attacker.cardName+" -> PeacePipe => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //34
    public IEnumerator RecurveBow(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " shoots arrow";
        if (Random.value <= (attacker.strength / 10f))
        {
		receiver.TakeDamage(5);
        }
        else
        {
            yield return new WaitForSeconds(1);
            dialogText.text = "arrow missed";
        }
        Debug.Log(attacker.cardName+" -> RecurveBow => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //35
    public IEnumerator Fury(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " is furious!";
        StartCoroutine(attacker.AddEffect(6,3));//fury
        attacker.HandleStrength(5);
        attacker.HandleAttack(5);
        attacker.HandleDefense(-3);
        Debug.Log(attacker.cardName+" -> Fury => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //36
    public IEnumerator Guerilla(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " sends Guerillas";
		receiver.TakeDamage(Random.Range(1, attacker.attack+2));
        if (Random.value <= 0.2f) receiver.HandleAttack(-(Random.Range(2, 4)));//critical hit
        yield return new WaitForSeconds(2);
        Debug.Log(attacker.cardName+" -> Guerilla => "+receiver.cardName);
    }
    //37
    public IEnumerator Famine(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        if (receiver.CheckEffect(7))
        {
            dialogText.text = "Be a human, " + attacker.cardName + "!";
            yield return new WaitForSeconds(2);
        }
        else
        {
            dialogText.text = attacker.cardName + " caused a famine";
            int r = Random.Range(5, 10);
            receiver.TakeDamage(2 * r);
            receiver.HandleDefense(-2 * r);
            StartCoroutine(receiver.AddEffect(7, r));
            yield return new WaitForSeconds(2);
        }
        Debug.Log(attacker.cardName+" -> Famine => "+receiver.cardName);
    }
    //38
    public IEnumerator Marxism(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " explains a Marxism";
        receiver.HandleKnowledge(-(int)System.Math.Ceiling((double)attacker.charisma / 3));
        yield return new WaitForSeconds(2);
        Debug.Log(attacker.cardName+" -> Marxism => "+receiver.cardName);
    }
    //39
    public IEnumerator TeslaCoil(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " shocks enemy";
        receiver.TakeDamage(attacker.attack);
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.7f)
        {
            dialogText.text = receiver.cardName + " is shocked";
            StartCoroutine(receiver.AddEffect(8, 5));
            yield return new WaitForSeconds(2);
        }
        Debug.Log(attacker.cardName+" -> TeslaCoil => "+receiver.cardName);
    }
    //40
    public IEnumerator WirelessCharger(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " is charging wireless!!!";
        attacker.HandleAttack(Random.Range(2, 4));
        attacker.Heal(1);
        Debug.Log(attacker.cardName+" -> WirelessCharger => "+attacker.cardName);
        yield return new WaitForSeconds(2);
	}
    //41
    public IEnumerator Experiment(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " is trying something";
        receiver.TakeDamage(Random.Range(0, 2));
        attacker.TakeDamage(Random.Range(0, 2));
        attacker.HandleKnowledge(2);
        Debug.Log(attacker.cardName+" -> Experiment => "+attacker.cardName);
        yield return new WaitForSeconds(2);
	}
    //42
    public IEnumerator TommyGun(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = "Ratatata!";
		receiver.TakeDamage(Random.Range(1, 10));
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.5f) 
        {
            dialogText.text = receiver.cardName + " is wounded";
            yield return StartCoroutine(receiver.AddEffect(1,Random.Range(1, 4)));//bleed
        }
        Debug.Log(attacker.cardName+" -> TommyGun => "+receiver.cardName);
	}
    //43
    public IEnumerator TieUp(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " tries catch enemy";
        yield return new WaitForSeconds(2);
        if (attacker.attack > receiver.speed && Random.value <= 0.9f) 
        {
            StartCoroutine(receiver.AddEffect(9,1));//Tether
            dialogText.text = receiver.cardName + " was captured";
            yield return new WaitForSeconds(2);
        }
        Debug.Log(attacker.cardName+" -> TieUp => "+receiver.cardName);
	}
    //44
    public IEnumerator Corruption(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " corrupted everybody around";
        int r = Random.Range(1, 3);
		receiver.HandleSpeed(-r);
        receiver.HandleDefense(-r);
        attacker.HandleAttack(2 * r);
        attacker.HandleCharisma(-1);
        yield return new WaitForSeconds(2);
        Debug.Log(attacker.cardName+" -> Corruption => "+receiver.cardName);
    }
    //45
    public IEnumerator Colt1911(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        if (Random.value <= (attacker.attack / 12f))
        {
        dialogText.text = "Bang! " + attacker.cardName + " shoots";
		receiver.TakeDamage(7);
        }
        else
        {
            dialogText.text = "Bang! aaaand miss";
        }
        Debug.Log(attacker.cardName+" -> Colt1911 => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //46
    public IEnumerator Mortar(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        if (Random.value <= 0.9f) 
        {
            dialogText.text = "Mortar fires";
            receiver.TakeDamage(Random.Range(5, 10));
        }
        else
        {
            dialogText.text = "Mortar exploded";
            attacker.TakeDamage(Random.Range(3, 7));
        }
        Debug.Log(attacker.cardName+" -> Mortar => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //47
    public IEnumerator GreatArmy(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " is assembling an army";
        attacker.HandleStrength((int)System.Math.Ceiling((double)receiver.charisma / 10));
        attacker.HandleAttack((int)System.Math.Ceiling((double)receiver.charisma / 10));
        attacker.HandleDefense((int)System.Math.Ceiling((double)receiver.charisma / 10));
        attacker.HandleCharisma(1);
        Debug.Log(attacker.cardName+" -> GrandArmee => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //48
    public IEnumerator ScorchedEarth(Kard attacker, Kard receiver, TMP_Text dialogText)                     //nepouzite
	{

        dialogText.text = attacker.cardName + " is Burning all in his wake";
        yield return new WaitForSeconds(2);
        yield return StartCoroutine(receiver.AddEffect(10,Random.Range(1, attacker.defense)));//Starving
        Debug.Log(attacker.cardName+" -> ScorchedEarth => "+receiver.cardName);
	}
    //49
    public IEnumerator DoubleEnvelopment(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " Launching the maneuver!";
        StartCoroutine(attacker.AddEffect(11,1));//Envelopment
        attacker.state = CardState.STAY;
        attacker.HandleDefense(-3);
        Debug.Log(attacker.cardName+" -> DoubleEnvelopment => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //50
    public IEnumerator ContinentalBlockade(Kard attacker, Kard receiver, TMP_Text dialogText)
	{

        dialogText.text = attacker.cardName + " Enforcing the blockade";
        yield return new WaitForSeconds(2);
        yield return StartCoroutine(receiver.AddEffect(12,1));//Blockade
        Debug.Log(attacker.cardName+" -> ContinentalBlockade => "+receiver.cardName);
	}
    //51
    public IEnumerator Depression(Kard attacker, Kard receiver, TMP_Text dialogText)
	{

        dialogText.text = attacker.cardName + " is depressed";
        yield return new WaitForSeconds(2);
        receiver.HandleStrength(Random.Range(-3,0));
        receiver.HandleSpeed(Random.Range(-3,0));
        receiver.HandleAttack(Random.Range(-3,0));
        receiver.HandleDefense(Random.Range(-3,0));
        receiver.HandleCharisma(Random.Range(-3,0));
        yield return StartCoroutine(receiver.AddEffect(13,1));//Depression
        dialogText.text = receiver.cardName + " feels bad for enemy";
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.3f) 
        {
            dialogText.text = attacker.cardName + " is empowered by muse";
            attacker.state = CardState.STAY;
            yield return StartCoroutine(attacker.AddEffect(14, 3));//ArtInspiration
        }
        Debug.Log(attacker.cardName+" -> Depression => "+receiver.cardName);
	}
    //52
    public IEnumerator SelfIsolation(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " feels happy alone";
        attacker.HandleDefense(3);
        if (Random.value <= 0.3f) 
        {
            dialogText.text = attacker.cardName + " is empowered by muse";
            attacker.state = CardState.STAY;
            yield return StartCoroutine(attacker.AddEffect(14, 3));//ArtInspiration
        }
        Debug.Log(attacker.cardName+" -> SelfIsolation => "+attacker.cardName);
        yield return new WaitForSeconds(2);
	}
    //53
    public IEnumerator Knife(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " cuts enemy with knife";
		receiver.TakeDamage(1 + ((attacker.strength + attacker.speed + attacker.attack) / 3) - ((receiver.defense - receiver.strength) / 2));
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.3f) 
        {
            dialogText.text = receiver.cardName + " is wounded";
            yield return StartCoroutine(receiver.AddEffect(1,Random.Range(1, 3)));//bleed
        }
        Debug.Log(attacker.cardName+" -> Knife => "+receiver.cardName);
	}
    //54
    public IEnumerator Autoportrait(Kard attacker, Kard receiver, TMP_Text dialogText)
	{

        dialogText.text = attacker.cardName + " discovers himself through art";
        yield return new WaitForSeconds(2);
        attacker.HandleKnowledge(1);
        attacker.HandleStrength(1);
        attacker.HandleDefense(1);
        attacker.state = CardState.STAY;
        yield return StartCoroutine(attacker.AddEffect(15,Random.Range(1, 3)));//Autoportrait
        Debug.Log(attacker.cardName+" -> Autoportrait => "+receiver.cardName);
	}
    //55
    public IEnumerator GravityPull(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        int r = Random.Range(0,3);
        string[] objects = {"piano", "boiler", "anvil", "gases"};
        int[] weight = {6,5,4,0};
        dialogText.text = "Gravity pulled " + objects[r] + " to " + receiver.cardName;
		receiver.TakeDamage(weight[r]);
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.1f) 
        {
            dialogText.text = receiver.cardName + " falls asleep";
            yield return StartCoroutine(receiver.AddEffect(3,Random.Range(1, 3)));//sleep
        }
        Debug.Log(attacker.cardName+" -> GravityPull => "+receiver.cardName);
	}
    //56
    public IEnumerator Kamikaze(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        int dmg = attacker.health;
        if (Random.value <= 0.9f) 
        {
            dialogText.text = "Kamikaze strikes";
            attacker.TakeDamage(dmg);
            receiver.TakeDamage(Random.Range(1, (attacker.knowledge + attacker.speed + attacker.attack)));
        }
        else
        {
            dialogText.text = "Ou! Nasty miss";
            attacker.TakeDamage(dmg);
        }
        Debug.Log(attacker.cardName+" -> Kamikaze => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //57
    public IEnumerator TookOff(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        if (Random.value <= 0.75f) 
        {
            dialogText.text = attacker.cardName + " took off";
            attacker.HandleSpeed(3);
        }
        else
        {
            dialogText.text = attacker.cardName + " crashes";
            attacker.TakeDamage(Random.Range(1, attacker.speed));
        }
        Debug.Log(attacker.cardName+" -> TookOff => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //58
    public IEnumerator AirStrike(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        int dmg = attacker.health;

        dialogText.text = attacker.cardName + " strikes from air";
        receiver.TakeDamage(Random.Range(attacker.attack, attacker.speed + attacker.attack) - receiver.defense);

        if (Random.value <= (attacker.attack / 30f)) 
        {
            receiver.TakeDamage(attacker.speed);
        }

        Debug.Log(attacker.cardName+" -> AirStrike => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //59
    public IEnumerator JusticeCrusade(Kard attacker, Kard receiver, TMP_Text dialogText)     //nepouzite
	{
        dialogText.text = attacker.cardName + " fights enemy for justice";
		receiver.TakeDamage(((2 + attacker.knowledge + attacker.charisma)/2) - receiver.defense);
        yield return new WaitForSeconds(2);
        Debug.Log(attacker.cardName + " -> JusticeCrusade => " + receiver.cardName);
	}
    //60
    public IEnumerator Rapier(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " cuts wit Rapier";
		receiver.TakeDamage(2 + ((attacker.strength + attacker.speed + attacker.attack) / 3) - ((receiver.defense - receiver.strength) / 2));
        if (Random.value <= 0.2f) receiver.TakeDamage(3);//critical hit
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.6f) 
        {
            dialogText.text = receiver.cardName + " is wounded";
            yield return StartCoroutine(receiver.AddEffect(1,Random.Range(2, 7)));//bleed
        }
        Debug.Log(attacker.cardName+" -> Rapier => "+receiver.cardName);
    }
    //61
    public IEnumerator ExpeditionaryAssault(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        if (Random.value <= (20f + attacker.knowledge) / 50f) 
        {
            int[] boost = DistributeRandomly(Random.Range(3, 6));
            dialogText.text = attacker.cardName + " gets useful staff";
            attacker.HandleStrength(boost[0]);
            attacker.HandleAttack(boost[1]);
            attacker.HandleDefense(boost[2]);
            attacker.HandleCharisma(boost[3]);
            attacker.HandleKnowledge(boost[4]);
        }
        else
        {
            dialogText.text = attacker.cardName + " almost died on sea";
            attacker.TakeDamage(Random.Range(3, 7));
        }
        Debug.Log(attacker.cardName+" -> ExpeditionaryAssault => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //62
    public IEnumerator Culverin(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
		if (Random.value <= 0.8f)
        {
        dialogText.text = attacker.cardName + "'s cannon fires";
		receiver.TakeDamage(Random.Range(1, 15));
        }
        else
        {
            dialogText.text = "Bang! aaaand miss";
        }
        Debug.Log(attacker.cardName+" -> Culverin => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //63
    public IEnumerator FireShip(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
		if (Random.value <= 0.5f)
        {
        dialogText.text = "Fire ship impacts";
		receiver.TakeDamage(3);
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.5f) 
        {
            dialogText.text = receiver.cardName + " is burning";
            yield return StartCoroutine(receiver.AddEffect(16,1));//burn
            yield return new WaitForSeconds(2);
        }
        }
        else
        {
            dialogText.text = receiver.cardName + " compelled to reposition";
            receiver.HandleDefense(-2);
            yield return new WaitForSeconds(2);
        }
        Debug.Log(attacker.cardName+" -> Culverin => "+receiver.cardName);
	}
    //64
    public IEnumerator HandcuffEscape(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        int[] idsToRemove = { 3, 8, 9, 12, 16 };
        attacker.RemoveEffectsById(idsToRemove);
        dialogText.text = attacker.cardName + " slips outta trouble";
        yield return new WaitForSeconds(2);
		if (Random.value <= 0.5f)
        {
            yield return StartCoroutine(receiver.AddEffect(17,2));//confusion
            dialogText.text = receiver.cardName + " is confused";
            attacker.HandleCharisma(1);
            yield return new WaitForSeconds(2);
        }

        Debug.Log(attacker.cardName+" -> HandcuffEscape => "+receiver.cardName);
	}
    //65
    public IEnumerator Illusion(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + "'s illusion confuses the enemy";
        yield return StartCoroutine(receiver.AddEffect(17,Random.Range(2, 5)));//confusion
        attacker.HandleCharisma(1);
        yield return new WaitForSeconds(2);
        Debug.Log(attacker.cardName+" -> Illusion => "+receiver.cardName);
	}
    //66
    public IEnumerator CarcanoM91(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        if (Random.value <= ((20 + attacker.attack) / 37f))
        {
        dialogText.text = "Bang! " + attacker.cardName + " shoots";
		receiver.TakeDamage(Random.Range(6, 9));
        }
        else
        {
            dialogText.text = "Bang! aaaand miss";
        }
        Debug.Log(attacker.cardName+" -> CarcanoM91 => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //67
    public IEnumerator Winchester(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        if (Random.value <= ((20 + attacker.attack) / 40f))
        {
            dialogText.text = "Bang! " + attacker.cardName + " shoots";
            receiver.TakeDamage(Random.Range(6, 9));
        }
        else
        {
            dialogText.text = "Bang! aaaand miss";
        }
        Debug.Log(attacker.cardName+" -> Winchester => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //68
    public IEnumerator Ambush(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = receiver.cardName + " ambushed with surprise";
        receiver.TakeDamage(2);
        receiver.HandleCharisma(-1);
        attacker.HandleCharisma(1);
        yield return new WaitForSeconds(2);
        Debug.Log(attacker.cardName+" -> Ambush => "+receiver.cardName);
	}
    //69
    public IEnumerator JupiterC(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        if (Random.value <= 0.9f)
        {
            dialogText.text = attacker.cardName + " launches satellite";
            yield return new WaitForSeconds(2);
            attacker.HandleAttack(1);
            attacker.HandleDefense(1);
            yield return StartCoroutine(attacker.AddEffect(18,0));//Satellite
        }
        else
        {
            dialogText.text = "Rocket exploded";
        }
        Debug.Log(attacker.cardName+" -> JupiterC => "+receiver.cardName);
	}
    //70
    public IEnumerator V2(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        if (Random.value <= 0.25f)
        {
            dialogText.text = "V2 hits target";
            receiver.TakeDamage(Random.Range(15, 20));
        }
        else
        {
            dialogText.text = "Missile missed";
        }
        Debug.Log(attacker.cardName+" -> V2 => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //71
    public IEnumerator BattleCry(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " roared into battle";
        attacker.HandleStrength((int)System.Math.Ceiling((double)receiver.charisma / 5));
        attacker.HandleAttack((int)System.Math.Ceiling((double)receiver.charisma / 10));
        Debug.Log(attacker.cardName+" -> BattleCry => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //72
    public IEnumerator Revelation(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = "God is with " + attacker.cardName;
        attacker.HandleCharisma(2);
        attacker.HandleKnowledge(2);
        Debug.Log(attacker.cardName+" -> Revelation => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //73
    public IEnumerator Standard(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + "'s Banner Ignites Valor";
        receiver.TakeDamage(2);
        if (Random.value <= 0.5f)
        {
            attacker.HandleCharisma(1);
        }
        else
        {
            attacker.HandleStrength(1);
        }
        Debug.Log(attacker.cardName+" -> V2 => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //74
    public IEnumerator Pen(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = "The pen is mightier than the sword";
		receiver.TakeDamage(2 + ((attacker.knowledge + attacker.charisma + attacker.attack) / 3) - (receiver.defense));
        receiver.HandleStrength(-1);
        yield return new WaitForSeconds(2);
        Debug.Log(attacker.cardName+" -> Pen => "+receiver.cardName);
	}
    //75
    public IEnumerator IambicPentameter(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + "'s poetry confuses the enemy";
        yield return StartCoroutine(receiver.AddEffect(17,Random.Range(2, 5)));//confusion
        receiver.HandleKnowledge(-1);
        yield return new WaitForSeconds(2);
        Debug.Log(attacker.cardName+" -> IambicPentameter => "+receiver.cardName);
	}
    //76
    public IEnumerator Ghost(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " summons ghost, enemy fears";
        StartCoroutine(receiver.AddEffect(19,2));//fear
        receiver.HandleStrength(-6);
        receiver.HandleAttack(-5);
        receiver.HandleDefense(3);
        Debug.Log(attacker.cardName+" -> Ghost => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //77
    public IEnumerator BuffaloHorns(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " Launching the maneuver!";
        StartCoroutine(attacker.AddEffect(20,1));//Horns
        attacker.state = CardState.STAY;
        receiver.HandleAttack(2);
        attacker.HandleDefense(-1);
        Debug.Log(attacker.cardName+" -> BuffaloHorns => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //78
    public IEnumerator Iklwa(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " attacks wit Iklwa";
		receiver.TakeDamage(((attacker.strength + attacker.attack) / 2) - receiver.defense );
        if (Random.value <= 0.3f) receiver.TakeDamage(5);//critical hit
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.3f) 
        {
            dialogText.text = receiver.cardName + " is wounded";
            yield return StartCoroutine(receiver.AddEffect(1,Random.Range(2, 6)));//bleed
            yield return new WaitForSeconds(2);
        }
        Debug.Log(attacker.cardName+" -> Iklwa => "+receiver.cardName);
    }
    //79
    public IEnumerator Iwisa(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " throws Iwisa";
        if (Random.value <= (attacker.strength / 10f))
        {
            receiver.TakeDamage(5);
            if (Random.value <= 0.3f) 
            {
                yield return new WaitForSeconds(1);
                dialogText.text = receiver.cardName + " is wounded";
                yield return StartCoroutine(receiver.AddEffect(1,Random.Range(2, 5)));//bleed
            }
        }
        else
        {
            yield return new WaitForSeconds(1);
            dialogText.text = "throw missed";
        }
        Debug.Log(attacker.cardName+" -> Iwisa => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //80
    public IEnumerator NitenIchiRyu(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " attacks wit Katana";
		receiver.TakeDamage(attacker.strength - (receiver.defense / 2));
        yield return new WaitForSeconds(1);
        dialogText.text = attacker.cardName + " attacks wit Wakizashi";
        receiver.TakeDamage(attacker.speed - (receiver.defense / 2));
        yield return new WaitForSeconds(1);
        Debug.Log(attacker.cardName+" -> NitenIchiRyu => "+receiver.cardName);
    }
    //81
    public IEnumerator Tessenjutsu (Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " moves like Kitana";
        attacker.HandleDefense(2);
        yield return new WaitForSeconds(2);
        Debug.Log(attacker.cardName+" -> Tessenjutsu  => "+receiver.cardName);
	}
    //82
    public IEnumerator Iaijutsu (Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = "Flash of steel by" + attacker.cardName;
        receiver.TakeDamage(attacker.speed - (receiver.defense / 2));
        yield return new WaitForSeconds(2);
        Debug.Log(attacker.cardName+" -> Iaijutsu  => "+receiver.cardName);
	}
    //83
    public IEnumerator Katana(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " slashes with katana ";
		receiver.TakeDamage(3 + ((attacker.knowledge + attacker.attack + attacker.speed) / 3) - ((receiver.defense - receiver.speed) / 2));
        if (Random.value <= 0.2f) receiver.TakeDamage(3);//critical hit
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.3f) 
        {
            dialogText.text = receiver.cardName + " is wounded";
            yield return StartCoroutine(receiver.AddEffect(1,Random.Range(2, 5)));//bleed
        }
        Debug.Log(attacker.cardName+" -> Katana => "+receiver.cardName);
    }
    //84
    public IEnumerator Nodachi(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " slashes with Nodachi ";
		receiver.TakeDamage(3 + ((attacker.knowledge + attacker.strength + attacker.speed) / 3) - ((receiver.defense - receiver.speed) / 2));
        if (Random.value <= 0.2f) receiver.TakeDamage(3);//critical hit
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.3f) 
        {
            dialogText.text = receiver.cardName + " is wounded";
            yield return StartCoroutine(receiver.AddEffect(1,Random.Range(2, 5)));//bleed
        }
        Debug.Log(attacker.cardName+" -> Nodachi => "+receiver.cardName);
    }
    //85
    public IEnumerator Yumi(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " shoots arrow";
        if (Random.value <= (attacker.strength / 10f))
        {
		receiver.TakeDamage(3);
        }
        else
        {
            yield return new WaitForSeconds(1);
            dialogText.text = "arrow missed";
        }
        Debug.Log(attacker.cardName+" -> Yumi => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //86
    public IEnumerator Jujutsu(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + "'s Jujutsu Takedown";
		receiver.TakeDamage(attacker.attack + attacker.knowledge - receiver.defense);
        if (Random.value <= 0.3f) receiver.HandleDefense(2);//critical hit
        yield return new WaitForSeconds(2);
        Debug.Log(attacker.cardName+" -> Jujutsu => "+receiver.cardName);
	}
    //87
    public IEnumerator Espionage(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " spies on enemy";
		receiver.HandleDefense(-((attacker.knowledge + attacker.charisma) / 7));
        receiver.HandleSpeed(-((attacker.knowledge + attacker.charisma) / 7));
        Debug.Log(attacker.cardName+" -> Espionage => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //88
    public IEnumerator Sabre(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " cuts with Sabre ";
		receiver.TakeDamage(3 + ((attacker.attack + attacker.strength + attacker.speed) / 3) - ((receiver.defense - receiver.speed) / 2));
        if (Random.value <= 0.2f) receiver.TakeDamage(3);//critical hit
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.3f) 
        {
            dialogText.text = receiver.cardName + " is wounded";
            yield return StartCoroutine(receiver.AddEffect(1,Random.Range(2, 5)));//bleed
        }
        Debug.Log(attacker.cardName+" -> Sabre => "+receiver.cardName);
    }
    //89
    public IEnumerator Gamble(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " is raising bets";
        yield return new WaitForSeconds(1);
        int bet = Random.Range(1, 3);
        if (Random.value <= 0.66f)
        {
            dialogText.text = "And wins";
            attacker.HandleCharisma(bet);
            receiver.HandleCharisma(-bet);
        }
        else
        {
            dialogText.text = "And loses";
            attacker.HandleCharisma(-bet);
            receiver.HandleCharisma(bet);
        }
        Debug.Log(attacker.cardName+" -> Gamble => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //90
    public IEnumerator Philosophy(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " philosophizes!!!";
        yield return new WaitForSeconds(1);
        if (attacker.knowledge <= receiver.knowledge) 
        {
            dialogText.text = receiver.cardName + " is intersted in philosophy";
            receiver.HandleKnowledge(1);
        }
        else if (5 <= receiver.knowledge) 
        {
            dialogText.text = receiver.cardName + " is bored by Philosophy";
            if (Random.value <= 0.3f) 
            {
                yield return new WaitForSeconds(1);
                dialogText.text = receiver.cardName + " falls asleep";
                yield return StartCoroutine(receiver.AddEffect(3,Random.Range(1, 3)));//sleep
            }
        }
        else
        {
            dialogText.text = receiver.cardName + " doesn't understand that Philosophy";
            if (Random.value <= 0.9f) 
            {
                yield return new WaitForSeconds(1);
                dialogText.text = receiver.cardName + " is confused";
                yield return StartCoroutine(receiver.AddEffect(17,Random.Range(2, 5)));//confusion
            }
        }
        yield return new WaitForSeconds(2);
        Debug.Log(attacker.cardName+" -> Philosophy => "+receiver.cardName);
	}
    //91
    public IEnumerator Calm(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " calmed down his enemy";
        StartCoroutine(receiver.AddEffect(21,3));//calm
        receiver.HandleStrength(-3);
        receiver.HandleAttack(-3);
        Debug.Log(attacker.cardName+" -> Calm => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //92
    public IEnumerator Honesty(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + "is brutally honest";
		receiver.TakeDamage(((attacker.knowledge + attacker.attack + attacker.charisma)/2) - (receiver.defense));
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.1f)
        {
            attacker.HandleStrength(Random.Range(-3,0));
            attacker.HandleSpeed(Random.Range(-3,0));
            attacker.HandleAttack(Random.Range(-3,0));
            attacker.HandleDefense(Random.Range(-3,0));
            attacker.HandleCharisma(Random.Range(-3,0));
            yield return StartCoroutine(attacker.AddEffect(13,1));//Depression
            dialogText.text = attacker.cardName + " feels bad for enemy";
            yield return new WaitForSeconds(2);
        }
        Debug.Log(attacker.cardName + " -> Punch => " + receiver.cardName);
	}
    //93
    public IEnumerator Valaska(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + "'s valaska strikes";
		receiver.TakeDamage(3 + ((attacker.attack + attacker.strength + attacker.speed) / 3) - ((receiver.defense - receiver.speed) / 2));
        if (Random.value <= 0.5f) receiver.TakeDamage(2);//critical hit
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.1f) 
        {
            dialogText.text = receiver.cardName + " is wounded";
            yield return StartCoroutine(receiver.AddEffect(1,Random.Range(2, 4)));//bleed
        }
        Debug.Log(attacker.cardName+" -> Valaska => "+receiver.cardName);
    }
    //94
    public IEnumerator Moonshine(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " is getting pretty drunk";
        attacker.Heal(1);
        attacker.HandleStrength(2);
        attacker.HandleAttack(2);
        attacker.HandleDefense(-2);
        yield return new WaitForSeconds(2);
        if (Random.value <= 0.1f) 
        {
            dialogText.text = receiver.cardName + " falls asleep";
            yield return StartCoroutine(attacker.AddEffect(3,Random.Range(1, 3)));//sleep
        }
        else if (Random.value <= 0.2f) 
        {
            dialogText.text = attacker.cardName + " is furious!";
            StartCoroutine(attacker.AddEffect(6,3));//fury
            attacker.HandleStrength(5);
            attacker.HandleAttack(5);
            attacker.HandleDefense(-3);
        }
        Debug.Log(attacker.cardName+" -> Moonshine => "+receiver.cardName);
	}
    //95
    public IEnumerator OutlawBand(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + "'s Band Attacks";
        yield return new WaitForSeconds(2);
        int loot = Random.Range(1,3);
        if (receiver.charisma > receiver.strength) 
        {
            receiver.TakeDamage(Random.Range(5, 10));
            attacker.HandleCharisma(loot);
            receiver.HandleCharisma(-loot);
        }
        else
        {
            dialogText.text = "The outlaw band stumbled";
            attacker.TakeDamage(Random.Range(3, 7));
            yield return new WaitForSeconds(2);
        }

        Debug.Log(attacker.cardName+" -> OutlawBand => "+receiver.cardName);
	}
    //96
    public IEnumerator FlintlockPistol(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " is cleaning his flintlock";
        StartCoroutine(attacker.AddEffect(22,Random.Range(1, 2)));//Reloading
        attacker.state = CardState.STAY;
        attacker.HandleDefense(-1);
        yield return new WaitForSeconds(2);
        Debug.Log(attacker.cardName+" -> FlintlockPistol => "+receiver.cardName);
    }
    //97
    public IEnumerator PassiveResistance(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " is resisting passively!!!";
        receiver.HandleAttack(-1);
        attacker.HandleDefense(2);
        yield return new WaitForSeconds(2);
        Debug.Log(attacker.cardName+" -> PassiveResistance => "+receiver.cardName);
	}
    //98
    public IEnumerator HungerStrike(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " refuses to eat";
		attacker.TakeDamage(5);
        yield return new WaitForSeconds(2);
        if (Random.value <= ((40 - receiver.strength) / 30f))
        {
            receiver.HandleStrength(Random.Range(-3,0));
            receiver.HandleSpeed(Random.Range(-3,0));
            receiver.HandleAttack(Random.Range(-3,0));
            receiver.HandleDefense(Random.Range(-3,0));
            receiver.HandleCharisma(Random.Range(-3,0));
            yield return StartCoroutine(receiver.AddEffect(13,1));//Depression
            dialogText.text = receiver.cardName + " feels bad for enemy";
        }
        else
        {
            dialogText.text = receiver.cardName + " desn't care";
        }
        yield return new WaitForSeconds(2);
        Debug.Log(attacker.cardName + " -> HungerStrike => " + receiver.cardName);
	}
    //99
    public IEnumerator Gladius(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " attacks with gladius";
		receiver.TakeDamage(2 + ((attacker.attack + attacker.strength + attacker.speed) / 3) - ((receiver.defense - receiver.speed) / 2));
        if (Random.value <= 0.5f) receiver.TakeDamage(4);//critical hit
        yield return new WaitForSeconds(2);
        Debug.Log(attacker.cardName+" -> Gladius => "+receiver.cardName);
    }
    //100
    public IEnumerator ShieldBash(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " smashes with his shield";
		receiver.TakeDamage(2);
        if (Random.value <= 0.5f) attacker.HandleDefense(2);//critical hit
        yield return new WaitForSeconds(2);
        Debug.Log(attacker.cardName+" -> ShieldBash => "+receiver.cardName);
    }
    //101
    public IEnumerator Yperit(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        if (Random.value <= 0.9f) 
        {
            dialogText.text = "Yperit strikes";
            receiver.TakeDamage(Random.Range(5, 10));
        }
        else
        {
            dialogText.text = "A breeze blows";
            attacker.TakeDamage(Random.Range(3, 7));
        }
        yield return new WaitForSeconds(2);
        Debug.Log(attacker.cardName+" -> Yperit => "+receiver.cardName);
	}
    //102
    public IEnumerator Blitzkrieg(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        if (attacker.speed < receiver.speed)
        {
            dialogText.text = "Too slow, buddy";
            attacker.TakeDamage(receiver.speed - attacker.speed);
        }
        else
        {
            dialogText.text = attacker.cardName+" uses Blitzkrieg tactics";
            receiver.TakeDamage(3 + attacker.speed - receiver.speed);
        }
        Debug.Log(attacker.cardName+" -> Blitzkrieg => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //103
    public IEnumerator Propaganda(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " uses propaganda";
		receiver.HandleAttack(-((attacker.knowledge + attacker.charisma) / 7));
        receiver.HandleKnowledge(-((attacker.knowledge + attacker.charisma) / 7));
        Debug.Log(attacker.cardName+" -> Propaganda => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //104
    public IEnumerator Retiarius(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " throws the net";
        yield return new WaitForSeconds(2);
        if (attacker.attack > receiver.speed && Random.value <= 0.9f) 
        {
            StartCoroutine(receiver.AddEffect(9,1));//Tether
            dialogText.text = receiver.cardName + " was captured";
            attacker.state = CardState.STAY;
            StartCoroutine(attacker.AddEffect(23,1));//Trident
            yield return new WaitForSeconds(2);
        }
        Debug.Log(attacker.cardName+" -> Retiarius => "+receiver.cardName);
	}
    //105
    public IEnumerator Shuriken(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " is throwing stars";
        if (Random.value <= ((20 + attacker.attack) / 30f))
        {
            receiver.TakeDamage(1);
            receiver.HandleSpeed(-1);
        }
        else
        {
            yield return new WaitForSeconds(1);
            dialogText.text = "throw missed";
        }
        Debug.Log(attacker.cardName+" -> Shuriken => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //106
    public IEnumerator Kusarigama(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + "'s Kusarigama strikes";
		receiver.TakeDamage(2 + ((attacker.attack + attacker.knowledge + attacker.speed) / 3) - ((receiver.defense - receiver.speed) / 2));
        if (Random.value <= 0.1f) 
        {
            dialogText.text = receiver.cardName + " is trapped in chain";
            yield return StartCoroutine(receiver.AddEffect(9,1));//Tether
        }
        Debug.Log(attacker.cardName+" -> Kusarigama => "+receiver.cardName);
    }
    //107
    public IEnumerator Ninjutsu(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        List<AttackDelegate> attacks = new List<AttackDelegate> { Kick, Punch, Shuriken, Kusarigama };

        System.Random rnd = new System.Random();
        attacks = attacks.OrderBy(x => rnd.Next()).ToList();

        yield return StartCoroutine(attacks[0](attacker, receiver, dialogText));
        yield return StartCoroutine(attacks[1](attacker, receiver, dialogText));

        Debug.Log(attacker.cardName + " -> Ninjutsu => " + receiver.cardName);
    }
    //108
    public IEnumerator OrientalSpice(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " offers some spices to enemy";
        yield return StartCoroutine(receiver.AddEffect(24,0));//Poison
        yield return new WaitForSeconds(2);
        Debug.Log(attacker.cardName+" -> OrientalSpice => "+receiver.cardName);
	}
    //109
    public IEnumerator FieryArquebus(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = "Bum! Fiery Arquebus gun explosed";
		receiver.TakeDamage(Random.Range(1,Random.Range(3, 7)));
        if (Random.value <= 0.5f) attacker.TakeDamage(Random.Range(2, 5));
        yield return new WaitForSeconds(2);
        Debug.Log(attacker.cardName+" -> FieryArquebus => "+receiver.cardName);
	}
    //110
    public IEnumerator PirateRaid(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        if (((attacker.speed + attacker.charisma) / 2) < receiver.defense && Random.value <= 0.8f)
        {
            dialogText.text = "Raiding doesn't went well";
            attacker.TakeDamage(receiver.defense);
        }
        else
        {
            dialogText.text = attacker.cardName+" raids the enemy";
            receiver.TakeDamage((attacker.speed + attacker.charisma) / 2);
        }
        Debug.Log(attacker.cardName+" -> PirateRaid => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //111
    public IEnumerator Axe(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " attacks with axe";
		receiver.TakeDamage(((attacker.attack + (attacker.strength * 2)) / 3) - ((receiver.defense - receiver.speed) / 2));
        if (Random.value <= 0.5f) receiver.TakeDamage(6);//critical hit
        yield return new WaitForSeconds(2);
        Debug.Log(attacker.cardName+" -> Axe => "+receiver.cardName);
    }
    //112
    public IEnumerator JaguarWarriors(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " sends Jaguar Warriors";
		receiver.TakeDamage(Random.Range(1, attacker.attack+2));
        if (Random.value <= 0.2f) receiver.HandleAttack(-(Random.Range(2, 4)));//critical hit
        yield return new WaitForSeconds(2);
        Debug.Log(attacker.cardName+" -> JaguarWarriors => "+receiver.cardName);
    }
    //113
    public IEnumerator Atlatl(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " throws spear";
        if (Random.value <= (attacker.strength / 10f))
        {
            receiver.TakeDamage(3);
            if (Random.value <= 0.2f) yield return StartCoroutine(receiver.AddEffect(1,Random.Range(2, 4)));//bleed
        }
        else
        {
            yield return new WaitForSeconds(1);
            dialogText.text = "spear missed";
        }
        Debug.Log(attacker.cardName+" -> Atlatl => "+receiver.cardName);
        yield return new WaitForSeconds(2);
	}
    //114
    public IEnumerator Macuahuitl(Kard attacker, Kard receiver, TMP_Text dialogText)
	{
        dialogText.text = attacker.cardName + " smashes with Macuahuitl";
		receiver.TakeDamage(((attacker.attack + attacker.strength * 2) / 2) - ((receiver.defense - receiver.speed) / 2));
        if (Random.value <= 0.2f) receiver.TakeDamage(3);//critical hit
        yield return new WaitForSeconds(2);
        Debug.Log(attacker.cardName+" -> Macuahuitl => "+receiver.cardName);
    }






    public int[] DistributeRandomly(int value)
    {
        int[] result = new int[6];

        // Nastaviť všetky prvky poľa na hodnotu 0
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = 0;
        }

        // Pridať zvyšok hodnoty do pola náhodne
        int remainingValue = value;

        while (remainingValue > 0)
        {
            for (int j = 0; j < result.Length && remainingValue > 0; j++)
            {
                if (UnityEngine.Random.Range(0f, 1f) < 0.2f)
                {
                    result[j] += 1;
                    remainingValue--;
                }
            }
        }

        return result;
    }

}

