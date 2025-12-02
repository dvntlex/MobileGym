using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreManager : MonoBehaviour
{
    public static StoreManager Instance { get; private set; }

    [Header("Store Items")]
    [SerializeField] private List<StoreItem> storeItems = new List<StoreItem>();

    [Header("Grid View")]
    [SerializeField] private Transform gridContentParent;
    [SerializeField] private GameObject storeItemPrefab;

    [Header("Popup Message")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TextMeshProUGUI popupText;
    [SerializeField] private Button popupCloseButton;
    [SerializeField] private float popupDuration = 2f;

    /// <summary>
    /// Called when the script instance is loaded.
    /// Sets up the singleton pattern and loads store items.
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
    /// Called on the first frame.
    /// Loads all store items into the grid and sets up popup.
    /// </summary>
    private void Start()
    {
        LoadStoreItems();

        if (popupPanel != null)
            popupPanel.SetActive(false);

        if (popupCloseButton != null)
            popupCloseButton.onClick.AddListener(HidePopup);
    }

    /// <summary>
    /// Clears existing panels and creates a panel for each store item.
    /// </summary>
    private void LoadStoreItems()
    {
        // Clear existing panels
        foreach (Transform child in gridContentParent)
        {
            Destroy(child.gameObject);
        }

        // Create panel for each item
        foreach (var item in storeItems)
        {
            GameObject panel = Instantiate(storeItemPrefab, gridContentParent);

            Image itemImage = panel.transform.Find("ItemImage").GetComponent<Image>();
            TextMeshProUGUI nameText = panel.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI descriptionText = panel.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI costText = panel.transform.Find("CostText").GetComponent<TextMeshProUGUI>();
            Button buyButton = panel.transform.Find("BuyButton").GetComponent<Button>();

            itemImage.sprite = item.itemImage;
            nameText.text = item.itemName;
            descriptionText.text = item.itemDescription;
            costText.text = item.itemCost.ToString() + " coins";

            // Capture item for button listener
            StoreItem currentItem = item;
            buyButton.onClick.AddListener(() => PurchaseItem(currentItem));
        }
    }

    /// <summary>
    /// Attempts to purchase an item using PlayerManager coins.
    /// Shows popup feedback for success or failure.
    /// </summary>
    /// <param name="item">The item to purchase</param>
    public void PurchaseItem(StoreItem item)
    {
        if (PlayerManager.Instance.SpendCoins(item.itemCost))
        {
            // Purchase successful
            ShowPopup("Purchased " + item.itemName + "!");
            OnItemPurchased(item);
        }
        else
        {
            // Not enough coins
            int currentCoins = PlayerManager.Instance.GetCoins();
            int needed = item.itemCost - currentCoins;
            ShowPopup("Not enough coins! You need " + needed + " more.");
        }
    }

    /// <summary>
    /// Called when an item is successfully purchased.
    /// Override or extend this to add items to inventory, apply effects, etc.
    /// </summary>
    /// <param name="item">The purchased item</param>
    private void OnItemPurchased(StoreItem item)
    {
        // Add your purchase logic here
        // Examples: add to inventory, apply stat boost, unlock content
    }

    /// <summary>
    /// Shows the popup panel with a message.
    /// Auto-hides after popupDuration seconds.
    /// </summary>
    /// <param name="message">Message to display</param>
    private void ShowPopup(string message)
    {
        if (popupPanel == null || popupText == null) return;

        popupText.text = message;
        popupPanel.SetActive(true);

        CancelInvoke(nameof(HidePopup));
        Invoke(nameof(HidePopup), popupDuration);
    }

    /// <summary>
    /// Hides the popup panel.
    /// </summary>
    private void HidePopup()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }

    /// <summary>
    /// Refreshes the store grid.
    /// </summary>
    public void RefreshStore()
    {
        LoadStoreItems();
    }
}

[Serializable]
public class StoreItem
{
    public string itemName;
    public Sprite itemImage;
    [TextArea(2, 4)]
    public string itemDescription;
    public int itemCost;
}