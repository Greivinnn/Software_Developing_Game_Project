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
    public float spawnLeadTime = 3f;

    [Header("Beat Detection")]
    // Higher = fewer beats detected (less sensitive)
    // Lower  = more beats detected (more sensitive)
    // Good range: 1.3 (very sensitive) to 2.0 (only strong hits)
    public float beatSensitivity = 1.5f;

    // If true: uses beat detection on the audio.
    // If false: uses the hand-authored notes in the JSON chart.
    public bool useBeatDetection = false;

    // --- private state ---
    private ChartData currentChart;
    private List<PreProcessedNote> noteQueue;   // fully pre-processed, sorted by spawnTime
    private int nextNoteIndex = 0;
    private bool isPlaying = false;

    private float songTime => audioSource.time;

    private void Awake() => Instance = this;

    private void Start()
    {
        LoadAndPlay("FirstLevelSong");
    }

    // -------------------------------------------------------
    // Public API
    // -------------------------------------------------------
    public void LoadAndPlay(string chartFileName)
    {
        StartCoroutine(LoadChartAndStart(chartFileName));
    }

    // -------------------------------------------------------
    // Loading + pre-processing (all happens before playback)
    // -------------------------------------------------------
    IEnumerator LoadChartAndStart(string chartFileName)
    {
        // 1. Load JSON chart
        string chartPath = Path.Combine(
            Application.streamingAssetsPath, "charts", chartFileName + ".json");

        using (UnityWebRequest request = UnityWebRequest.Get(chartPath))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load chart: " + chartPath);
                yield break;
            }

            currentChart = JsonUtility.FromJson<ChartData>(request.downloadHandler.text);
            Debug.Log("Chart loaded: " + currentChart.songName);
        }

        // 2. Load audio clip
        ResourceRequest audioRequest = Resources.LoadAsync<AudioClip>(
            "Audio/" + currentChart.audioFile);
        yield return audioRequest;

        AudioClip clip = audioRequest.asset as AudioClip;
        if (clip == null)
        {
            Debug.LogError("Failed to load audio: " + currentChart.audioFile);
            yield break;
        }

        audioSource.clip = clip;
        Debug.Log("Audio loaded: " + clip.name);

        // 3. Pre-process notes (this is the new step — happens before Play())
        float spawnX = noteManager.spawnPoint.position.x;
        float hitX = 0f;

        if (useBeatDetection)
        {
            Debug.Log("Analysing beats...");
            List<float> beats = null;
            yield return noteManager.StartCoroutine(
                BeatAnalyser.AnalyseAsync(clip, beatSensitivity, result => beats = result));
            noteQueue = ChartPreProcessor.Process(beats, spawnX, hitX, spawnLeadTime);
        }
        else
        {
            // Use the hand-authored notes from the JSON
            noteQueue = ChartPreProcessor.ProcessFromChart(
                currentChart, spawnX, hitX, spawnLeadTime);
        }

        nextNoteIndex = 0;

        // 4. Apply offset then start
        if (currentChart.offset > 0f)
            yield return new WaitForSeconds(currentChart.offset);

        audioSource.Play();
        isPlaying = true;

        GameManager.Instance.OnSongStart(audioSource);
        Debug.Log("Now playing: " + currentChart.songName);
    }

    // -------------------------------------------------------
    // Spawn loop — just executes the pre-built queue
    // -------------------------------------------------------
    private void Update()
    {
        if (!isPlaying || noteQueue == null) return;

        // Walk through the pre-processed list and spawn anything whose
        // spawnTime has arrived. Since the list is sorted we can stop early.
        while (nextNoteIndex < noteQueue.Count &&
               noteQueue[nextNoteIndex].spawnTime <= songTime)
        {
            PreProcessedNote n = noteQueue[nextNoteIndex];
            nextNoteIndex++;
            noteManager.SpawnPreProcessedNote(n);
        }

        // End of song
        if (nextNoteIndex >= noteQueue.Count && !audioSource.isPlaying)
        {
            isPlaying = false;
            GameManager.Instance.OnSongEnd();
        }
    }

    public float GetSongTime() => songTime;
}