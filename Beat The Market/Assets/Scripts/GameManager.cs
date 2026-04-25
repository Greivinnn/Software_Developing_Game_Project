using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float songTime = 0f;
    public int money = 0;
    public int multiplier = 1;
    public float baseNoteValue = 10f;

    private AudioSource audioSource;
    private bool syncToAudio = false;

    private void Awake() => Instance = this;

    private void Update()
    {
        // Prefer audio time — it never drifts. Fall back to deltaTime before song loads.
        if (syncToAudio && audioSource != null && audioSource.isPlaying)
            songTime = audioSource.time;
        else
            songTime += Time.deltaTime;
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
        // TODO: show results screen
    }

    public void HitNote()
    {
        multiplier++;
        money += (int)(baseNoteValue * multiplier);
        Debug.Log("HIT | Money: " + money + " | Mult: " + multiplier);
    }

    public void MissNote()
    {
        multiplier = 1;
        Debug.Log("MISS | Money: " + money + " | Mult: " + multiplier);
    }
}