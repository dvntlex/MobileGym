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
    [SerializeField] private Button completeWorkoutButton;

    [Header("History Panel")]
    [SerializeField] private GameObject historyPanel;
    [SerializeField] private Transform historyContentParent;
    [SerializeField] private GameObject historySessionPrefab;

    private List<SelectedEntry> selectedEntries = new List<SelectedEntry>();
    private ExerciseHistory exerciseHistory = new ExerciseHistory();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadHistory();
    }

    private void Start()
    {
        LoadExercises();
        
        if (closeDetailButton != null)
            closeDetailButton.onClick.AddListener(HideExerciseDetails);

        if (completeWorkoutButton != null)
            completeWorkoutButton.onClick.AddListener(CompleteWorkout);
        
        if (detailPanel != null)
            detailPanel.SetActive(false);

        if (historyPanel != null)
            historyPanel.SetActive(false);
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
        
        TextMeshProUGUI nameText = entryObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        Transform setsContainer = entryObj.transform.Find("SetsContainer");
        GameObject setTemplate = entryObj.transform.Find("SetsContainer/SetTemplate")?.gameObject;
        Button addSetButton = entryObj.transform.Find("AddSetButton")?.GetComponent<Button>();

        SelectedEntry entry = new SelectedEntry
        {
            exercise = exercise,
            entryObject = entryObj,
            setsContainer = setsContainer,
            setTemplate = setTemplate,
            sets = new List<SetEntry>()
        };

        selectedEntries.Add(entry);

        if (nameText != null)
            nameText.text = selectedEntries.Count + ". " + exercise.exerciseName;

        // Hide the template
        if (setTemplate != null)
            setTemplate.SetActive(false);

        // Add first set automatically
        AddSetToEntry(entry);

        // Setup add set button
        if (addSetButton != null)
        {
            SelectedEntry capturedEntry = entry;
            addSetButton.onClick.AddListener(() => AddSetToEntry(capturedEntry));
        }
    }

    /// <summary>
    /// Adds a new set to an exercise entry.
    /// </summary>
    private void AddSetToEntry(SelectedEntry entry)
    {
        if (entry.setsContainer == null || entry.setTemplate == null) return;

        GameObject setObj = Instantiate(entry.setTemplate, entry.setsContainer);
        setObj.SetActive(true);
        
        TextMeshProUGUI setNumberText = setObj.transform.Find("SetNumberText")?.GetComponent<TextMeshProUGUI>();
        TMP_InputField repsInput = setObj.transform.Find("RepsInput")?.GetComponent<TMP_InputField>();
        TMP_InputField weightInput = setObj.transform.Find("WeightInput")?.GetComponent<TMP_InputField>();
        Button removeButton = setObj.transform.Find("RemoveButton")?.GetComponent<Button>();

        SetEntry setEntry = new SetEntry
        {
            setObject = setObj,
            repsInput = repsInput,
            weightInput = weightInput
        };

        entry.sets.Add(setEntry);

        if (setNumberText != null)
            setNumberText.text = "Set " + entry.sets.Count;

        if (repsInput != null)
            repsInput.contentType = TMP_InputField.ContentType.IntegerNumber;

        if (weightInput != null)
            weightInput.contentType = TMP_InputField.ContentType.DecimalNumber;

        // Setup remove button
        if (removeButton != null)
        {
            SelectedEntry capturedEntry = entry;
            SetEntry capturedSet = setEntry;
            removeButton.onClick.AddListener(() => RemoveSetFromEntry(capturedEntry, capturedSet));
        }
    }

    /// <summary>
    /// Removes a set from an exercise entry.
    /// </summary>
    private void RemoveSetFromEntry(SelectedEntry entry, SetEntry setEntry)
    {
        if (entry.sets.Count <= 1) return; // Keep at least one set

        entry.sets.Remove(setEntry);
        Destroy(setEntry.setObject);

        // Update set numbers
        for (int i = 0; i < entry.sets.Count; i++)
        {
            TextMeshProUGUI setNumberText = entry.sets[i].setObject.transform.Find("SetNumberText")?.GetComponent<TextMeshProUGUI>();
            if (setNumberText != null)
                setNumberText.text = "Set " + (i + 1);
        }
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
            foreach (var set in entry.sets)
            {
                int reps = 0;
                float weight = 0f;

                if (set.repsInput != null)
                    int.TryParse(set.repsInput.text, out reps);

                if (set.weightInput != null)
                    float.TryParse(set.weightInput.text, out weight);

                data.Add(new SelectedExerciseData
                {
                    exercise = entry.exercise,
                    reps = reps,
                    weight = weight
                });
            }
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

    /// <summary>
    /// Completes the current workout and saves it to history.
    /// </summary>
    public void CompleteWorkout()
    {
        if (selectedEntries.Count == 0) return;

        // Create new workout session
        WorkoutSession session = new WorkoutSession();
        session.date = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        session.exercises = new List<CompletedExercise>();

        int totalPoints = 0;

        foreach (var entry in selectedEntries)
        {
            foreach (var set in entry.sets)
            {
                int reps = 0;
                float weight = 0f;

                if (set.repsInput != null)
                    int.TryParse(set.repsInput.text, out reps);

                if (set.weightInput != null)
                    float.TryParse(set.weightInput.text, out weight);

                CompletedExercise completed = new CompletedExercise();
                completed.exerciseName = entry.exercise.exerciseName;
                completed.reps = reps;
                completed.weight = weight;
                completed.points = entry.exercise.points;

                session.exercises.Add(completed);
            }

            totalPoints += entry.exercise.points * entry.sets.Count;
        }

        session.totalPoints = totalPoints;

        // Add to history
        exerciseHistory.sessions.Insert(0, session);
        SaveHistory();

        // Award points to player
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.AddXP(totalPoints);
        }

        // Clear selected exercises
        ClearSelectedExercises();

        Debug.Log("Workout completed! Total points: " + totalPoints);
    }

    /// <summary>
    /// Shows the exercise history panel.
    /// </summary>
    public void ShowHistory()
    {
        if (historyPanel != null)
            historyPanel.SetActive(true);

        LoadHistoryUI();
    }

    /// <summary>
    /// Hides the exercise history panel.
    /// </summary>
    public void HideHistory()
    {
        if (historyPanel != null)
            historyPanel.SetActive(false);
    }

    /// <summary>
    /// Loads the history UI with all past workout sessions.
    /// </summary>
    private void LoadHistoryUI()
    {
        if (historyContentParent == null) return;

        // Clear existing entries
        foreach (Transform child in historyContentParent)
        {
            Destroy(child.gameObject);
        }

        if (exerciseHistory.sessions.Count == 0)
        {
            if (historySessionPrefab != null)
            {
                GameObject emptyObj = Instantiate(historySessionPrefab, historyContentParent);
                TextMeshProUGUI dateText = emptyObj.transform.Find("DateText")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI exercisesText = emptyObj.transform.Find("ExercisesText")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI pointsText = emptyObj.transform.Find("PointsText")?.GetComponent<TextMeshProUGUI>();

                if (dateText != null) dateText.text = "No History";
                if (exercisesText != null) exercisesText.text = "Complete a workout to see your history.";
                if (pointsText != null) pointsText.text = "";
            }
            return;
        }

        // Create entry for each session
        foreach (var session in exerciseHistory.sessions)
        {
            if (historySessionPrefab == null) continue;

            GameObject sessionObj = Instantiate(historySessionPrefab, historyContentParent);

            TextMeshProUGUI dateText = sessionObj.transform.Find("DateText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI exercisesText = sessionObj.transform.Find("ExercisesText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI pointsText = sessionObj.transform.Find("PointsText")?.GetComponent<TextMeshProUGUI>();

            // Set date
            if (dateText != null)
                dateText.text = session.date;

            // Set points
            if (pointsText != null)
                pointsText.text = session.totalPoints + " pts";

            // Build exercises list grouped by name
            if (exercisesText != null)
            {
                string exercisesList = "";
                string currentExercise = "";
                int setCount = 0;

                foreach (var exercise in session.exercises)
                {
                    if (exercise.exerciseName != currentExercise)
                    {
                        if (exercisesList.Length > 0)
                            exercisesList += "\n";

                        currentExercise = exercise.exerciseName;
                        setCount = 1;
                        exercisesList += exercise.exerciseName + ":";
                    }

                    exercisesList += "\n  Set " + setCount + ": " + exercise.reps + " reps @ " + exercise.weight + " lbs";
                    setCount++;
                }
                exercisesText.text = exercisesList;
            }
        }
    }

    /// <summary>
    /// Saves exercise history to PlayerPrefs.
    /// </summary>
    private void SaveHistory()
    {
        string json = JsonUtility.ToJson(exerciseHistory);
        PlayerPrefs.SetString("ExerciseHistory", json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads exercise history from PlayerPrefs.
    /// </summary>
    private void LoadHistory()
    {
        if (PlayerPrefs.HasKey("ExerciseHistory"))
        {
            string json = PlayerPrefs.GetString("ExerciseHistory");
            exerciseHistory = JsonUtility.FromJson<ExerciseHistory>(json);

            if (exerciseHistory == null)
                exerciseHistory = new ExerciseHistory();

            if (exerciseHistory.sessions == null)
                exerciseHistory.sessions = new List<WorkoutSession>();
        }
    }

    /// <summary>
    /// Clears all exercise history.
    /// </summary>
    public void ClearHistory()
    {
        exerciseHistory.sessions.Clear();
        SaveHistory();
        LoadHistoryUI();
    }

    /// <summary>
    /// Gets the total number of workouts completed.
    /// </summary>
    public int GetTotalWorkouts()
    {
        return exerciseHistory.sessions.Count;
    }

    /// <summary>
    /// Gets the total points earned from all workouts.
    /// </summary>
    public int GetTotalPointsEarned()
    {
        int total = 0;
        foreach (var session in exerciseHistory.sessions)
        {
            total += session.totalPoints;
        }
        return total;
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
    public Transform setsContainer;
    public GameObject setTemplate;
    public List<SetEntry> sets;
}

[Serializable]
public class SetEntry
{
    public GameObject setObject;
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

[Serializable]
public class ExerciseHistory
{
    public List<WorkoutSession> sessions = new List<WorkoutSession>();
}

[Serializable]
public class WorkoutSession
{
    public string date;
    public List<CompletedExercise> exercises;
    public int totalPoints;
}

[Serializable]
public class CompletedExercise
{
    public string exerciseName;
    public int reps;
    public float weight;
    public int points;
}