using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    private static GameManager m_Instance;
    public static GameManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = FindObjectOfType<GameManager>();
                if (m_Instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(GameManager).Name);
                    m_Instance = singletonObject.AddComponent<GameManager>();
                }
            }
            return m_Instance;
        }
    }

    [Header("Heroes & Monsters")]
    [SerializeField] private PlayerController m_PlayerController;
    [SerializeField] private List<Sprite> m_HeroesSpriteList;
    [SerializeField] private List<Sprite> m_MonsterSpriteList;

    [Header("Game Over")]
    [SerializeField] private GameObject m_GameOverPanel;
    [SerializeField] private TMP_Text m_MonsterEliminatedText;
    [SerializeField] private Button m_RestartBtn;

    [Header("Content")]
    [SerializeField] private Transform m_HeroContent;
    [SerializeField] private Transform m_ObstacleContent;
    [SerializeField] private Transform m_MonsterContent;

    [Header("Prefabs")]
    [SerializeField] private GameObject m_HeroPrefab;
    [SerializeField] private GameObject m_ObstaclePrefab;
    [SerializeField] private GameObject m_CollectHeroPrefab;
    [SerializeField] private GameObject m_MonsterPrefab;

    [Header("Spawn Configured")]
    [SerializeField] private StatRange m_HealthStat;
    [SerializeField] private StatRange m_AttackStat;
    [SerializeField] private int m_StartNumObstacles;
    [SerializeField] private int m_StartNumollectHeros;
    [SerializeField] private int m_StartNumMonsters;

    private List<ObjectStat> m_HeroesInLineList = new();
    private List<Vector2> m_SpawnGridList = new();

    public Action ActionCallback;

    private Vector2 m_NewPosition;
    private Vector2 m_FirstHeroLastPosition;
    private Vector2 m_LastHeroPosition;
    private int m_MonsterEliminatedCount;

    private void Awake()
    {
        if (m_Instance == null)
        {
            m_Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        SpawnPlayerHero();
        SpawnObstacles(m_StartNumObstacles);
        SpawnCollectHero(m_StartNumollectHeros);
        SpawnMonsters(m_StartNumMonsters);

        m_RestartBtn.onClick.AddListener(OnRestart);
    }

    #region Init State
    private void SpawnPlayerHero()
    {
        GameObject hero = Instantiate(m_HeroPrefab, Vector2.zero, Quaternion.identity);
        ObjectStat obs = hero.GetComponent<ObjectStat>();
        obs.Init(new ObjectData
        {
            Health = Random.Range(m_HealthStat.min, m_HealthStat.max),
            Attack = Random.Range(m_AttackStat.min, m_AttackStat.max),
            ObjectSprite = m_HeroPrefab.GetComponent<SpriteRenderer>().sprite
        });
        hero.AddComponent<CollisionHandler>();
        hero.transform.parent = m_HeroContent;
        m_PlayerController.SetCurrentHero(hero, Vector2.zero);
        m_HeroesInLineList.Add(obs);
        m_SpawnGridList.Add(Vector2.zero);
        m_PlayerController.gameObject.SetActive(true);
    }

    private void SpawnCollectHero(int numollectHeros)
    {
        for (int i = 0; i < numollectHeros; i++)
        {
            Vector2 spawnPosition = GetRandomPosition();
            GameObject collectHero = Instantiate(m_CollectHeroPrefab, spawnPosition, Quaternion.identity);
            collectHero.GetComponent<ObjectStat>().Init(new ObjectData
            {
                Health = Random.Range(m_HealthStat.min, m_HealthStat.max),
                Attack = Random.Range(m_AttackStat.min, m_AttackStat.max),
                ObjectSprite = m_HeroesSpriteList[Random.Range(0, 3)]
            });
            collectHero.transform.parent = m_HeroContent;
        }
    }

    public void SpawnMonsters(int numMonsters)
    {
        for (int i = 0; i < numMonsters; i++)
        {
            Vector2 spawnPosition = GetRandomPosition();
            GameObject monster = Instantiate(m_MonsterPrefab, spawnPosition, Quaternion.identity);
            monster.GetComponent<ObjectStat>().Init(new ObjectData
            {
                Health = (int)Random.Range(m_HealthStat.min / 2f, m_HealthStat.max / 2f),
                Attack = (int)Random.Range(m_AttackStat.min / 2f, m_AttackStat.max / 2f),
                ObjectSprite = m_MonsterSpriteList[Random.Range(0, 3)]
            });
            monster.transform.parent = m_MonsterContent;
        }
    }
    #endregion

    #region SpawnObstacles
    void SpawnObstacles(int numObstacles)
    {
        for (int i = 0; i < numObstacles; i++)
        {
            Vector2 obstaclePosition = GetRandomPosition();
            int obstacleSizeX = Random.Range(1, 3);
            int obstacleSizeY = Random.Range(1, 3);

            if (IsPositionAvailable(obstaclePosition, obstacleSizeX, obstacleSizeY))
            {
                SpawnObstacle(obstaclePosition, obstacleSizeX, obstacleSizeY);
            }
            else
            {
                i--;
            }
        }
    }

    bool IsPositionAvailable(Vector2 position, int sizeX, int sizeY)
    {
        for (float x = position.x; x < position.x + sizeX; x++)
        {
            for (float y = position.y; y < position.y; y++)
            {
                if (x - sizeX / 2f >= -8 && x + sizeX / 2f <= 8 &&
                    y - sizeY / 2f >= -8 && y + sizeY / 2f <= 8 &&
                    m_SpawnGridList.Contains(new Vector2(x, y)))
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void SpawnObstacle(Vector2 position, int sizeX, int sizeY)
    {
        for (float x = position.x; x < position.x + sizeX; x++)
        {
            for (float y = position.y; y < position.y + sizeY; y++)
            {
                Vector2 spawnPosition = new(x, y);
                GameObject obstacle = Instantiate(m_ObstaclePrefab, spawnPosition, Quaternion.identity);
                m_SpawnGridList.Add(new Vector2(x, y));
                obstacle.transform.parent = m_ObstacleContent;
            }
        }
    }
    #endregion

    #region  Gameplay State
    public void GetEliminated()
    {
        Destroy(m_HeroesInLineList[0].gameObject);
        m_HeroesInLineList.RemoveAt(0);
        if (m_HeroesInLineList.Count > 0)
        {
            m_HeroesInLineList[0].tag = "Player";
            m_PlayerController.SetCurrentHero(m_HeroesInLineList[0].gameObject, m_HeroesInLineList.Count > 1 ?
                m_HeroesInLineList[0].transform.position - m_HeroesInLineList[1].transform.position : Vector2.zero);
            m_HeroesInLineList[0].gameObject.AddComponent<CollisionHandler>();
            m_HeroesInLineList[0].GetComponent<BoxCollider2D>().isTrigger = true;
        }
        else
        {
            LoadGameOver();
        }
    }

    public void CollectHeroesToList(GameObject newHero)
    {
        newHero.transform.position = m_LastHeroPosition;
        newHero.tag = "HeroLine";
        m_HeroesInLineList.Add(newHero.GetComponent<ObjectStat>());
        SpawnCollectHero(1);
    }

    public void BattleMonster(GameObject monster)
    {
        bool xMon = false;

        ObjectStat playerStat = m_HeroesInLineList[0];
        ObjectStat monsterStat = monster.GetComponent<ObjectStat>();

        if ((monsterStat.Health - playerStat.Data.Attack) <= 0)
        {
            xMon = true;
        }
        else
        {
            monsterStat.Health -= playerStat.Data.Attack;
        }

        if ((playerStat.Health - monsterStat.Data.Attack) <= 0)
        {
            GetEliminated();
        }
        else
        {
            playerStat.Health -= monsterStat.Data.Attack;
        }

        if (xMon)
        {
            Vector2 spawnPosition = GetRandomPosition();
            monsterStat.Init(new ObjectData
            {
                Health = (int)Random.Range(m_HealthStat.min / 2f, m_HealthStat.max / 2f),
                Attack = (int)Random.Range(m_AttackStat.min / 2f, m_AttackStat.max / 2f),
                ObjectSprite = m_MonsterSpriteList[Random.Range(0, 3)]
            });
            monster.transform.position = spawnPosition;
            m_MonsterEliminatedCount++;
        }
        else
        {
            OnMoveBack();
        }
    }

    public void MovePlayer(Vector2 newPosition)
    {
        m_NewPosition = newPosition;
        m_FirstHeroLastPosition = m_HeroesInLineList[0].transform.position;
        m_LastHeroPosition = m_HeroesInLineList[^1].transform.position;
        m_SpawnGridList.Remove(m_LastHeroPosition);
        m_SpawnGridList.Add(newPosition);
    }

    public void OnMove()
    {
        if (m_HeroesInLineList.Count != 1)
        {
            for (int i = m_HeroesInLineList.Count - 1; i > 0; i--)
            {
                if (i == 1)
                {
                    m_HeroesInLineList[i].transform.position = m_FirstHeroLastPosition;
                }
                else
                {
                    m_HeroesInLineList[i].transform.position = m_HeroesInLineList[i - 1].transform.position;
                }
            }
        }
        ActionCallback?.Invoke();
        ActionCallback = null;
    }

    private void OnMoveBack()
    {
        m_HeroesInLineList[0].GetComponent<BoxCollider2D>().isTrigger = false;
        m_SpawnGridList.Remove(m_NewPosition);
        m_SpawnGridList.Add(m_LastHeroPosition);
        for (int i = 0; i < m_HeroesInLineList.Count; i++)
        {
            if (i == m_HeroesInLineList.Count - 1)
            {
                m_HeroesInLineList[i].transform.position = m_LastHeroPosition;
            }
            else
            {
                m_HeroesInLineList[i].transform.position = m_HeroesInLineList[i + 1].transform.position;
            }
        }
        m_PlayerController.SetCurrentHero(m_HeroesInLineList[0].gameObject, m_HeroesInLineList.Count > 1 ?
            m_HeroesInLineList[0].transform.position - m_HeroesInLineList[1].transform.position : Vector2.zero);
        m_HeroesInLineList[0].GetComponent<BoxCollider2D>().isTrigger = true;
    }

    public void RotateCharacters(bool clockwise, Action action)
    {
        if (m_HeroesInLineList.Count > 1)
        {
            if (clockwise)
            {
                ObjectData lastHero = m_HeroesInLineList[^1].Data;
                for (int i = m_HeroesInLineList.Count - 1; i > 0; i--)
                {
                    m_HeroesInLineList[i].Init(m_HeroesInLineList[i - 1].Data);
                }
                m_HeroesInLineList[0].Init(lastHero);
            }
            else
            {
                ObjectData firstHero = m_HeroesInLineList[0].Data;
                for (int i = 1; i < m_HeroesInLineList.Count; i++)
                {
                    m_HeroesInLineList[i - 1].Init(m_HeroesInLineList[i].Data);
                }
                m_HeroesInLineList[^1].Init(firstHero);
            }
        }
        action?.Invoke();
    }

    private void OnRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Init();
    }

    public void LoadGameOver()
    {
        m_PlayerController.gameObject.SetActive(false);
        m_GameOverPanel.SetActive(true);
        m_MonsterEliminatedText.text = $"Monster eliminated :{m_MonsterEliminatedCount}";
    }
    #endregion

    private Vector2 GetRandomPosition()
    {
        Vector2 randomPosition = Vector2Int.zero;
        do
        {
            randomPosition.x = Random.Range(-7, 9);
            randomPosition.y = Random.Range(-7, 9);
        } while (m_SpawnGridList.Contains(randomPosition));
        m_SpawnGridList.Add(randomPosition);

        return randomPosition;
    }
}
