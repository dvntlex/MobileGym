using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExercisePanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private Button panelButton;
    [SerializeField] private Button selectButton;

    private ExerciseData exerciseData;

    public void Setup(ExerciseData data)
    {
        exerciseData = data;

        nameText.text = data.exerciseName;
        descriptionText.text = data.shortDescription;
        pointsText.text = data.points.ToString() + " pts";

        panelButton.onClick.AddListener(OnPanelClicked);
        selectButton.onClick.AddListener(OnSelectClicked);
    }

    private void OnPanelClicked()
    {
        ExerciseManager.Instance.ShowExerciseDetails(exerciseData);
    }

    private void OnSelectClicked()
    {
        ExerciseManager.Instance.SelectExercise(exerciseData);
    }
}