using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AttackDescriptions : MonoBehaviour
{
    public TMP_Text button1Text;
    public TMP_Text button2Text;
    public TMP_Text button3Text;
    public TMP_Text button4Text;

    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;

    public FightSystem fightSystem;

    private UnityEngine.Events.UnityAction buttonCallback;

    int attack;

    public int LoadAttackCount(Kard attacker, int attack)
    {
        int count = 0;

        switch (attack)
        {
            case 1://Punch
                count = attacker.strength * 5;
                break;
            case 2://Kick
                count = attacker.strength * 5;
                break;
            case 3://Heal
                count = attacker.knowledge;
                break;
            case 4://Forgiveness
                count = attacker.charisma / 2;
                break;
            case 5://Crusade
                count = 9;
                break;
            case 6://"Water to Wine
                count = attacker.knowledge;
                break;
            case 7://Car Hit
                count = attacker.knowledge;
                break;
            case 8://Steam Gun
                count = attacker.knowledge / 2;
                break;
            case 9://Radiation
                count = attacker.knowledge / 2;
                break;
            case 10://Scratch
                count = attacker.attack * 5;
                break;
            case 11://Scientific Lecture
                count = attacker.knowledge;
                break;
            case 12://Chi Sau
                count = attacker.attack;
                break;
            case 13://One Inch Punch
                count = attacker.attack;
                break;
            case 14://Up In Smoke
                count = attacker.charisma;
                break;
            case 15://Sing
                count = attacker.knowledge;
                break;
            case 16://Revolver
                count = 6;
                break;
            case 17://Heavy Artillery
                count = (int)System.Math.Ceiling((double)attacker.charisma / 5);
                break;
            case 18://Blood Sucking
                count = attacker.attack;
                break;
            case 19://Sword
                count = attacker.strength * 5;
                break;
            case 20://Pike
                count = attacker.strength * 5;
                break;
            case 21://Terrify
                count = attacker.defense * 5;
                break;
            case 22://Drink Wine
                count = attacker.knowledge;
                break;
            case 23://Flaming Gun
                count = Random.Range(5, 15);
                break;
            case 24://Cleaver
                count = attacker.strength * 5;
                break;
            case 25://Pan
                count = attacker.strength * 5;
                break;
            case 26://Boost
                count = attacker.charisma;
                break;
            case 27://Temptation
                count = attacker.charisma;
                break;
            case 28://Shamshir
                count = attacker.strength * 5;
                break;
            case 29://Diplomacy
                count = attacker.knowledge;
                break;
            case 30://Siege
                count = attacker.knowledge / 2;
                break;
            case 31://TreeStratagem
                count = (int)System.Math.Ceiling((double)attacker.knowledge / 5);
                break;
            case 32://Tomahawk
                count = attacker.strength * 5;
                break;
            case 33://PeacePipe
                count = attacker.charisma;
                break;
            case 34://RecurveBow
                count = attacker.strength * 2;
                break;
            case 35://Fury
                count = attacker.attack;
                break;
            case 36://Guerilla
                count = (int) (attacker.attack * 5);
                break;
            case 37://Famine
                count = attacker.strength;
                break;
            case 38://Marxism
                count = attacker.knowledge * 2;
                break;
            case 39://TeslaCoil
                count = attacker.knowledge / 2;
                break;
            case 40://WirelessCharger
                count = attacker.knowledge * 2;
                break;
            case 41://Experiment
                count = attacker.knowledge * 10;
                break;
            case 42://TommyGun
                count = new int[] {2, 3, 5, 10}[Random.Range(0, 4)];
                break;
            case 43://TieUp
                count = 10;
                break;
            case 44://Corruption
                count = attacker.charisma;
                break;
            case 45://Colt1911
                count = 7;
                break;
            case 46://Mortar
                count = 7;
                break;
            case 47://GreatArmy
                count = 3;
                break;
            case 48://ScorchedEarth
                count = attacker.attack;
                break;
            case 49://DoubleEnvelopment
                count = attacker.knowledge * 2;
                break;
            case 50://ContinentalBlockade
                count = attacker.strength * 10;
                break;
            case 51://Depression
                count = attacker.knowledge + 10;
                break;
            case 52://SelfIsolation
                count = attacker.knowledge + 10;
                break;
            case 53://Knife
                count = attacker.strength * 10;
                break;
            case 54://Autoportrait
                count = 10;
                break;
            case 55://GravityPull
                count = attacker.knowledge + 5;
                break;
            case 56://Kamikaze
                count = 1;
                break;
            case 57://TookOff
                count = attacker.knowledge * 2;
                break;
            case 58://AirStrike
                count = attacker.attack * 3;
                break;
            case 59://JusticeCrusade
                count = attacker.strength * 5;
                break;
            case 60://Rapier
                count = attacker.strength * 5;
                break;
            case 61://ExpeditionaryAssault
                count = attacker.charisma;
                break;
            case 62://Culverin
                count = attacker.strength;
                break;
            case 63://FireShip
                count = attacker.knowledge / 2;
                break;
            case 101://Yperit
                count = 3;
                break;
            case 102://Blitzkrieg
                count = attacker.attack;
                break;
            case 103://Propaganda
                count = attacker.knowledge * 2;
                break;
            case 104://nictuneni
                count = attacker.strength * 10;
                break;
            default:
                Debug.LogError("Invalid attack type.");
                break;
        }
        return count;
    }


    public void DisplayAttack(Kard attacker, int attackType, TMP_Text dialogText)
    {

        switch (attackType)
        {
            case 1:
                attack = attacker.attack1;
                break;
            case 2:
                attack = attacker.attack2;
                break;
            case 3:
                attack = attacker.attack3;
                break;
            case 4:
                attack = attacker.attack4;
                break;
            default:
                Debug.LogError("Invalid attack type from button.");
                break;
        }

        switch (attack)
        {
            case 1:
                dialogText.text = "Punch chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 2:
                dialogText.text = "Kick chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 3:
                dialogText.text = "Heal chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 4:
                dialogText.text = "Forgiveness chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 5:
                dialogText.text = "Crusade chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 6:
                dialogText.text = "Water to Wine chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 7:
                dialogText.text = "Car Hit chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 8:
                dialogText.text = "Steam Gun chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 9:
                dialogText.text = "Radiation chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 10:
                dialogText.text = "Scratch chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 11:
                dialogText.text = "Scientific Lecture chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 12:
                dialogText.text = "Chi Sau chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 13:
                dialogText.text = "One Inch Punch chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 14:
                dialogText.text = "Up In Smoke chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 15:
                dialogText.text = "Sing chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 16:
                dialogText.text = "Revolver chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 17:
                dialogText.text = "1st Minnesota Heavy Artillery Regiment chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 18:
                dialogText.text = "Blood Sucking chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 19:
                dialogText.text = "Sword chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 20:
                dialogText.text = "Pike chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 21:
                dialogText.text = "Terrify chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 22:
                dialogText.text = "Drink Wine chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 23:
                dialogText.text = "Flaming Gun chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 24:
                dialogText.text = "Cleaver chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 25:
                dialogText.text = "Pan chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 26:
                dialogText.text = "Boost chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 27:
                dialogText.text = "Temptation chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 28:
                dialogText.text = "Shamshir chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 29:
                dialogText.text = "Diplomacy chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 30:
                dialogText.text = "Siege chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 31:
                dialogText.text = "Tree Stratagem chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 32:
                dialogText.text = "Tomahawk chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 33:
                dialogText.text = "Peace Pipe chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 34:
                dialogText.text = "Recurve Bow chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 35:
                dialogText.text = "Fury chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 36:
                dialogText.text = "Guerilla chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 37:
                dialogText.text = "Famine chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 38:
                dialogText.text = "Marxism chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 39:
                dialogText.text = "Tesla Coil chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 40:
                dialogText.text = "Wireless Charger chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 41:
                dialogText.text = "Experiment chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 42:
                dialogText.text = "Tommy Gun chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 43:
                dialogText.text = "Tie Up chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 44:
                dialogText.text = "Corruption chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 45:
                dialogText.text = "Colt 1911 chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 46:
                dialogText.text = "Mortar chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 47:
                dialogText.text = "Great Army chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 48:
                dialogText.text = "Scorched Earth chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 49:
                dialogText.text = "Double Envelopment chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 50:
                dialogText.text = "Continental Blockade chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 51:
                dialogText.text = "Depression chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 52:
                dialogText.text = "Self-Isolation chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 53:
                dialogText.text = "Knife chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 54:
                dialogText.text = "Autoportrait chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 55:
                dialogText.text = "Gravity Pull chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 56:
                dialogText.text = "Kamikaze chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 57:
                dialogText.text = "Took Off chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 58:
                dialogText.text = "Air Strike chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 59:
                dialogText.text = "Justice Crusade chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 60:
                dialogText.text = "Rapier chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 61:
                dialogText.text = "Expeditionary Assault chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 62:
                dialogText.text = "Culverin chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 63:
                dialogText.text = "Fire Ship chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 101:
                dialogText.text = "Yperit chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 102:
                dialogText.text = "Blitzkrieg chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 103:
                dialogText.text = "Propaganda chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 104:
                dialogText.text = "Punch chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            default:
                Debug.LogError("Invalid attack type.");
                break;
        }
    }


    public void ShowDescription(Kard card)
	{
        ShowDescriptionButton(card.attack1,button1Text,button1);
        ShowDescriptionButton(card.attack2,button2Text,button2);
        ShowDescriptionButton(card.attack3,button3Text,button3);
        ShowDescriptionButton(card.attack4,button4Text,button4);
	}

    public void ShowDescriptionButton(int attackType, TMP_Text buttonText, Button button)
    {
        switch (attackType)
        {
            case 1:
                Punch(buttonText);
                break;
            case 2:
                Kick(buttonText);
                break;
            case 3:
                Heal(buttonText);
                break;
            case 4:
                Forgiveness(buttonText);
                break;
            case 5:
                Crusade(buttonText);
                break;
            case 6:
                WaterToWine(buttonText);
                break;
            case 7:
                CarHit(buttonText);
                break;
            case 8:
                SteamGun(buttonText);
                break;
            case 9:
                Radiation(buttonText);
                break;
            case 10:
                Scratch(buttonText);
                break;
            case 11:
                ScientificLecture(buttonText);
                break;
            case 12:
                ChiSau(buttonText);
                break;
            case 13:
                OneInchPunch(buttonText);
                break;
            case 14:
                UpInSmoke(buttonText);
                break;
            case 15:
                Sing(buttonText);
                break;
            case 16:
                Revolver(buttonText);
                break;
            case 17:
                ArtilleryRegiment(buttonText);
                break;
            case 18:
                BloodSucking(buttonText);
                break;
            case 19:
                Sword(buttonText);
                break;
            case 20:
                Pike(buttonText);
                break;
            case 21:
                Terrify(buttonText);
                break;
            case 22:
                DrinkWine(buttonText);
                break;
            case 23:
                FlamingGun(buttonText);
                break;
            case 24:
                Cleaver(buttonText);
                break;
            case 25:
                Pan(buttonText);
                break;
            case 26:
                Boost(buttonText);
                break;
            case 27:
                Temptation(buttonText);
                break;
            case 28:
                Shamshir(buttonText);
                break;
            case 29:
                Diplomacy(buttonText);
                break;
            case 30:
                Siege(buttonText);
                break;
            case 31:
                TreeStratagem(buttonText);
                break;
            case 32:
                Tomahawk(buttonText);
                break;
            case 33:
                PeacePipe(buttonText);
                break;
            case 34:
                RecurveBow(buttonText);
                break;
            case 35:
                Fury(buttonText);
                break;
            case 36:
                Guerilla(buttonText);
                break;
            case 37:
                Famine(buttonText);
                break;
            case 38:
                Marxism(buttonText);
                break;
            case 39:
                TeslaCoil(buttonText);
                break;
            case 40:
                WirelessCharger(buttonText);
                break;
            case 41:
                Experiment(buttonText);
                break;
            case 42:
                TommyGun(buttonText);
                break;
            case 43:
                TieUp(buttonText);
                break;
            case 44:
                Corruption(buttonText);
                break;
            case 45:
                Colt1911(buttonText);
                break;
            case 46:
                Mortar(buttonText);
                break;
            case 47:
                GreatArmy(buttonText);
                break;
            case 48:
                ScorchedEarth(buttonText);
                break;
            case 49:
                DoubleEnvelopment(buttonText);
                break;
            case 50:
                ContinentalBlockade(buttonText);
                break;
            case 51:
                Depression(buttonText);
                break;
            case 52:
                SelfIsolation(buttonText);
                break;
            case 53:
                Knife(buttonText);
                break;
            case 54:
                Autoportrait(buttonText);
                break;
            case 55:
                GravityPull(buttonText);
                break;
            case 56:
                Kamikaze(buttonText);
                break;
            case 57:
                TookOff(buttonText);
                break;
            case 58:
                AirStrike(buttonText);
                break;
            case 59:
                JusticeCrusade(buttonText);
                break;
            case 60:
                Rapier(buttonText);
                break;
            case 61:
                ExpeditionaryAssault(buttonText);
                break;
            case 62:
                Culverin(buttonText);
                break;
            case 63:
                FireShip(buttonText);
                break;
            case 101:
                Yperit(buttonText);
                break;
            case 102:
                Blitzkrieg(buttonText);
                break;
            case 103:
                Propaganda(buttonText);
                break;
            case 104:
                Propaganda(buttonText);
                break;
            default:
                Debug.LogError("Invalid attack type.");
                break;
        }
    }
    //1
    public void Punch(TMP_Text buttonText)
	{
        buttonText.text = "Punch";
	}

    //2
    public void Kick(TMP_Text buttonText)
	{
        buttonText.text = "Kick";
	}

    //3
    public void Heal(TMP_Text buttonText)
	{
        buttonText.text = "Heal";
	}

    //4
    public void Forgiveness(TMP_Text buttonText)
	{
        buttonText.text = "Forgiveness";
	}

    //5
    public void Crusade(TMP_Text buttonText)
	{
        buttonText.text = "Crusade";
	}

    //6
    public void WaterToWine(TMP_Text buttonText)
	{
        buttonText.text = "Water To Wine";
	}

    //7
    public void CarHit(TMP_Text buttonText)
	{
        buttonText.text = "Car Hit";
	}

    //8
    public void SteamGun(TMP_Text buttonText)
	{
        buttonText.text = "Steam Gun";
	}

    //9
    public void Radiation(TMP_Text buttonText)
	{
        buttonText.text = "Radiation";
	}

    //10
    public void Scratch(TMP_Text buttonText)
	{
        buttonText.text = "Scratch";
	}

    //11
    public void ScientificLecture(TMP_Text buttonText)
	{
        buttonText.text = "Scientific Lecture";
	}

    //12
    public void ChiSau(TMP_Text buttonText)
	{
        buttonText.text = "Chi Sau";
	}

    //13
    public void OneInchPunch(TMP_Text buttonText)
	{
        buttonText.text = "One Inch Punch";
	}

    //14
    public void UpInSmoke(TMP_Text buttonText)
	{
        buttonText.text = "Up In Smoke";
	}

    //15
    public void Sing(TMP_Text buttonText)
	{
        buttonText.text = "Sing";
	}

    //16
    public void Revolver(TMP_Text buttonText)
	{
        buttonText.text = "Revolver";
	}

    //17
    public void ArtilleryRegiment(TMP_Text buttonText)
	{
        buttonText.text = "Artillery Regiment";
	}

    //18
    public void BloodSucking(TMP_Text buttonText)
	{
        buttonText.text = "Blood Sucking";
	}

    //19
    public void Sword(TMP_Text buttonText)
	{
        buttonText.text = "Sword";
	}

    //20
    public void Pike(TMP_Text buttonText)
	{
        buttonText.text = "Pike";
	}

    //21
    public void Terrify(TMP_Text buttonText)
	{
        buttonText.text = "Terrify";
	}

    //22
    public void DrinkWine(TMP_Text buttonText)
	{
        buttonText.text = "Drink Wine";
	}

    //23
    public void FlamingGun(TMP_Text buttonText)
	{
        buttonText.text = "Flaming Gun";
	}

    //24
    public void Cleaver(TMP_Text buttonText)
	{
        buttonText.text = "Cleaver";
	}

    //25
    public void Pan(TMP_Text buttonText)
	{
        buttonText.text = "Pan";
	}

    //26
    public void Boost(TMP_Text buttonText)
	{
        buttonText.text = "Boost";
	}

    //27
    public void Temptation(TMP_Text buttonText)
	{
        buttonText.text = "Temptation";
	}

    //28
    public void Shamshir(TMP_Text buttonText)
	{
        buttonText.text = "Shamshir";
	}

    //29
    public void Diplomacy(TMP_Text buttonText)
	{
        buttonText.text = "Diplomacy";
	}

    //30
    public void Siege(TMP_Text buttonText)
	{
        buttonText.text = "Siege";
	}

    //31
    public void TreeStratagem(TMP_Text buttonText)
	{
        buttonText.text = "Tree Stratagem";
	}

    //32
    public void Tomahawk(TMP_Text buttonText)
	{
        buttonText.text = "Tomahawk";
	}

    //33
    public void PeacePipe(TMP_Text buttonText)
	{
        buttonText.text = "Peace Pipe";
	}

    //34
    public void RecurveBow(TMP_Text buttonText)
	{
        buttonText.text = "Recurve Bow";
	}

    //35
    public void Fury(TMP_Text buttonText)
	{
        buttonText.text = "Fury";
	}

    //36
    public void Guerilla(TMP_Text buttonText)
	{
        buttonText.text = "Guerilla";
	}

    //37
    public void Famine(TMP_Text buttonText)
	{
        buttonText.text = "Famine";
	}

    //38
    public void Marxism(TMP_Text buttonText)
	{
        buttonText.text = "Marxism";
	}

    //39
    public void TeslaCoil(TMP_Text buttonText)
	{
        buttonText.text = "Tesla Coil";
	}

    //40
    public void WirelessCharger(TMP_Text buttonText)
	{
        buttonText.text = "Wireless Charger";
	}

    //41
    public void Experiment(TMP_Text buttonText)
	{
        buttonText.text = "Experiment";
	}

    //42
    public void TommyGun(TMP_Text buttonText)
	{
        buttonText.text = "Tommy Gun";
	}

    //43
    public void TieUp(TMP_Text buttonText)
	{
        buttonText.text = "Tie Up";
	}

    //44
    public void Corruption(TMP_Text buttonText)
	{
        buttonText.text = "Corruption";
	}

    //45
    public void Colt1911(TMP_Text buttonText)
	{
        buttonText.text = "Colt 1911";
	}

    //46
    public void Mortar(TMP_Text buttonText)
	{
        buttonText.text = "Mortar";
	}

    //47
    public void GreatArmy(TMP_Text buttonText)
	{
        buttonText.text = "Great Army";
	}

    //48
    public void ScorchedEarth(TMP_Text buttonText)
	{
        buttonText.text = "Scorched Earth";
	}

    //49
    public void DoubleEnvelopment(TMP_Text buttonText)
	{
        buttonText.text = "Double Envelopment";
	}

    //50
    public void ContinentalBlockade(TMP_Text buttonText)
	{
        buttonText.text = "Continental Blockade";
	}

    //51
    public void Depression(TMP_Text buttonText)
	{
        buttonText.text = "Depression";
	}

    //52
    public void SelfIsolation(TMP_Text buttonText)
	{
        buttonText.text = "Self-Isolation";
	}

    //53
    public void Knife(TMP_Text buttonText)
	{
        buttonText.text = "Knife";
	}

    //54
    public void Autoportrait(TMP_Text buttonText)
	{
        buttonText.text = "Autoportrait";
	}

    //55
    public void GravityPull(TMP_Text buttonText)
	{
        buttonText.text = "Gravity Pull";
	}

    //56
    public void Kamikaze(TMP_Text buttonText)
	{
        buttonText.text = "Kamikaze";
	}

    //57
    public void TookOff(TMP_Text buttonText)
	{
        buttonText.text = "Took Off";
	}

    //58
    public void AirStrike(TMP_Text buttonText)
	{
        buttonText.text = "Air Strike";
	}

    //59
    public void JusticeCrusade(TMP_Text buttonText)
	{
        buttonText.text = "Justice Crusade";
	}

    //60
    public void Rapier(TMP_Text buttonText)
	{
        buttonText.text = "Rapier";
	}

    //61
    public void ExpeditionaryAssault(TMP_Text buttonText)
	{
        buttonText.text = "Expeditionary Assault";
	}

    //62
    public void Culverin(TMP_Text buttonText)
	{
        buttonText.text = "Culverin";
	}

    //63
    public void FireShip(TMP_Text buttonText)
	{
        buttonText.text = "Fire Ship";
	}

    //101
    public void Yperit(TMP_Text buttonText)
	{
        buttonText.text = "Yperit";
	}

    //102
    public void Blitzkrieg(TMP_Text buttonText)
	{
        buttonText.text = "Blitzkrieg";
	}

    //103
    public void Propaganda(TMP_Text buttonText)
	{
        buttonText.text = "Propaganda";
	}
}
