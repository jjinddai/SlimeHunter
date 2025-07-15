using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Monster : MonoBehaviour
{
    [SerializeField] private HpBarSlider hpBar;
    [SerializeField] private Text nameText;
    [SerializeField] private string monsterName;
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private int minGold = 10;
    [SerializeField] private int maxGold = 30;

    private float curHp;
    private bool isDead = false;
    private Animator animator;

    private void Awake()
    {
        curHp = maxHp;

        animator = GetComponent<Animator>();

        if (nameText != null)
        {
            nameText.text = monsterName;
        }
    }

    void Start()
    {
        if (hpBar != null)
        {
            hpBar.SetMaxHp(maxHp);
            hpBar.SetHp(curHp);
        }
    }

    public void OnHit(float damage)
    {
        if (isDead) return;

        curHp -= damage;

        if (curHp <= 0)
        {
            curHp = 0;
            isDead = true;
        }

        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        if (hpBar != null)
        {
            hpBar.SetHp(curHp);
        }

        if (isDead)
        {
            if (animator != null)
            {
                animator.SetTrigger("Death");
            }
            OnDeath();
        }
    }

    private void OnDeath()
    {
        DropGold();

        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            if (gameObject.CompareTag("BossSlime"))
            {
                gameManager.OnBossKilled();
            }
            else
            {
                gameManager.OnMonsterKilled();
            }
        }

        if (nameText != null)
        {
            nameText.text = $"{monsterName} (Dead)";
        }

        Destroy(gameObject, 1.5f);
    }

    private void DropGold()
    {
        int goldMultiplier = 1;

        if (gameObject.CompareTag("BossSlime"))
        {
            goldMultiplier = 5;
        }

        int goldAmount = Random.Range(minGold, maxGold + 1) * goldMultiplier;

        GoldManager goldManager = FindObjectOfType<GoldManager>();
        if (goldManager != null)
        {
            goldManager.AddGold(goldAmount);
        }

        OnGoldDropped?.Invoke(goldAmount, transform.position);
    }

    public static System.Action<int, Vector3> OnGoldDropped;

    public void Heal(float healAmount)
    {
        if (isDead) return;

        curHp += healAmount;
        if (curHp > maxHp)
        {
            curHp = maxHp;
        }

        if (hpBar != null)
        {
            hpBar.SetHp(curHp);
        }
    }

    public float GetCurrentHp()
    {
        return curHp;
    }

    public float GetMaxHp()
    {
        return maxHp;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public void SetMaxHp(float newMaxHp)
    {
        maxHp = newMaxHp;
        if (curHp > maxHp)
        {
            curHp = maxHp;
        }

        if (hpBar != null)
        {
            hpBar.SetMaxHp(maxHp);
            hpBar.SetHp(curHp);
        }
    }

    public void SetGoldDropRange(int min, int max)
    {
        minGold = Mathf.Max(0, min);
        maxGold = Mathf.Max(minGold, max);
    }

    public void SetMonsterName(string newName)
    {
        monsterName = newName;
        if (nameText != null)
        {
            nameText.text = monsterName;
        }
    }

    public string GetMonsterName()
    {
        return monsterName;
    }

    public void ForceDropGold()
    {
        DropGold();
    }

    public string GetGoldDropInfo()
    {
        string multiplier = gameObject.CompareTag("BossSlime") ? " x5 (보스)" : "";
        return $"골드 드롭: {minGold} ~ {maxGold}{multiplier}";
    }

    public bool IsBossMonster()
    {
        return gameObject.CompareTag("BossSlime");
    }
}