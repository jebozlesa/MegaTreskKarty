using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class Attack : MonoBehaviour
{
   // public MoveImage moveImageAnimation;
    public AttackAnimations attackAnimations;

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

    private IEnumerator ShowAttackDialog(TMP_Text dialogText, string message)
    {
        dialogText.text = "";
        yield return new WaitForSeconds(0.2f);
        dialogText.text = message;
        yield return new WaitForSeconds(0.8f);
    }

    private IEnumerator ShowDialog(TMP_Text dialogText, string message)
    {
        dialogText.text = "";
        yield return new WaitForSeconds(0.2f);
        dialogText.text = message;
        yield return new WaitForSeconds(1.8f);
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
                yield return StartCoroutine(TakeOff(attacker, receiver, dialogText));
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
            case 115:
                yield return StartCoroutine(Cubism(attacker, receiver, dialogText));
                break;
            case 116:
                yield return StartCoroutine(CosaNostra(attacker, receiver, dialogText));
                break;
            case 117:
                yield return StartCoroutine(Act(attacker, receiver, dialogText));
                break;
            case 118:
                yield return StartCoroutine(Football(attacker, receiver, dialogText));
                break;
            case 119:
                yield return StartCoroutine(BicycleKick(attacker, receiver, dialogText));
                break;
            case 120:
                yield return StartCoroutine(WorldChampion(attacker, receiver, dialogText));
                break;
            case 121:
                yield return StartCoroutine(ShaolinSoccer(attacker, receiver, dialogText));
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
        yield return StartCoroutine(attackAnimations.PlayPunchAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(((attacker.strength + attacker.attack) / 2) - ((receiver.defense + receiver.strength) / 2));   //DEMIDZ
        yield return StartCoroutine(ShowDialog(dialogText, "Puf! punch from " + attacker.cardName));                        //TEXT A CAKANI                             <--- POUPRAVOVAT

        if (Random.value <= 0.1f)
        {
            yield return StartCoroutine(receiver.AddEffect(3, Random.Range(1, 3))); //sleep
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " falls asleep"));
        }

        Debug.Log(attacker.cardName + " -> Punch => " + receiver.cardName);
    }

    //2
    public IEnumerator Kick(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(attackAnimations.PlayKickAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(((attacker.speed + attacker.attack) / 2) - ((receiver.defense + receiver.strength) / 2));
        if (Random.value <= 0.2f) receiver.TakeDamage(4); //extra dmg
        yield return StartCoroutine(ShowDialog(dialogText, "Plesk! kick from " + attacker.cardName));

        Debug.Log(attacker.cardName + " -> Kick => " + receiver.cardName);
    }
    //3
    public IEnumerator Heal(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(attackAnimations.PlayHealAnimation(attacker.transform));        //ANIMACIA
        attacker.Heal(attacker.knowledge / 2);
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " Heals himself"));

        Debug.Log(attacker.cardName + " -> Heal => " + attacker.cardName);
    }
    //4
    public IEnumerator Forgiveness(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(attackAnimations.PlayForgivenessAnimation(attacker.transform));        //ANIMACIA
        receiver.HandleAttack(-(int)System.Math.Ceiling((double)attacker.charisma / 5));
        receiver.HandleAttack(-(int)System.Math.Ceiling((double)attacker.charisma / 5));
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " forgives your heresy"));

        if (Random.value <= 0.5f)
        {
            yield return StartCoroutine(receiver.AddEffect(2, Random.Range(1, 3))); //Asceticism
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " began to repent"));
        }

        Debug.Log(attacker.cardName + " -> Forgiveness => " + receiver.cardName);
    }
    //5
    public IEnumerator Crusade(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(attackAnimations.PlayCrusadeAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage((attacker.strength + attacker.charisma) - ((receiver.defense + receiver.charisma) / 2));
        yield return StartCoroutine(ShowDialog(dialogText, "In the name of Christ!!! damage was done"));

        Debug.Log(attacker.cardName + " -> Crusade => " + receiver.cardName);
    }
    //6
    public IEnumerator WaterToWine(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(attackAnimations.PlayWaterToWineAnimation(attacker.transform));        //ANIMACIA
        attacker.HandleAttack(2);
        attacker.HandleStrength(2);
        attacker.HandleDefense(-1);
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " changes his water to wine"));

        Debug.Log(attacker.cardName + " -> WaterToWine => " + attacker.cardName);
    }
    //7
    public IEnumerator CarHit(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(attackAnimations.PlayCarHitAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage((attacker.speed * 3) - receiver.strength - receiver.defense);
        yield return StartCoroutine(ShowDialog(dialogText, "Tresk! hit by " + attacker.cardName + "'s car"));

        Debug.Log(attacker.cardName + " -> CarHit => " + receiver.cardName);
    }
    //8
    public IEnumerator SteamGun(Kard attacker, Kard receiver, TMP_Text dialogText)                  //ASI DAT DOPICI
    {
        receiver.TakeDamage(Random.Range(1, attacker.knowledge));
        if (Random.value <= 0.5f) attacker.TakeDamage(attacker.knowledge / 5);
        yield return StartCoroutine(ShowDialog(dialogText, "Bum! Steam gun exploded"));

        Debug.Log(attacker.cardName + " -> SteamGun => " + receiver.cardName);
    }
    //9
    public IEnumerator Radiation(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        dialogText.text = attacker.cardName + " showing radiation";
        yield return StartCoroutine(attackAnimations.PlayRadioactivityAnimation(attacker.transform));        //ANIMACIA
        if (Random.value <= 0.8f)
        {
            yield return StartCoroutine(receiver.AddEffect(4, 0)); //exposure
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is hit by radiation"));
        }
        if (Random.value <= 0.3f)
        {
            yield return StartCoroutine(attacker.AddEffect(4, 0)); //exposure
            yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " is hit by radiation"));
        }

        Debug.Log(attacker.cardName + " -> Radiation => " + receiver.cardName);
    }
    //10
    public IEnumerator Scratch(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(attackAnimations.PlayScratchAnimation(attacker.transform));        //ANIMACIA
        receiver.TakeDamage(attacker.attack - receiver.defense);
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " scratches opponent!"));

        if (Random.value <= 0.1f)
        {
            yield return StartCoroutine(receiver.AddEffect(1, Random.Range(1, 3))); //bleed
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is wounded"));
        }

        Debug.Log(attacker.cardName + " -> Scratch => " + receiver.cardName);
    }
    //11
    public IEnumerator ScientificLecture(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(attackAnimations.PlayScientificLectureAnimation(attacker.transform));        //ANIMACIA
        receiver.TakeDamage(attacker.knowledge - receiver.knowledge);
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " explaining science!!!"));

        if (Random.value <= 0.3f) 
        {
            yield return StartCoroutine(receiver.AddEffect(3, Random.Range(1, 3))); //sleep
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " falls asleep"));
        }

        Debug.Log(attacker.cardName + " -> ScientificLecture => " + receiver.cardName);
    }
    //12
    public IEnumerator ChiSau(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        int multiply = Random.Range(1, 5);

        yield return StartCoroutine(attackAnimations.PlayChiSauAnimation(attacker.transform, receiver.transform, multiply));        //ANIMACIA
        receiver.TakeDamage(multiply * (attacker.speed - receiver.defense));
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " attacks with his sticky hands"));

        Debug.Log(attacker.cardName + " -> ChiSau => " + receiver.cardName);
    }
    //13
    public IEnumerator OneInchPunch(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(attackAnimations.PlayOneInchPunchAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(attacker.attack + attacker.knowledge - receiver.defense);
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " pokes enemy with finger"));

        if (Random.value <= 0.2f) receiver.TakeDamage(attacker.strength); //critical hit

        Debug.Log(attacker.cardName + " -> OneInchPunch => " + receiver.cardName);
    }
    //14
    public IEnumerator UpInSmoke(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(attackAnimations.PlayUpInSmokeAnimation(attacker.transform));        //ANIMACIA
        attacker.Heal((int)System.Math.Ceiling((double)attacker.strength / 4));
        attacker.HandleStrength(1);
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " smokes some s#!t and feels good"));

        if (Random.value <= 0.1f) 
        {
            yield return StartCoroutine(attacker.AddEffect(3, Random.Range(1, 3))); //sleep
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " falls asleep"));
        }

        Debug.Log(attacker.cardName + " -> UpInSmoke => " + receiver.cardName);
    }
    //15
    public IEnumerator Sing(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(attackAnimations.PlaySingAnimation(attacker.transform));        //ANIMACIA
        if (Random.value <= 0.5f) receiver.HandleAttack(-(attacker.charisma / 3));
        if (Random.value <= 0.5f) receiver.HandleDefense(-(attacker.charisma / 3));
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " sings a banger"));

        Debug.Log(attacker.cardName + " -> Sing => " + receiver.cardName);
    }
    //16
    public IEnumerator Revolver(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Revolver"));

        if (Random.value <= (attacker.attack / 15f))
        {
            yield return StartCoroutine(attackAnimations.PlayRevolverAnimation(attacker.transform, receiver.transform, true));        //ANIMACIA
            receiver.TakeDamage(7);
            yield return StartCoroutine(ShowDialog(dialogText, "Bang! " + attacker.cardName + " hits target"));
            
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayRevolverAnimation(attacker.transform, receiver.transform, false));        //ANIMACIA
            yield return StartCoroutine(ShowDialog(dialogText, "Bang! aaaand miss"));
        }

        Debug.Log(attacker.cardName + " -> Revolver => " + receiver.cardName);
    }
    //17
    public IEnumerator ArtilleryRegiment(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Artillery Regiment"));

        if (Random.value <= 0.7f)
        {
            yield return StartCoroutine(attackAnimations.PlayArtilleryRegimentAnimation(attacker.transform, receiver.transform, true));        //ANIMACIA
            receiver.TakeDamage(15);
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is heavily bombarded"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayArtilleryRegimentAnimation(attacker.transform, receiver.transform, false));        //ANIMACIA
            yield return StartCoroutine(ShowDialog(dialogText, "Bang! aaaand miss"));
        }

        Debug.Log(attacker.cardName + " -> ArtilleryRegiment => " + receiver.cardName);
    }
    //18
    public IEnumerator BloodSucking(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Blood Sucking"));

        yield return StartCoroutine(attackAnimations.PlayBloodSuckingAnimation(receiver.transform, attacker.transform));        //ANIMACIA
        int a = Random.Range(1, attacker.attack);
        receiver.TakeDamage(a);
        attacker.Heal(a);
        yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " was drained"));

        Debug.Log(attacker.cardName + " -> BloodSucking => " + receiver.cardName);
    }
    //19
    public IEnumerator Sword(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Sword"));
        yield return StartCoroutine(attackAnimations.PlaySwordAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(3 + ((attacker.strength + attacker.speed + attacker.attack) / 3) - ((receiver.defense - receiver.strength) / 2));
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " cuts enemy with sword"));

        if (Random.value <= 0.4f) 
        {
            yield return StartCoroutine(receiver.AddEffect(1, Random.Range(1, 6))); //bleed
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is wounded"));
        }

        Debug.Log(attacker.cardName + " -> Sword => " + receiver.cardName);
    }
    //20
    public IEnumerator Pike(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Pike"));
        yield return StartCoroutine(attackAnimations.PlayPikeAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(((attacker.strength + attacker.speed + attacker.attack) / 2) - receiver.defense);
        yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is impaled on a pike"));

        if (Random.value <= 0.2f) 
        {
            yield return StartCoroutine(receiver.AddEffect(1, Random.Range(1, 4))); //bleed
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is wounded"));
        }

        Debug.Log(attacker.cardName + " -> Pike => " + receiver.cardName);
    }
    //21
    public IEnumerator Terrify(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Terrify"));
        yield return StartCoroutine(attackAnimations.PlayTerrifyAnimation(attacker.transform));        //ANIMACIA

        if (Random.value <= (int)System.Math.Ceiling((double)receiver.strength / 20))
        {
            receiver.HandleStrength((-1) - (attacker.attack/7));
            receiver.HandleAttack((-1) - (attacker.attack/7));
            receiver.HandleDefense(1);
            receiver.HandleCharisma((-(attacker.attack/3)));
            StartCoroutine(attackAnimations.PlayAnimationTerrified(receiver.transform));        //ANIMACIA
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is terribly frightened"));
        }
        else
        {
            StartCoroutine(attackAnimations.PlayAnimationNotImpressed(receiver.transform));        //ANIMACIA
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " does not fear you"));
        }

        Debug.Log(attacker.cardName + " -> Terrify => " + receiver.cardName);
    }
    //22
    public IEnumerator DrinkWine(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Drink Wine"));
        yield return StartCoroutine(attackAnimations.PlayDrinkWineAnimation(attacker.transform));        //ANIMACIA
        attacker.HandleStrength(2);
        attacker.Heal(attacker.charisma/3);
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " is getting slightly drunk"));

        Debug.Log(attacker.cardName + " -> DrinkWine => " + receiver.cardName);
    }
    //23
    public IEnumerator FlamingGun(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Flaming Gun"));
        yield return StartCoroutine(attackAnimations.PlayFlamingGunAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(4);
        yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is being caramelized"));

        if (Random.value <= 0.1f) 
        {
            yield return StartCoroutine(receiver.AddEffect(16, 1)); //burn
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is burning"));
        }

        Debug.Log(attacker.cardName + " -> FlamingGun => " + receiver.cardName);
    }
    //24
    public IEnumerator Cleaver(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Cleaver"));
        yield return StartCoroutine(attackAnimations.PlayCleaverAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(1 + ((attacker.strength + attacker.attack) / 2) - ((receiver.defense - receiver.strength) / 2));
        yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " gets chopped by cleaver"));

        if (Random.value <= 0.3f) 
        {
            yield return StartCoroutine(receiver.AddEffect(1, Random.Range(2, 5))); //bleed
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is wounded"));
        }

        Debug.Log(attacker.cardName + " -> Cleaver => " + receiver.cardName);
    }
    //25
    public IEnumerator Pan(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Pan"));
        yield return StartCoroutine(attackAnimations.PlayPanAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(1 + attacker.strength - receiver.defense);
        yield return StartCoroutine(ShowDialog(dialogText, "Tongggg!!! " + receiver.cardName + " gets hit by the pan"));

        if (Random.value <= 0.3f)
        {
            yield return StartCoroutine(receiver.AddEffect(3, Random.Range(2, 4))); //sleep
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " falls asleep"));
        }

        Debug.Log(attacker.cardName + " -> Pan => " + receiver.cardName);
    }
    //26
    public IEnumerator Boost(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Boost"));
        yield return StartCoroutine(attackAnimations.PlayBoostAnimation(attacker.transform));        //ANIMACIA
        attacker.HandleStrength(2);
        attacker.HandleAttack(2);
        attacker.HandleDefense(2);
        attacker.HandleCharisma(-1);
        attacker.HandleKnowledge(-2);
        attacker.RemoveEffectById(3);
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " gets some good boost"));

        Debug.Log(attacker.cardName + " -> Boost => " + attacker.cardName);
    }
    //27
    public IEnumerator Temptation(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Temptation"));
        yield return StartCoroutine(attackAnimations.PlayTemptationAnimation(receiver.transform));        //ANIMACIA
        receiver.HandleAttack(- (attacker.charisma - receiver.charisma));
        receiver.HandleDefense(- (attacker.charisma - receiver.charisma));
        receiver.HandleCharisma(1);
        receiver.HandleKnowledge(- ((attacker.charisma - receiver.charisma) / 2));
        yield return StartCoroutine(ShowDialog(dialogText, "ohhh!!! " + receiver.cardName + " falls in love <3"));

        Debug.Log(attacker.cardName + " -> Temptation => " + receiver.cardName);
    }
    //28
    public IEnumerator Shamshir(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Shamshir"));
        yield return StartCoroutine(attackAnimations.PlayShamshirAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(3 + ((attacker.strength + attacker.speed + attacker.attack) / 3) - ((receiver.defense - receiver.strength) / 2));
        if (Random.value <= 0.2f)
        {
            receiver.TakeDamage(3); //critical hit
        }
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " cuts with shamshir"));

        if (Random.value <= 0.5f) 
        {
            yield return StartCoroutine(receiver.AddEffect(1, Random.Range(2, 6))); //bleed
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is wounded"));
        }

        Debug.Log(attacker.cardName + " -> Shamshir => " + receiver.cardName);
    }
    //29
    public IEnumerator Diplomacy(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Diplomacy"));
        yield return StartCoroutine(attackAnimations.PlayDiplomacyAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.HandleDefense(-(int)System.Math.Ceiling((double)attacker.charisma / 10));
        receiver.HandleKnowledge(-(int)System.Math.Ceiling((double)attacker.charisma / 10));
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " made diplomatic gesture"));

        Debug.Log(attacker.cardName + " -> Diplomacy => " + receiver.cardName);
    }
    //30
    public IEnumerator Siege(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Siege"));
        yield return StartCoroutine(attackAnimations.PlaySiegeAnimation(receiver.transform));        //ANIMACIA
        StartCoroutine(attacker.AddEffect(5, 3)); //siege
        attacker.state = CardState.STAY;
        attacker.HandleDefense(10);
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " is building siege equipment"));

        Debug.Log(attacker.cardName + " -> Siege => " + receiver.cardName);
    }

    //31
    public IEnumerator TreeStratagem(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Tree Stratagem"));
        yield return StartCoroutine(attackAnimations.PlayTreeStratagemAnimation(receiver.transform));        //ANIMACIA
        if (attacker.knowledge >= receiver.knowledge)
        {
            receiver.TakeDamage(Random.Range(1, 3));
            yield return StartCoroutine(ShowDialog(dialogText, "Trees attacks!"));
        }
        else
        {
            StartCoroutine(attackAnimations.PlayAnimationNotImpressed(receiver.transform));        //ANIMACIA
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + ": Yo think am stupid?!"));
        }

        Debug.Log(attacker.cardName + " -> TreeStratagem => " + receiver.cardName);
    }

    //32
    public IEnumerator Tomahawk(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Tomahawk"));
        yield return StartCoroutine(attackAnimations.PlayTomahawkAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(2 + ((attacker.strength + attacker.speed + attacker.attack) / 3) - receiver.defense);

        if (Random.value <= 0.2f)
        {
            receiver.TakeDamage(5); //critical hit
        }
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " hits with tomahawk"));

        if (Random.value <= 0.2f) 
        {
            yield return StartCoroutine(receiver.AddEffect(1, Random.Range(2, 6))); //bleed
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is wounded"));
        }

        Debug.Log(attacker.cardName + " -> Tomahawk => " + receiver.cardName);
    }

    //33
    public IEnumerator PeacePipe(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Peace Pipe"));
        StartCoroutine(attackAnimations.PlayUpInSmokeAnimation(receiver.transform));        //ANIMACIA
        yield return StartCoroutine(attackAnimations.PlayUpInSmokeAnimation(attacker.transform));        //ANIMACIA

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
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " offers peace pipe"));

        Debug.Log(attacker.cardName + " -> PeacePipe => " + receiver.cardName);
    }

    //34
    public IEnumerator RecurveBow(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Recurve Bow"));
        if (Random.value <= (attacker.strength / 10f))
        {
            yield return StartCoroutine(attackAnimations.PlayRecurveBowAnimation(attacker.transform, receiver.transform, true));        //ANIMACIA
            receiver.TakeDamage(5);
            yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " shoots arrow"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayRecurveBowAnimation(attacker.transform, receiver.transform, false));        //ANIMACIA
            yield return StartCoroutine(ShowDialog(dialogText, "arrow missed"));
        }
        Debug.Log(attacker.cardName + " -> RecurveBow => " + receiver.cardName);
    }

    //35
    public IEnumerator Fury(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Fury"));
        yield return StartCoroutine(attackAnimations.PlayFuryAnimation(attacker.transform));        //ANIMACIA
        StartCoroutine(attacker.AddEffect(6, 3)); //fury
        attacker.HandleStrength(5);
        attacker.HandleAttack(5);
        attacker.HandleDefense(-3);
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " is furious!"));
        Debug.Log(attacker.cardName + " -> Fury => " + receiver.cardName);
    }

    //36
    public IEnumerator Guerilla(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Guerilla"));
        yield return StartCoroutine(attackAnimations.PlayGuerillaAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(Random.Range(1, attacker.attack + 2));
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " sends Guerillas"));
        if (Random.value <= 0.2f) receiver.HandleAttack(-(Random.Range(2, 4))); //critical hit
        Debug.Log(attacker.cardName + " -> Guerilla => " + receiver.cardName);
    }

    //37
    public IEnumerator Famine(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Famine"));
        if (receiver.CheckEffect(7))
        {
            StartCoroutine(attackAnimations.PlayAnimationNotImpressed(receiver.transform));        //ANIMACIA
            yield return StartCoroutine(ShowDialog(dialogText, "Be a human, " + attacker.cardName + "!"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayFamineAnimation(receiver.transform));        //ANIMACIA
            int r = Random.Range(5, 10);
            receiver.TakeDamage(2 * r);
            receiver.HandleDefense(-2 * r);
            StartCoroutine(receiver.AddEffect(7, r));
            yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " caused a famine"));
        }
        Debug.Log(attacker.cardName + " -> Famine => " + receiver.cardName);
    }

    //38
    public IEnumerator Marxism(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Marxism"));
        yield return StartCoroutine(attackAnimations.PlayMarxismAnimation(attacker.transform));        //ANIMACIA
        receiver.HandleKnowledge(-(int)System.Math.Ceiling((double)attacker.charisma / 3));
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " explains Marxism"));
        Debug.Log(attacker.cardName + " -> Marxism => " + receiver.cardName);
    }

    //39
    public IEnumerator TeslaCoil(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Tesla Coil"));
        yield return StartCoroutine(attackAnimations.PlayTeslaCoilAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(attacker.attack);
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " shocks enemy"));
        if (Random.value <= 0.7f)
        {
            yield return StartCoroutine(receiver.AddEffect(8, 5)); //shock
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is shocked"));
        }
        Debug.Log(attacker.cardName + " -> TeslaCoil => " + receiver.cardName);
    }

    //40
    public IEnumerator WirelessCharger(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Wireless Charger"));
        yield return StartCoroutine(attackAnimations.PlayWirelessChargerAnimation(attacker.transform));        //ANIMACIA
        attacker.HandleAttack(Random.Range(2, 4));
        attacker.Heal(1);
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " is charging wirelessly!!!"));
        Debug.Log(attacker.cardName + " -> WirelessCharger => " + attacker.cardName);
    }

    //41
    public IEnumerator Experiment(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Experiment"));
        yield return StartCoroutine(attackAnimations.PlayExperimentAnimation(attacker.transform));        //ANIMACIA
        receiver.TakeDamage(Random.Range(0, 2));
        attacker.TakeDamage(Random.Range(0, 2));
        attacker.HandleKnowledge(2);
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " is trying something"));
        Debug.Log(attacker.cardName + " -> Experiment => " + attacker.cardName);
    }

    //42
    public IEnumerator TommyGun(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Tommy Gun"));
        
        int hits = Random.Range(1, 10);

        yield return StartCoroutine(attackAnimations.PlayTommyGunAnimation(attacker.transform, receiver.transform, hits));        //ANIMACIA
        receiver.TakeDamage(hits);
        yield return StartCoroutine(ShowDialog(dialogText, "Ratatata!"));
        if (Random.value <= 0.5f)
        {
            yield return StartCoroutine(receiver.AddEffect(1, Random.Range(1, 4))); //bleed
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is wounded"));
        }
        Debug.Log(attacker.cardName + " -> TommyGun => " + receiver.cardName);
    }

    //43
    public IEnumerator TieUp(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Tie Up"));
        yield return StartCoroutine(attackAnimations.PlayTieUpAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        if (attacker.attack > receiver.speed && Random.value <= 0.9f)
        {
            StartCoroutine(attackAnimations.PlayAnimationTiedUp(receiver.transform));        //ANIMACIA
            yield return StartCoroutine(receiver.AddEffect(9, 1)); //Tether
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " was captured"));
        }
        else
        {
            StartCoroutine(attackAnimations.PlayAnimationNotImpressed(receiver.transform));        //ANIMACIA
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " runs away"));
        }
        Debug.Log(attacker.cardName + " -> TieUp => " + receiver.cardName);
    }

        //44
    public IEnumerator Corruption(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        int r = Random.Range(1, 3);
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Corruption"));
        yield return StartCoroutine(attackAnimations.PlayCorruptionAnimation(attacker.transform, r));        //ANIMACIA
        receiver.HandleSpeed(-r);
        receiver.HandleDefense(-r);
        attacker.HandleAttack(2 * r);
        attacker.HandleCharisma(-r);
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " corrupted everybody around"));
        Debug.Log(attacker.cardName + " -> Corruption => " + receiver.cardName);
    }

    //45
    public IEnumerator Colt1911(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Colt 1911"));
        if (Random.value <= (attacker.attack / 12f))
        {
            yield return StartCoroutine(attackAnimations.PlayColt1911Animation(attacker.transform, receiver.transform, true));        //ANIMACIA
            receiver.TakeDamage(7);
            yield return StartCoroutine(ShowDialog(dialogText, "Bang! " + attacker.cardName + " shoots"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayColt1911Animation(attacker.transform, receiver.transform, false));        //ANIMACIA
            yield return StartCoroutine(ShowDialog(dialogText, "Bang! aaaand miss"));
        }
        Debug.Log(attacker.cardName + " -> Colt1911 => " + receiver.cardName);
    }

    //46
    public IEnumerator Mortar(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Mortar"));
        if (Random.value <= 0.9f) 
        {
            yield return StartCoroutine(attackAnimations.PlayMortarAnimation(attacker.transform, receiver.transform, true));        //ANIMACIA
            receiver.TakeDamage(Random.Range(5, 10));
            yield return StartCoroutine(ShowDialog(dialogText, "Mortar fires"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayMortarAnimation(attacker.transform, attacker.transform, true));        //ANIMACIA
            attacker.TakeDamage(Random.Range(3, 7));
            yield return StartCoroutine(ShowDialog(dialogText, "Mortar exploded"));
        }
        Debug.Log(attacker.cardName + " -> Mortar => " + receiver.cardName);
    }

    //47
    public IEnumerator GreatArmy(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Great Army"));
        yield return StartCoroutine(attackAnimations.PlayGreatArmyAnimation(attacker.transform));        //ANIMACIA
        attacker.HandleStrength((int)System.Math.Ceiling((double)receiver.charisma / 10));
        attacker.HandleAttack((int)System.Math.Ceiling((double)receiver.charisma / 10));
        attacker.HandleDefense((int)System.Math.Ceiling((double)receiver.charisma / 10));
        attacker.HandleCharisma(1);
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " is assembling an army"));
        Debug.Log(attacker.cardName + " -> GrandArmee => " + receiver.cardName);
    }

    //48
    public IEnumerator ScorchedEarth(Kard attacker, Kard receiver, TMP_Text dialogText)                                                                     //   <==================TODO animaciu
    {
        yield return StartCoroutine(receiver.AddEffect(10,Random.Range(1, attacker.defense)));//Starving
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " is Burning all in his wake"));
        Debug.Log(attacker.cardName + " -> ScorchedEarth => " + receiver.cardName);
    }

    //49
    public IEnumerator DoubleEnvelopment(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Double Envelopment"));
        yield return StartCoroutine(attackAnimations.PlayDoubleEnvelopmentAnimation(attacker.transform));        //ANIMACIA
        StartCoroutine(attacker.AddEffect(11,1));//Envelopment
        attacker.state = CardState.STAY;
        attacker.HandleDefense(-3);
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " Launching the maneuver!"));
        Debug.Log(attacker.cardName + " -> DoubleEnvelopment => " + receiver.cardName);
    }

    //50
    public IEnumerator ContinentalBlockade(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Continental Blockade"));
        yield return StartCoroutine(attackAnimations.PlayContinentalBlockadeAnimation(attacker.transform, receiver.transform));        //ANIMACIA

        yield return StartCoroutine(receiver.AddEffect(12,5));//Blockade
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " Enforcing the blockade"));
        Debug.Log(attacker.cardName + " -> ContinentalBlockade => " + receiver.cardName);
    }

    //51
    public IEnumerator Depression(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Depression"));
        yield return StartCoroutine(attackAnimations.PlayDepressionAnimation(attacker.transform));        //ANIMACIA
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " is depressed"));
        if (receiver.CheckEffect(13))
        {
            // StartCoroutine(attackAnimations.PlayAnimationNotImpressed(receiver.transform));        //ANIMACIA
            // yield return StartCoroutine(ShowDialog(dialogText, "Be a human, " + attacker.cardName + "!"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayDepressionStartAnimation(receiver.transform));        //ANIMACIA
            receiver.HandleStrength(Random.Range(-3,0));
            receiver.HandleSpeed(Random.Range(-3,0));
            receiver.HandleAttack(Random.Range(-3,0));
            receiver.HandleDefense(Random.Range(-3,0));
            receiver.HandleCharisma(Random.Range(-3,0));
            yield return StartCoroutine(receiver.AddEffect(13,1));//Depression
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " feels bad for enemy"));
        }
        if (Random.value <= 0.3f) 
        {
            attacker.state = CardState.STAY;
            StartCoroutine(attackAnimations.PlayArtInspirationStartAnimation(attacker.transform));        //ANIMACIA
            yield return StartCoroutine(attacker.AddEffect(14, 3));//ArtInspiration
            yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " is empowered by muse"));
        }
        Debug.Log(attacker.cardName + " -> Depression => " + receiver.cardName);
    }

    //52
    public IEnumerator SelfIsolation(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses SelfIsolation"));
        yield return StartCoroutine(attackAnimations.PlaySelfIsolationAnimation(attacker.transform));        //ANIMACIA
        attacker.HandleDefense(3);
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " feels happy alone"));
        if (Random.value <= 0.3f) 
        {
            attacker.state = CardState.STAY;
            yield return StartCoroutine(attackAnimations.PlayArtInspirationStartAnimation(attacker.transform));        //ANIMACIA
            yield return StartCoroutine(attacker.AddEffect(14, 3));//ArtInspiration
            yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " is empowered by muse"));
        }
        Debug.Log(attacker.cardName + " -> SelfIsolation => " + attacker.cardName);
    }

    //53
    public IEnumerator Knife(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Knife"));
        yield return StartCoroutine(attackAnimations.PlayKnifeAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(1 + ((attacker.strength + attacker.speed + attacker.attack) / 3) - ((receiver.defense - receiver.strength) / 2));
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " cuts enemy with knife"));
        if (Random.value <= 0.3f) 
        {
            StartCoroutine(attackAnimations.PlayBleedStartAnimation(receiver.transform));        //ANIMACIA
            yield return StartCoroutine(receiver.AddEffect(1,Random.Range(1, 3)));//bleed
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is wounded"));
        }
        Debug.Log(attacker.cardName + " -> Knife => " + receiver.cardName);
    }
    //54
    public IEnumerator Autoportrait(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Autoportrait"));
        yield return StartCoroutine(attackAnimations.PlayAutoportraitAnimation(attacker.transform));        //ANIMACIA
        attacker.HandleKnowledge(1);
        attacker.HandleStrength(1);
        attacker.HandleDefense(1);
        attacker.state = CardState.STAY;
        yield return StartCoroutine(attacker.AddEffect(15,Random.Range(1, 3)));//Autoportrait
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " discovers himself through art"));
        Debug.Log(attacker.cardName + " -> Autoportrait => " + receiver.cardName);
    }

    //55
    public IEnumerator GravityPull(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        int r = Random.Range(0,4);
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Gravity Pull"));
        yield return StartCoroutine(attackAnimations.PlayGravityPullAnimation(attacker.transform, receiver.transform, r));        //ANIMACIA
        string[] objects = {"piano", "boiler", "anvil", "gases"};
        int[] weight = {6,5,4,0};
        if (r != 3) { receiver.TakeDamage(weight[r]); }
        yield return StartCoroutine(ShowDialog(dialogText, "Gravity pulled " + objects[r] + " to " + receiver.cardName));
        if (Random.value <= 0.1f && r != 3) 
        {
            yield return StartCoroutine(receiver.AddEffect(3,Random.Range(1, 3)));//sleep
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " falls asleep"));
        }
        Debug.Log(attacker.cardName + " -> GravityPull => " + receiver.cardName);
    }

    //56
    public IEnumerator Kamikaze(Kard attacker, Kard receiver, TMP_Text dialogText)
    { 
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Kamikaze"));
        int dmg = attacker.health;
        if (Random.value <= 0.9f) 
        {
            yield return StartCoroutine(attackAnimations.PlayKamikazeAnimation(attacker.transform, receiver.transform, true));        //ANIMACIA
            attacker.TakeDamage(dmg);
            receiver.TakeDamage(Random.Range(1, (attacker.knowledge + attacker.speed + attacker.attack)));
            yield return StartCoroutine(ShowDialog(dialogText, "Kamikaze strikes"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayKamikazeAnimation(attacker.transform, receiver.transform, false));        //ANIMACIA
            attacker.TakeDamage(dmg);
            yield return StartCoroutine(ShowDialog(dialogText, "Ou! Nasty miss"));
        }
        Debug.Log(attacker.cardName + " -> Kamikaze => " + receiver.cardName);
    }

    //57
    public IEnumerator TakeOff(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Take Off"));
        if (Random.value <= 0.75f) 
        {
            yield return StartCoroutine(attackAnimations.PlayTakeOffAnimation(attacker.transform));        //ANIMACIA
            attacker.HandleSpeed(3);
            yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " took off"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayTakeOffCrashAnimation(attacker.transform));        //ANIMACIA
            attacker.TakeDamage(Random.Range(1, attacker.speed));
            yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " crashes"));
        }
        Debug.Log(attacker.cardName + " -> TakeOff => " + receiver.cardName);
    }

    //58
    public IEnumerator AirStrike(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses AirStrike"));
        int hits = Random.Range(attacker.attack, attacker.speed + attacker.attack) - receiver.defense;
        yield return StartCoroutine(attackAnimations.PlayAirStrikeAnimation(attacker.transform, receiver.transform, hits));        //ANIMACIA
        if (hits >= 1) { receiver.TakeDamage(hits); }
        if (Random.value <= (attacker.attack / 30f)) 
        {
            yield return StartCoroutine(attackAnimations.PlayAirStrikeCriticalAnimation(receiver.transform));        //ANIMACIA
            receiver.TakeDamage(attacker.speed);
        }
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " strikes from air"));
        Debug.Log(attacker.cardName + " -> AirStrike => " + receiver.cardName);
    }

    //59
    public IEnumerator JusticeCrusade(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Justice Crusade"));
        yield return StartCoroutine(attackAnimations.PlayJusticeCrusadeAnimation(attacker.transform));        //ANIMACIA
        receiver.TakeDamage(Random.Range((receiver.strength + receiver.attack)/4 , (receiver.strength + receiver.attack)/2));
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " fights enemy for justice"));
        Debug.Log(attacker.cardName + " -> JusticeCrusade => " + receiver.cardName);
    }

    //60
    public IEnumerator Rapier(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Rapier"));
        yield return StartCoroutine(attackAnimations.PlayRapierAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(2 + ((attacker.strength + attacker.speed + attacker.attack) / 3) - ((receiver.defense - receiver.strength) / 2));
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " cuts with Rapier"));
        if (Random.value <= 0.2f) receiver.TakeDamage(3);//critical hit
        if (Random.value <= 0.6f) 
        {
            StartCoroutine(attackAnimations.PlayBleedStartAnimation(receiver.transform));        //ANIMACIA
            yield return StartCoroutine(receiver.AddEffect(1,Random.Range(2, 7)));//bleed
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is wounded"));
        }
        Debug.Log(attacker.cardName + " -> Rapier => " + receiver.cardName);
    }

    //61
    public IEnumerator ExpeditionaryAssault(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Expeditionary Assault"));
        yield return StartCoroutine(attackAnimations.PlayExpeditionaryAssaultAnimation(attacker.transform));        //ANIMACIA
        if (Random.value <= (20f + attacker.knowledge) / 50f) 
        {
            yield return StartCoroutine(attackAnimations.PlayExpeditionaryAssaultSuccessAnimation(attacker.transform, receiver.transform));        //ANIMACIA
            int[] boost = DistributeRandomly(Random.Range(3, 6));
            attacker.HandleStrength(boost[0]);
            attacker.HandleAttack(boost[1]);
            attacker.HandleDefense(boost[2]);
            attacker.HandleCharisma(boost[3]);
            attacker.HandleKnowledge(boost[4]);
            yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " gets useful stuff"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayExpeditionaryAssaultFailAnimation(attacker.transform));        //ANIMACIA
            attacker.TakeDamage(Random.Range(3, 7));
            yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " almost died on sea"));
        }
        Debug.Log(attacker.cardName + " -> ExpeditionaryAssault => " + receiver.cardName);
    }

    //62
    public IEnumerator Culverin(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        if (Random.value <= 0.65f)
        {
            yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Culverin"));
            yield return StartCoroutine(attackAnimations.PlayCulverinAnimation(attacker.transform, receiver.transform, true));        //ANIMACIA
            receiver.TakeDamage(Random.Range(1, 15));
            yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + "'s cannon fires"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayCulverinAnimation(attacker.transform, receiver.transform, false));        //ANIMACIA
            yield return StartCoroutine(ShowDialog(dialogText, "Bang! aaaand miss"));
        }
        Debug.Log(attacker.cardName + " -> Culverin => " + receiver.cardName);
    }

    //63
    public IEnumerator FireShip(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses FireShip"));
        yield return StartCoroutine(attackAnimations.PlayFireShipAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(3);
        yield return StartCoroutine(ShowDialog(dialogText, "Fire ship impacts"));
        if (Random.value <= 0.5f) 
        {
            StartCoroutine(attackAnimations.PlayBurnStartAnimation(receiver.transform));        //ANIMACIA
            yield return StartCoroutine(receiver.AddEffect(16,1));//burn
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is burning"));
        }
        Debug.Log(attacker.cardName + " -> FireShip => " + receiver.cardName);
    }
    //64
    public IEnumerator HandcuffEscape(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Danger Escape"));
        yield return StartCoroutine(attackAnimations.PlayHandcuffEscapeAnimation(attacker.transform));        //ANIMACIA
        int[] idsToRemove = { 3, 8, 9, 12, 16 };
        attacker.RemoveEffectsById(idsToRemove);
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " slips outta trouble"));
        if (Random.value <= 0.2f)
        {
            StartCoroutine(attackAnimations.PlayConfusionStartAnimation(receiver.transform));        //ANIMACIA
            yield return StartCoroutine(receiver.AddEffect(17,2));//confusion
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is confused"));
        }
        Debug.Log(attacker.cardName + " -> HandcuffEscape => " + receiver.cardName);
    }

    //65
    public IEnumerator Illusion(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Illusion"));
        yield return StartCoroutine(attackAnimations.PlayIllusionAnimation(attacker.transform, Random.Range(0, 5)));        //ANIMACIA
        attacker.HandleCharisma(1);
        if (receiver.CheckEffect(17) == false)
        {
            StartCoroutine(attackAnimations.PlayConfusionStartAnimation(receiver.transform));        //ANIMACIA
            yield return StartCoroutine(receiver.AddEffect(17,Random.Range(2, 5)));//confusion
            yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + "'s illusion confuses the enemy"));
        }
        Debug.Log(attacker.cardName + " -> Illusion => " + receiver.cardName);
    }

    //66
    public IEnumerator CarcanoM91(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Carcano M91"));
        if (Random.value <= ((20 + attacker.attack) / 37f))
        {
            yield return StartCoroutine(attackAnimations.PlayCarcanoAnimation(attacker.transform, receiver.transform, true));        //ANIMACIA
            receiver.TakeDamage(Random.Range(6, 9));
            yield return StartCoroutine(ShowDialog(dialogText, "Bang! " + attacker.cardName + " shoots"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayCarcanoAnimation(attacker.transform, receiver.transform, false));        //ANIMACIA
            yield return StartCoroutine(ShowDialog(dialogText, "Bang! aaaand miss"));
        }
        Debug.Log(attacker.cardName + " -> CarcanoM91 => " + receiver.cardName);
    }

    //67
    public IEnumerator Winchester(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Winchester"));
        if (Random.value <= ((20 + attacker.attack) / 40f))
        {
            yield return StartCoroutine(attackAnimations.PlayWinchesterAnimation(attacker.transform, receiver.transform, true));        //ANIMACIA
            receiver.TakeDamage(Random.Range(6, 9));
            yield return StartCoroutine(ShowDialog(dialogText, "Bang! " + attacker.cardName + " shoots"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayWinchesterAnimation(attacker.transform, receiver.transform, false));        //ANIMACIA
            yield return StartCoroutine(ShowDialog(dialogText, "Bang! aaaand miss"));
        }
        Debug.Log(attacker.cardName + " -> Winchester => " + receiver.cardName);
    }

    //68
    public IEnumerator Ambush(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Ambush"));
        yield return StartCoroutine(attackAnimations.PlayAmbushAnimation(receiver.transform));        //ANIMACIA
        receiver.TakeDamage(2);
        receiver.HandleCharisma(-1);
        attacker.HandleCharisma(1);
        yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " ambushed with surprise"));
        Debug.Log(attacker.cardName + " -> Ambush => " + receiver.cardName);
    }

    //69
    public IEnumerator JupiterC(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Space Rocket"));
        if (Random.value <= 0.7f)
        {
            yield return StartCoroutine(attackAnimations.PlaySpaceRocketAnimation(attacker.transform, true));        //ANIMACIA
            attacker.HandleAttack(1);
            attacker.HandleDefense(1);
            yield return StartCoroutine(attacker.AddEffect(18,0));//Satellite
            yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " launches satellite"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlaySpaceRocketAnimation(attacker.transform, false));        //ANIMACIA
            yield return StartCoroutine(ShowDialog(dialogText, "Rocket exploded"));
            attacker.HandleCharisma(-2);
        }
        Debug.Log(attacker.cardName + " -> JupiterC => " + receiver.cardName);
    }

    //70
    public IEnumerator V2(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses V-2"));
        if (Random.value <= 0.25f)
        {
            yield return StartCoroutine(attackAnimations.PlayV2Animation(attacker.transform, receiver.transform, true));        //ANIMACIA
            receiver.TakeDamage(Random.Range(15, 20));
            yield return StartCoroutine(ShowDialog(dialogText, "V2 hits target"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayV2Animation(attacker.transform, receiver.transform, false));        //ANIMACIA
            yield return StartCoroutine(ShowDialog(dialogText, "Missile missed"));
        }
        Debug.Log(attacker.cardName + " -> V2 => " + receiver.cardName);
    }

    //71
    public IEnumerator BattleCry(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses BattleCry"));
        yield return StartCoroutine(attackAnimations.PlayBattleCryAnimation(attacker.transform));        //ANIMACIA
        attacker.HandleStrength((int)System.Math.Ceiling((double)receiver.charisma / 5));
        attacker.HandleAttack((int)System.Math.Ceiling((double)receiver.charisma / 10));
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " roared into battle"));
        Debug.Log(attacker.cardName + " -> BattleCry => " + receiver.cardName);
    }

    //72
    public IEnumerator Revelation(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Revelation"));
        int variantIndex = 0;
        if (attacker.cardId == 6) { variantIndex = 2; }
        if (attacker.cardId == 12) { variantIndex = 4; }
        if (attacker.cardId == 43) { variantIndex = 3; }
        if (attacker.cardId == 46) { variantIndex = 5; }

        yield return StartCoroutine(attackAnimations.PlayRevelationAnimation(attacker.transform, variantIndex));        //ANIMACIA
        attacker.HandleCharisma(2);
        attacker.HandleKnowledge(2);
        yield return StartCoroutine(ShowDialog(dialogText, "God is with " + attacker.cardName));
        Debug.Log(attacker.cardName + " -> Revelation => " + receiver.cardName);
    }

    //73
    public IEnumerator Standard(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Standard"));
        yield return StartCoroutine(attackAnimations.PlayStandardAnimation(attacker.transform));        //ANIMACIA
        receiver.TakeDamage(2);
        if (Random.value <= 0.5f)
        {
            attacker.HandleCharisma(2);
        }
        else
        {
            attacker.HandleStrength(2);
        }
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + "'s Banner Raised Morale"));
        Debug.Log(attacker.cardName + " -> Standard => " + receiver.cardName);
    }

    //74
    public IEnumerator Pen(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Pen"));
        yield return StartCoroutine(attackAnimations.PlayPenAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(2 + ((attacker.knowledge + attacker.charisma + attacker.attack) / 3) - receiver.defense);
        receiver.HandleStrength(-1);
        if (Random.value <= 0.6f && receiver.CheckEffect(24) == false) 
        {
            StartCoroutine(attackAnimations.PlayPoisonStartAnimation(receiver.transform));        //ANIMACIA
            yield return StartCoroutine(receiver.AddEffect(24,0)); //Poison
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is wounded"));
        }
        yield return StartCoroutine(ShowDialog(dialogText, "The pen is mightier than the sword"));
        Debug.Log(attacker.cardName + " -> Pen => " + receiver.cardName);
    }

    //75
    public IEnumerator IambicPentameter(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Iambic Pentameter"));
        yield return StartCoroutine(attackAnimations.PlayIambicPentameterAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        if (Random.value <= 0.7f && receiver.CheckEffect(17) == false) 
        {
            StartCoroutine(attackAnimations.PlayConfusionStartAnimation(receiver.transform));        //ANIMACIA
            yield return StartCoroutine(receiver.AddEffect(17,Random.Range(2, 5)));//confusion
            yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + "'s poetry confuses the enemy"));
        }
        else if (receiver.charisma <= 7)
        {
        receiver.HandleKnowledge(-1);
        yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " does not understand"));
        }
        else
        {
            receiver.Heal(1);
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " likes this poetry"));
        }
        Debug.Log(attacker.cardName + " -> IambicPentameter => " + receiver.cardName);
    }

    //76
    public IEnumerator Ghost(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Ghost"));
        yield return StartCoroutine(attackAnimations.PlayGhostAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        if (receiver.CheckEffect(19) == false)
        {
            
            StartCoroutine(attackAnimations.PlayFearStartAnimation(receiver.transform));        //ANIMACIA
            StartCoroutine(receiver.AddEffect(19,2));//fear
            receiver.HandleStrength(-6);
            receiver.HandleAttack(-5);
            receiver.HandleDefense(3);
            yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " summons ghost, enemy fears"));
        }
        else
        {
            yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " summons ghost, no effect"));
        }
        Debug.Log(attacker.cardName + " -> Ghost => " + receiver.cardName);
    }

    //77
    public IEnumerator BuffaloHorns(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Buffalo Horns"));
        yield return StartCoroutine(attackAnimations.PlayBuffaloHornsAnimation(attacker.transform));        //ANIMACIA
        StartCoroutine(attacker.AddEffect(20,1));//Horns
        attacker.state = CardState.STAY;
        receiver.HandleAttack(2);
        attacker.HandleDefense(-1);
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " Launching the maneuver!"));
        Debug.Log(attacker.cardName + " -> BuffaloHorns => " + receiver.cardName);
    }

    //78
    public IEnumerator Iklwa(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Iklwa"));
        if (Random.value <= (attacker.strength / 10f))
        {
            yield return StartCoroutine(attackAnimations.PlayIklwaAnimation(attacker.transform, receiver.transform, true));        //ANIMACIA
            receiver.TakeDamage(5);
            yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " throws Iklwa"));
            if (Random.value <= 0.3f) 
            {
                StartCoroutine(attackAnimations.PlayBleedStartAnimation(receiver.transform));        //ANIMACIA
                yield return StartCoroutine(receiver.AddEffect(1,Random.Range(2, 5)));                              //bleed
                yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is wounded"));
            }
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayIklwaAnimation(attacker.transform, receiver.transform, false));        //ANIMACIA
            yield return StartCoroutine(ShowDialog(dialogText, "throw missed"));
        }
        Debug.Log(attacker.cardName + " -> Iklwa => " + receiver.cardName);
    }

    //79
    public IEnumerator Iwisa(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Iwisa"));
        yield return StartCoroutine(attackAnimations.PlayIwisaAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(((attacker.strength + attacker.attack) / 2) - receiver.defense );
        if (Random.value <= 0.3f) receiver.TakeDamage(5);                                                               //critical hit
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " attacks with Iwisa"));
        if (Random.value <= 0.3f) 
        {
            yield return StartCoroutine(attackAnimations.PlayKnockoutAnimation(receiver.transform));        //ANIMACIA
            yield return StartCoroutine(receiver.AddEffect(3,Random.Range(1, 3)));                                      //sleep
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " falls asleep"));
        }
        Debug.Log(attacker.cardName + " -> Iwisa => " + receiver.cardName);
    }

    //80
    public IEnumerator NitenIchiRyu(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Niten Ichi-Ryu"));
        yield return StartCoroutine(attackAnimations.PlayNitenIchiRyuKatanaAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(attacker.strength - (receiver.defense / 2));
        yield return StartCoroutine(attackAnimations.PlayNitenIchiRyuWakizashiAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(attacker.speed - (receiver.defense / 2));
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " attacks wit Katana and Wakizashi"));
        Debug.Log(attacker.cardName + " -> NitenIchiRyu => " + receiver.cardName);
    }

    //81
    public IEnumerator Tessenjutsu(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Tessenjutsu"));
        yield return StartCoroutine(attackAnimations.PlayTessenjutsuAnimation(attacker.transform));        //ANIMACIA
        attacker.HandleDefense(2);
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " moves like Kitana"));
        Debug.Log(attacker.cardName + " -> Tessenjutsu  => " + receiver.cardName);
    }

    //82
    public IEnumerator Iaijutsu(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Iaijutsu"));
        yield return StartCoroutine(attackAnimations.PlayIaijutsuAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(attacker.speed - (receiver.defense / 2));
        yield return StartCoroutine(ShowDialog(dialogText, "Flash of steel by" + attacker.cardName));
        Debug.Log(attacker.cardName + " -> Iaijutsu  => " + receiver.cardName);
    }

    //83
    public IEnumerator Katana(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Katana"));
        yield return StartCoroutine(attackAnimations.PlayKatanaAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(3 + ((attacker.knowledge + attacker.attack + attacker.speed) / 3) - ((receiver.defense - receiver.speed) / 2));
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " slashes with katana "));
        if (Random.value <= 0.2f) receiver.TakeDamage(3);//critical hit
        if (Random.value <= 0.3f) 
        {
            StartCoroutine(attackAnimations.PlayBleedStartAnimation(receiver.transform));        //ANIMACIA
            yield return StartCoroutine(receiver.AddEffect(1,Random.Range(2, 5)));//bleed
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is wounded"));
        }
        Debug.Log(attacker.cardName + " -> Katana => " + receiver.cardName);
    }
    //84
    public IEnumerator Nodachi(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Nodachi"));
        yield return StartCoroutine(attackAnimations.PlayNodachiAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(3 + ((attacker.knowledge + attacker.strength + attacker.speed) / 3) - ((receiver.defense - receiver.speed) / 2));
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " slashes with Nodachi "));
        if (Random.value <= 0.2f) receiver.TakeDamage(3);//critical hit
        if (Random.value <= 0.3f) 
        {
            StartCoroutine(attackAnimations.PlayBleedStartAnimation(receiver.transform));        //ANIMACIA
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is wounded"));
            yield return StartCoroutine(receiver.AddEffect(1,Random.Range(2, 5)));//bleed
        }
        Debug.Log(attacker.cardName + " -> Nodachi => " + receiver.cardName);
    }

    //85
    public IEnumerator Yumi(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Yumi"));
        if (Random.value <= (attacker.strength / 10f))
        {
            yield return StartCoroutine(attackAnimations.PlayYumiAnimation(attacker.transform, receiver.transform, true));        //ANIMACIA
            receiver.TakeDamage(3);
            yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " shoots arrow"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayYumiAnimation(attacker.transform, receiver.transform, false));        //ANIMACIA
            yield return StartCoroutine(ShowDialog(dialogText, "arrow missed"));
        }
        Debug.Log(attacker.cardName + " -> Yumi => " + receiver.cardName);
    }

    //86
    public IEnumerator Jujutsu(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Jujutsu"));
        yield return StartCoroutine(attackAnimations.PlayJujutsuAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(attacker.attack + attacker.knowledge - (receiver.defense + 3));
        if (Random.value <= 0.3f) receiver.HandleDefense(2);//critical hit
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + "'s Jujutsu Takedown"));
        Debug.Log(attacker.cardName + " -> Jujutsu => " + receiver.cardName);
    }

    //87
    public IEnumerator Espionage(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Espionage"));
        yield return StartCoroutine(attackAnimations.PlayEspionageAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.HandleDefense(-((attacker.knowledge + attacker.charisma) / 7));
        receiver.HandleSpeed(-((attacker.knowledge + attacker.charisma) / 7));
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " spies on enemy"));
        Debug.Log(attacker.cardName + " -> Espionage => " + receiver.cardName);
    }

    //88
    public IEnumerator Sabre(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Sabre"));
        yield return StartCoroutine(attackAnimations.PlaySabreAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(3 + ((attacker.attack + attacker.strength + attacker.speed) / 3) - ((receiver.defense - receiver.speed) / 2));
        if (Random.value <= 0.2f) receiver.TakeDamage(3);//critical hit
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " cuts with Sabre "));
        if (Random.value <= 0.3f) 
        {
            StartCoroutine(attackAnimations.PlayBleedStartAnimation(receiver.transform));        //ANIMACIA
            yield return StartCoroutine(receiver.AddEffect(1,Random.Range(2, 5)));//bleed
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is wounded"));
        }
        Debug.Log(attacker.cardName + " -> Sabre => " + receiver.cardName);
    }

    //89
    public IEnumerator Gamble(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Gamble"));
        int bet = Random.Range(1, 3);
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " is taking bets"));
        if (Random.value <= 0.66f)
        {
            yield return StartCoroutine(attackAnimations.PlayGambleAnimation(attacker.transform, receiver.transform, true));        //ANIMACIA
            attacker.HandleCharisma(bet);
            receiver.HandleCharisma(-bet);
            yield return StartCoroutine(ShowDialog(dialogText, "And wins"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayGambleAnimation(attacker.transform, receiver.transform, false));        //ANIMACIA
            attacker.HandleCharisma(-bet);
            receiver.HandleCharisma(bet);
            yield return StartCoroutine(ShowDialog(dialogText, "And loses"));
        }
        Debug.Log(attacker.cardName + " -> Gamble => " + receiver.cardName);
    }

    //90
    public IEnumerator Philosophy(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Philosophy"));
        yield return StartCoroutine(attackAnimations.PlayPhilosophyAnimation(attacker.transform));        //ANIMACIA
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " philosophizes!!!"));
        if (attacker.knowledge <= receiver.knowledge) 
        {
            yield return StartCoroutine(attackAnimations.PlayAnimationImpressed(receiver.transform));        //ANIMACIA
            receiver.HandleKnowledge(1);
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is interested in philosophy"));
        }
        else if ((5 <= receiver.knowledge) && (Random.value <= 0.5f) && !receiver.CheckEffect(3))
        {
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is bored by Philosophy"));
            StartCoroutine(attackAnimations.PlayBoredomAnimation(receiver.transform));        //ANIMACIA
            yield return StartCoroutine(receiver.AddEffect(3,Random.Range(1, 3)));//sleep
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " falls asleep"));

        }
        else if (Random.value <= 0.9f && !receiver.CheckEffect(17))
        {
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " doesn't understand that Philosophy"));
            StartCoroutine(attackAnimations.PlayConfusionStartAnimation(receiver.transform));        //ANIMACIA
            yield return StartCoroutine(receiver.AddEffect(17,Random.Range(2, 5)));//confusion
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is confused"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayAnimationNotEffective(receiver.transform));        //ANIMACIA
            yield return StartCoroutine(ShowDialog(dialogText,"Attack has no effect"));
        }
        Debug.Log(attacker.cardName + " -> Philosophy => " + receiver.cardName);
    }

    //91
    public IEnumerator Calm(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Calm"));
        yield return StartCoroutine(attackAnimations.PlayCalmAnimation(attacker.transform));        //ANIMACIA
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + "trying to calm enemy"));
        if (!receiver.CheckEffect(21))
        {
            yield return StartCoroutine(attackAnimations.PlayCalmStartAnimation(receiver.transform));        //ANIMACIA
            StartCoroutine(receiver.AddEffect(21,3));//calm
            receiver.HandleStrength(-3);
            receiver.HandleAttack(-3);
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is calm"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayAnimationNotEffective(receiver.transform));        //ANIMACIA
            yield return StartCoroutine(ShowDialog(dialogText,receiver.cardName + " is calm already"));
        }
        Debug.Log(attacker.cardName + " -> Calm => " + receiver.cardName);
    }

    //92
    public IEnumerator Honesty(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Honesty"));
        yield return StartCoroutine(attackAnimations.PlayHonestyAnimation(attacker.transform));        //ANIMACIA
        receiver.TakeDamage(((attacker.knowledge + attacker.attack + attacker.charisma)/2) - (receiver.defense));
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + "is brutally honest"));
        if ((Random.value <= 0.1f) && (!attacker.CheckEffect(17)))
        {
            yield return StartCoroutine(attackAnimations.PlayDepressionStartAnimation(attacker.transform));        //ANIMACIA
            attacker.HandleStrength(Random.Range(-3,0));
            attacker.HandleSpeed(Random.Range(-3,0));
            attacker.HandleAttack(Random.Range(-3,0));
            attacker.HandleDefense(Random.Range(-3,0));
            attacker.HandleCharisma(Random.Range(-3,0));
            yield return StartCoroutine(attacker.AddEffect(13,1));//Depression
            yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " feels bad for enemy"));
        }
        Debug.Log(attacker.cardName + " -> Honesty => " + receiver.cardName);
    }

    //93
    public IEnumerator Valaska(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Valaska"));
        yield return StartCoroutine(attackAnimations.PlayValaskaAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(3 + ((attacker.attack + attacker.strength + attacker.speed) / 3) - ((receiver.defense - receiver.speed) / 2));
        if (Random.value <= 0.5f) receiver.TakeDamage(2);//critical hit
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + "'s valaska strikes"));
        if (Random.value <= 0.1f) 
        {
            StartCoroutine(attackAnimations.PlayBleedStartAnimation(receiver.transform));        //ANIMACIA
            yield return StartCoroutine(receiver.AddEffect(1,Random.Range(2, 4)));//bleed
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is wounded"));
        }
        Debug.Log(attacker.cardName + " -> Valaska => " + receiver.cardName);
    }

    //94
    public IEnumerator Moonshine(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Moonshine"));
        yield return StartCoroutine(attackAnimations.PlayMoonshineAnimation(attacker.transform));        //ANIMACIA
        attacker.Heal(1);
        attacker.HandleStrength(2);
        attacker.HandleAttack(2);
        attacker.HandleDefense(-2);
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " is getting pretty drunk"));
        if (Random.value <= 0.1f) 
        {
            StartCoroutine(attackAnimations.PlayDrunkAnimation(attacker.transform));        //ANIMACIA
            yield return StartCoroutine(attacker.AddEffect(3,Random.Range(1, 3)));//sleep
            yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " falls asleep"));
        }
        else if (Random.value <= 0.2f) 
        {
            yield return StartCoroutine(attackAnimations.PlayFuryAnimation(attacker.transform));        //ANIMACIA
            StartCoroutine(attacker.AddEffect(6,3));//fury
            attacker.HandleStrength(5);
            attacker.HandleAttack(5);
            attacker.HandleDefense(-3);
            yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " is furious!"));
        }
        Debug.Log(attacker.cardName + " -> Moonshine => " + receiver.cardName);
    }

    //95
    public IEnumerator OutlawBand(Kard attacker, Kard receiver, TMP_Text dialogText)                            //dajak toto zmenit treba, somarina to je? rovnake ako ambush
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Outlaw Band"));
        yield return StartCoroutine(attackAnimations.PlayOutlawBandAnimation(receiver.transform));        //ANIMACIA
        int loot = Random.Range(1,3);
        if (receiver.charisma > receiver.strength) 
        {
            receiver.TakeDamage(Random.Range(5, 10));
            attacker.HandleCharisma(loot);
            receiver.HandleCharisma(-loot);
            yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + "'s Band Attacks"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayOutlawBandFailAnimation(receiver.transform));        //ANIMACIA
            attacker.TakeDamage(Random.Range(3, 7));
            yield return StartCoroutine(ShowDialog(dialogText, "The outlaw band run in fear"));
        }
        Debug.Log(attacker.cardName + " -> OutlawBand => " + receiver.cardName);
    }

    //96
    public IEnumerator FlintlockPistol(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Flintlock Pistol"));
        yield return StartCoroutine(attackAnimations.PlayFlintlockPistolLoadingAnimation(attacker.transform));        //ANIMACIA
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " is loading his pistol"));
        StartCoroutine(attacker.AddEffect(22,Random.Range(1, 2)));//Reloading
        attacker.state = CardState.STAY;
        attacker.HandleDefense(-1);
        Debug.Log(attacker.cardName + " -> FlintlockPistol => " + receiver.cardName);
    }

    //97
    public IEnumerator PassiveResistance(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Passive Resistance"));
        yield return StartCoroutine(attackAnimations.PlayPassiveResistanceAnimation(attacker.transform));        //ANIMACIA
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " is resisting passively!!!"));
        receiver.HandleAttack(-1);
        attacker.HandleDefense(2);
        Debug.Log(attacker.cardName + " -> PassiveResistance => " + receiver.cardName);
    }

    //98
    public IEnumerator HungerStrike(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Hunger Strike"));
        yield return StartCoroutine(attackAnimations.PlayHungerStrikeAnimation(attacker.transform));        //ANIMACIA
        attacker.TakeDamage(5);
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " refuses to eat"));
        if (Random.value <= ((40 - receiver.strength) / 30f) && !receiver.CheckEffect(13))
        {
            yield return StartCoroutine(attackAnimations.PlayDepressionStartAnimation(receiver.transform));        //ANIMACIA
            receiver.HandleStrength(Random.Range(-3,0));
            receiver.HandleSpeed(Random.Range(-3,0));
            receiver.HandleAttack(Random.Range(-3,0));
            receiver.HandleDefense(Random.Range(-3,0));
            receiver.HandleCharisma(Random.Range(-3,0));
            yield return StartCoroutine(receiver.AddEffect(13,1));//Depression
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " feels bad for enemy"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayAnimationNotEffective(receiver.transform));        //ANIMACIA
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " doesn't care"));
        }
        Debug.Log(attacker.cardName + " -> HungerStrike => " + receiver.cardName);
    }

    //99
    public IEnumerator Gladius(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Gladius"));
        yield return StartCoroutine(attackAnimations.PlayGladiusAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(2 + ((attacker.attack + attacker.strength + attacker.speed) / 3) - ((receiver.defense - receiver.speed) / 2));
        if (Random.value <= 0.5f) receiver.TakeDamage(4);//critical hit
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " attacks with gladius"));
        Debug.Log(attacker.cardName + " -> Gladius => " + receiver.cardName);
    }

    //100
    public IEnumerator ShieldBash(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Shield Bash"));
        yield return StartCoroutine(attackAnimations.PlayShieldBashAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(2);
        if (Random.value <= 0.5f) attacker.HandleDefense(2);//critical hit
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " smashes with his shield"));
        Debug.Log(attacker.cardName + " -> ShieldBash => " + receiver.cardName);
    }

    //101
    public IEnumerator Yperit(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Yperit"));
        if (Random.value <= 0.9f) 
        {
            yield return StartCoroutine(attackAnimations.PlayYperitSuccessAnimation(attacker.transform, receiver.transform));        //ANIMACIA
            receiver.TakeDamage(Random.Range(5, 10));
            yield return StartCoroutine(ShowDialog(dialogText, "Yperit strikes"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayYperitFailAnimation(receiver.transform));        //ANIMACIA
            attacker.TakeDamage(Random.Range(3, 7));
            yield return StartCoroutine(ShowDialog(dialogText, "A breeze blows"));
        }
        Debug.Log(attacker.cardName + " -> Yperit => " + receiver.cardName);
    }

    //102
    public IEnumerator Blitzkrieg(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Blitzkrieg"));
        if (attacker.speed < receiver.speed)
        {
            yield return StartCoroutine(attackAnimations.PlayAnimationNotEffective(receiver.transform));        //ANIMACIA
            attacker.TakeDamage(receiver.speed - attacker.speed);
            yield return StartCoroutine(ShowDialog(dialogText, "Too slow"));
        }
        else
        {
            yield return StartCoroutine(attackAnimations.PlayBlitzkriegAnimation(attacker.transform, receiver.transform));        //ANIMACIA
            receiver.TakeDamage(3 + attacker.speed - receiver.speed);
            yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " uses Blitzkrieg tactics"));
        }
        Debug.Log(attacker.cardName + " -> Blitzkrieg => " + receiver.cardName);
    }

    //103
    public IEnumerator Propaganda(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowAttackDialog(dialogText,attacker.cardName + " uses Propaganda"));
        yield return StartCoroutine(attackAnimations.PlayPropagandaAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.HandleAttack(-((attacker.knowledge + attacker.charisma) / 7));
        receiver.HandleKnowledge(-((attacker.knowledge + attacker.charisma) / 7));
        yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " was hit by propaganda"));
        Debug.Log(attacker.cardName + " -> Propaganda => " + receiver.cardName);
    }

    //104
    public IEnumerator Retiarius(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " throws the net"));

        if (attacker.attack > receiver.speed && Random.value <= 0.9f) 
        {
            StartCoroutine(receiver.AddEffect(9,1)); //Tether
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " was captured"));
            attacker.state = CardState.STAY;
            StartCoroutine(attacker.AddEffect(23,1)); //Trident
        }

        Debug.Log(attacker.cardName + " -> Retiarius => " + receiver.cardName);
    }

    //105
    public IEnumerator Shuriken(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " is throwing stars"));

        if (Random.value <= ((20 + attacker.attack) / 30f))
        {
            receiver.TakeDamage(1);
            receiver.HandleSpeed(-1);
        }
        else
        {
            yield return StartCoroutine(ShowDialog(dialogText, "throw missed"));
        }

        Debug.Log(attacker.cardName + " -> Shuriken => " + receiver.cardName);
    }

    //106
    public IEnumerator Kusarigama(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + "'s Kusarigama strikes"));
        receiver.TakeDamage(2 + ((attacker.attack + attacker.knowledge + attacker.speed) / 3) - ((receiver.defense - receiver.speed) / 2));

        if (Random.value <= 0.1f) 
        {
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is trapped in chain"));
            yield return StartCoroutine(receiver.AddEffect(9,1)); //Tether
        }

        Debug.Log(attacker.cardName + " -> Kusarigama => " + receiver.cardName);
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
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " offers some spices to enemy"));
        yield return StartCoroutine(receiver.AddEffect(24,0)); //Poison

        Debug.Log(attacker.cardName + " -> OrientalSpice => " + receiver.cardName);
    }

    //109
    public IEnumerator FieryArquebus(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        receiver.TakeDamage(Random.Range(1,Random.Range(3, 7)));
        yield return StartCoroutine(ShowDialog(dialogText, "Bum! Fiery Arquebus gun exploded"));

        if (Random.value <= 0.5f) 
        {
            attacker.TakeDamage(Random.Range(2, 5));
        }

        Debug.Log(attacker.cardName + " -> FieryArquebus => " + receiver.cardName);
    }

    //110
    public IEnumerator PirateRaid(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        if (((attacker.speed + attacker.charisma) / 2) < receiver.defense && Random.value <= 0.8f)
        {
            attacker.TakeDamage(receiver.defense);
            yield return StartCoroutine(ShowDialog(dialogText, "Raiding didn't go well"));
        }
        else
        {
            receiver.TakeDamage((attacker.speed + attacker.charisma) / 2);
            yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " raids the enemy"));
        }

        Debug.Log(attacker.cardName + " -> PirateRaid => " + receiver.cardName);
    }

    //111
    public IEnumerator Axe(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(attackAnimations.PlayAxeAnimation(attacker.transform, receiver.transform));        //ANIMACIA
        receiver.TakeDamage(((attacker.attack + (attacker.strength * 2)) / 3) - ((receiver.defense - receiver.speed) / 2));
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " attacks with axe"));

        if (Random.value <= 0.5f) 
        {
            receiver.TakeDamage(6); //critical hit
        }

        Debug.Log(attacker.cardName + " -> Axe => " + receiver.cardName);
    }

    //112
    public IEnumerator JaguarWarriors(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        receiver.TakeDamage(Random.Range(1, attacker.attack+2));
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " sends Jaguar Warriors"));

        if (Random.value <= 0.2f) 
        {
            receiver.HandleAttack(-(Random.Range(2, 4))); //critical hit
        }

        Debug.Log(attacker.cardName + " -> JaguarWarriors => " + receiver.cardName);
    }

    //113
    public IEnumerator Atlatl(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " throws spear"));

        if (Random.value <= (attacker.strength / 10f))
        {
            receiver.TakeDamage(3);

            if (Random.value <= 0.2f) 
            {
                yield return StartCoroutine(receiver.AddEffect(1,Random.Range(2, 4))); //bleed
            }
        }
        else
        {
            yield return StartCoroutine(ShowDialog(dialogText, "spear missed"));
        }

        Debug.Log(attacker.cardName + " -> Atlatl => " + receiver.cardName);
    }
    //114
    public IEnumerator Macuahuitl(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        receiver.TakeDamage(((attacker.attack + attacker.strength * 2) / 2) - ((receiver.defense - receiver.speed) / 2));
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " smashes with Macuahuitl"));

        if (Random.value <= 0.2f) 
        {
            receiver.TakeDamage(3); //critical hit
        }

        Debug.Log(attacker.cardName + " -> Macuahuitl => " + receiver.cardName);
    }

    //115
    public IEnumerator Cubism(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(receiver.AddEffect(17, Random.Range(2, 5))); //confusion
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + "'s picture confuses the enemy"));

        Debug.Log(attacker.cardName + " -> Cubism => " + receiver.cardName);
    }

    //116
    public IEnumerator CosaNostra(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        receiver.TakeDamage(3 + (attacker.charisma - receiver.defense));
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " orders mafia to attack"));

        if (Random.value <= 0.5f) 
        {
            receiver.HandleCharisma(-2); //critical hit
        }

        Debug.Log(attacker.cardName + " -> CosaNostra => " + receiver.cardName);
    }

    //117
    public IEnumerator Act(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " behaves like insane"));

        if (Random.value <= (int)System.Math.Ceiling((double)receiver.strength / 20))
        {
            receiver.HandleStrength(-1);
            receiver.HandleAttack(-1);
            receiver.HandleDefense(1);
            receiver.HandleCharisma(-1);
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is scared"));
        }
        else
        {
            receiver.HandleStrength(UnityEngine.Random.Range(-2, 0));
            receiver.HandleSpeed(UnityEngine.Random.Range(-2, 0));
            receiver.HandleAttack(UnityEngine.Random.Range(-2, 0));
            receiver.HandleDefense(UnityEngine.Random.Range(-2, 0));
            receiver.HandleKnowledge(UnityEngine.Random.Range(0, 3));
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is impressed"));
        }

        Debug.Log(attacker.cardName + " -> Act => " + receiver.cardName);
    }

    //118
    public IEnumerator Football(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        receiver.HandleSpeed(1);
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " plays football"));

        if (7 <= receiver.knowledge) 
        {
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is bored by Football"));

            if (Random.value <= 0.8f) 
            {
                yield return StartCoroutine(receiver.AddEffect(3, Random.Range(1, 3))); //sleep
                yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " falls asleep"));
            }
        }
        else
        {
            receiver.HandleCharisma(1);
            yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " likes it"));
        }

        Debug.Log(attacker.cardName + " -> Football => " + receiver.cardName);
    }

    //119
    public IEnumerator BicycleKick(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " tries bicycle kick"));

        if (Random.value <= 0.7f)
        {
            receiver.TakeDamage((5 + (attacker.speed + attacker.attack) / 2) - ((receiver.defense + receiver.strength) / 2));
            yield return StartCoroutine(ShowDialog(dialogText, "Plesk! Big kick from " + attacker.cardName));

            if (Random.value <= 0.4f) 
            {
                receiver.TakeDamage(4); //extra dmg
            }
        }
        else
        {
            attacker.TakeDamage(1);
            yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " faces gravity"));
        }

        Debug.Log(attacker.cardName + " -> BicycleKick => " + receiver.cardName);
    }

    //120
    public IEnumerator WorldChampion(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        attacker.HandleStrength(1);
        attacker.HandleCharisma(2);
        yield return StartCoroutine(ShowDialog(dialogText, attacker.cardName + " is world champion, remember!"));

        Debug.Log(attacker.cardName + " -> WorldChampion => " + receiver.cardName);
    }

    //121
    public IEnumerator ShaolinSoccer(Kard attacker, Kard receiver, TMP_Text dialogText)
    {
        List<AttackDelegate> attacks = new List<AttackDelegate> { Kick, Punch, BicycleKick, Act, Honesty, Jujutsu, Fury, Corruption, Scratch };
        System.Random rnd = new System.Random();
        attacks = attacks.OrderBy(x => rnd.Next()).ToList();

        yield return StartCoroutine(Football(attacker, receiver, dialogText));
        yield return StartCoroutine(attacks[0](attacker, receiver, dialogText));

        Debug.Log(attacker.cardName + " -> ShaolinSoccer => " + receiver.cardName);
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

