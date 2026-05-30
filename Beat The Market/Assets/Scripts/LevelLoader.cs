using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public LevelData levelData;

    private void Start()
    {
        if (levelData == null)
        {
            Debug.LogError("LevelLoader: No LevelData assigned!");
            return;
        }

        // Push the money target into GameManager
        if (GameManager.Instance != null)
            GameManager.Instance.SetMoneyTarget(levelData.moneyTarget);

        // Push the next level scene name into ResultsUI
        if (GameManager.Instance != null && GameManager.Instance.resultsUI != null)
            GameManager.Instance.resultsUI.nextLevelSceneName = levelData.nextLevelSceneName;

        Debug.Log($"LevelLoader: Loaded '{levelData.levelName}' | Target: ${levelData.moneyTarget}");
    }
}