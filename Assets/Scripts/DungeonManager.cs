using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    [Header("Dungeon Generation")]
    [SerializeField] private int dungeonWidth = 7;
    [SerializeField] private int dungeonHeight = 7;
    [SerializeField] private int enemyCount = 5;
    [SerializeField] private int chestCount = 3;

    [Header("Dungeon Map UI")]
    [SerializeField] private GameObject dungeonMapPanel;
    [SerializeField] private Transform tileGridParent;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private float tileSize = 50f;

    [Header("Movement Buttons")]
    [SerializeField] private Button moveUpButton;
    [SerializeField] private Button moveDownButton;
    [SerializeField] private Button moveLeftButton;
    [SerializeField] private Button moveRightButton;

    [Header("Tile Colors")]
    [SerializeField] private Color floorColor = Color.gray;
    [SerializeField] private Color wallColor = Color.black;
    [SerializeField] private Color playerColor = Color.blue;
    [SerializeField] private Color enemyColor = Color.red;
    [SerializeField] private Color chestColor = Color.yellow;
    [SerializeField] private Color exitColor = Color.green;
    [SerializeField] private Color visitedColor = new Color(0.5f, 0.5f, 0.5f);

    [Header("Player Combat Settings")]
    [SerializeField] private int playerAttackMin = 10;
    [SerializeField] private int playerAttackMax = 20;
    [SerializeField] private int playerSpellMin = 20;
    [SerializeField] private int playerSpellMax = 35;
    [SerializeField] private int playerHealMin = 15;
    [SerializeField] private int playerHealMax = 25;
    [SerializeField] private float defendReduction = 0.5f;

    [Header("Chest Rewards")]
    [SerializeField] private int chestCoinsMin = 20;
    [SerializeField] private int chestCoinsMax = 50;

    [Header("Battle UI")]
    [SerializeField] private GameObject battlePanel;
    [SerializeField] private GameObject fightUIPanel;
    [SerializeField] private Button startDungeonButton;
    [SerializeField] private TextMeshProUGUI battleLogText;
    [SerializeField] private Button fightButton;
    [SerializeField] private Button defendButton;
    [SerializeField] private Button spellButton;
    [SerializeField] private Button healButton;

    [Header("Dungeon Complete")]
    [SerializeField] private GameObject dungeonCompletePanel;
    [SerializeField] private TextMeshProUGUI totalRewardsText;
    [SerializeField] private TextMeshProUGUI rewardLogText;
    [SerializeField] private Button exitDungeonButton;
    [SerializeField] private Button retryDungeonButton;

    // Dungeon grid
    private TileType[,] dungeonGrid;
    private Image[,] tileImages;
    private Vector2Int playerPosition;
    private Vector2Int exitPosition;
    private bool[,] visitedTiles;

    // Dungeon progress
    private int enemiesDefeated = 0;
    private int totalCoinsEarned = 0;
    private int chestsOpened = 0;
    private bool isDefending = false;
    private bool inBattle = false;
    private bool dungeonActive = false;
    private List<string> rewardLog = new List<string>();

    public enum TileType
    {
        Wall,
        Floor,
        Enemy,
        Chest,
        Exit,
        Start
    }

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
    /// Sets up button listeners and hides panels.
    /// </summary>
    private void Start()
    {
        if (startDungeonButton != null)
            startDungeonButton.onClick.AddListener(StartDungeon);

        if (fightButton != null)
            fightButton.onClick.AddListener(OnFightPressed);

        if (defendButton != null)
            defendButton.onClick.AddListener(OnDefendPressed);

        if (spellButton != null)
            spellButton.onClick.AddListener(OnSpellPressed);

        if (healButton != null)
            healButton.onClick.AddListener(OnHealPressed);

        if (exitDungeonButton != null)
            exitDungeonButton.onClick.AddListener(ExitDungeon);

        if (retryDungeonButton != null)
            retryDungeonButton.onClick.AddListener(RetryDungeon);

        // Movement buttons
        if (moveUpButton != null)
            moveUpButton.onClick.AddListener(OnMoveUpPressed);

        if (moveDownButton != null)
            moveDownButton.onClick.AddListener(OnMoveDownPressed);

        if (moveLeftButton != null)
            moveLeftButton.onClick.AddListener(OnMoveLeftPressed);

        if (moveRightButton != null)
            moveRightButton.onClick.AddListener(OnMoveRightPressed);

        // Hide all panels initially
        if (dungeonMapPanel != null)
            dungeonMapPanel.SetActive(false);

        if (battlePanel != null)
            battlePanel.SetActive(false);

        if (fightUIPanel != null)
            fightUIPanel.SetActive(false);

        if (dungeonCompletePanel != null)
            dungeonCompletePanel.SetActive(false);

        // Hide enemy panel
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.HideEnemyPanel();
    }

    /// <summary>
    /// Called when the Up movement button is pressed.
    /// </summary>
    private void OnMoveUpPressed()
    {
        if (!dungeonActive || inBattle) return;
        MovePlayer(Vector2Int.up);
    }

    /// <summary>
    /// Called when the Down movement button is pressed.
    /// </summary>
    private void OnMoveDownPressed()
    {
        if (!dungeonActive || inBattle) return;
        MovePlayer(Vector2Int.down);
    }

    /// <summary>
    /// Called when the Left movement button is pressed.
    /// </summary>
    private void OnMoveLeftPressed()
    {
        if (!dungeonActive || inBattle) return;
        MovePlayer(Vector2Int.left);
    }

    /// <summary>
    /// Called when the Right movement button is pressed.
    /// </summary>
    private void OnMoveRightPressed()
    {
        if (!dungeonActive || inBattle) return;
        MovePlayer(Vector2Int.right);
    }

    /// <summary>
    /// Starts a new dungeon run. Generates dungeon and places player.
    /// </summary>
    public void StartDungeon()
    {
        enemiesDefeated = 0;
        totalCoinsEarned = 0;
        chestsOpened = 0;
        isDefending = false;
        inBattle = false;
        dungeonActive = true;
        rewardLog.Clear();

        SetButtonsInteractable(true);

        if (dungeonCompletePanel != null)
            dungeonCompletePanel.SetActive(false);

        if (battlePanel != null)
            battlePanel.SetActive(false);

        if (fightUIPanel != null)
            fightUIPanel.SetActive(false);

        GenerateDungeon();
        CreateDungeonUI();

        if (dungeonMapPanel != null)
            dungeonMapPanel.SetActive(true);

        LogBattle("Use the arrows to explore the dungeon.");
    }

    /// <summary>
    /// Retries the dungeon after defeat.
    /// </summary>
    public void RetryDungeon()
    {
        // Reset player health to full
        PlayerManager.Instance.AddHealth(PlayerManager.Instance.GetMaxHealth());

        if (dungeonCompletePanel != null)
            dungeonCompletePanel.SetActive(false);

        StartDungeon();
    }

    /// <summary>
    /// Generates a random connected dungeon layout.
    /// </summary>
    private void GenerateDungeon()
    {
        dungeonGrid = new TileType[dungeonWidth, dungeonHeight];
        visitedTiles = new bool[dungeonWidth, dungeonHeight];

        // Fill with walls
        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int y = 0; y < dungeonHeight; y++)
            {
                dungeonGrid[x, y] = TileType.Wall;
                visitedTiles[x, y] = false;
            }
        }

        // Generate path using random walk
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = new Vector2Int(0, dungeonHeight / 2);
        dungeonGrid[current.x, current.y] = TileType.Start;
        playerPosition = current;
        path.Add(current);

        // Random walk to create connected path
        int steps = (dungeonWidth * dungeonHeight) / 2;
        for (int i = 0; i < steps; i++)
        {
            List<Vector2Int> directions = new List<Vector2Int>
            {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
            };

            // Shuffle directions
            for (int j = directions.Count - 1; j > 0; j--)
            {
                int rand = UnityEngine.Random.Range(0, j + 1);
                Vector2Int temp = directions[j];
                directions[j] = directions[rand];
                directions[rand] = temp;
            }

            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;
                if (IsInBounds(next))
                {
                    if (dungeonGrid[next.x, next.y] == TileType.Wall)
                    {
                        dungeonGrid[next.x, next.y] = TileType.Floor;
                        path.Add(next);
                    }
                    current = next;
                    break;
                }
            }
        }

        // Add some extra floor tiles for branching
        foreach (var tile in path)
        {
            List<Vector2Int> neighbors = GetNeighbors(tile);
            foreach (var neighbor in neighbors)
            {
                if (UnityEngine.Random.value < 0.3f && dungeonGrid[neighbor.x, neighbor.y] == TileType.Wall)
                {
                    dungeonGrid[neighbor.x, neighbor.y] = TileType.Floor;
                }
            }
        }

        // Place exit at furthest point from start
        exitPosition = FindFurthestFloor(playerPosition);
        dungeonGrid[exitPosition.x, exitPosition.y] = TileType.Exit;

        // Place enemies on random floor tiles
        PlaceRandomTiles(TileType.Enemy, enemyCount);

        // Place chests on random floor tiles
        PlaceRandomTiles(TileType.Chest, chestCount);
    }

    /// <summary>
    /// Checks if a position is within dungeon bounds.
    /// </summary>
    private bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < dungeonWidth && pos.y >= 0 && pos.y < dungeonHeight;
    }

    /// <summary>
    /// Gets valid neighboring positions.
    /// </summary>
    private List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in directions)
        {
            Vector2Int next = pos + dir;
            if (IsInBounds(next))
                neighbors.Add(next);
        }

        return neighbors;
    }

    /// <summary>
    /// Finds the floor tile furthest from a starting position.
    /// </summary>
    private Vector2Int FindFurthestFloor(Vector2Int start)
    {
        Vector2Int furthest = start;
        int maxDistance = 0;

        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int y = 0; y < dungeonHeight; y++)
            {
                if (dungeonGrid[x, y] == TileType.Floor)
                {
                    int distance = Mathf.Abs(x - start.x) + Mathf.Abs(y - start.y);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        furthest = new Vector2Int(x, y);
                    }
                }
            }
        }

        return furthest;
    }

    /// <summary>
    /// Places a number of tiles of a specific type on random floor tiles.
    /// </summary>
    private void PlaceRandomTiles(TileType type, int count)
    {
        List<Vector2Int> floorTiles = new List<Vector2Int>();

        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int y = 0; y < dungeonHeight; y++)
            {
                if (dungeonGrid[x, y] == TileType.Floor)
                    floorTiles.Add(new Vector2Int(x, y));
            }
        }

        for (int i = 0; i < count && floorTiles.Count > 0; i++)
        {
            int index = UnityEngine.Random.Range(0, floorTiles.Count);
            Vector2Int pos = floorTiles[index];
            dungeonGrid[pos.x, pos.y] = type;
            floorTiles.RemoveAt(index);
        }
    }

    /// <summary>
    /// Creates the visual UI for the dungeon grid.
    /// </summary>
    private void CreateDungeonUI()
    {
        if (tileGridParent == null)
        {
            Debug.LogError("DungeonManager: tileGridParent is not assigned!");
            return;
        }

        if (tilePrefab == null)
        {
            Debug.LogError("DungeonManager: tilePrefab is not assigned!");
            return;
        }

        // Clear existing tiles
        foreach (Transform child in tileGridParent)
        {
            Destroy(child.gameObject);
        }

        tileImages = new Image[dungeonWidth, dungeonHeight];

        for (int y = dungeonHeight - 1; y >= 0; y--)
        {
            for (int x = 0; x < dungeonWidth; x++)
            {
                GameObject tile = Instantiate(tilePrefab, tileGridParent);
                tile.SetActive(true);

                RectTransform rect = tile.GetComponent<RectTransform>();
                if (rect != null)
                    rect.sizeDelta = new Vector2(tileSize, tileSize);

                Image img = tile.GetComponent<Image>();
                if (img == null)
                {
                    img = tile.AddComponent<Image>();
                }

                tileImages[x, y] = img;
            }
        }

        UpdateDungeonUI();

        Debug.Log("Dungeon UI created with " + (dungeonWidth * dungeonHeight) + " tiles.");
    }

    /// <summary>
    /// Updates the dungeon UI to reflect current state.
    /// </summary>
    private void UpdateDungeonUI()
    {
        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int y = 0; y < dungeonHeight; y++)
            {
                Color color = wallColor;

                if (x == playerPosition.x && y == playerPosition.y)
                {
                    color = playerColor;
                }
                else if (visitedTiles[x, y])
                {
                    color = visitedColor;
                }
                else
                {
                    switch (dungeonGrid[x, y])
                    {
                        case TileType.Floor:
                        case TileType.Start:
                            color = floorColor;
                            break;
                        case TileType.Wall:
                            color = wallColor;
                            break;
                        case TileType.Enemy:
                            color = enemyColor;
                            break;
                        case TileType.Chest:
                            color = chestColor;
                            break;
                        case TileType.Exit:
                            color = exitColor;
                            break;
                    }
                }

                tileImages[x, y].color = color;
            }
        }
    }

    /// <summary>
    /// Moves the player in a direction if valid.
    /// </summary>
    private void MovePlayer(Vector2Int direction)
    {
        Vector2Int newPos = playerPosition + direction;

        if (!IsInBounds(newPos)) return;
        if (dungeonGrid[newPos.x, newPos.y] == TileType.Wall) return;

        // Mark current tile as visited
        visitedTiles[playerPosition.x, playerPosition.y] = true;

        // Move player
        playerPosition = newPos;
        UpdateDungeonUI();

        // Check what's on the new tile
        TileType currentTile = dungeonGrid[playerPosition.x, playerPosition.y];

        switch (currentTile)
        {
            case TileType.Enemy:
                StartBattle();
                break;
            case TileType.Chest:
                OpenChest();
                break;
            case TileType.Exit:
                CompleteDungeon();
                break;
        }
    }

    /// <summary>
    /// Starts a battle with an enemy on the current tile.
    /// </summary>
    private void StartBattle()
    {
        inBattle = true;

        if (dungeonMapPanel != null)
            dungeonMapPanel.SetActive(false);

        if (battlePanel != null)
            battlePanel.SetActive(true);

        if (fightUIPanel != null)
            fightUIPanel.SetActive(true);

        EnemyManager.Instance.ShowEnemyPanel();
        string enemyName = EnemyManager.Instance.SpawnRandomEnemy();
        LogBattle("A wild " + enemyName + " attacks!");
    }

    /// <summary>
    /// Opens a chest and gives rewards.
    /// </summary>
    private void OpenChest()
    {
        int coins = UnityEngine.Random.Range(chestCoinsMin, chestCoinsMax + 1);
        PlayerManager.Instance.AddCoins(coins);
        totalCoinsEarned += coins;
        chestsOpened++;

        // Log reward
        string logEntry = "Chest: +" + coins + " coins";
        rewardLog.Add(logEntry);
        Debug.Log("Added to reward log: " + logEntry);

        // Remove chest from map
        dungeonGrid[playerPosition.x, playerPosition.y] = TileType.Floor;
        UpdateDungeonUI();

        LogBattle("You found a chest with " + coins + " coins!");
    }

    /// <summary>
    /// Adds a message to the battle log.
    /// </summary>
    private void LogBattle(string message)
    {
        if (battleLogText != null)
            battleLogText.text = message;
    }

    /// <summary>
    /// Called when the Fight button is pressed.
    /// </summary>
    private void OnFightPressed()
    {
        isDefending = false;

        int damage = UnityEngine.Random.Range(playerAttackMin, playerAttackMax + 1);
        EnemyManager.Instance.TakeDamage(damage);

        LogBattle("You attack for " + damage + " damage!");

        CheckEnemyDefeated();
    }

    /// <summary>
    /// Called when the Defend button is pressed.
    /// </summary>
    private void OnDefendPressed()
    {
        isDefending = true;
        LogBattle("You brace for the enemy's attack!");
        EnemyTurn();
    }

    /// <summary>
    /// Called when the Spell button is pressed.
    /// </summary>
    private void OnSpellPressed()
    {
        isDefending = false;

        int damage = UnityEngine.Random.Range(playerSpellMin, playerSpellMax + 1);
        EnemyManager.Instance.TakeDamage(damage);

        LogBattle("You cast a spell for " + damage + " damage!");

        CheckEnemyDefeated();
    }

    /// <summary>
    /// Called when the Heal button is pressed.
    /// </summary>
    private void OnHealPressed()
    {
        isDefending = false;

        int healAmount = UnityEngine.Random.Range(playerHealMin, playerHealMax + 1);
        PlayerManager.Instance.AddHealth(healAmount);

        LogBattle("You heal for " + healAmount + " HP!");

        EnemyTurn();
    }

    /// <summary>
    /// Checks if the enemy is defeated.
    /// </summary>
    private void CheckEnemyDefeated()
    {
        if (EnemyManager.Instance.IsEnemyDefeated())
        {
            int reward = EnemyManager.Instance.GetEnemyReward();
            string enemyName = EnemyManager.Instance.GetEnemyName();

            PlayerManager.Instance.AddCoins(reward);
            totalCoinsEarned += reward;
            enemiesDefeated++;

            // Log reward
            string logEntry = enemyName + ": +" + reward + " coins";
            rewardLog.Add(logEntry);
            Debug.Log("Added to reward log: " + logEntry);

            LogBattle(enemyName + " defeated! You earned " + reward + " coins!");

            // Remove enemy from map
            dungeonGrid[playerPosition.x, playerPosition.y] = TileType.Floor;
            UpdateDungeonUI();

            EndBattle();
        }
        else
        {
            EnemyTurn();
        }
    }

    /// <summary>
    /// Enemy attacks the player.
    /// </summary>
    private void EnemyTurn()
    {
        int damage = EnemyManager.Instance.GetEnemyAttack();
        string enemyName = EnemyManager.Instance.GetEnemyName();

        if (isDefending)
        {
            damage = Mathf.RoundToInt(damage * defendReduction);
            LogBattle(enemyName + " attacks! You block and take " + damage + " damage!");
        }
        else
        {
            LogBattle(enemyName + " attacks for " + damage + " damage!");
        }

        PlayerManager.Instance.TakeDamage(damage);
        isDefending = false;

        // Check if player is defeated
        int currentHealth = PlayerManager.Instance.GetHealth();
        Debug.Log("Player health after attack: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Player defeated! Showing completion panel.");
            PlayerDefeated();
        }
    }

    /// <summary>
    /// Ends the current battle and returns to exploration.
    /// </summary>
    private void EndBattle()
    {
        inBattle = false;

        if (battlePanel != null)
            battlePanel.SetActive(false);

        if (fightUIPanel != null)
            fightUIPanel.SetActive(false);

        if (dungeonMapPanel != null)
            dungeonMapPanel.SetActive(true);

        EnemyManager.Instance.HideEnemyPanel();

        LogBattle("Continue exploring the dungeon.");
    }

    /// <summary>
    /// Called when player health reaches 0.
    /// </summary>
    private void PlayerDefeated()
    {
        Debug.Log("PlayerDefeated() called");

        dungeonActive = false;
        inBattle = false;

        SetButtonsInteractable(false);

        EnemyManager.Instance.HideEnemyPanel();

        if (battlePanel != null)
            battlePanel.SetActive(false);
        else
            Debug.LogWarning("battlePanel is null");

        if (fightUIPanel != null)
            fightUIPanel.SetActive(false);
        else
            Debug.LogWarning("fightUIPanel is null");

        if (dungeonMapPanel != null)
            dungeonMapPanel.SetActive(false);
        else
            Debug.LogWarning("dungeonMapPanel is null");

        // Update texts before showing panel
        if (totalRewardsText != null)
        {
            totalRewardsText.text = "DEFEATED!\n\n" +
                "Enemies beaten: " + enemiesDefeated + "\n" +
                "Chests opened: " + chestsOpened + "\n" +
                "Total coins earned: " + totalCoinsEarned;
            Debug.Log("totalRewardsText updated");
        }
        else
        {
            Debug.LogWarning("totalRewardsText is null");
        }

        UpdateRewardLog();

        // Show panel after texts are set
        if (dungeonCompletePanel != null)
        {
            dungeonCompletePanel.SetActive(true);
            Debug.Log("dungeonCompletePanel set to active");
        }
        else
        {
            Debug.LogError("dungeonCompletePanel is NULL - assign it in the Inspector!");
        }

        LogBattle("You have been defeated!");
    }

    /// <summary>
    /// Called when player reaches the exit.
    /// </summary>
    private void CompleteDungeon()
    {
        dungeonActive = false;

        if (dungeonMapPanel != null)
            dungeonMapPanel.SetActive(false);

        // Update texts before showing panel
        if (totalRewardsText != null)
            totalRewardsText.text = "DUNGEON COMPLETE!\n\n" +
                "Enemies beaten: " + enemiesDefeated + "\n" +
                "Chests opened: " + chestsOpened + "\n" +
                "Total coins earned: " + totalCoinsEarned;

        UpdateRewardLog();

        // Show panel after texts are set
        if (dungeonCompletePanel != null)
            dungeonCompletePanel.SetActive(true);

        LogBattle("Victory! You escaped the dungeon!");
    }

    /// <summary>
    /// Updates the reward log text with all collected rewards.
    /// </summary>
    private void UpdateRewardLog()
    {
        if (rewardLogText == null)
        {
            Debug.LogWarning("DungeonManager: rewardLogText is not assigned!");
            return;
        }

        if (rewardLog.Count == 0)
        {
            rewardLogText.text = "No rewards collected.";
            return;
        }

        string log = "--- Reward Log ---\n";
        foreach (string entry in rewardLog)
        {
            log += entry + "\n";
        }

        rewardLogText.text = log;

        Debug.Log("Reward Log Updated: " + rewardLog.Count + " entries");
    }

    /// <summary>
    /// Exits the dungeon and hides all panels.
    /// </summary>
    public void ExitDungeon()
    {
        dungeonActive = false;
        inBattle = false;

        EnemyManager.Instance.HideEnemyPanel();

        if (dungeonMapPanel != null)
            dungeonMapPanel.SetActive(false);

        if (battlePanel != null)
            battlePanel.SetActive(false);

        if (fightUIPanel != null)
            fightUIPanel.SetActive(false);

        if (dungeonCompletePanel != null)
            dungeonCompletePanel.SetActive(false);

        SetButtonsInteractable(true);
    }

    /// <summary>
    /// Enables or disables all battle buttons.
    /// </summary>
    private void SetButtonsInteractable(bool interactable)
    {
        if (fightButton != null) fightButton.interactable = interactable;
        if (defendButton != null) defendButton.interactable = interactable;
        if (spellButton != null) spellButton.interactable = interactable;
        if (healButton != null) healButton.interactable = interactable;
    }
}