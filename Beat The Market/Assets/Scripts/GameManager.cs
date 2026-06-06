using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float songTime = 0f;
    public int money = 0;
    public int notePenalty = 50;
    public int multiplier = 1;
    public float baseNoteValue = 10f;

    public int speedLevel = 1;
    public float baseNoteSpeed = 1f;
    public float noteSpeedMultiplierScale = 0.2f;
    public float lastNoteHitTime = -1f;

    public HitZone hitZone;

    public float CurrentPitch => Mathf.Min(1f + (speedLevel - 1) * noteSpeedMultiplierScale, 1.3f);

    public TextMeshProUGUI moneyTextEvent;
    public TextMeshProUGUI multiplierText;
    public ResultsUI resultsUI;

    public Animator multNumber;
    public Animator CurrentMoney;

    public int moneyTarget = 0;

    private AudioSource audioSource;
    private bool syncToAudio = false;
    private float currentPitch = 1f;

    private void Awake() => Instance = this;

    private void Update()
    {
        if (syncToAudio && audioSource != null && audioSource.isPlaying)
        {
            currentPitch = Mathf.Lerp(currentPitch, CurrentPitch, Time.deltaTime * 2f);
            audioSource.pitch = currentPitch;
            songTime = audioSource.time;
        }
        else
            songTime += Time.deltaTime;

        moneyTextEvent.text = $"${money}";
        multiplierText.text = $"x{multiplier}";
    }

    public void SetMoneyTarget(int target)
    {
        moneyTarget = target;
    }

    public void OnSongStart(AudioSource source)
    {
        audioSource = source;
        syncToAudio = true;
        songTime = 0f;
        money = 0;
        multiplier = 1;
    }

    // Called by SongManager when the song ends
    public void OnSongEnd(bool forceFail = false)
    {
        syncToAudio = false;
        bool won = !forceFail && money >= moneyTarget;
        Debug.Log($"Song over! Final score: {money} | Target: {moneyTarget} | Result: {(won ? "WIN" : "LOSS")}");

        // Stop the audio
        if (audioSource != null)
            audioSource.Stop();

        if (hitZone != null)
            hitZone.HideLines();

        // Stop note spawning
        if (NoteManager.Instance != null)
            NoteManager.Instance.enabled = false;

        Time.timeScale = 0f;

        if (resultsUI != null)
            resultsUI.Show(won, money, moneyTarget);
        else
            Debug.LogWarning("GameManager: ResultsUI not assigned!");
    }

    public void HitNote()
    {
        multiplier = Mathf.Min(multiplier + 1, 10);
        multNumber.Play("IncreaseAnim");
        speedLevel = Mathf.Min(speedLevel + 1, 10);
        money += (int)(baseNoteValue * multiplier);
        CurrentMoney.Play("CurrentMoney");
        lastNoteHitTime = songTime;
        Debug.Log("HIT | Money: " + money + " | Mult: " + multiplier);
    }

    public void MissNote()
    {
        multiplier = 1;
        speedLevel = Mathf.Max(1, speedLevel - 1);
        money -= notePenalty;
        money = Mathf.Max(0, money); // clamp so money never goes negative
        Debug.Log("MISS | Money: " + money + " | Mult: " + multiplier);
    }
}