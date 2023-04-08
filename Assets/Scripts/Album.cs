using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Album : MonoBehaviour
{

    public TextAsset cardDatabase;
    private List<string> kartyData;
    public GameObject kartaPrefab;
    public GameObject album;
    public AttackDescriptions attackDescriptions;
    public GameObject content;



    // Start is called before the first frame update
    void Start()
    {
        kartyData = new List<string>(cardDatabase.text.TrimEnd().Split('\n'));
        StartCoroutine(VytvorKarty());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator VytvorKarty()
    {
    
        for (int i = 0; i < kartyData.Count; i++)
        {

            Debug.Log("vytvaram kartu " + i);
            string kartaString = kartyData[i];
            string[] kartaHodnoty = kartaString.Split(',');

            string[] farbaKarty = kartaHodnoty[9].Split(';');
            Color32 cardColor = new Color32(byte.Parse(farbaKarty[0]), byte.Parse(farbaKarty[1]), byte.Parse(farbaKarty[2]), 255);

            GameObject novaKarta = Instantiate(kartaPrefab, transform);

            novaKarta.GetComponent<Kard>().cardName = kartaHodnoty[0];
            novaKarta.GetComponent<Kard>().health = int.Parse(kartaHodnoty[1]);
            novaKarta.GetComponent<Kard>().strength = int.Parse(kartaHodnoty[2]);
            novaKarta.GetComponent<Kard>().speed = int.Parse(kartaHodnoty[3]);
            novaKarta.GetComponent<Kard>().attack = int.Parse(kartaHodnoty[4]);
            novaKarta.GetComponent<Kard>().defense = int.Parse(kartaHodnoty[5]);
            novaKarta.GetComponent<Kard>().knowledge = int.Parse(kartaHodnoty[6]);
            novaKarta.GetComponent<Kard>().charisma = int.Parse(kartaHodnoty[7]);
            novaKarta.GetComponent<Kard>().image = kartaHodnoty[8];
            novaKarta.GetComponent<Kard>().color = cardColor;
            novaKarta.GetComponent<Kard>().level = int.Parse(kartaHodnoty[10]);
            novaKarta.GetComponent<Kard>().attack1 = int.Parse(kartaHodnoty[11]);
            novaKarta.GetComponent<Kard>().countAttack1 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(),int.Parse(kartaHodnoty[11]));
            novaKarta.GetComponent<Kard>().attack2 = int.Parse(kartaHodnoty[12]);
            novaKarta.GetComponent<Kard>().countAttack2 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(),int.Parse(kartaHodnoty[12]));
            novaKarta.GetComponent<Kard>().attack3 = int.Parse(kartaHodnoty[13]);
            novaKarta.GetComponent<Kard>().countAttack3 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(),int.Parse(kartaHodnoty[13]));
            novaKarta.GetComponent<Kard>().attack4 = int.Parse(kartaHodnoty[14]);
            novaKarta.GetComponent<Kard>().countAttack4 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(),int.Parse(kartaHodnoty[14]));

            novaKarta.GetComponent<Kard>().transform.SetParent(content.transform); 
            novaKarta.GetComponent<Kard>().transform.localScale = Vector3.one;

            yield return new WaitForSeconds(0.05f);
        }
    }
}
