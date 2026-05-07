using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds all data needed to spawn one note — calculated upfront before gameplay.
/// SongManager pre-processes the entire chart into a list of these, then
/// Update() walks the list and spawns notes as their spawnTime arrives.
/// </summary>
public class PreProcessedNote
{
    public float spawnTime;  // when to instantiate the GameObject (hitTime - spawnLeadTime)
    public float hitTime;    // when the note should reach the hit zone (X=0)
    public KeyCode key;      // which key the player must press
    public float speed;      // exact units/sec so the note arrives at hitX on time
    public float laneY;      // world Y position for this note's lane
}

/// <summary>
/// Converts a ChartData JSON into a pre-sorted list of PreProcessedNotes.
/// All timing and speed calculations happen here, once, before the song starts.
/// </summary>
public static class ChartPreProcessor
{
    // Lane Y positions — must match NoteManager.keyLanes
    private static readonly Dictionary<KeyCode, float> keyLanes = new Dictionary<KeyCode, float>
    {
        { KeyCode.D, -1.5f },
        { KeyCode.F, -0.5f },
        { KeyCode.J,  0.5f },
        { KeyCode.K,  1.5f }
    };

    /// <summary>
    /// Converts a hand-authored ChartData (loaded from JSON) into a pre-processed
    /// note list ready for SongManager to execute.
    /// </summary>
    /// <param name="chart">The deserialized chart JSON.</param>
    /// <param name="spawnX">World X of the note spawn point.</param>
    /// <param name="hitX">World X of the hit zone (typically 0).</param>
    /// <param name="spawnLeadTime">Seconds before hitTime to spawn the note.</param>
    public static List<PreProcessedNote> ProcessFromChart(
        ChartData chart,
        float spawnX,
        float hitX,
        float spawnLeadTime)
    {
        List<PreProcessedNote> result = new List<PreProcessedNote>(chart.notes.Count);

        float distance = spawnX - hitX; // travel distance, always positive
        float speed = distance / spawnLeadTime; // uniform speed: all notes travel the same distance in the same lead time

        foreach (ChartNote cn in chart.notes)
        {
            KeyCode key = ParseKey(cn.key);

            result.Add(new PreProcessedNote
            {
                spawnTime = cn.time - spawnLeadTime,
                hitTime = cn.time,
                key = key,
                speed = speed,
                laneY = keyLanes.TryGetValue(key, out float laneY) ? laneY : 0f
            });
        }

        // Sort by spawn time in case the JSON isn't in order
        result.Sort((a, b) => a.spawnTime.CompareTo(b.spawnTime));

        Debug.Log($"ChartPreProcessor: {result.Count} notes prepared from '{chart.songName}'.");
        return result;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Parses a key string from the chart JSON into a Unity KeyCode.
    /// Supports bare letters ("D") and full KeyCode names ("Alpha1").
    /// Defaults to D on failure so notes are never silently dropped.
    /// </summary>
    private static KeyCode ParseKey(string keyStr)
    {
        if (string.IsNullOrEmpty(keyStr))
        {
            Debug.LogWarning("ChartPreProcessor: Empty key string, defaulting to D.");
            return KeyCode.D;
        }

        if (System.Enum.TryParse(keyStr, ignoreCase: true, out KeyCode parsed))
            return parsed;

        Debug.LogWarning($"ChartPreProcessor: Unknown key '{keyStr}', defaulting to D.");
        return KeyCode.D;
    }
}