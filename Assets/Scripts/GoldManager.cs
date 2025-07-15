using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoldManager : MonoBehaviour
{
    [SerializeField] private Text goldText;
    [SerializeField] private Text goldChangeText;
    [SerializeField] private string goldTextFormat = "Gold: {0}";
    [SerializeField] private bool showGoldChangeAnimation = true;
    [SerializeField] private float animationDuration = 1.5f;
    [SerializeField] private Color positiveGoldColor = Color.yellow;
    [SerializeField] private Color negativeGoldColor = Color.red;
    [SerializeField] private bool saveGoldData = true;
    [SerializeField] private string goldSaveKey = "PlayerGold";
    [SerializeField] private int currentGold = 0;
    [SerializeField] private int totalGoldEarned = 0;
    [SerializeField] private int totalGoldSpent = 0;

    public static GoldManager Instance { get; private set; }

    public static System.Action<int> OnGoldChanged;
    public static System.Action<int> OnGoldAdded;
    public static System.Action<int> OnGoldSpent;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        LoadGoldData();
        InitializeUI();
        UpdateGoldUI();
        Monster.OnGoldDropped += OnMonsterGoldDropped;
    }

    private void OnDestroy()
    {
        Monster.OnGoldDropped -= OnMonsterGoldDropped;
    }

    private void InitializeUI()
    {
        if (goldText == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                Transform foundText = canvas.transform.Find("GoldText");
                if (foundText != null)
                {
                    goldText = foundText.GetComponent<Text>();
                }
            }
        }

        if (goldChangeText != null)
        {
            goldChangeText.gameObject.SetActive(false);
        }
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        currentGold += amount;
        totalGoldEarned += amount;

        UpdateGoldUI();

        if (showGoldChangeAnimation)
        {
            ShowGoldChangeEffect($"+{amount}", positiveGoldColor);
        }

        if (saveGoldData)
        {
            SaveGoldData();
        }

        OnGoldChanged?.Invoke(currentGold);
        OnGoldAdded?.Invoke(amount);
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0) return false;

        if (currentGold < amount)
        {
            if (showGoldChangeAnimation)
            {
                ShowGoldChangeEffect("°ñµå ºÎÁ·!", negativeGoldColor);
            }

            return false;
        }

        currentGold -= amount;
        totalGoldSpent += amount;

        UpdateGoldUI();

        if (showGoldChangeAnimation)
        {
            ShowGoldChangeEffect($"-{amount}", negativeGoldColor);
        }

        if (saveGoldData)
        {
            SaveGoldData();
        }

        OnGoldChanged?.Invoke(currentGold);
        OnGoldSpent?.Invoke(amount);

        return true;
    }

    private void UpdateGoldUI()
    {
        if (goldText != null)
        {
            goldText.text = string.Format(goldTextFormat, currentGold.ToString("N0"));
        }
    }

    private void ShowGoldChangeEffect(string text, Color color)
    {
        if (goldChangeText == null) return;

        goldChangeText.text = text;
        goldChangeText.color = color;
        goldChangeText.gameObject.SetActive(true);

        StartCoroutine(GoldChangeAnimation());
    }

    private IEnumerator GoldChangeAnimation()
    {
        if (goldChangeText == null) yield break;

        RectTransform rect = goldChangeText.GetComponent<RectTransform>();
        Vector3 originalPos = rect.localPosition;
        Vector3 originalScale = rect.localScale;
        Color originalColor = goldChangeText.color;

        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            float progress = elapsedTime / animationDuration;

            Vector3 currentPos = originalPos + Vector3.up * (progress * 50f);
            rect.localPosition = currentPos;

            float scale = Mathf.Lerp(1.2f, 0.8f, progress);
            rect.localScale = originalScale * scale;

            Color currentColor = originalColor;
            currentColor.a = Mathf.Lerp(1f, 0f, progress);
            goldChangeText.color = currentColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rect.localPosition = originalPos;
        rect.localScale = originalScale;
        goldChangeText.color = originalColor;
        goldChangeText.gameObject.SetActive(false);
    }

    private void OnMonsterGoldDropped(int goldAmount, Vector3 position)
    {
        AddGold(goldAmount);
    }

    private void SaveGoldData()
    {
        PlayerPrefs.SetInt(goldSaveKey, currentGold);
        PlayerPrefs.SetInt(goldSaveKey + "_TotalEarned", totalGoldEarned);
        PlayerPrefs.SetInt(goldSaveKey + "_TotalSpent", totalGoldSpent);
        PlayerPrefs.Save();
    }

    private void LoadGoldData()
    {
        if (saveGoldData)
        {
            currentGold = PlayerPrefs.GetInt(goldSaveKey, 0);
            totalGoldEarned = PlayerPrefs.GetInt(goldSaveKey + "_TotalEarned", 0);
            totalGoldSpent = PlayerPrefs.GetInt(goldSaveKey + "_TotalSpent", 0);
        }
    }

    public int GetCurrentGold()
    {
        return currentGold;
    }

    public bool HasEnoughGold(int amount)
    {
        return currentGold >= amount;
    }

    public void SetGold(int amount)
    {
        currentGold = Mathf.Max(0, amount);
        UpdateGoldUI();

        if (saveGoldData)
        {
            SaveGoldData();
        }

        OnGoldChanged?.Invoke(currentGold);
    }

    public void ResetGold()
    {
        currentGold = 0;
        totalGoldEarned = 0;
        totalGoldSpent = 0;

        UpdateGoldUI();

        if (saveGoldData)
        {
            SaveGoldData();
        }

        OnGoldChanged?.Invoke(currentGold);
    }

    public string GetGoldStats()
    {
        return $"ÇöÀç °ñµå: {currentGold}, ÃÑ È¹µæ: {totalGoldEarned}, ÃÑ ¼Ò¸ð: {totalGoldSpent}";
    }

    [ContextMenu("Add 100 Gold")]
    public void DebugAdd100Gold()
    {
        AddGold(100);
    }

    [ContextMenu("Spend 50 Gold")]
    public void DebugSpend50Gold()
    {
        SpendGold(50);
    }
}