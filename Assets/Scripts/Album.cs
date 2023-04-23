using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Album : MonoBehaviour
{

    public TextAsset cardDatabase;
    private List<string> kartyData;
    public TextAsset cardStories;
    private List<string> kartyStories;
    public GameObject kartaPrefab;
    public GameObject album;
    public GameObject zoomedCardHolder;
    public AttackDescriptions attackDescriptions;
    public GameObject content;



    // Start is called before the first frame update
    void Start()
    {
        kartyData = new List<string>(cardDatabase.text.TrimEnd().Split('\n'));
        kartyStories = new List<string>(cardStories.text.TrimEnd().Split('$'));
        StartCoroutine(VytvorKarty());

        
    }

    // Update is called once per frame
    string LoadStory(int cardID)
    {
        string cardStory = "";
        for (int j = 1; j < kartyStories.Count; j++)
        {
            string[] storyParts = kartyStories[j].Split('#');
            Debug.Log("KRISTABNOHA   '" + storyParts[1] + "'");
            int storyCardID = int.Parse(storyParts[0]);

            if (cardID == storyCardID)
            {
                cardStory = storyParts[2];
                break;
            }
        }
        return cardStory;
    }

    private IEnumerator VytvorKarty()
    {
    
        for (int i = 0; i < kartyData.Count; i++)
        {

            Debug.Log("vytvaram kartu " + i);
            string kartaString = kartyData[i];
            string[] kartaHodnoty = kartaString.Split(',');

            string[] farbaKarty = kartaHodnoty[10].Split(';');
            Color32 cardColor = new Color32(byte.Parse(farbaKarty[0]), byte.Parse(farbaKarty[1]), byte.Parse(farbaKarty[2]), 255);

            GameObject novaKarta = Instantiate(kartaPrefab, transform);

            novaKarta.GetComponent<Card>().cardName = kartaHodnoty[1];
            novaKarta.GetComponent<Card>().health = int.Parse(kartaHodnoty[2]);
            novaKarta.GetComponent<Card>().strength = int.Parse(kartaHodnoty[3]);
            novaKarta.GetComponent<Card>().speed = int.Parse(kartaHodnoty[4]);
            novaKarta.GetComponent<Card>().attack = int.Parse(kartaHodnoty[5]);
            novaKarta.GetComponent<Card>().defense = int.Parse(kartaHodnoty[6]);
            novaKarta.GetComponent<Card>().knowledge = int.Parse(kartaHodnoty[7]);
            novaKarta.GetComponent<Card>().charisma = int.Parse(kartaHodnoty[8]);
            novaKarta.GetComponent<Card>().image = kartaHodnoty[9];
            novaKarta.GetComponent<Card>().color = cardColor;
            novaKarta.GetComponent<Card>().level = int.Parse(kartaHodnoty[11]);
            novaKarta.GetComponent<Card>().attack1 = int.Parse(kartaHodnoty[12]);
           // novaKarta.GetComponent<Card>().countAttack1 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(),int.Parse(kartaHodnoty[12]));
            novaKarta.GetComponent<Card>().attack2 = int.Parse(kartaHodnoty[13]);
           // novaKarta.GetComponent<Card>().countAttack2 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(),int.Parse(kartaHodnoty[13]));
            novaKarta.GetComponent<Card>().attack3 = int.Parse(kartaHodnoty[14]);
           // novaKarta.GetComponent<Card>().countAttack3 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(),int.Parse(kartaHodnoty[14]));
            novaKarta.GetComponent<Card>().attack4 = int.Parse(kartaHodnoty[15]);
          //  novaKarta.GetComponent<Card>().countAttack4 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(),int.Parse(kartaHodnoty[15]));

            novaKarta.GetComponent<Card>().story = LoadStory(int.Parse(kartaHodnoty[0]));

            novaKarta.GetComponent<Card>().zoomedCardHolder = zoomedCardHolder;

            novaKarta.GetComponent<Card>().transform.SetParent(content.transform); 
            novaKarta.GetComponent<Card>().transform.localScale = Vector3.one;

            yield return new WaitForSeconds(0.05f);
        }
    }
}
