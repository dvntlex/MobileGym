using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance { get; private set; }

    [Header("Canvases")]
    [SerializeField] private List<CanvasEntry> canvases = new List<CanvasEntry>();

    [Header("GameObjects")]
    [SerializeField] private List<GameObjectEntry> gameObjects = new List<GameObjectEntry>();

    [Header("Settings")]
    [SerializeField] private string startingCanvasName;

    private Dictionary<string, Canvas> canvasLookup = new Dictionary<string, Canvas>();
    private Dictionary<string, GameObject> gameObjectLookup = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Initialize();
    }

    private void Initialize()
    {
        // Setup canvases
        foreach (var entry in canvases)
        {
            if (entry.canvas == null) continue;

            canvasLookup[entry.canvasName] = entry.canvas;
            entry.canvas.gameObject.SetActive(false);

            foreach (var mapping in entry.switchButtons)
            {
                if (mapping.button == null) continue;

                string target = mapping.targetCanvasName;
                mapping.button.onClick.AddListener(() => SwitchTo(target));
            }
        }

        // Setup gameObjects
        foreach (var entry in gameObjects)
        {
            if (entry.targetObject == null) continue;

            gameObjectLookup[entry.objectName] = entry.targetObject;
            entry.targetObject.SetActive(false);

            foreach (var btn in entry.openButtons)
            {
                if (btn == null) continue;
                string name = entry.objectName;
                btn.onClick.AddListener(() => OpenObject(name));
            }

            foreach (var btn in entry.closeButtons)
            {
                if (btn == null) continue;
                string name = entry.objectName;
                btn.onClick.AddListener(() => CloseObject(name));
            }
        }

        if (!string.IsNullOrEmpty(startingCanvasName))
            SwitchTo(startingCanvasName);
    }

    public void SwitchTo(string canvasName)
    {
        foreach (var entry in canvases)
        {
            if (entry.canvas != null)
                entry.canvas.gameObject.SetActive(false);
        }

        if (canvasLookup.TryGetValue(canvasName, out Canvas target))
            target.gameObject.SetActive(true);
    }

    public void OpenObject(string objectName)
    {
        if (gameObjectLookup.TryGetValue(objectName, out GameObject target))
            target.SetActive(true);
    }

    public void CloseObject(string objectName)
    {
        if (gameObjectLookup.TryGetValue(objectName, out GameObject target))
            target.SetActive(false);
    }

    public void ToggleObject(string objectName)
    {
        if (gameObjectLookup.TryGetValue(objectName, out GameObject target))
            target.SetActive(!target.activeSelf);
    }
}

[Serializable]
public class CanvasEntry
{
    public string canvasName;
    public Canvas canvas;
    public List<SwitchButton> switchButtons = new List<SwitchButton>();
}

[Serializable]
public class SwitchButton
{
    public Button button;
    public string targetCanvasName;
}

[Serializable]
public class GameObjectEntry
{
    public string objectName;
    public GameObject targetObject;
    public List<Button> openButtons = new List<Button>();
    public List<Button> closeButtons = new List<Button>();
}