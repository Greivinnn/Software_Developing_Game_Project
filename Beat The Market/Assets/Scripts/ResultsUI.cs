using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class ResultsUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject resultsPanel;       // The root panel to show/hide

    [Header("Text Elements")]
    public TextMeshProUGUI titleText;      // "LEVEL COMPLETE" or "LEVEL FAILED"
    public TextMeshProUGUI scoreText;      // Final score display
    public TextMeshProUGUI targetText;     // "Goal: X"
    public TextMeshProUGUI resultDetailText; // e.g. "You earned $430 / $500"

    [Header("Buttons")]
    public GameObject retryButton;        // Always shown
    public GameObject nextLevelButton;    // Only shown on win
    public GameObject mainMenuButton;     // Always shown

    [Header("Styling (optional)")]
    public UnityEngine.UI.Image panelBackground; // Panel background image
    public Color winColor = new Color(0.2f, 0.8f, 0.3f);
    public Color loseColor = new Color(0.9f, 0.2f, 0.2f);

    [Header("Next Level")]
    // Set the name of the next scene to load on win.
    // Leave blank to disable the Next Level button.
    public string nextLevelSceneName = "";

    private void Awake()
    {
        // Make sure the panel is hidden at start
        if (resultsPanel != null)
            resultsPanel.SetActive(false);
    }

    public void Show(bool won, int finalScore, int target)
    {
        if (resultsPanel == null)
        {
            Debug.LogError("ResultsUI: resultsPanel is not assigned!");
            return;
        }

        resultsPanel.SetActive(true);

        // Title
        if (titleText != null)
            titleText.text = won ? "LEVEL COMPLETE!" : "LEVEL FAILED";

        // Score
        if (scoreText != null)
            scoreText.text = $"${finalScore}";

        // Target
        if (targetText != null)
            targetText.text = $"Goal: ${target}";

        // Detail line
        if (resultDetailText != null)
        {
            resultDetailText.text = won
                ? $"You hit the target! (${finalScore} / ${target})"
                : $"So close... (${finalScore} / ${target})";
        }

        // Panel color
        if (panelBackground != null)
            panelBackground.color = won ? winColor : loseColor;

        // Next level button — only show on win AND if a scene name is set
        if (nextLevelButton != null)
            nextLevelButton.SetActive(won && !string.IsNullOrEmpty(nextLevelSceneName));

        // Retry / main menu always visible
        if (retryButton != null)
            retryButton.SetActive(true);
        if (mainMenuButton != null)
            mainMenuButton.SetActive(true);
    }

    // ── Button callbacks ─────────────────────────────────────────────────────

    public void OnRetryClicked()
    {
        Time.timeScale = 1f; // Always restore time scale before loading
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnNextLevelClicked()
    {
        if (string.IsNullOrEmpty(nextLevelSceneName))
        {
            Debug.LogWarning("ResultsUI: nextLevelSceneName is empty.");
            return;
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextLevelSceneName);
    }

    public void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}