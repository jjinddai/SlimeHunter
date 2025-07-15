using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBarSlider : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;
    [SerializeField] private float maxHp = 100f;
    private float currentHp;

    void Start()
    {
        currentHp = maxHp;
        InitializeHpBar();
    }

    void InitializeHpBar()
    {
        if (hpSlider == null)
        {
            hpSlider = GetComponent<Slider>();
            if (hpSlider == null)
            {
                return;
            }
        }

        hpSlider.minValue = 0f;
        hpSlider.maxValue = 1f;
        hpSlider.interactable = false;

        UpdateHpBar();
    }

    public void SetHp(float hp)
    {
        currentHp = Mathf.Clamp(hp, 0f, maxHp);
        UpdateHpBar();
    }

    public void ChangeHpBarAmount(float amount)
    {
        if (hpSlider == null)
        {
            return;
        }

        amount = Mathf.Clamp01(amount);
        hpSlider.value = amount;
    }

    private void UpdateHpBar()
    {
        float hpRatio = currentHp / maxHp;
        ChangeHpBarAmount(hpRatio);
    }

    public void TakeDamage(float damage)
    {
        SetHp(currentHp - damage);
    }

    public void Heal(float healAmount)
    {
        SetHp(currentHp + healAmount);
    }

    public float GetCurrentHp()
    {
        return currentHp;
    }

    public float GetMaxHp()
    {
        return maxHp;
    }

    public void SetMaxHp(float newMaxHp)
    {
        maxHp = newMaxHp;
        currentHp = Mathf.Clamp(currentHp, 0f, maxHp);
        UpdateHpBar();
    }
}