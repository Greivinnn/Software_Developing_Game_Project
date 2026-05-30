using UnityEngine;

[CreateAssetMenu(fileName = "Level1Data", menuName = "RythmGame/Level1Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public string levelName = "Level 1";
    public string sceneName = "01_Prototype";         // Scene to load for this level
    public string nextLevelSceneName = "";       // Leave blank if this is the last level

    [Header("Goal")]
    public int moneyTarget = 500;               // How much money the player needs to win

    [Header("Audio")]
    public AudioClip song;
}
