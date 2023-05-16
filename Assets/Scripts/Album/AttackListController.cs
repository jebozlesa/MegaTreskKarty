using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;

public class AttackListController : MonoBehaviour
{
    public struct AttackData
    {
        public int AttackID;
        public string AttackName;
        public string Description;
        public string Attributes;
        public string Special;
    }

    public Card card;

    public GameObject attackPrefab;
    public Transform attackListContainer;
    public string connectionString;

    private void Start()
    {
        connectionString = $"URI=file:{Database.Instance.GetDatabasePath()}";
    }

    public void ClearAttackList()
    {
        foreach (Transform child in attackListContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public List<AttackData> LoadAttacksFromDatabase(int characterId)
    {
        Debug.Log("LoadAttacksFromDatabase =======>  " + characterId);

        List<AttackData> attacks = new List<AttackData>();

        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = $"SELECT Attacks.AttackID, Attacks.AttackName, Attacks.Description, Attacks.Attributes, Attacks.Special " +
                                $"FROM CharacterAttacks " +
                                $"JOIN Attacks ON CharacterAttacks.AttackID = Attacks.AttackID " +
                                $"WHERE CharacterAttacks.CharacterID = {characterId}";

        IDataReader reader = dbCommand.ExecuteReader();

        while (reader.Read())
        {
            int currentAttackId = reader.GetInt32(0);
            if (!card.ContainsAttack(currentAttackId))
            {
                AttackData attackData = new AttackData
                {
                    AttackID = reader.GetInt32(0),
                    AttackName = reader.GetString(1),
                    Description = reader.GetString(2),
                    Attributes = reader.GetString(3),
                    Special = reader.GetString(4)
                };
                Debug.Log("LoadAttacksFromDatabase - attack =======>  " + reader.GetString(1));

                attacks.Add(attackData);
            }
        }

        reader.Close();
        dbCommand.Dispose();
        dbConnection.Close();

        return attacks;
    }

    public void GenerateAttackList(List<AttackData> attackDataList, int originalAttack)
    {
        Debug.Log("GenerateAttackList called"); // Pridajte tento riadok

        foreach (AttackData attackData in attackDataList)
        {
            GameObject attackItem = Instantiate(attackPrefab, attackListContainer.transform);
            Debug.Log("Parent of attack item2: " + attackItem.transform.parent.name);

            AviableAttack aviableAttackScript = attackItem.GetComponent<AviableAttack>();
            aviableAttackScript.card = card;
            aviableAttackScript.attackId = attackData.AttackID;
            aviableAttackScript.originalAttackId = originalAttack;
            aviableAttackScript.attackName.text = attackData.AttackName;
            aviableAttackScript.attackDescription.text = attackData.Description;
            aviableAttackScript.attackAttributes.text = attackData.Attributes;
            aviableAttackScript.attackSpecial.text = attackData.Special;

            attackItem.transform.SetParent(attackListContainer.transform);
            attackItem.transform.localPosition = Vector3.zero;
            attackItem.transform.localRotation = Quaternion.identity;
            attackItem.transform.localScale = Vector3.one;


            Debug.Log("Attack item created: " + attackData.AttackName); // Pridajte tento riadok
        }
    }



    public void ShowAttackList(int characterId, int originalAttack)
    {
        List<AttackData> attackDataList = LoadAttacksFromDatabase(characterId);
        GenerateAttackList(attackDataList,originalAttack);
        attackListContainer.gameObject.SetActive(true);
    }

}
