using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    public static NoteManager Instance;

    public GameObject notePrefab;
    public Transform spawnPoint;
    public float noteSpeed = 5f;
    public List<NoteObject> activeNotes = new List<NoteObject>();

    // Random spawn settings (kept for testing/sandbox mode)
    public float minBurstInterval = 0.15f;
    public float maxBurstInterval = 0.3f;
    public float minCalmInterval = 0.8f;
    public float maxCalmInterval = 1.5f;
    public float speedPerMultiplier = 0.3f;
    public float minNoteDistance = 0.8f;
    public float minSpawnY = -2f;
    public float maxSpawnY = 2f;

    // Set to true when a chart is loaded; disables random spawning
    [HideInInspector] public bool chartMode = false;

    private float lastSpawnY = 0f;

    private readonly Dictionary<KeyCode, float> keyLanes = new Dictionary<KeyCode, float>
    {
        { KeyCode.D, -1.5f },
        { KeyCode.F, -0.5f },
        { KeyCode.J,  0.5f },
        { KeyCode.K,  1.5f }
    };

    private void Awake() => Instance = this;

    private void Start()
    {
        if (!chartMode)
            StartCoroutine(SpawnLoop());
    }

    public void SpawnChartNote(KeyCode key, float hitTime)
    {
        float spawnX = spawnPoint.position.x;
        float hitX = 0f; // hit zone is at X=0

        // Calculate exact speed so this note arrives at hitX precisely at hitTime.
        // distance / time_remaining = required speed
        float timeRemaining = hitTime - SongManager.Instance.GetSongTime();
        float distance = spawnX - hitX; // positive because spawnX > hitX

        // Guard against bad timing data
        if (timeRemaining <= 0f)
        {
            Debug.LogWarning($"Note at time {hitTime} is already late, skipping.");
            return;
        }

        float exactSpeed = distance / timeRemaining;

        float spawnY = keyLanes.ContainsKey(key) ? keyLanes[key] : 0f;
        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0);

        GameObject obj = Instantiate(notePrefab, spawnPos, Quaternion.identity);
        NoteObject note = obj.GetComponent<NoteObject>();

        NoteData data = new NoteData
        {
            time = hitTime,
            key = key,
            hit = false
        };

        note.data = data;
        note.speed = exactSpeed; // precise per-note speed
        activeNotes.Add(note);
    }

    // -------------------------------------------------------
    // Random spawn (original logic)
    // -------------------------------------------------------
    IEnumerator SpawnLoop()
    {
        while (true)
        {
            bool isBurst = Random.value > 0.5f;
            if (isBurst)
            {
                int burstCount = Random.Range(4, 8);
                for (int i = 0; i < burstCount; i++)
                {
                    SpawnRandomNote(GetRandomKey());
                    yield return new WaitForSeconds(Random.Range(minBurstInterval, maxBurstInterval));
                }
            }
            else
            {
                int calmCount = Random.Range(2, 3);
                for (int i = 0; i < calmCount; i++)
                {
                    SpawnRandomNote(GetRandomKey());
                    yield return new WaitForSeconds(Random.Range(minCalmInterval, maxCalmInterval));
                }
            }
            yield return new WaitForSeconds(Random.Range(0.3f, 0.7f));
        }
    }

    void SpawnRandomNote(KeyCode key)
    {
        foreach (var existing in activeNotes)
        {
            if (Mathf.Abs(existing.transform.position.x - spawnPoint.position.x) < minNoteDistance)
                return;
        }

        float step = Random.Range(0.3f, 1.2f);
        float direction = Random.value > 0.5f ? 1f : -1f;
        lastSpawnY = Mathf.Clamp(lastSpawnY + (step * direction), minSpawnY, maxSpawnY);

        Vector3 spawnPos = new Vector3(spawnPoint.position.x, lastSpawnY, 0);

        GameObject obj = Instantiate(notePrefab, spawnPos, Quaternion.identity);
        NoteObject note = obj.GetComponent<NoteObject>();

        NoteData data = new NoteData
        {
            time = GameManager.Instance.songTime,
            key = key,
            hit = false
        };

        note.data = data;
        note.speed = noteSpeed + (GameManager.Instance.multiplier * speedPerMultiplier);
        activeNotes.Add(note);
    }

    KeyCode GetRandomKey()
    {
        KeyCode[] keys = { KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K };
        return keys[Random.Range(0, keys.Length)];
    }

    public void RemoveNote(NoteObject note) => activeNotes.Remove(note);

    public NoteObject GetClosestNote(KeyCode key)
    {
        NoteObject closest = null;
        float minDist = float.MaxValue;

        foreach (var note in activeNotes)
        {
            if (note.data.key != key || note.data.hit) continue;
            float dist = Mathf.Abs(note.transform.position.x);
            if (dist < minDist) { minDist = dist; closest = note; }
        }

        return closest;
    }
}