using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    [Header("Dungeon Generation")]
    [SerializeField] private int dungeonWidth = 20;
    [SerializeField] private int dungeonHeight = 20;
    [SerializeField] private int viewportWidth = 7;
    [SerializeField] private int viewportHeight = 7;
    [SerializeField] private int enemyCount = 8;
    [SerializeField] private int chestCount = 5;

    [Header("Room Generation")]
    [SerializeField] private int minRooms = 4;
    [SerializeField] private int maxRooms = 8;
    [SerializeField] private int minRoomSize = 3;
    [SerializeField] private int maxRoomSize = 6;

    public enum RoomShape
    {
        Rectangle,
        LShape,
        TShape,
        Cross,
        UShape
    }

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

    [Header("Tile Sprites")]
    [SerializeField] private Sprite floorSprite;
    [SerializeField] private Sprite playerSprite;
    [SerializeField] private Sprite enemySprite;
    [SerializeField] private Sprite chestSprite;
    [SerializeField] private Sprite exitSprite;
    [SerializeField] private Sprite visitedSprite;
    [SerializeField] private Sprite fogSprite;

    [Header("Wall Sprites")]
    [SerializeField] private Sprite wallSolidSprite;
    [SerializeField] private Sprite wallTopSprite;
    [SerializeField] private Sprite wallBottomSprite;
    [SerializeField] private Sprite wallLeftSprite;
    [SerializeField] private Sprite wallRightSprite;

    [Header("Outer Corner Sprites")]
    [SerializeField] private Sprite cornerTopLeftSprite;
    [SerializeField] private Sprite cornerTopRightSprite;
    [SerializeField] private Sprite cornerBottomLeftSprite;
    [SerializeField] private Sprite cornerBottomRightSprite;

    [Header("Inner Corner Sprites")]
    [SerializeField] private Sprite innerCornerTopLeftSprite;
    [SerializeField] private Sprite innerCornerTopRightSprite;
    [SerializeField] private Sprite innerCornerBottomLeftSprite;
    [SerializeField] private Sprite innerCornerBottomRightSprite;

    [Header("Tile Colors (used if no sprite assigned)")]
    [SerializeField] private Color floorColor = Color.gray;
    [SerializeField] private Color wallColor = Color.black;
    [SerializeField] private Color playerColor = Color.blue;
    [SerializeField] private Color enemyColor = Color.red;
    [SerializeField] private Color chestColor = Color.yellow;
    [SerializeField] private Color exitColor = Color.green;
    [SerializeField] private Color visitedColor = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private Color fogColor = new Color(0.2f, 0.2f, 0.2f);

    [Header("Fog of War")]
    [SerializeField] private int visionRadius = 2;

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
    private bool[,] revealedTiles;

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
    /// Generates a dungeon with rooms of various shapes connected by corridors.
    /// </summary>
    private void GenerateDungeon()
    {
        dungeonGrid = new TileType[dungeonWidth, dungeonHeight];
        visitedTiles = new bool[dungeonWidth, dungeonHeight];
        revealedTiles = new bool[dungeonWidth, dungeonHeight];

        // Fill with walls
        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int y = 0; y < dungeonHeight; y++)
            {
                dungeonGrid[x, y] = TileType.Wall;
                visitedTiles[x, y] = false;
                revealedTiles[x, y] = false;
            }
        }

        // Generate rooms
        List<Room> rooms = new List<Room>();
        int roomCount = UnityEngine.Random.Range(minRooms, maxRooms + 1);

        for (int i = 0; i < roomCount; i++)
        {
            Room room = TryCreateRoom(rooms);
            if (room != null)
            {
                rooms.Add(room);
                CarveRoom(room);
            }
        }

        // Connect rooms with corridors
        for (int i = 1; i < rooms.Count; i++)
        {
            ConnectRooms(rooms[i - 1], rooms[i]);
        }

        // Set player start position in first room
        if (rooms.Count > 0)
        {
            playerPosition = rooms[0].center;
            dungeonGrid[playerPosition.x, playerPosition.y] = TileType.Start;
        }

        // Place exit in last room
        if (rooms.Count > 1)
        {
            exitPosition = rooms[rooms.Count - 1].center;
            dungeonGrid[exitPosition.x, exitPosition.y] = TileType.Exit;
        }
        else
        {
            exitPosition = FindFurthestFloor(playerPosition);
            dungeonGrid[exitPosition.x, exitPosition.y] = TileType.Exit;
        }

        // Place enemies and chests
        PlaceRandomTiles(TileType.Enemy, enemyCount);
        PlaceRandomTiles(TileType.Chest, chestCount);

        // Reveal tiles around starting position
        RevealTilesAroundPlayer();
    }

    /// <summary>
    /// Tries to create a room that doesn't overlap with existing rooms.
    /// </summary>
    private Room TryCreateRoom(List<Room> existingRooms)
    {
        int maxAttempts = 30;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            int width = UnityEngine.Random.Range(minRoomSize, maxRoomSize + 1);
            int height = UnityEngine.Random.Range(minRoomSize, maxRoomSize + 1);
            int x = UnityEngine.Random.Range(2, dungeonWidth - width - 2);
            int y = UnityEngine.Random.Range(2, dungeonHeight - height - 2);

            RoomShape shape = (RoomShape)UnityEngine.Random.Range(0, 5);
            Room room = new Room(x, y, width, height, shape);

            bool overlaps = false;
            foreach (Room existing in existingRooms)
            {
                if (RoomsOverlap(room, existing))
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
                return room;
        }

        return null;
    }

    /// <summary>
    /// Checks if two rooms overlap (with padding).
    /// </summary>
    private bool RoomsOverlap(Room a, Room b)
    {
        int padding = 2;
        return a.x - padding < b.x + b.width + padding &&
               a.x + a.width + padding > b.x - padding &&
               a.y - padding < b.y + b.height + padding &&
               a.y + a.height + padding > b.y - padding;
    }

    /// <summary>
    /// Carves out a room based on its shape.
    /// </summary>
    private void CarveRoom(Room room)
    {
        switch (room.shape)
        {
            case RoomShape.Rectangle:
                CarveRectangle(room.x, room.y, room.width, room.height);
                break;

            case RoomShape.LShape:
                CarveLShape(room);
                break;

            case RoomShape.TShape:
                CarveTShape(room);
                break;

            case RoomShape.Cross:
                CarveCross(room);
                break;

            case RoomShape.UShape:
                CarveUShape(room);
                break;
        }
    }

    /// <summary>
    /// Carves a rectangular area.
    /// </summary>
    private void CarveRectangle(int startX, int startY, int width, int height)
    {
        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                if (IsInsideBorder(new Vector2Int(x, y)))
                    dungeonGrid[x, y] = TileType.Floor;
            }
        }
    }

    /// <summary>
    /// Carves an L-shaped room.
    /// </summary>
    private void CarveLShape(Room room)
    {
        int halfWidth = room.width / 2;
        int halfHeight = room.height / 2;

        // Vertical part
        CarveRectangle(room.x, room.y, halfWidth + 1, room.height);
        // Horizontal part
        CarveRectangle(room.x, room.y, room.width, halfHeight + 1);
    }

    /// <summary>
    /// Carves a T-shaped room.
    /// </summary>
    private void CarveTShape(Room room)
    {
        int halfWidth = room.width / 2;
        int thirdHeight = room.height / 3;

        // Top horizontal bar
        CarveRectangle(room.x, room.y + room.height - thirdHeight - 1, room.width, thirdHeight + 1);
        // Vertical stem
        CarveRectangle(room.x + halfWidth - 1, room.y, 3, room.height);
    }

    /// <summary>
    /// Carves a cross-shaped room.
    /// </summary>
    private void CarveCross(Room room)
    {
        int thirdWidth = room.width / 3;
        int thirdHeight = room.height / 3;

        // Horizontal bar
        CarveRectangle(room.x, room.y + thirdHeight, room.width, thirdHeight + 1);
        // Vertical bar
        CarveRectangle(room.x + thirdWidth, room.y, thirdWidth + 1, room.height);
    }

    /// <summary>
    /// Carves a U-shaped room.
    /// </summary>
    private void CarveUShape(Room room)
    {
        int thirdWidth = room.width / 3;
        int halfHeight = room.height / 2;

        // Left vertical
        CarveRectangle(room.x, room.y, thirdWidth + 1, room.height);
        // Right vertical
        CarveRectangle(room.x + room.width - thirdWidth - 1, room.y, thirdWidth + 1, room.height);
        // Bottom horizontal
        CarveRectangle(room.x, room.y, room.width, halfHeight);
    }

    /// <summary>
    /// Connects two rooms with a corridor.
    /// </summary>
    private void ConnectRooms(Room a, Room b)
    {
        Vector2Int start = a.center;
        Vector2Int end = b.center;

        // Random choice: horizontal first or vertical first
        if (UnityEngine.Random.value > 0.5f)
        {
            CarveHorizontalCorridor(start.x, end.x, start.y);
            CarveVerticalCorridor(start.y, end.y, end.x);
        }
        else
        {
            CarveVerticalCorridor(start.y, end.y, start.x);
            CarveHorizontalCorridor(start.x, end.x, end.y);
        }
    }

    /// <summary>
    /// Carves a horizontal corridor.
    /// </summary>
    private void CarveHorizontalCorridor(int x1, int x2, int y)
    {
        int minX = Mathf.Min(x1, x2);
        int maxX = Mathf.Max(x1, x2);

        for (int x = minX; x <= maxX; x++)
        {
            if (IsInsideBorder(new Vector2Int(x, y)))
                dungeonGrid[x, y] = TileType.Floor;

            // Make corridor 2 tiles wide
            if (IsInsideBorder(new Vector2Int(x, y + 1)))
                dungeonGrid[x, y + 1] = TileType.Floor;
        }
    }

    /// <summary>
    /// Carves a vertical corridor.
    /// </summary>
    private void CarveVerticalCorridor(int y1, int y2, int x)
    {
        int minY = Mathf.Min(y1, y2);
        int maxY = Mathf.Max(y1, y2);

        for (int y = minY; y <= maxY; y++)
        {
            if (IsInsideBorder(new Vector2Int(x, y)))
                dungeonGrid[x, y] = TileType.Floor;

            // Make corridor 2 tiles wide
            if (IsInsideBorder(new Vector2Int(x + 1, y)))
                dungeonGrid[x + 1, y] = TileType.Floor;
        }
    }

    /// <summary>
    /// Checks if a position is inside the border (not on the edge).
    /// </summary>
    private bool IsInsideBorder(Vector2Int pos)
    {
        return pos.x > 0 && pos.x < dungeonWidth - 1 && pos.y > 0 && pos.y < dungeonHeight - 1;
    }

    /// <summary>
    /// Room data class.
    /// </summary>
    private class Room
    {
        public int x, y, width, height;
        public RoomShape shape;
        public Vector2Int center;

        public Room(int x, int y, int width, int height, RoomShape shape)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.shape = shape;
            this.center = new Vector2Int(x + width / 2, y + height / 2);
        }
    }

    /// <summary>
    /// Reveals tiles within the vision radius around the player.
    /// </summary>
    private void RevealTilesAroundPlayer()
    {
        for (int x = -visionRadius; x <= visionRadius; x++)
        {
            for (int y = -visionRadius; y <= visionRadius; y++)
            {
                Vector2Int checkPos = new Vector2Int(playerPosition.x + x, playerPosition.y + y);

                if (IsInBounds(checkPos))
                {
                    // Use circular vision (optional - remove the if for square vision)
                    if (x * x + y * y <= visionRadius * visionRadius)
                    {
                        revealedTiles[checkPos.x, checkPos.y] = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Checks if a position is within dungeon bounds.
    /// </summary>
    private bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < dungeonWidth && pos.y >= 0 && pos.y < dungeonHeight;
    }

    /// <summary>
    /// Gets valid neighboring positions (inside border).
    /// </summary>
    private List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in directions)
        {
            Vector2Int next = pos + dir;
            if (IsInsideBorder(next))
                neighbors.Add(next);
        }

        return neighbors;
    }

    /// <summary>
    /// Finds the floor tile furthest from a starting position (inside border).
    /// </summary>
    private Vector2Int FindFurthestFloor(Vector2Int start)
    {
        Vector2Int furthest = start;
        int maxDistance = 0;

        for (int x = 1; x < dungeonWidth - 1; x++)
        {
            for (int y = 1; y < dungeonHeight - 1; y++)
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
    /// Places a number of tiles of a specific type on random floor tiles (inside border).
    /// </summary>
    private void PlaceRandomTiles(TileType type, int count)
    {
        List<Vector2Int> floorTiles = new List<Vector2Int>();

        for (int x = 1; x < dungeonWidth - 1; x++)
        {
            for (int y = 1; y < dungeonHeight - 1; y++)
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
    /// Creates the visual UI for the dungeon viewport.
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

        // Create viewport-sized grid (not full dungeon)
        tileImages = new Image[viewportWidth, viewportHeight];

        for (int y = viewportHeight - 1; y >= 0; y--)
        {
            for (int x = 0; x < viewportWidth; x++)
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
        
        Debug.Log("Dungeon viewport created with " + (viewportWidth * viewportHeight) + " tiles.");
    }

    /// <summary>
    /// Checks if a tile is walkable (not a wall).
    /// </summary>
    private bool IsWalkable(int x, int y)
    {
        if (!IsInBounds(new Vector2Int(x, y)))
            return false;

        TileType tile = dungeonGrid[x, y];
        return tile != TileType.Wall;
    }

    /// <summary>
    /// Gets the appropriate wall sprite based on neighboring tiles.
    /// </summary>
    private Sprite GetWallSprite(int x, int y)
    {
        bool top = IsWalkable(x, y + 1);
        bool bottom = IsWalkable(x, y - 1);
        bool left = IsWalkable(x - 1, y);
        bool right = IsWalkable(x + 1, y);

        bool topLeft = IsWalkable(x - 1, y + 1);
        bool topRight = IsWalkable(x + 1, y + 1);
        bool bottomLeft = IsWalkable(x - 1, y - 1);
        bool bottomRight = IsWalkable(x + 1, y - 1);

        // Outer corners (wall with two adjacent walkable sides)
        if (bottom && right && !top && !left)
            return cornerTopLeftSprite ?? wallSolidSprite;
        if (bottom && left && !top && !right)
            return cornerTopRightSprite ?? wallSolidSprite;
        if (top && right && !bottom && !left)
            return cornerBottomLeftSprite ?? wallSolidSprite;
        if (top && left && !bottom && !right)
            return cornerBottomRightSprite ?? wallSolidSprite;

        // Inner corners (wall surrounded by walls but with diagonal walkable)
        if (!top && !bottom && !left && !right)
        {
            if (bottomRight)
                return innerCornerTopLeftSprite ?? wallSolidSprite;
            if (bottomLeft)
                return innerCornerTopRightSprite ?? wallSolidSprite;
            if (topRight)
                return innerCornerBottomLeftSprite ?? wallSolidSprite;
            if (topLeft)
                return innerCornerBottomRightSprite ?? wallSolidSprite;
        }

        // Edge walls (wall with one adjacent walkable side)
        if (bottom && !top && !left && !right)
            return wallTopSprite ?? wallSolidSprite;
        if (top && !bottom && !left && !right)
            return wallBottomSprite ?? wallSolidSprite;
        if (right && !top && !bottom && !left)
            return wallLeftSprite ?? wallSolidSprite;
        if (left && !top && !bottom && !right)
            return wallRightSprite ?? wallSolidSprite;

        // Default solid wall
        return wallSolidSprite;
    }

    /// <summary>
    /// Updates the dungeon viewport to show tiles around the player.
    /// Player is always in the center of the viewport.
    /// Unrevealed tiles show as fog.
    /// </summary>
    private void UpdateDungeonUI()
    {
        // Calculate offset to center player in viewport
        int offsetX = playerPosition.x - (viewportWidth / 2);
        int offsetY = playerPosition.y - (viewportHeight / 2);

        for (int vx = 0; vx < viewportWidth; vx++)
        {
            for (int vy = 0; vy < viewportHeight; vy++)
            {
                // Calculate actual dungeon position
                int dungeonX = offsetX + vx;
                int dungeonY = offsetY + vy;

                Color color = fogColor;
                Sprite sprite = fogSprite;

                // Check if this viewport tile is the player (center of viewport)
                if (vx == viewportWidth / 2 && vy == viewportHeight / 2)
                {
                    color = playerColor;
                    sprite = playerSprite;
                }
                // Check if position is outside dungeon bounds
                else if (!IsInBounds(new Vector2Int(dungeonX, dungeonY)))
                {
                    color = fogColor;
                    sprite = fogSprite;
                }
                // Check if tile is not revealed (fog of war)
                else if (!revealedTiles[dungeonX, dungeonY])
                {
                    color = fogColor;
                    sprite = fogSprite;
                }
                // Check if visited
                else if (visitedTiles[dungeonX, dungeonY])
                {
                    color = visitedColor;
                    sprite = visitedSprite;
                }
                else
                {
                    switch (dungeonGrid[dungeonX, dungeonY])
                    {
                        case TileType.Floor:
                        case TileType.Start:
                            color = floorColor;
                            sprite = floorSprite;
                            break;
                        case TileType.Wall:
                            color = wallColor;
                            sprite = GetWallSprite(dungeonX, dungeonY);
                            break;
                        case TileType.Enemy:
                            color = enemyColor;
                            sprite = enemySprite;
                            break;
                        case TileType.Chest:
                            color = chestColor;
                            sprite = chestSprite;
                            break;
                        case TileType.Exit:
                            color = exitColor;
                            sprite = exitSprite;
                            break;
                    }
                }

                // Apply sprite if assigned, otherwise use color
                if (sprite != null)
                {
                    tileImages[vx, vy].sprite = sprite;
                    tileImages[vx, vy].color = Color.white;
                }
                else
                {
                    tileImages[vx, vy].sprite = null;
                    tileImages[vx, vy].color = color;
                }
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

        // Reveal tiles around new position
        RevealTilesAroundPlayer();

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