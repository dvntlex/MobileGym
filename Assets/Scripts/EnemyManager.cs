using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [Header("Enemy Prefabs")]
    [SerializeField] private List<GameObject> enemyPrefabs = new List<GameObject>();

    [Header("Enemy UI")]
    [SerializeField] private GameObject enemyPanel;
    [SerializeField] private Image enemyImage;
    [SerializeField] private TextMeshProUGUI enemyNameText;
    [SerializeField] private TextMeshProUGUI enemyDescriptionText;
    [SerializeField] private TextMeshProUGUI enemyHealthText;
    [SerializeField] private RectTransform enemyHealthBarFill;

    // Current enemy stats
    private string currentEnemyName;
    private int currentEnemyHealth;
    private int currentEnemyMaxHealth;
    private int currentEnemyAttackMin;
    private int currentEnemyAttackMax;
    private int currentEnemyReward;

    /// <summary>
    /// Sets up the singleton pattern.
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Shows the enemy panel.
    /// </summary>
    public void ShowEnemyPanel()
    {
        if (enemyPanel != null)
            enemyPanel.SetActive(true);
    }

    /// <summary>
    /// Hides the enemy panel.
    /// </summary>
    public void HideEnemyPanel()
    {
        if (enemyPanel != null)
            enemyPanel.SetActive(false);
    }

    /// <summary>
    /// Spawns a random enemy from the enemy prefabs list.
    /// Gets data from the Enemy component and randomizes stats.
    /// </summary>
    /// <returns>The name of the spawned enemy</returns>
    public string SpawnRandomEnemy()
    {
        if (enemyPrefabs.Count == 0)
        {
            Debug.LogWarning("No enemy prefabs defined!");
            return "";
        }

        // Pick random enemy prefab
        GameObject enemyPrefab = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Count)];
        Enemy enemy = enemyPrefab.GetComponent<Enemy>();

        if (enemy == null)
        {
            Debug.LogWarning("Enemy prefab is missing Enemy component!");
            return "";
        }

        // Randomize stats within ranges
        currentEnemyName = enemy.GetName();
        currentEnemyMaxHealth = UnityEngine.Random.Range(enemy.GetHealthMin(), enemy.GetHealthMax() + 1);
        currentEnemyHealth = currentEnemyMaxHealth;
        currentEnemyAttackMin = enemy.GetAttackMin();
        currentEnemyAttackMax = enemy.GetAttackMax();
        currentEnemyReward = UnityEngine.Random.Range(enemy.GetRewardMin(), enemy.GetRewardMax() + 1);

        // Update UI
        if (enemyImage != null)
            enemyImage.sprite = enemy.GetSprite();

        if (enemyNameText != null)
            enemyNameText.text = currentEnemyName;

        if (enemyDescriptionText != null)
            enemyDescriptionText.text = enemy.GetDescription();

        UpdateEnemyHealthUI();

        return currentEnemyName;
    }

    /// <summary>
    /// Deals damage to the current enemy.
    /// </summary>
    /// <param name="damage">Amount of damage to deal</param>
    public void TakeDamage(int damage)
    {
        currentEnemyHealth -= damage;
        if (currentEnemyHealth < 0)
            currentEnemyHealth = 0;

        UpdateEnemyHealthUI();
    }

    /// <summary>
    /// Gets a random attack value from the current enemy.
    /// </summary>
    /// <returns>Random damage value between enemy's attack min and max</returns>
    public int GetEnemyAttack()
    {
        return UnityEngine.Random.Range(currentEnemyAttackMin, currentEnemyAttackMax + 1);
    }

    /// <summary>
    /// Updates the enemy health text and bar.
    /// </summary>
    private void UpdateEnemyHealthUI()
    {
        if (enemyHealthText != null)
            enemyHealthText.text = currentEnemyHealth + " / " + currentEnemyMaxHealth;

        if (enemyHealthBarFill != null)
            enemyHealthBarFill.localScale = new Vector3((float)currentEnemyHealth / currentEnemyMaxHealth, 1f, 1f);
    }

    /// <summary>
    /// Checks if the current enemy is defeated.
    /// </summary>
    /// <returns>True if enemy health is 0 or below</returns>
    public bool IsEnemyDefeated()
    {
        return currentEnemyHealth <= 0;
    }

    /// <summary>
    /// Gets the current enemy's name.
    /// </summary>
    public string GetEnemyName() => currentEnemyName;

    /// <summary>
    /// Gets the current enemy's health.
    /// </summary>
    public int GetEnemyHealth() => currentEnemyHealth;

    /// <summary>
    /// Gets the current enemy's max health.
    /// </summary>
    public int GetEnemyMaxHealth() => currentEnemyMaxHealth;

    /// <summary>
    /// Gets the current enemy's coin reward.
    /// </summary>
    public int GetEnemyReward() => currentEnemyReward;
}