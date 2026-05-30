using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class ResultsUI : MonoBehaviour
{
    public GameObject resultsPanel;       

    public TextMeshProUGUI titleText;      
    public TextMeshProUGUI scoreText;      
    public TextMeshProUGUI targetText;    
    public TextMeshProUGUI resultDetailText; 

    public GameObject retryButton;        
    public GameObject nextLevelButton;    
    public GameObject mainMenuButton;     

    public UnityEngine.UI.Image panelBackground; 
    public Color winColor = new Color(0.2f, 0.8f, 0.3f);
    public Color loseColor = new Color(0.9f, 0.2f, 0.2f);

    public string nextLevelSceneName = "";

    private void Awake()
    {
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

        if (titleText != null)
            titleText.text = won ? "LEVEL COMPLETE!" : "LEVEL FAILED";

        if (scoreText != null)
            scoreText.text = $"${finalScore}";

        if (targetText != null)
            targetText.text = $"${target}";

        if (resultDetailText != null)
        {
            resultDetailText.text = won
                ? $"You hit the target! (${finalScore} / ${target})"
                : $"So close... (${finalScore} / ${target})";
        }

        if (panelBackground != null)
            panelBackground.color = won ? winColor : loseColor;

        if (nextLevelButton != null)
            nextLevelButton.SetActive(won && !string.IsNullOrEmpty(nextLevelSceneName));

        if (retryButton != null)
            retryButton.SetActive(true);
        if (mainMenuButton != null)
            mainMenuButton.SetActive(true);
    }


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