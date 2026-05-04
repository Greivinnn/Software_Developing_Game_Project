using System.Collections.Generic;
using UnityEngine;

// Holds everything needed to spawn one note — all calculated upfront
public class PreProcessedNote
{
    public float spawnTime;  // when to instantiate the GameObject
    public float hitTime;    // when it should reach the hit zone
    public KeyCode key;        // which key the player must press
    public float speed;      // exact units/sec to arrive on time
    public float laneY;      // world Y position for the note's lane
}

public static class ChartPreProcessor
{
    // Lane Y positions matching NoteManager.keyLanes
    private static readonly Dictionary<KeyCode, float> keyLanes = new Dictionary<KeyCode, float>
    {
        { KeyCode.D, -1.5f },
        { KeyCode.F, -0.5f },
        { KeyCode.J,  0.5f },
        { KeyCode.K,  1.5f }
    };

    private static readonly KeyCode[] keys = { KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K };

    // Main entry point — call this after beat detection, before gameplay starts.
    // spawnX       = world X of your spawnPoint transform
    // hitX         = world X of your hit zone (0f)
    // spawnLeadTime = how many seconds before hitTime to spawn the note
    public static List<PreProcessedNote> Process(
        List<float> beatTimes,
        float spawnX,
        float hitX,
        float spawnLeadTime)
    {
        List<PreProcessedNote> result = new List<PreProcessedNote>();

        float distance = spawnX - hitX; // always positive

        // Pattern state — keeps track of what key came last so we don't
        // repeat the same key twice in a row, and alternate lanes nicely
        int lastKeyIndex = -1;

        for (int i = 0; i < beatTimes.Count; i++)
        {
            float hitTime = beatTimes[i];

            // Pick the next key — avoid repeating the same lane twice
            int keyIndex = PickKeyIndex(lastKeyIndex, i);
            lastKeyIndex = keyIndex;

            KeyCode key = keys[keyIndex];

            float spawnTime = hitTime - spawnLeadTime;
            float speed = distance / spawnLeadTime; // uniform since we control spawn time

            PreProcessedNote note = new PreProcessedNote
            {
                spawnTime = spawnTime,
                hitTime = hitTime,
                key = key,
                speed = speed,
                laneY = keyLanes[key]
            };

            result.Add(note);
        }

        Debug.Log($"ChartPreProcessor: prepared {result.Count} notes.");
        return result;
    }

    // Also accepts a ChartData (hand-authored JSON chart) and pre-processes that
    // instead of auto-detected beats — gives you the best of both worlds
    public static List<PreProcessedNote> ProcessFromChart(
        ChartData chart,
        float spawnX,
        float hitX,
        float spawnLeadTime)
    {
        List<PreProcessedNote> result = new List<PreProcessedNote>();

        float distance = spawnX - hitX;

        foreach (ChartNote cn in chart.notes)
        {
            KeyCode key = ParseKey(cn.key);

            PreProcessedNote note = new PreProcessedNote
            {
                spawnTime = cn.time - spawnLeadTime,
                hitTime = cn.time,
                key = key,
                speed = distance / spawnLeadTime,
                laneY = keyLanes.ContainsKey(key) ? keyLanes[key] : 0f
            };

            result.Add(note);
        }

        result.Sort((a, b) => a.spawnTime.CompareTo(b.spawnTime));

        Debug.Log($"ChartPreProcessor: prepared {result.Count} notes from chart '{chart.songName}'.");
        return result;
    }

    // Picks a key index that avoids the last one and loosely alternates sides
    private static int PickKeyIndex(int lastIndex, int step)
    {
        // Simple pattern: cycle through keys with slight variation
        int next = (lastIndex + 1 + (step % 2)) % keys.Length;
        if (next == lastIndex) next = (next + 1) % keys.Length;
        return next;
    }

    private static KeyCode ParseKey(string keyStr)
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
}