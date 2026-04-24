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

    // Spawn timing
    public float minBurstInterval = 0.15f;
    public float maxBurstInterval = 0.3f;
    public float minCalmInterval = 0.8f;
    public float maxCalmInterval = 1.5f;

    // Difficulty scaling
    public float speedPerMultiplier = 0.3f;

    // minimum gap between notes to prevent overlapping
    public float minNoteDistance = 0.8f;

    // Y spawn range (top, middle, bottom)
    public float minSpawnY = -2f;
    public float maxSpawnY = 2f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            bool isBurst = Random.value > 0.5f;

            if (isBurst)
            {
                // fast burst phase: spawns 4-8 notes quickly
                int burstCount = Random.Range(4, 8);
                for (int i = 0; i < burstCount; i++)
                {
                    SpawnNote(GetRandomKey());
                    yield return new WaitForSeconds(Random.Range(minBurstInterval, maxBurstInterval));
                }
            }
            else
            {
                // calm phase: spawns 2-3 notes slowly
                int calmCount = Random.Range(2, 3);
                for (int i = 0; i < calmCount; i++)
                {
                    SpawnNote(GetRandomKey());
                    yield return new WaitForSeconds(Random.Range(minCalmInterval, maxCalmInterval));
                }
            }

            // brief pause between phases
            yield return new WaitForSeconds(Random.Range(0.3f, 0.7f));
        }
    }

    private float lastSpawnY = 0f; // tracks last Y position

    void SpawnNote(KeyCode key)
    {
        foreach (var existing in activeNotes)
        {
            if (Mathf.Abs(existing.transform.position.x - spawnPoint.position.x) < minNoteDistance)
                return;
        }

        // move up or down a small step from last position, like a chart line
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

    public void RemoveNote(NoteObject note)
    {
        activeNotes.Remove(note);
    }

    public NoteObject GetClosestNote(KeyCode key)
    {
        NoteObject closest = null;
        float minDistance = float.MaxValue;

        foreach (var note in activeNotes)
        {
            if (note.data.key != key || note.data.hit)
                continue;

            float dist = Mathf.Abs(note.transform.position.x);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = note;
            }
        }

        return closest;
    }
}