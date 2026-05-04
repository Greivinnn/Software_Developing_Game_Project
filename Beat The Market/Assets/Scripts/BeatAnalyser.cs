using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BeatAnalyser
{
    // Synchronous version (fine for short songs under ~1 min)
    public static List<float> Analyse(AudioClip clip, float sensitivity = 2.5f)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);
        return ProcessSamples(samples, clip.frequency, clip.channels, sensitivity);
    }

    // Coroutine version — spreads work across frames, no freeze on long songs
    // Usage: yield return StartCoroutine(
    //            BeatAnalyser.AnalyseAsync(clip, sensitivity, result => beats = result));
    public static IEnumerator AnalyseAsync(
        AudioClip clip,
        float sensitivity,
        System.Action<List<float>> onComplete)
    {
        Debug.Log("BeatAnalyser: starting async analysis...");

        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        int channels = clip.channels;
        int sampleRate = clip.frequency;
        int windowSize = Mathf.RoundToInt(sampleRate * 0.023f);
        int historySize = 43;

        Queue<float> energyHistory = new Queue<float>();
        float historySum = 0f;
        List<float> beats = new List<float>();

        int totalWindows = samples.Length / (windowSize * channels);

        // Process in batches so Unity doesn't freeze
        // 500 windows per frame ≈ ~11ms of audio per frame, very smooth
        int batchSize = 500;

        for (int w = 0; w < totalWindows; w++)
        {
            float energy = 0f;
            int start = w * windowSize * channels;

            for (int i = start; i < start + windowSize * channels && i < samples.Length; i++)
                energy += samples[i] * samples[i];

            energy = Mathf.Sqrt(energy / windowSize);

            float avgEnergy = energyHistory.Count > 0
                ? historySum / energyHistory.Count : 0f;

            if (energy > avgEnergy * sensitivity && energyHistory.Count >= historySize / 2)
                beats.Add((float)(w * windowSize) / sampleRate);

            energyHistory.Enqueue(energy);
            historySum += energy;

            if (energyHistory.Count > historySize)
                historySum -= energyHistory.Dequeue();

            // Yield every batch to keep the game responsive
            if (w % batchSize == 0)
            {
                // Optional: report progress
                float progress = (float)w / totalWindows * 100f;
                Debug.Log($"BeatAnalyser: {progress:F0}% complete...");
                yield return null;
            }
        }

        // Merge beats that are too close (< 80ms apart)
        List<float> merged = new List<float>();
        float minGap = 0.08f;

        foreach (float b in beats)
        {
            if (merged.Count == 0 || b - merged[merged.Count - 1] >= minGap)
                merged.Add(b);
        }

        Debug.Log($"BeatAnalyser: found {merged.Count} beats in '{clip.name}'");
        onComplete(merged);
    }

    private static List<float> ProcessSamples(
        float[] samples, int sampleRate, int channels, float sensitivity)
    {
        int windowSize = Mathf.RoundToInt(sampleRate * 0.023f);
        int historySize = 43;
        Queue<float> history = new Queue<float>();
        float historySum = 0f;
        List<float> beats = new List<float>();
        int total = samples.Length / (windowSize * channels);

        for (int w = 0; w < total; w++)
        {
            float energy = 0f;
            int start = w * windowSize * channels;

            for (int i = start; i < start + windowSize * channels && i < samples.Length; i++)
                energy += samples[i] * samples[i];

            energy = Mathf.Sqrt(energy / windowSize);
            float avg = history.Count > 0 ? historySum / history.Count : 0f;

            if (energy > avg * sensitivity && history.Count >= historySize / 2)
                beats.Add((float)(w * windowSize) / sampleRate);

            history.Enqueue(energy);
            historySum += energy;
            if (history.Count > historySize) historySum -= history.Dequeue();
        }

        List<float> merged = new List<float>();
        foreach (float b in beats)
            if (merged.Count == 0 || b - merged[merged.Count - 1] >= 0.08f)
                merged.Add(b);

        Debug.Log($"BeatAnalyser: found {merged.Count} beats.");
        return merged;
    }
}