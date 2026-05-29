using UnityEngine;
using System.Collections.Generic;

public class ChartGraph : MonoBehaviour
{
    public static ChartGraph Instance;

    public LineRenderer lineRenderer;
    public float graphX = -8f;         // left edge of graph
    public float graphWidth = 4f;      // total width of graph
    public float graphHeight = 2f;     // how tall the Y range is
    public float totalSongDuration = 112f;

    private List<Vector3> points = new List<Vector3>();

    private void Awake() => Instance = this;

    private void Start()
    {
        lineRenderer.positionCount = 0;
    }

    public void AddPoint(float hitTime, float laneY)
    {
        // spread points across graph using actual hit time
        float t = hitTime / totalSongDuration;
        float x = graphX + t * graphWidth;

        // scale laneY (-3.5 to 3.5) down to fit graph height
        float y = (laneY / 3.5f) * graphHeight;

        // offset graph to sit in the bottom left area
        y += -2f;

        points.Add(new Vector3(x, y, 0f));
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }
}