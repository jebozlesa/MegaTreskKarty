using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class KartyManager : MonoBehaviour
{
    public GameObject kartaPrefab;
    public GameObject hrac;
    public GameObject enemy;
    public TextAsset cardDatabase;

    private List<string> kartyData;

    private IEnumerator Start()
    {
        // Načítajte data zo súboru a uložte ich do zoznamu
        kartyData = new List<string>(cardDatabase.text.TrimEnd().Split('\n'));

        // Vytvorte šesť náhodných kariet pre hráča
        //StartCoroutine(VytvorSestKariet());

        StartCoroutine(VytvorKarty(6,hrac));
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(VytvorKarty(6,enemy));
    }

    private IEnumerator VytvorKarty(int pocet, GameObject player)
    {
        // Pre každú kartu vytvorte náhodné atribúty
        for (int i = 0; i < pocet; i++)
        {
            // Vyberte náhodnú kartu zo súboru
            int index = Random.Range(0, kartyData.Count);
            string kartaString = kartyData[index];

            // Rozdelenie riadku na hodnoty atribútov
            string[] kartaHodnoty = kartaString.Split(',');

            string[] farbaKarty = kartaHodnoty[9].Split(';');
            Color32 cardColor = new Color32(byte.Parse(farbaKarty[0]), byte.Parse(farbaKarty[1]), byte.Parse(farbaKarty[2]), 255);

            // Vytvorenie novej karty
            GameObject novaKarta = Instantiate(kartaPrefab, player.transform);
            novaKarta.GetComponent<Kard>().name = kartaHodnoty[0];
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

            yield return new WaitForSeconds(0);
        }
    }

    
}
