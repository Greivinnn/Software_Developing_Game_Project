using UnityEngine;

public class HitZone : MonoBehaviour
{
    public float lineYBottom = -2.5f;  
    public float lineYTop = 2.5f;      
    public float lineWidth = 0.05f;
    public float hitWindow = 1.5f;

    void Start()
    {
        DrawLine(transform.position.x, Color.yellow, 0.08f);
        DrawLine(transform.position.x + hitWindow, Color.red, 0.03f);
        DrawLine(transform.position.x - hitWindow, Color.red, 0.03f);
    }

    void DrawLine(float x, Color color, float width)
    {
        GameObject obj = new GameObject("Line");
        obj.transform.parent = transform;

        LineRenderer lr = obj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, new Vector3(x, lineYBottom, 0));
        lr.SetPosition(1, new Vector3(x, lineYTop, 0));

        lr.startWidth = width;
        lr.endWidth = width;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.sortingOrder = 1;
    }

    public void HideLines()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
    }
}