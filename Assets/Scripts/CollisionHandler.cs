using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
        {
            ObstacleCollision();
        }
        else if (other.CompareTag("Hero"))
        {
            CollectHero(other.gameObject);
        }
        else if (other.CompareTag("Monster"))
        {
            BattleMonster(other.gameObject);
        }
        else if (other.CompareTag("HeroLine"))
        {
            ObstacleCollision();
        }
    }

    private void ObstacleCollision()
    {
        GameManager.Instance.LoadGameOver();
    }

    private void CollectHero(GameObject hero)
    {
        GameManager.Instance.ActionCallback = () => GameManager.Instance.CollectHeroesToList(hero);
    }

    private void BattleMonster(GameObject monster)
    {
        GameManager.Instance.ActionCallback = () => GameManager.Instance.BattleMonster(monster);
    }
}
