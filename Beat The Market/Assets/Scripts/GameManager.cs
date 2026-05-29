using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float songTime = 0f;
    public int money = 0;
    public int multiplier = 1;
    public float baseNoteValue = 10f;

    public int speedLevel = 1;
    public float baseNoteSpeed = 1f;
    public float noteSpeedMultiplierScale = 0.2f; // how much each multiplier step adds

    public float CurrentNoteSpeed => Mathf.Min(baseNoteSpeed + (speedLevel - 1) * noteSpeedMultiplierScale, 25f);

    public InputFeedback inputFeedback;

    public TextMeshProUGUI moneyTextEvent;

    private AudioSource audioSource;
    private bool syncToAudio = false;

    private void Awake() => Instance = this;

    private void Update()
    {
        // If possible alway use audio source time for better syncing and no bugs
        // fall back to delta time if anything fails
        if (syncToAudio && audioSource != null && audioSource.isPlaying)
        {
            songTime = audioSource.time;
        }
        else
        {
            songTime += Time.deltaTime;
        }
        moneyTextEvent.text = "Money: " + money;
    }

    // Called by SongManager when audio starts
    public void OnSongStart(AudioSource source)
    {
        audioSource = source;
        syncToAudio = true;
        songTime = 0f;
        money = 0;
        multiplier = 1;
    }

    // Called by SongManager when the song ends
    public void OnSongEnd()
    {
        syncToAudio = false;
        Debug.Log($"Song over! Final score: {money}");
        SceneManager.LoadScene("MainMenu");
        // TODO: show results screen
        // TODO: stop song from playing 
    }

    public void HitNote()
    {
        multiplier = Mathf.Min(multiplier + 1, 10);
        speedLevel = Mathf.Min(speedLevel + 1, 10);
        money += (int)(baseNoteValue * multiplier);
        inputFeedback.ShowHit();
        Debug.Log("HIT | Money: " + money + " | Mult: " + multiplier);
    }

    public void MissNote()
    {
        multiplier = 1;
        speedLevel = Mathf.Max(1, speedLevel - 3);
        inputFeedback.ShowMiss();
        Debug.Log("MISS | Money: " + money + " | Mult: " + multiplier);
        money -= 10; // penalty for missing
    }
}