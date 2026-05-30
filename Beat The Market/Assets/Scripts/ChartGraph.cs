using UnityEngine;
using System.Collections.Generic;

public class ChartGraph : MonoBehaviour
{
    public static ChartGraph Instance;

    public LineRenderer lineRenderer;
    public float anchorX = 0f;
    public float graphX = -11.6f;
    public float graphWidth = 10f;
    public float graphHeight = 6f;
    public float totalSongDuration = 112f;
    public bool HasNoHits() => lastHitSongTime < 0f;

    [Header("Clip")]
    public float clipLeftX = -7f;

    [Header("Game Over")]
    public float gameOverX = -5f;
    public float gameOverInactivityTime = 8f;

    [Header("Lane Y Overrides")]
    public float laneY_D = 3.5f;
    public float laneY_F = 1.5f;
    public float laneY_J = -1.5f;
    public float laneY_K = -3.5f;

    private struct HitRecord { public float songTime; public float y; }
    private List<HitRecord> hits = new List<HitRecord>();
    private float lastHitSongTime = -1f;
    private bool gameOverTriggered = false;

    private void Awake() => Instance = this;

    private void Update()
    {
        if (GameManager.Instance == null || gameOverTriggered) return;

        float now = GameManager.Instance.songTime;

        // Keep lastHitSongTime current while any hold note is active
        if (NoteManager.Instance != null)
        {
            foreach (KeyCode key in new[] { KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K })
            {
                if (NoteManager.Instance.GetActiveHoldNote(key) != null)
                {
                    lastHitSongTime = now;
                    break;
                }
            }
        }

        float secondsVisible = graphWidth;

        // Cull hits too old to be on screen
        hits.RemoveAll(h => (now - h.songTime) > secondsVisible);

        // --- Game Over Check ---
        if (lastHitSongTime >= 0f)
        {
            float age = now - lastHitSongTime;
            float lastHitX = anchorX - (age / secondsVisible) * graphWidth;

            if (lastHitX < gameOverX)
            {
                TriggerGameOver();
                return;
            }
        }
        else if (now > gameOverInactivityTime)
        {
            TriggerGameOver();
            return;
        }

        // --- Rebuild line, skipping points left of clipLeftX ---
        var visiblePositions = new List<Vector3>();

        foreach (var h in hits)
        {
            float age = now - h.songTime;
            float x = anchorX - (age / secondsVisible) * graphWidth;

            if (x < clipLeftX) continue;

            visiblePositions.Add(new Vector3(x, h.y, 0f));
        }

        lineRenderer.positionCount = visiblePositions.Count;
        if (visiblePositions.Count > 0)
            lineRenderer.SetPositions(visiblePositions.ToArray());
    }

    public void AddPoint(float hitTime, float laneY, KeyCode key)
    {
        float y;
        if (key == KeyCode.D) y = laneY_D;
        else if (key == KeyCode.F) y = laneY_F;
        else if (key == KeyCode.J) y = laneY_J;
        else if (key == KeyCode.K) y = laneY_K;
        else y = (laneY / 3.5f) * (graphHeight * 0.5f);

        hits.Add(new HitRecord { songTime = hitTime, y = y });
        lastHitSongTime = hitTime;
    }

    public void AddPoint(float hitTime, float laneY) =>
        AddPoint(hitTime, laneY, KeyCode.None);

    public float GetLastHitX()
    {
        if (lastHitSongTime < 0f || GameManager.Instance == null)
            return anchorX;

        float now = GameManager.Instance.songTime;
        float age = now - lastHitSongTime;
        return anchorX - (age / graphWidth) * graphWidth;
    }

    private void TriggerGameOver()
    {
        if (gameOverTriggered) return;

        // Don't game over if player already hit the money target
        if (GameManager.Instance != null && GameManager.Instance.money >= GameManager.Instance.moneyTarget)
        {
            gameOverTriggered = true;
            GameManager.Instance.OnSongEnd(forceFail: false);
            return;
        }

        gameOverTriggered = true;
        lineRenderer.positionCount = 0;
        Debug.Log("ChartGraph: Game Over - line left the screen.");
        GameManager.Instance.OnSongEnd(forceFail: true);
    }
}