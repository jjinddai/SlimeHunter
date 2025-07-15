using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("슬라임 관련")]
    [SerializeField] private Monster[] slimes;
    private Monster curSlime;

    [Header("씬 전환 관련")]
    [SerializeField] private string clearSceneName = "Clear";

    [Header("슬라임 스폰 시스템")]
    [SerializeField] private int killCount = 0; 
    [SerializeField] private int bossSpawnCount = 10; 
    private bool bossSpawned = false; 

    [Header("대미지 관련")]
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float currentDamage = 10f;

    [Header("대미지 업그레이드 관련")]
    [SerializeField] private int upgradeLevel = 0; 
    [SerializeField] private int upgradeBaseCost = 100; 
    [SerializeField] private float upgradeCostMultiplier = 1.5f; 
    [SerializeField] private float damageIncreaseAmount = 5f; 
    [SerializeField] private float damageIncreaseMultiplier = 1.2f; 
    [SerializeField] private bool useMultiplierUpgrade = false; 

    [Header("업그레이드 UI")]
    [SerializeField] private Button upgradeButton; 
    [SerializeField] private Text upgradeCostText; 
    [SerializeField] private Text damageText;
    [SerializeField] private Text upgradeLevelText; 

    [Header("크리티컬 관련")]
    [SerializeField] private float criticalChance = 0.2f;
    [SerializeField] private float criticalMultiplier = 10f; 

    [Header("효과음 관련")]
    [SerializeField] private AudioSource audioSource; 
    [SerializeField] private AudioClip normalHitSound;
    [SerializeField] private AudioClip criticalHitSound; 
    [SerializeField] private AudioClip upgradeSound;
    [SerializeField] private float soundVolume = 1f; 
    [SerializeField] private bool playClickSounds = true; 

    [Header("클릭 이펙트 관련")]
    [SerializeField] private GameObject normalHitEffect; 
    [SerializeField] private GameObject criticalHitEffect; 
    [SerializeField] private Camera mainCamera; 
    [SerializeField] private LayerMask groundLayer = -1; 
    [SerializeField] private float effectOffsetY = 0.1f; 
    [SerializeField] private float effectLifetime = 3f; 
    [SerializeField] private bool useWorldPosition = true; 

    private List<GameObject> activeEffects = new List<GameObject>();

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (audioSource != null)
        {
            audioSource.volume = soundVolume;
            audioSource.playOnAwake = false;
        }

        InitializeUpgradeSystem();
        currentDamage = baseDamage;
        killCount = 0;
        bossSpawned = false;
        UpdateUpgradeUI();
    }

    void Update()
    {
        if (curSlime == null)
        {
            SpawnSlime();
        }
    }

    private void InitializeUpgradeSystem()
    {
        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(UpgradeDamage);
        }

        ResetUpgradeToDefault();
    }

    public void UpgradeDamage()
    {
        int upgradeCost = GetUpgradeCost();

        if (GoldManager.Instance != null && GoldManager.Instance.HasEnoughGold(upgradeCost))
        {
            if (GoldManager.Instance.SpendGold(upgradeCost))
            {
                upgradeLevel++;

                if (useMultiplierUpgrade)
                {
                    currentDamage = baseDamage * (1 + upgradeLevel * damageIncreaseMultiplier);
                }
                else
                {
                    currentDamage = baseDamage + (upgradeLevel * damageIncreaseAmount);
                }

                UpdateUpgradeUI();
                PlaySound(upgradeSound);
            }
        }
        else
        {
            if (upgradeButton != null)
            {
                StartCoroutine(ShakeButton());
            }
        }
    }

    private int GetUpgradeCost()
    {
        if (upgradeLevel == 0)
        {
            return upgradeBaseCost;
        }
        else
        {
            return Mathf.RoundToInt(upgradeBaseCost * Mathf.Pow(upgradeCostMultiplier, upgradeLevel));
        }
    }

    private void UpdateUpgradeUI()
    {
        int upgradeCost = GetUpgradeCost();
        bool canUpgrade = GoldManager.Instance != null && GoldManager.Instance.HasEnoughGold(upgradeCost);

        if (upgradeCostText != null)
        {
            upgradeCostText.text = $"업그레이드 비용: {upgradeCost} 골드";
            upgradeCostText.color = canUpgrade ? Color.white : Color.red;
        }

        if (damageText != null)
        {
            damageText.text = $"현재 대미지: {currentDamage:F1}";
        }

        if (upgradeLevelText != null)
        {
            upgradeLevelText.text = $"업그레이드 레벨: {upgradeLevel}";
        }

        if (upgradeButton != null)
        {
            upgradeButton.interactable = canUpgrade;
        }
    }

    private IEnumerator ShakeButton()
    {
        if (upgradeButton == null) yield break;

        RectTransform buttonRect = upgradeButton.GetComponent<RectTransform>();
        Vector3 originalPos = buttonRect.localPosition;
        float shakeTime = 0.3f;
        float shakeStrength = 5f;

        float elapsedTime = 0f;

        while (elapsedTime < shakeTime)
        {
            Vector3 randomPos = originalPos + Random.insideUnitSphere * shakeStrength;
            randomPos.z = originalPos.z;
            buttonRect.localPosition = randomPos;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        buttonRect.localPosition = originalPos;
    }

    private void ResetUpgradeToDefault()
    {
        upgradeLevel = 0;
        currentDamage = baseDamage;
        UpdateUpgradeUI();
    }

    public void SpawnSlime()
    {
        if (slimes == null || slimes.Length == 0)
        {
            return;
        }

        int spawnIndex;

        if (killCount < bossSpawnCount)
        {
            spawnIndex = UnityEngine.Random.Range(0, 2);
            bossSpawned = false;
        }
        else if (killCount >= bossSpawnCount && !bossSpawned)
        {
            spawnIndex = 2;
            bossSpawned = true;
        }
        else
        {
            spawnIndex = UnityEngine.Random.Range(0, 2);
            bossSpawned = false;
        }

        if (spawnIndex >= slimes.Length)
        {
            spawnIndex = 0;
        }

        GameObject newSlime = Instantiate(slimes[spawnIndex].gameObject);
        curSlime = newSlime.GetComponent<Monster>();
    }

    public void OnMonsterKilled()
    {
        killCount++;
    }

    public void OnBossKilled()
    {
        SceneManager.LoadScene(clearSceneName);
    }

    public void ResetKillCount()
    {
        killCount = 0;
        bossSpawned = false;
    }

    public void HitSlime()
    {
        if (curSlime != null)
        {
            bool isCritical = CheckCriticalHit();

            float finalDamage = currentDamage;
            if (isCritical)
            {
                finalDamage *= criticalMultiplier;
                PlaySound(criticalHitSound);
            }
            else
            {
                PlaySound(normalHitSound);
            }

            ShowClickEffect(isCritical);
            curSlime.OnHit(finalDamage);
        }
    }

    private void OnEnable()
    {
        if (GoldManager.OnGoldChanged != null)
        {
            GoldManager.OnGoldChanged += OnGoldChanged;
        }
    }

    private void OnDisable()
    {
        if (GoldManager.OnGoldChanged != null)
        {
            GoldManager.OnGoldChanged -= OnGoldChanged;
        }
    }

    private void OnGoldChanged(int newGoldAmount)
    {
        UpdateUpgradeUI();
    }

    private void ShowClickEffect(bool isCritical)
    {
        if (mainCamera == null) return;

        Vector3 worldPosition = GetWorldPositionFromMouse();

        GameObject effectPrefab = isCritical ? criticalHitEffect : normalHitEffect;

        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, worldPosition, Quaternion.identity);
            activeEffects.Add(effect);

            ParticleSystem[] particles = effect.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem particle in particles)
            {
                if (particle != null)
                {
                    particle.Play();

                    if (isCritical)
                    {
                        var main = particle.main;
                        main.startSize = main.startSize.constant * 1.5f;
                        main.startSpeed = main.startSpeed.constant * 1.3f;
                        main.startColor = new Color(1f, 0.3f, 0.3f, 1f);
                    }
                }
            }

            Animator animator = effect.GetComponent<Animator>();
            if (animator != null)
            {
                if (isCritical)
                {
                    animator.SetTrigger("Critical");
                }
                else
                {
                    animator.SetTrigger("Normal");
                }
            }

            StartCoroutine(RemoveEffectAfterTime(effect));
        }
    }

    private Vector3 GetWorldPositionFromMouse()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 worldPosition = Vector3.zero;

        if (useWorldPosition)
        {
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
            {
                worldPosition = hit.point + Vector3.up * effectOffsetY;
            }
            else
            {
                worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 10f));
                worldPosition.y += effectOffsetY;
            }
        }
        else
        {
            worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 10f));
        }

        return worldPosition;
    }

    public void ShowEffectOnMonster(bool isCritical)
    {
        if (curSlime == null) return;

        Vector3 monsterPosition = curSlime.transform.position + Vector3.up * effectOffsetY;

        GameObject effectPrefab = isCritical ? criticalHitEffect : normalHitEffect;

        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, monsterPosition, Quaternion.identity);
            activeEffects.Add(effect);

            ParticleSystem[] particles = effect.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem particle in particles)
            {
                if (particle != null)
                {
                    particle.Play();
                }
            }

            StartCoroutine(RemoveEffectAfterTime(effect));
        }
    }

    private IEnumerator RemoveEffectAfterTime(GameObject effect)
    {
        yield return new WaitForSeconds(effectLifetime);

        if (effect != null)
        {
            ParticleSystem[] particles = effect.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem particle in particles)
            {
                if (particle != null)
                {
                    particle.Stop();
                }
            }

            yield return new WaitForSeconds(1f);

            activeEffects.Remove(effect);
            Destroy(effect);
        }
    }

    public void ClearAllEffects()
    {
        foreach (GameObject effect in activeEffects)
        {
            if (effect != null)
            {
                Destroy(effect);
            }
        }
        activeEffects.Clear();
    }

    private bool CheckCriticalHit()
    {
        float randomValue = UnityEngine.Random.Range(0f, 1f);
        return randomValue <= criticalChance;
    }

    public void SetCriticalChance(float newChance)
    {
        criticalChance = Mathf.Clamp01(newChance);
    }

    public void SetCriticalMultiplier(float newMultiplier)
    {
        criticalMultiplier = Mathf.Max(1f, newMultiplier);
    }

    public void SetBaseDamage(float newDamage)
    {
        baseDamage = Mathf.Max(0f, newDamage);

        if (useMultiplierUpgrade)
        {
            currentDamage = baseDamage * (1 + upgradeLevel * damageIncreaseMultiplier);
        }
        else
        {
            currentDamage = baseDamage + (upgradeLevel * damageIncreaseAmount);
        }

        UpdateUpgradeUI();
    }

    public string GetUpgradeInfo()
    {
        return $"업그레이드 레벨: {upgradeLevel}, 현재 대미지: {currentDamage:F1}, 다음 업그레이드 비용: {GetUpgradeCost()}";
    }

    public Monster GetCurrentSlime()
    {
        return curSlime;
    }

    public void SetEffectLifetime(float newLifetime)
    {
        effectLifetime = Mathf.Max(0.1f, newLifetime);
    }

    public void SetEffectOffsetY(float newOffset)
    {
        effectOffsetY = newOffset;
    }

    public void TriggerMonsterEffect(bool isCritical)
    {
        ShowEffectOnMonster(isCritical);
    }

    public float GetCurrentDamage()
    {
        return currentDamage;
    }

    public int GetUpgradeLevel()
    {
        return upgradeLevel;
    }

    public int GetKillCount()
    {
        return killCount;
    }

    public bool IsBossSpawned()
    {
        return bossSpawned;
    }

    private void PlaySound(AudioClip sound)
    {
        if (!playClickSounds || audioSource == null || sound == null) return;

        audioSource.volume = soundVolume;
        audioSource.PlayOneShot(sound);
    }

    public void SetSoundVolume(float volume)
    {
        soundVolume = Mathf.Clamp01(volume);
        if (audioSource != null)
        {
            audioSource.volume = soundVolume;
        }
    }

    public void SetPlayClickSounds(bool enable)
    {
        playClickSounds = enable;
    }
}