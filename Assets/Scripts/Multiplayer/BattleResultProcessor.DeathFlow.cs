using UnityEngine;

public partial class BattleResultProcessor
{
    private void CheckBattleOutcome(Kard myCard, Kard enemyCard)
    {
        if (roundCoordinator == null)
        {
            roundCoordinator = new BattleRoundCoordinator(
                fightSystem,
                multiplayerService,
                dialogText
            );
        }

        bool myCardDead = myCard.health <= 0;
        bool enemyCardDead = enemyCard.health <= 0;

        if (myCardDead && enemyCardDead)
        {
            if (dialogText != null)
            {
                dialogText.text = "Both cards destroyed!";
            }

            Debug.Log("[BattleResultProcessor] Both cards died - checking remaining cards");
            StartCoroutine(roundCoordinator.HandleBothCardsDeath(myCard, enemyCard));
        }
        else if (myCardDead)
        {
            Debug.Log("[BattleResultProcessor] My card died - checking if I have more cards");
            StartCoroutine(roundCoordinator.HandlePlayerCardDeath(myCard));
        }
        else if (enemyCardDead)
        {
            if (dialogText != null)
            {
                dialogText.text = "Enemy card destroyed!";
            }

            Debug.Log("[BattleResultProcessor] Enemy card died");
            StartCoroutine(roundCoordinator.HandleEnemyCardDeath(enemyCard));
        }
        else
        {
            Debug.Log("[BattleResultProcessor] Battle continues - preparing next turn");
            StartCoroutine(roundCoordinator.PrepareNextTurn());
        }
    }
}
