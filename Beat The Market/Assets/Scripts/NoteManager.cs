using System.Collections.Generic;
using UnityEngine;

// <summary>
// Manages all active notes. Notes are spawned externally by SongManager via
// SpawnPreProcessedNote().
// </summary>
public class NoteManager : MonoBehaviour
{
    public static NoteManager Instance;

    public GameObject notePrefab;
    public Transform spawnPoint;

    public List<NoteObject> activeNotes = new List<NoteObject>();

    // Set to true by SongManager before playback begins
    [HideInInspector] public bool chartMode = false;

    public Sprite spriteD;
    public Sprite spriteF;
    public Sprite spriteJ;
    public Sprite spriteK;

    private Dictionary<KeyCode, Sprite> noteSprites;

    private void Awake()
    {
        Instance = this;

        noteSprites = new Dictionary<KeyCode, Sprite>
        {
            { KeyCode.D, spriteD },
            { KeyCode.F, spriteF },
            { KeyCode.J, spriteJ },
            { KeyCode.K, spriteK }
        };
    }

    // -------------------------------------------------------------------------
    // Spawning
    // -------------------------------------------------------------------------

    // <summary>
    // Spawns a note using data pre-calculated by ChartPreProcessor.
    // Called by SongManager.Update() when a note's spawnTime arrives.
    // </summary>
    public void SpawnPreProcessedNote(PreProcessedNote n)
    {
        Vector3 spawnPos = new Vector3(spawnPoint.position.x, n.laneY, 0f);

        GameObject obj = Instantiate(notePrefab, spawnPos, Quaternion.identity);
        NoteObject note = obj.GetComponent<NoteObject>();

        note.data = new NoteData
        {
            time = n.hitTime,
            key = n.key,
            hit = false,
            duration = n.duration
        };
        note.speed = n.speed;
        note.Init();

        // Apply the correct sprite for this key
        if (noteSprites.TryGetValue(n.key, out Sprite sprite))
        {
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = sprite;
        }

        activeNotes.Add(note);
    }

    // -------------------------------------------------------------------------
    // Queries & Removal
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the closest unhit note for the given key to the hit zone (X=0).
    /// Used by InputManager to determine what the player is trying to hit.
    /// </summary>
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

    public void RemoveNote(NoteObject note) => activeNotes.Remove(note);

    // Returns a hold note that is currently being held for the given key
    public NoteObject GetActiveHoldNote(KeyCode key)
    {
        foreach (var note in activeNotes)
        {
            if (note.data.key == key && note.data.isBeingHeld)
                return note;
        }
        return null;
    }
}