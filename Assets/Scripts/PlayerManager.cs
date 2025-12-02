using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    // Singleton instance - allows other scripts to access this manager via PlayerManager.Instance
    public static PlayerManager Instance { get; private set; }

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;

    [Header("Stamina")]
    [SerializeField] private int maxStamina = 100;
    [SerializeField] private int currentStamina = 100;

    [Header("Level & XP")]
    [SerializeField] private int level = 1;
    [SerializeField] private int currentXP = 0;
    [SerializeField] private int xpToNextLevel = 100;
    [SerializeField] private float xpMultiplier = 1.5f;

    [Header("Currency")]
    [SerializeField] private int coins = 0;

    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI staminaText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI xpText;
    [SerializeField] private TextMeshProUGUI coinsText;

    [Header("UI Bars")]
    [SerializeField] private RectTransform healthBarFill;
    [SerializeField] private RectTransform staminaBarFill;
    [SerializeField] private RectTransform xpBarFill;

    [Header("Testing")]
    [SerializeField] private Button resetButton;

    // Events that other scripts can subscribe to for reacting to stat changes
    public event Action<int, int> OnHealthChanged;      // Parameters: currentHealth, maxHealth
    public event Action<int, int> OnStaminaChanged;     // Parameters: currentStamina, maxStamina
    public event Action<int, int> OnXPChanged;          // Parameters: currentXP, xpToNextLevel
    public event Action<int> OnLevelUp;                 // Parameter: newLevel
    public event Action<int> OnCoinsChanged;            // Parameter: currentCoins

    /// <summary>
    /// Called when the script instance is loaded.
    /// Sets up the singleton pattern and loads saved stats.
    /// </summary>
    private void Awake()
    {
        // Singleton pattern - ensures only one PlayerManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Load saved data and update UI
        LoadStats();
        UpdateAllUI();

        // Setup reset button for testing
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetStats);
    }

    /// <summary>
    /// Called every frame. Used for testing input.
    /// Press Space to reduce health by 10.
    /// Press E to add 25 XP.
    /// Press C to add 25 coins.
    /// </summary>
    private void Update()
    {
        // Test: Press Space to take 10 damage
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TakeDamage(10);
        }

        // Test: Press E to add 25 XP
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            AddXP(25);
        }

        // Test: Press C to add 25 coins
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            AddCoins(25);
        }
    }

    /// <summary>
    /// Called when the application is closing.
    /// Saves all player stats before the app shuts down.
    /// </summary>
    private void OnApplicationQuit()
    {
        SaveStats();
    }

    /// <summary>
    /// Called when the application is paused (e.g., user switches apps on mobile).
    /// Saves stats to prevent data loss if the app is killed in the background.
    /// </summary>
    private void OnApplicationPause(bool paused)
    {
        if (paused)
            SaveStats();
    }

    /// <summary>
    /// Saves all player stats to PlayerPrefs for persistent storage.
    /// Data survives app restarts and device reboots.
    /// </summary>
    public void SaveStats()
    {
        PlayerPrefs.SetInt("MaxHealth", maxHealth);
        PlayerPrefs.SetInt("CurrentHealth", currentHealth);
        PlayerPrefs.SetInt("MaxStamina", maxStamina);
        PlayerPrefs.SetInt("CurrentStamina", currentStamina);
        PlayerPrefs.SetInt("Level", level);
        PlayerPrefs.SetInt("CurrentXP", currentXP);
        PlayerPrefs.SetInt("XPToNextLevel", xpToNextLevel);
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads all player stats from PlayerPrefs.
    /// If no save exists, keeps the default Inspector values.
    /// </summary>
    public void LoadStats()
    {
        // Only load if save data exists (checks for Level key as indicator)
        if (!PlayerPrefs.HasKey("Level")) return;

        maxHealth = PlayerPrefs.GetInt("MaxHealth", 100);
        currentHealth = PlayerPrefs.GetInt("CurrentHealth", 100);
        maxStamina = PlayerPrefs.GetInt("MaxStamina", 100);
        currentStamina = PlayerPrefs.GetInt("CurrentStamina", 100);
        level = PlayerPrefs.GetInt("Level", 1);
        currentXP = PlayerPrefs.GetInt("CurrentXP", 0);
        xpToNextLevel = PlayerPrefs.GetInt("XPToNextLevel", 100);
        coins = PlayerPrefs.GetInt("Coins", 0);
    }

    /// <summary>
    /// Resets all stats to their default values and clears saved data.
    /// Use for "New Game" or debug purposes.
    /// </summary>
    public void ResetStats()
    {
        // Clear all saved data
        PlayerPrefs.DeleteAll();

        // Reset to defaults
        maxHealth = 100;
        currentHealth = 100;
        maxStamina = 100;
        currentStamina = 100;
        level = 1;
        currentXP = 0;
        xpToNextLevel = 100;
        coins = 0;

        // Refresh UI and notify subscribers
        UpdateAllUI();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        OnXPChanged?.Invoke(currentXP, xpToNextLevel);
        OnCoinsChanged?.Invoke(coins);
    }

    /// <summary>
    /// Updates all UI text elements to reflect current stat values.
    /// Called on load and reset.
    /// </summary>
    private void UpdateAllUI()
    {
        UpdateHealthUI();
        UpdateStaminaUI();
        UpdateLevelUI();
        UpdateXPUI();
        UpdateCoinsUI();
    }

    /// <summary>
    /// Updates the health text and bar display. Shows "current / max" format.
    /// Bar scales horizontally based on current/max health percentage.
    /// </summary>
    private void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = currentHealth + " / " + maxHealth;

        if (healthBarFill != null)
            healthBarFill.localScale = new Vector3((float)currentHealth / maxHealth, 1f, 1f);
    }

    /// <summary>
    /// Updates the stamina text and bar display. Shows "current / max" format.
    /// Bar scales horizontally based on current/max stamina percentage.
    /// </summary>
    private void UpdateStaminaUI()
    {
        if (staminaText != null)
            staminaText.text = currentStamina + " / " + maxStamina;

        if (staminaBarFill != null)
            staminaBarFill.localScale = new Vector3((float)currentStamina / maxStamina, 1f, 1f);
    }

    /// <summary>
    /// Updates the level text display. Shows "Level X" format.
    /// </summary>
    private void UpdateLevelUI()
    {
        if (levelText != null)
            levelText.text = "Level " + level;
    }

    /// <summary>
    /// Updates the XP text and bar display. Shows "current / needed XP" format.
    /// Bar scales horizontally based on current/needed XP percentage.
    /// </summary>
    private void UpdateXPUI()
    {
        if (xpText != null)
            xpText.text = currentXP + " / " + xpToNextLevel + " XP";

        if (xpBarFill != null)
            xpBarFill.localScale = new Vector3((float)currentXP / xpToNextLevel, 1f, 1f);
    }

    /// <summary>
    /// Updates the coins text display. Shows just the number.
    /// </summary>
    private void UpdateCoinsUI()
    {
        if (coinsText != null)
            coinsText.text = coins.ToString();
    }

    /// <summary>
    /// Increases current health by the specified amount.
    /// Health is clamped between 0 and maxHealth.
    /// </summary>
    /// <param name="amount">Amount of health to restore</param>
    public void AddHealth(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateHealthUI();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Decreases current health by the specified amount.
    /// Health is clamped between 0 and maxHealth.
    /// </summary>
    /// <param name="amount">Amount of damage to take</param>
    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        UpdateHealthUI();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Sets a new maximum health value.
    /// Current health is adjusted if it exceeds the new maximum.
    /// </summary>
    /// <param name="amount">New maximum health value</param>
    public void SetMaxHealth(int amount)
    {
        maxHealth = amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Increases current stamina by the specified amount.
    /// Stamina is clamped between 0 and maxStamina.
    /// </summary>
    /// <param name="amount">Amount of stamina to restore</param>
    public void AddStamina(int amount)
    {
        currentStamina = Mathf.Clamp(currentStamina + amount, 0, maxStamina);
        UpdateStaminaUI();
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    /// <summary>
    /// Decreases current stamina by the specified amount.
    /// Stamina is clamped between 0 and maxStamina.
    /// </summary>
    /// <param name="amount">Amount of stamina to consume</param>
    public void UseStamina(int amount)
    {
        currentStamina = Mathf.Clamp(currentStamina - amount, 0, maxStamina);
        UpdateStaminaUI();
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    /// <summary>
    /// Sets a new maximum stamina value.
    /// Current stamina is adjusted if it exceeds the new maximum.
    /// </summary>
    /// <param name="amount">New maximum stamina value</param>
    public void SetMaxStamina(int amount)
    {
        maxStamina = amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        UpdateStaminaUI();
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    /// <summary>
    /// Adds XP to the player. Automatically triggers level ups
    /// if enough XP is accumulated. Excess XP carries over.
    /// </summary>
    /// <param name="amount">Amount of XP to add</param>
    public void AddXP(int amount)
    {
        currentXP += amount;
        UpdateXPUI();
        OnXPChanged?.Invoke(currentXP, xpToNextLevel);

        // Check for level up (while loop handles multiple level ups at once)
        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    /// <summary>
    /// Handles the level up process. Called automatically by AddXP.
    /// Subtracts XP cost, increases level, and scales XP requirement.
    /// </summary>
    private void LevelUp()
    {
        // Subtract the XP cost and carry over excess
        currentXP -= xpToNextLevel;
        level++;

        // Increase XP requirement for next level by multiplier
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * xpMultiplier);

        UpdateLevelUI();
        UpdateXPUI();
        OnLevelUp?.Invoke(level);
        OnXPChanged?.Invoke(currentXP, xpToNextLevel);
    }

    /// <summary>
    /// Adds coins to the player's balance.
    /// </summary>
    /// <param name="amount">Amount of coins to add</param>
    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateCoinsUI();
        OnCoinsChanged?.Invoke(coins);
    }

    /// <summary>
    /// Attempts to spend coins. Only succeeds if player has enough.
    /// </summary>
    /// <param name="amount">Amount of coins to spend</param>
    /// <returns>True if purchase successful, false if insufficient funds</returns>
    public bool SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            UpdateCoinsUI();
            OnCoinsChanged?.Invoke(coins);
            return true;
        }
        return false;
    }

    // Getter methods for accessing stat values from other scripts
    public int GetHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public int GetStamina() => currentStamina;
    public int GetMaxStamina() => maxStamina;
    public int GetLevel() => level;
    public int GetXP() => currentXP;
    public int GetXPToNextLevel() => xpToNextLevel;
    public int GetCoins() => coins;
}