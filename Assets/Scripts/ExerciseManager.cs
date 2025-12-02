using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExerciseManager : MonoBehaviour
{
    public static ExerciseManager Instance { get; private set; }

    [Header("Exercise Data")]
    [SerializeField] private List<ExerciseData> exercises = new List<ExerciseData>();

    [Header("Scroll View")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject exercisePanelPrefab;

    [Header("Detail Panel")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private TextMeshProUGUI detailName;
    [SerializeField] private TextMeshProUGUI detailDescription;
    [SerializeField] private TextMeshProUGUI detailPoints;
    [SerializeField] private TextMeshProUGUI detailMuscles;
    [SerializeField] private Button closeDetailButton;

    [Header("Selected Exercises Panel")]
    [SerializeField] private Transform selectedContentParent;
    [SerializeField] private GameObject selectedEntryPrefab;

    private List<SelectedEntry> selectedEntries = new List<SelectedEntry>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        LoadExercises();

        if (closeDetailButton != null)
            closeDetailButton.onClick.AddListener(HideExerciseDetails);

        if (detailPanel != null)
            detailPanel.SetActive(false);
    }

    private void LoadExercises()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var exercise in exercises)
        {
            GameObject panel = Instantiate(exercisePanelPrefab, contentParent);
            ExercisePanel exercisePanel = panel.GetComponent<ExercisePanel>();
            exercisePanel.Setup(exercise);
        }
    }

    public void ShowExerciseDetails(ExerciseData data)
    {
        detailPanel.SetActive(true);
        detailName.text = data.exerciseName;
        detailDescription.text = data.fullDescription;
        detailPoints.text = data.points.ToString() + " Points";
        detailMuscles.text = "Muscles: " + string.Join(", ", data.musclesAffected);
    }

    public void HideExerciseDetails()
    {
        detailPanel.SetActive(false);
    }

    public void SelectExercise(ExerciseData exercise)
    {
        if (exercise == null) return;

        GameObject entryObj = Instantiate(selectedEntryPrefab, selectedContentParent);

        TextMeshProUGUI nameText = entryObj.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        TMP_InputField repsInput = entryObj.transform.Find("RepsInput").GetComponent<TMP_InputField>();
        TMP_InputField weightInput = entryObj.transform.Find("WeightInput").GetComponent<TMP_InputField>();

        SelectedEntry entry = new SelectedEntry
        {
            exercise = exercise,
            entryObject = entryObj,
            repsInput = repsInput,
            weightInput = weightInput
        };

        selectedEntries.Add(entry);

        nameText.text = selectedEntries.Count + ". " + exercise.exerciseName;
        repsInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        weightInput.contentType = TMP_InputField.ContentType.DecimalNumber;
    }

    public void ClearSelectedExercises()
    {
        selectedEntries.Clear();

        foreach (Transform child in selectedContentParent)
        {
            Destroy(child.gameObject);
        }
    }

    public List<SelectedExerciseData> GetSelectedExercises()
    {
        List<SelectedExerciseData> data = new List<SelectedExerciseData>();

        foreach (var entry in selectedEntries)
        {
            int reps = 0;
            float weight = 0f;

            int.TryParse(entry.repsInput.text, out reps);
            float.TryParse(entry.weightInput.text, out weight);

            data.Add(new SelectedExerciseData
            {
                exercise = entry.exercise,
                reps = reps,
                weight = weight
            });
        }

        return data;
    }

    public void AddExercise(ExerciseData newExercise)
    {
        exercises.Add(newExercise);

        GameObject panel = Instantiate(exercisePanelPrefab, contentParent);
        ExercisePanel exercisePanel = panel.GetComponent<ExercisePanel>();
        exercisePanel.Setup(newExercise);
    }

    public void RefreshList()
    {
        LoadExercises();
    }
}

[Serializable]
public class ExerciseData
{
    public string exerciseName;
    [TextArea(2, 4)]
    public string shortDescription;
    [TextArea(4, 8)]
    public string fullDescription;
    public int points;
    public string[] musclesAffected;
}

[Serializable]
public class SelectedEntry
{
    public ExerciseData exercise;
    public GameObject entryObject;
    public TMP_InputField repsInput;
    public TMP_InputField weightInput;
}

[Serializable]
public class SelectedExerciseData
{
    public ExerciseData exercise;
    public int reps;
    public float weight;
}