using UnityEngine;

public class HitZone : MonoBehaviour
{
    public float lineHeight = 5f;
    public float lineWidth = 0.05f;
    public float hitWindow = 1.5f; // always match InputManager's hitWindow

    void Start()
    {
        DrawLine(transform.position.x, Color.yellow, 0.08f);        // exact hit line
        DrawLine(transform.position.x + hitWindow, Color.red, 0.03f); // right boundary
        DrawLine(transform.position.x - hitWindow, Color.red, 0.03f); // left boundary
    }

    void DrawLine(float x, Color color, float width)
    {
        GameObject obj = new GameObject("Line");
        obj.transform.parent = transform;

        LineRenderer lr = obj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, new Vector3(x, -lineHeight, 0));
        lr.SetPosition(1, new Vector3(x, lineHeight, 0));

        lr.startWidth = width;
        lr.endWidth = width;

        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.sortingOrder = 1;
    }
}
