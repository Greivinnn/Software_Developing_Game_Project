using UnityEngine;
using UnityEngine.UI;

public class DangerBorder : MonoBehaviour
{
    public static DangerBorder Instance;

    [Header("Safe Zone")]
    public float safeZoneEndSeconds = 5f; // suppress border in last N seconds of song

    [Header("References")]
    public Image[] borderPanels;

    [Header("Danger Threshold")]
    public float dangerStartX = -5f;
    public float dangerEndX = -7.51f;

    [Header("Danger Blink")]
    public float blinkSpeedMin = 1f;    // slow at first entering danger
    public float blinkSpeedMax = 8f;    // fast when at the wall
    public float maxAlpha = 0.3f;

    [Header("Idle Blink (no hits yet)")]
    public float idleBlinkSpeedMin = 0.5f;  // slow at start of song
    public float idleBlinkSpeedMax = 4f;    // fast when time is almost up
    public float idleMaxAlpha = 0.3f;
    public float idleTimeLimit = 8f;        // should match GameOverInactivityTime

    // Accumulated phase trackers — prevent speed resets when blinkSpeed changes
    private float idlePhase = 0f;
    private float dangerPhase = 0f;

    private void Awake() => Instance = this;
    private void Start() => SetAlpha(0f);

    private void Update()
    {
        if (borderPanels == null || borderPanels.Length == 0) return;
        if (ChartGraph.Instance == null || GameManager.Instance == null) return;

        // Fade out and suppress border near end of song
        // Hard cutoff — no border in last N seconds
        float timeRemaining = SongManager.Instance.GetSongLength() - GameManager.Instance.songTime;
        if (timeRemaining <= safeZoneEndSeconds)
        {
            SetAlpha(0f);
            idlePhase = 0f;
            dangerPhase = 0f;
            return;
        }

        float lastHitX = ChartGraph.Instance.GetLastHitX();
        bool noHitsYet = ChartGraph.Instance.HasNoHits();

        if (noHitsYet)
        {
            // Speed ramps up as we approach the inactivity time limit (starts slow, ends fast)
            float idleProgress = Mathf.Clamp01(GameManager.Instance.songTime / idleTimeLimit);
            float speed = Mathf.Lerp(idleBlinkSpeedMin, idleBlinkSpeedMax, idleProgress);

            // Advance phase by current speed so acceleration is smooth with no phase jumps
            idlePhase += speed * Time.deltaTime;

            float blink = (Mathf.Sin(idlePhase * Mathf.PI * 2f) + 1f) * 0.5f;
            SetAlpha(blink * idleMaxAlpha * idleProgress);
        }
        else if (lastHitX >= dangerStartX)
        {
            // Safe — tick dangerPhase at max speed so re-entry is never at a stale phase
            dangerPhase += blinkSpeedMax * Time.deltaTime;
            SetAlpha(0f);
        }
        else
        {
            // intensity: 0 = just entered danger, 1 = at the wall
            float intensity = Mathf.Clamp01(Mathf.InverseLerp(dangerStartX, dangerEndX, lastHitX));

            // Phase advances at speed matching current position — no resets, no jumps
            float speed = Mathf.Lerp(blinkSpeedMin, blinkSpeedMax, intensity);
            dangerPhase += speed * Time.deltaTime;

            float blink = (Mathf.Sin(dangerPhase * Mathf.PI * 2f) + 1f) * 0.5f;
            SetAlpha(blink * maxAlpha);
        }
    }

    private void SetAlpha(float a)
    {
        foreach (var img in borderPanels)
        {
            if (img == null) continue;
            Color c = img.color;
            c.a = a;
            img.color = c;
        }
    }
}