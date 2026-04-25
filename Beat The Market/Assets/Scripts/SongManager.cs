using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class SongManager : MonoBehaviour
{
    public static SongManager Instance;

    [Header("References")]
    public NoteManager noteManager;
    public AudioSource audioSource;

    [Header("Settings")]
    // How far ahead (in seconds) to spawn notes before they need to be hit.
    // Must be long enough for the note to travel from spawn point to hit zone.
    // Formula: spawnLead = distanceToHitZone / noteSpeed
    // Example: spawnPoint at X=15, noteSpeed=5 ? 15/5 = 3 seconds
    public float spawnLeadTime = 3f;

    // --- private state ---
    private ChartData currentChart;
    private List<ChartNote> pendingNotes;  // notes not yet spawned
    private bool isPlaying = false;
    private float songTime => audioSource.time; // always anchor to audio, not deltaTime

    private void Awake()
    {
        Instance = this;
    }

    // -------------------------------------------------------
    // Public API — call LoadAndPlay("my_song") from a menu
    // or wherever you want to start a song
    // -------------------------------------------------------
    public void LoadAndPlay(string chartFileName)
    {
        StartCoroutine(LoadChartAndStart(chartFileName));
    }

    IEnumerator LoadChartAndStart(string chartFileName)
    {
        // Load the JSON chart from StreamingAssets
        string chartPath = Path.Combine(Application.streamingAssetsPath, "charts", chartFileName + ".json");

        // UnityWebRequest works on all platforms including Android
        using (UnityWebRequest request = UnityWebRequest.Get(chartPath))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load chart: " + chartPath);
                yield break;
            }

            string json = request.downloadHandler.text;
            currentChart = JsonUtility.FromJson<ChartData>(json);
        }

        // Load the audio clip from Resources/Audio/
        ResourceRequest audioRequest = Resources.LoadAsync<AudioClip>("Audio/" + currentChart.audioFile);
        yield return audioRequest;

        AudioClip clip = audioRequest.asset as AudioClip;
        if (clip == null)
        {
            Debug.LogError("Failed to load audio: " + currentChart.audioFile +
                           ". Make sure it's in Resources/Audio/");
            yield break;
        }

        audioSource.clip = clip;

        // Clone the note list so we can dequeue from it during gameplay
        pendingNotes = new List<ChartNote>(currentChart.notes);

        // Sort by time just in case the JSON isn't ordered
        pendingNotes.Sort((a, b) => a.time.CompareTo(b.time));

        // Apply offset delay before starting audio
        if (currentChart.offset > 0f)
            yield return new WaitForSeconds(currentChart.offset);

        audioSource.Play();
        isPlaying = true;

        // Tell GameManager audio has started so it can sync
        GameManager.Instance.OnSongStart(audioSource);

        Debug.Log("Now playing: " + currentChart.songName);
    }

    // -------------------------------------------------------
    // Spawn loop — checks every frame if a note is due
    // -------------------------------------------------------
    private void Update()
    {
        if (!isPlaying || pendingNotes == null || pendingNotes.Count == 0)
            return;

        // Spawn any notes whose hit-time is within spawnLeadTime from now.
        // We check the front of the sorted list; once a note isn't ready yet,
        // nothing further in the list will be either.
        while (pendingNotes.Count > 0 && pendingNotes[0].time <= songTime + spawnLeadTime)
        {
            ChartNote chartNote = pendingNotes[0];
            pendingNotes.RemoveAt(0);

            KeyCode key = ParseKey(chartNote.key);
            // Pass the exact hit-time so NoteObject can calculate precise speed
            noteManager.SpawnChartNote(key, chartNote.time);
        }

        // Stop when all notes are done and audio finishes
        if (pendingNotes.Count == 0 && !audioSource.isPlaying)
        {
            isPlaying = false;
            GameManager.Instance.OnSongEnd();
        }
    }

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------
    private KeyCode ParseKey(string keyStr)
    {
        return keyStr.ToUpper() switch
        {
            "D" => KeyCode.D,
            "F" => KeyCode.F,
            "J" => KeyCode.J,
            "K" => KeyCode.K,
            _ => KeyCode.D
        };
    }

    public float GetSongTime() => songTime;
}