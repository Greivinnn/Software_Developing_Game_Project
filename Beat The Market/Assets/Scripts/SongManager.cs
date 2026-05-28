using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

// <summary>
// Owns the full chart-load → pre-process → playback pipeline.
// 
// LEVEL SELECTION: Set SongManager.ChartToLoad from the menu/scene before
// this scene loads, example:
//     SongManager.ChartToLoad = "song2";
//     SceneManager.LoadScene("GameScene");
// 
// Charts live in:  Assets/StreamingAssets/charts/<name>.json
// Audio lives in:  Assets/Resources/Audio/<audioFile>.mp3  (or .ogg / .wav)
// </summary>
public class SongManager : MonoBehaviour
{
    public static SongManager Instance;

    // Set this from your level-select screen before loading the game scene
    public static string ChartToLoad = "FirstLevelSong";

    public NoteManager noteManager;
    public AudioSource audioSource;

    public float spawnLeadTime = 3f;
    public float noteSpeed = 15f;

    // --- private state ---
    private System.Collections.Generic.List<PreProcessedNote> noteQueue;
    private int nextNoteIndex = 0;
    private bool isPlaying = false;
    private float preRollTimer = 0f;
    private bool songStarted = false;

    public float GetSongTime() => audioSource.time;
    public bool IsPlaying() => isPlaying;

    private void Awake() => Instance = this;

    private void Start()
    {
        StartCoroutine(LoadChartAndStart(ChartToLoad));
    }

    // -------------------------------------------------------------------------
    // Loading + pre-processing (all happens before playback begins)
    // -------------------------------------------------------------------------

    IEnumerator LoadChartAndStart(string chartFileName)
    {
        // 1. Load JSON chart
        string chartPath = Path.Combine(
            Application.streamingAssetsPath, "charts", chartFileName + ".json");

        ChartData chart = null;

        using (UnityWebRequest request = UnityWebRequest.Get(chartPath))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"SongManager: Failed to load chart at '{chartPath}': {request.error}");
                yield break;
            }

            chart = JsonUtility.FromJson<ChartData>(request.downloadHandler.text);

            if (chart == null || chart.notes == null || chart.notes.Count == 0)
            {
                Debug.LogError("SongManager: Chart is empty or malformed.");
                yield break;
            }

            Debug.Log($"SongManager: Loaded chart '{chart.songName}' ({chart.notes.Count} notes).");
        }

        // 2. Load audio clip
        ResourceRequest audioRequest = Resources.LoadAsync<AudioClip>("Audio/" + chart.audioFile);
        yield return audioRequest;

        AudioClip clip = audioRequest.asset as AudioClip;
        if (clip == null)
        {
            Debug.LogError($"SongManager: Failed to load audio clip 'Audio/{chart.audioFile}'.");
            yield break;
        }

        audioSource.clip = clip;
        Debug.Log($"SongManager: Audio loaded — '{clip.name}'.");

        // 3. Pre-process all notes upfront
        float spawnX = noteManager.spawnPoint.position.x;
        float hitX = noteManager.hitZone.transform.position.x;
        noteQueue = ChartPreProcessor.ProcessFromChart(chart, spawnX, hitX, spawnLeadTime, noteSpeed);
        nextNoteIndex = 0;

        // 4. Start spawning notes immediately so they can travel to the hit zone
        noteManager.chartMode = true;
        isPlaying = true;

        // 5. Wait for notes to travel, THEN play the song
        yield return new WaitForSeconds(spawnLeadTime);

        // 6. Fine-tune offset if needed
        if (chart.offset > 0f)
            yield return new WaitForSeconds(chart.offset);

        // 7. Switch Update() to audio time and start the song
        songStarted = true;
        audioSource.Play();
        GameManager.Instance.OnSongStart(audioSource);
        Debug.Log($"SongManager: Now playing '{chart.songName}'.");
    }

    // -------------------------------------------------------------------------
    // Spawn loop — executes the pre-built queue each frame
    // -------------------------------------------------------------------------

    private void Update()
    {
        if (!isPlaying || noteQueue == null) return;

        float songTime;

        if (songStarted)
        {
            songTime = audioSource.time;
        }
        else
        {
            // Count up from -spawnLeadTime toward 0 before song starts
            preRollTimer += Time.deltaTime;
            songTime = preRollTimer - spawnLeadTime;
        }

        while (nextNoteIndex < noteQueue.Count &&
               noteQueue[nextNoteIndex].spawnTime <= songTime)
        {
            noteManager.SpawnPreProcessedNote(noteQueue[nextNoteIndex]);
            nextNoteIndex++;
        }

        if (nextNoteIndex >= noteQueue.Count &&
            audioSource.clip != null &&
            audioSource.time >= audioSource.clip.length - 0.05f)
        {
            isPlaying = false;
            GameManager.Instance.OnSongEnd();
        }
    }
}