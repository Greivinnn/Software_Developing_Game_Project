using System.Collections.Generic;
using Unity.VisualScripting;
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

    public AnimatorOverrideController overrideD;
    public AnimatorOverrideController overrideF;
    public AnimatorOverrideController overrideJ;
    public AnimatorOverrideController overrideK;

    public HitZone hitZone;

    private Dictionary<KeyCode, AnimatorOverrideController> noteControllers;

    private void Awake()
    {
        Instance = this;
        noteControllers = new Dictionary<KeyCode, AnimatorOverrideController>
    {
        { KeyCode.D, overrideD },
        { KeyCode.F, overrideF },
        { KeyCode.J, overrideJ },
        { KeyCode.K, overrideK }
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
        if (noteControllers.TryGetValue(n.key, out AnimatorOverrideController controller))
        {
            Animator anim = obj.GetComponent<Animator>();
            if (anim != null) anim.runtimeAnimatorController = controller;
        }

        activeNotes.Add(note);
    }

    public NoteObject GetClosestNote(KeyCode key)
    {
        NoteObject closest = null;
        float minDist = float.MaxValue;
        float hitX = hitZone.transform.position.x;

        foreach (var note in activeNotes)
        {
            if (note.data.key != key || note.data.hit) continue;
            float dist = Mathf.Abs(note.transform.position.x - hitX);
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