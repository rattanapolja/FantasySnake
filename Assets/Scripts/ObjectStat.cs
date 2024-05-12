using System.Collections;
using TMPro;
using UnityEngine;

public class ObjectStat : MonoBehaviour
{
    [SerializeField] private SpriteRenderer m_SpriteRenderer;
    [SerializeField] private TMP_Text m_HealthText;
    [SerializeField] private TMP_Text m_AttackText;

    public int Health
    {
        get { return Data.Health; }
        set
        {
            Data.Health = value;
            m_HealthText.text = $"HP: {Data.Health}";
        }
    }

    public ObjectData Data { get; set; }

    private void Start()
    {
        StartCoroutine(IncreaseStatOvertime());
    }

    public void Init(ObjectData objectData)
    {
        Data = objectData;

        m_HealthText.text = $"HP: {Health}";
        m_AttackText.text = $"ATK: {Data.Attack}";

        if (objectData.ObjectSprite)
            m_SpriteRenderer.sprite = objectData.ObjectSprite;
    }

    private IEnumerator IncreaseStatOvertime()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);

            Health++;
            Data.Attack++;
            m_HealthText.text = $"HP: {Health}";
            m_AttackText.text = $"ATK: {Data.Attack}";
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}

[System.Serializable]
public class StatRange
{
    public int min;
    public int max;

    public StatRange(int min, int max)
    {
        this.min = min;
        this.max = max;
    }
}

public class ObjectData
{
    public int Health;
    public int Attack;
    public Sprite ObjectSprite;
}
