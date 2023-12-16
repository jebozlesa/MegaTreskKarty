using System;
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

    public int LoadAttackCount(IAttackCount attacker, int attack)
    {
        int count = 0;

        switch (attack)
        {
            case 1://Punch
                count = 20 + attacker.strength;
                break;
            case 2://Kick
                count = 20 + attacker.strength;
                break;
            case 3://Heal
                count = 3 + (attacker.knowledge / 3);
                break;
            case 4://Forgiveness
                count = attacker.charisma / 2;
                break;
            case 5://Crusade
                count = 10 + attacker.charisma;
                break;
            case 6://"Water to Wine
                count = 5 + (attacker.knowledge / 2);
                break;
            case 7://Car Hit
                count = 3 + (attacker.charisma / 3);
                break;
            case 8://MonkeyWrench
                count = 10 + attacker.strength;
                break;
            case 9://Radiation
                count = 5 + (attacker.knowledge / 2);
                break;
            case 10://Scratch
                count = 20 + attacker.strength;
                break;
            case 11://Scientific Lecture
                count = 5 + (attacker.knowledge / 2);
                break;
            case 12://Chi Sau
                count = 5 + (attacker.defense / 2);
                break;
            case 13://One Inch Punch
                count = 5 + (attacker.attack / 2);
                break;
            case 14://Up In Smoke
                count = 3 + (attacker.charisma / 3);
                break;
            case 15://Sing
                count = 3 + (attacker.knowledge / 3);
                break;
            case 16://Revolver
                count = 6;
                break;
            case 17://Heavy Artillery
                count = 3 + (attacker.charisma / 3);
                break;
            case 18://Bloodthirst
                count = 5 + (attacker.attack / 2);
                break;
            case 19://Sword
                count = 10 + attacker.strength;
                break;
            case 20://Pike
                count = 10 + attacker.strength;
                break;
            case 21://Terrify
                count = 5 + (attacker.defense / 2);
                break;
            case 22://Drink Wine
                count = 3 + (attacker.strength / 3);
                break;
            case 23://Flaming Gun
                count = 5 + (attacker.attack / 2);
                break;
            case 24://Cleaver
                count = 10 + attacker.strength;
                break;
            case 25://Pan
                count = 10 + attacker.strength;
                break;
            case 26://Boost
                count = 3 + (attacker.strength / 3);
                break;
            case 27://Temptation
                count = 5 + (attacker.charisma / 2);
                break;
            case 28://Shamshir
                count = 10 + attacker.strength;
                break;
            case 29://Diplomacy
                count = 5 + (attacker.knowledge / 2);
                break;
            case 30://Siege
                count = 3 + (attacker.attack / 3);
                break;
            case 31://TreeStratagem
                count = (int)System.Math.Ceiling((double)attacker.knowledge / 5);                         //KOKOTINA, DAT DO PICE!
                break;
            case 32://Tomahawk
                count = 10 + attacker.strength;
                break;
            case 33://PeacePipe
                count = 5 + (attacker.charisma / 2);
                break;
            case 34://RecurveBow
                count = attacker.strength * 2;
                break;
            case 35://Fury
                count = attacker.attack;
                break;
            case 36://Guerilla
                count = attacker.attack * 5;
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
                count = new int[] { 2, 3, 5, 10 }[UnityEngine.Random.Range(0, 4)];
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
            case 57://TakeOff
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
            case 64://HandcuffEscape
                count = attacker.knowledge * 2;
                break;
            case 65://Illusion
                count = attacker.knowledge * 2;
                break;
            case 66://CarcanoM91
                count = 6;
                break;
            case 67://Winchester
                count = 11;
                break;
            case 68://Ambush
                count = attacker.speed;
                break;
            case 69://JupiterC
                count = 3;
                break;
            case 70://V2
                count = 5 + attacker.charisma / 3;
                break;
            case 71://BattleCry
                count = attacker.charisma;
                break;
            case 72://Revelation
                count = 10;
                break;
            case 73://Standard
                count = attacker.strength * 10;
                break;
            case 74://Pen
                count = (attacker.knowledge * 5) + 10;
                break;
            case 75://IambicPentameter
                count = 3;
                break;
            case 76://Ghost
                count = 4;
                break;
            case 77://BuffaloHorns
                count = 10;
                break;
            case 78://Iklwa
                count = attacker.strength * 5;
                break;
            case 79://Iwisa
                count = 5;
                break;
            case 80://NitenIchiRyu
                count = attacker.attack;
                break;
            case 81://Tessenjutsu
                count = attacker.defense;
                break;
            case 82://Iaijutsu
                count = attacker.speed;
                break;
            case 83://Katana
                count = attacker.strength * 5;
                break;
            case 84://Nodachi
                count = attacker.strength * 5;
                break;
            case 85://Yumi
                count = 20;
                break;
            case 86://Jujutsu
                count = attacker.attack;
                break;
            case 87://Espionage
                count = 10;
                break;
            case 88://Sabre
                count = attacker.strength * 5;
                break;
            case 89://Gamble
                count = attacker.charisma;
                break;
            case 90://Philosophy
                count = 10;
                break;
            case 91://Calm
                count = attacker.charisma; ;
                break;
            case 92://Honesty
                count = attacker.knowledge * 2;
                break;
            case 93://Valaska
                count = attacker.strength * 5;
                break;
            case 94://Moonshine
                count = attacker.strength;
                break;
            case 95://OutlawBand
                count = attacker.charisma;
                break;
            case 96://FlintlockPistol
                count = attacker.attack * 3;
                break;
            case 97://PassiveResistance
                count = attacker.knowledge * 2;
                break;
            case 98://HungerStrike
                count = attacker.health / 5;
                break;
            case 99://Gladius
                count = attacker.strength * 5;
                break;
            case 100://ShieldBash
                count = attacker.defense * 3;
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
            case 104://Retiarius
                count = attacker.attack * 3;
                break;
            case 105://Shuriken
                count = attacker.attack;
                break;
            case 106://Kusarigama
                count = attacker.strength * 5;
                break;
            case 107://Ninjutsu
                count = (attacker.attack + attacker.knowledge) / 4;
                break;
            case 108://OrientalSpice
                count = (int)System.Math.Ceiling((double)attacker.charisma / 5);
                break;
            case 109://FieryArquebus
                count = attacker.attack;
                break;
            case 110://PirateRaid
                count = attacker.attack / 2;
                break;
            case 111://Axe
                count = attacker.strength * 5;
                break;
            case 112://JaguarWarriors
                count = attacker.attack * 5;
                break;
            case 113://Atlatl
                count = (int)Math.Round(attacker.strength * 1.2);
                break;
            case 114://Macuahuitl
                count = attacker.strength * 5;
                break;
            case 115://Cubism
                count = 3;
                break;
            case 116://CosaNostra
                count = attacker.charisma;
                break;
            case 117://Act
                count = 5;
                break;
            case 118://Football
                count = (int)Math.Round(attacker.strength * 1.5);
                break;
            case 119://BicycleKick
                count = attacker.strength * 3;
                break;
            case 120://WorldChampion
                count = 3;
                break;
            case 121://ShaolinSoccer
                count = 3;
                break;
            case 123://Curse
                count = 5;
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
                dialogText.text = "Monkey Wrench chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
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
                dialogText.text = "Bloodthirst chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
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
                dialogText.text = "Take Off chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
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
            case 64:
                dialogText.text = "Handcuff Escape chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 65:
                dialogText.text = "Illusion chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 66:
                dialogText.text = "Carcano M91 chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 67:
                dialogText.text = "Winchester chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 68:
                dialogText.text = "Ambush heist chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 69:
                dialogText.text = "Space Rocket chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 70:
                dialogText.text = "Vergeltungswaffe 2 chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 71:
                dialogText.text = "Battle Cry chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 72:
                dialogText.text = "Celestial Revelation chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 73:
                dialogText.text = "Standard chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 74:
                dialogText.text = "Pen chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 75:
                dialogText.text = "Iambic Pentameter chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 76:
                dialogText.text = "Ghost chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 77:
                dialogText.text = "Buffalo Horns chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 78:
                dialogText.text = "Iklwa chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 79:
                dialogText.text = "Iwisa chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 80:
                dialogText.text = "Niten Ichi-Ryu chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 81:
                dialogText.text = "Tessenjutsu Horns chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 82:
                dialogText.text = "Iaijutsu chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 83:
                dialogText.text = "Katana chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 84:
                dialogText.text = "Nodachi chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 85:
                dialogText.text = "Yumi chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 86:
                dialogText.text = "Jujutsu chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 87:
                dialogText.text = "Espionage chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 88:
                dialogText.text = "Sabre chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 89:
                dialogText.text = "Gamble chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 90:
                dialogText.text = "Philosophy chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 91:
                dialogText.text = "Calm chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 92:
                dialogText.text = "Honesty chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 93:
                dialogText.text = "Valaska chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 94:
                dialogText.text = "Moonshine chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 95:
                dialogText.text = "Outlaw Band chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 96:
                dialogText.text = "Flintlock Pistol chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 97:
                dialogText.text = "Passive Resistance chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 98:
                dialogText.text = "Hunger Strike chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 99:
                dialogText.text = "Gladius chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 100:
                dialogText.text = "Shield Bash Strike chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
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
                dialogText.text = "Retiarius chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 105:
                dialogText.text = "Shuriken chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 106:
                dialogText.text = "Kusarigama chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 107:
                dialogText.text = "Ninjutsu chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 108:
                dialogText.text = "Oriental Spice chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 109:
                dialogText.text = "Arquebus chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 110:
                dialogText.text = "Pirate Raid chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 111:
                dialogText.text = "Axe chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 112:
                dialogText.text = "Jaguar Warriors chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 113:
                dialogText.text = "Atlatl chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 114:
                dialogText.text = "Macuahuitl chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 115:
                dialogText.text = "Cubism chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 116:
                dialogText.text = "La cosa nostra chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 117:
                dialogText.text = "Act a fool chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 118:
                dialogText.text = "Football chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 119:
                dialogText.text = "Bicycle kick chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 120:
                dialogText.text = "World champion chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 121:
                dialogText.text = "Shaolin soccer chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            case 123:
                dialogText.text = "Curse chosen \nLaunch here !!! " + attacker.attackCount[attackType] + " remaining";
                break;
            default:
                Debug.LogError("Invalid attack type.");
                break;
        }
    }


    public void ShowDescription(Kard card)
    {
        ShowDescriptionButton(card.attack1, button1Text, button1);
        ShowDescriptionButton(card.attack2, button2Text, button2);
        ShowDescriptionButton(card.attack3, button3Text, button3);
        ShowDescriptionButton(card.attack4, button4Text, button4);
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
                MonkeyWrench(buttonText);
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
                Bloodthirst(buttonText);
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
                TakeOff(buttonText);
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
            case 64:
                HandcuffEscape(buttonText);
                break;
            case 65:
                Illusion(buttonText);
                break;
            case 66:
                CarcanoM91(buttonText);
                break;
            case 67:
                Winchester(buttonText);
                break;
            case 68:
                Ambush(buttonText);
                break;
            case 69:
                JupiterC(buttonText);
                break;
            case 70:
                V2(buttonText);
                break;
            case 71:
                BattleCry(buttonText);
                break;
            case 72:
                Revelation(buttonText);
                break;
            case 73:
                Standard(buttonText);
                break;
            case 74:
                Pen(buttonText);
                break;
            case 75:
                IambicPentameter(buttonText);
                break;
            case 76:
                Ghost(buttonText);
                break;
            case 77:
                BuffaloHorns(buttonText);
                break;
            case 78:
                Iklwa(buttonText);
                break;
            case 79:
                Iwisa(buttonText);
                break;
            case 80:
                NitenIchiRyu(buttonText);
                break;
            case 81:
                Tessenjutsu(buttonText);
                break;
            case 82:
                Iaijutsu(buttonText);
                break;
            case 83:
                Katana(buttonText);
                break;
            case 84:
                Nodachi(buttonText);
                break;
            case 85:
                Yumi(buttonText);
                break;
            case 86:
                Jujutsu(buttonText);
                break;
            case 87:
                Espionage(buttonText);
                break;
            case 88:
                Sabre(buttonText);
                break;
            case 89:
                Gamble(buttonText);
                break;
            case 90:
                Philosophy(buttonText);
                break;
            case 91:
                Calm(buttonText);
                break;
            case 92:
                Honesty(buttonText);
                break;
            case 93:
                Valaska(buttonText);
                break;
            case 94:
                Moonshine(buttonText);
                break;
            case 95:
                OutlawBand(buttonText);
                break;
            case 96:
                FlintlockPistol(buttonText);
                break;
            case 97:
                PassiveResistance(buttonText);
                break;
            case 98:
                HungerStrike(buttonText);
                break;
            case 99:
                Gladius(buttonText);
                break;
            case 100:
                ShieldBash(buttonText);
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
                Retiarius(buttonText);
                break;
            case 105:
                Shuriken(buttonText);
                break;
            case 106:
                Kusarigama(buttonText);
                break;
            case 107:
                Ninjutsu(buttonText);
                break;
            case 108:
                OrientalSpice(buttonText);
                break;
            case 109:
                FieryArquebus(buttonText);
                break;
            case 110:
                PirateRaid(buttonText);
                break;
            case 111:
                Axe(buttonText);
                break;
            case 112:
                JaguarWarriors(buttonText);
                break;
            case 113:
                Atlatl(buttonText);
                break;
            case 114:
                Macuahuitl(buttonText);
                break;
            case 115:
                Cubism(buttonText);
                break;
            case 116:
                CosaNostra(buttonText);
                break;
            case 117:
                Act(buttonText);
                break;
            case 118:
                Football(buttonText);
                break;
            case 119:
                BicycleKick(buttonText);
                break;
            case 120:
                WorldChampion(buttonText);
                break;
            case 121:
                ShaolinSoccer(buttonText);
                break;
            case 123:
                Curse(buttonText);
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
    public void MonkeyWrench(TMP_Text buttonText)
    {
        buttonText.text = "Monkey Wrench";
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
    public void Bloodthirst(TMP_Text buttonText)
    {
        buttonText.text = "Bloodthirst";
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
    public void TakeOff(TMP_Text buttonText)
    {
        buttonText.text = "Take Off";
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

    //64
    public void HandcuffEscape(TMP_Text buttonText)
    {
        buttonText.text = "Handcuff Escape";
    }

    //65
    public void Illusion(TMP_Text buttonText)
    {
        buttonText.text = "Illusion";
    }

    //66
    public void CarcanoM91(TMP_Text buttonText)
    {
        buttonText.text = "Carcano M91";
    }

    //67
    public void Winchester(TMP_Text buttonText)
    {
        buttonText.text = "Winchester";
    }

    //68
    public void Ambush(TMP_Text buttonText)
    {
        buttonText.text = "Ambush";
    }
    //69
    public void JupiterC(TMP_Text buttonText)
    {
        buttonText.text = "Space Rocket";
    }
    //70
    public void V2(TMP_Text buttonText)
    {
        buttonText.text = "V2";
    }
    //71
    public void BattleCry(TMP_Text buttonText)
    {
        buttonText.text = "Battle Cry";
    }
    //72
    public void Revelation(TMP_Text buttonText)
    {
        buttonText.text = "Revelation";
    }
    //73
    public void Standard(TMP_Text buttonText)
    {
        buttonText.text = "Standard";
    }
    //74
    public void Pen(TMP_Text buttonText)
    {
        buttonText.text = "Pen";
    }
    //75
    public void IambicPentameter(TMP_Text buttonText)
    {
        buttonText.text = "Iambic Pentameter";
    }
    //76
    public void Ghost(TMP_Text buttonText)
    {
        buttonText.text = "Ghost";
    }
    //77
    public void BuffaloHorns(TMP_Text buttonText)
    {
        buttonText.text = "Buffalo Horns";
    }
    //78
    public void Iklwa(TMP_Text buttonText)
    {
        buttonText.text = "Iklwa";
    }
    //79
    public void Iwisa(TMP_Text buttonText)
    {
        buttonText.text = "Iwisa";
    }
    //80
    public void NitenIchiRyu(TMP_Text buttonText)
    {
        buttonText.text = "Niten Ichi-Ryu";
    }
    //81
    public void Tessenjutsu(TMP_Text buttonText)
    {
        buttonText.text = "Tessenjutsu";
    }
    //82
    public void Iaijutsu(TMP_Text buttonText)
    {
        buttonText.text = "Iaijutsu";
    }
    //83
    public void Katana(TMP_Text buttonText)
    {
        buttonText.text = "Katana";
    }
    //84
    public void Nodachi(TMP_Text buttonText)
    {
        buttonText.text = "Nodachi";
    }
    //85
    public void Yumi(TMP_Text buttonText)
    {
        buttonText.text = "Yumi";
    }
    //86
    public void Jujutsu(TMP_Text buttonText)
    {
        buttonText.text = "Jujutsu";
    }
    //87
    public void Espionage(TMP_Text buttonText)
    {
        buttonText.text = "Espionage";
    }
    //88
    public void Sabre(TMP_Text buttonText)
    {
        buttonText.text = "Sabre";
    }
    //89
    public void Gamble(TMP_Text buttonText)
    {
        buttonText.text = "Gamble";
    }
    //90
    public void Philosophy(TMP_Text buttonText)
    {
        buttonText.text = "Philosophy";
    }
    //91
    public void Calm(TMP_Text buttonText)
    {
        buttonText.text = "Calm";
    }
    //92
    public void Honesty(TMP_Text buttonText)
    {
        buttonText.text = "Honesty";
    }
    //93
    public void Valaska(TMP_Text buttonText)
    {
        buttonText.text = "Valaska";
    }
    //94
    public void Moonshine(TMP_Text buttonText)
    {
        buttonText.text = "Moonshine";
    }
    //95
    public void OutlawBand(TMP_Text buttonText)
    {
        buttonText.text = "Outlaw Band";
    }
    //96
    public void FlintlockPistol(TMP_Text buttonText)
    {
        buttonText.text = "Flintlock Pistol";
    }
    //97
    public void PassiveResistance(TMP_Text buttonText)
    {
        buttonText.text = "Passive Resistance";
    }
    //98
    public void HungerStrike(TMP_Text buttonText)
    {
        buttonText.text = "Hunger Strike";
    }
    //99
    public void Gladius(TMP_Text buttonText)
    {
        buttonText.text = "Gladius";
    }
    //100
    public void ShieldBash(TMP_Text buttonText)
    {
        buttonText.text = "Shield Bash";
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
    //104
    public void Retiarius(TMP_Text buttonText)
    {
        buttonText.text = "Retiarius";
    }
    //105
    public void Shuriken(TMP_Text buttonText)
    {
        buttonText.text = "Shuriken";
    }
    //106
    public void Kusarigama(TMP_Text buttonText)
    {
        buttonText.text = "Kusarigama";
    }
    //107
    public void Ninjutsu(TMP_Text buttonText)
    {
        buttonText.text = "Ninjutsu";
    }
    //108
    public void OrientalSpice(TMP_Text buttonText)
    {
        buttonText.text = "Oriental Spice";
    }
    //109
    public void FieryArquebus(TMP_Text buttonText)
    {
        buttonText.text = "Arquebus";
    }
    //110
    public void PirateRaid(TMP_Text buttonText)
    {
        buttonText.text = "Pirate Raid";
    }
    //111
    public void Axe(TMP_Text buttonText)
    {
        buttonText.text = "Axe";
    }
    //112
    public void JaguarWarriors(TMP_Text buttonText)
    {
        buttonText.text = "Jaguar Warriors";
    }
    //113
    public void Atlatl(TMP_Text buttonText)
    {
        buttonText.text = "Atlatl";
    }
    //114
    public void Macuahuitl(TMP_Text buttonText)
    {
        buttonText.text = "Macuahuitl";
    }
    //115
    public void Cubism(TMP_Text buttonText)
    {
        buttonText.text = "Cubism";
    }
    //116
    public void CosaNostra(TMP_Text buttonText)
    {
        buttonText.text = "La Cosa Nostra";
    }
    //117
    public void Act(TMP_Text buttonText)
    {
        buttonText.text = "Act a fool";
    }
    //118
    public void Football(TMP_Text buttonText)
    {
        buttonText.text = "Football";
    }
    //119
    public void BicycleKick(TMP_Text buttonText)
    {
        buttonText.text = "Bicycle Kick";
    }
    //120
    public void WorldChampion(TMP_Text buttonText)
    {
        buttonText.text = "World Champion";
    }
    //121
    public void ShaolinSoccer(TMP_Text buttonText)
    {
        buttonText.text = "Shaolin Soccer";
    }
    //121
    public void Curse(TMP_Text buttonText)
    {
        buttonText.text = "Curse";
    }
}
