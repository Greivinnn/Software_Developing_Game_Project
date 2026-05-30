using UnityEngine;

public class NoteObject : MonoBehaviour
{
    public NoteData data;
    public float speed = 15f;
    public float hitPositionX = 0f;

    private float holdTimer = 0f;
    private bool holdCompleted = false;
    private LineRenderer holdLine;
    private float originalSpeed;
    private float holdNoteLineOffset = 0.19f;
    private Animator animator;

    public bool IsHoldNote => data.duration > 0f;

    public void Init()
    {
        hitPositionX = NoteManager.Instance.hitZone.transform.position.x; // read from scene
        originalSpeed = speed;
        animator = GetComponent<Animator>();
        if (IsHoldNote) DrawHoldLine();
    }


    private void DrawHoldLine()
    {
        GameObject lineObj = new GameObject("HoldLine");
        lineObj.transform.parent = transform;
        lineObj.transform.localPosition = Vector3.zero;

        holdLine = lineObj.AddComponent<LineRenderer>();
        holdLine.positionCount = 2;
        holdLine.useWorldSpace = false;

        float lineLength = originalSpeed * data.duration;

        holdLine.SetPosition(0, new Vector3(lineLength, 0f, 0f)); // extends left
        holdLine.SetPosition(1, new Vector3(holdNoteLineOffset, 0f, 0f));

        // line height thickness
        holdLine.startWidth = 0.15f;   
        holdLine.endWidth = 0.15f;

        holdLine.material = new Material(Shader.Find("Sprites/Default"));
        holdLine.startColor = Color.red;
        holdLine.endColor = Color.red;
        holdLine.sortingOrder = 0; 
    }

    private void Update()
    {
        if (!data.isBeingHeld)
            speed = GameManager.Instance.CurrentNoteSpeed;

        transform.position += Vector3.left * speed * Time.deltaTime;

        if (transform.position.x <= hitPositionX - 1f && !data.hit)
        {
            GameManager.Instance.MissNote();
            NoteManager.Instance.RemoveNote(this);
            Destroy(gameObject);
            return;
        }

        if (IsHoldNote && data.isBeingHeld)
        {
            holdTimer += Time.deltaTime;

            // Shrink line from left to right as the hold progresses
            if (holdLine != null)
            {
                float remaining = Mathf.Max(0f, data.duration - holdTimer);
                float remainingLength = originalSpeed * remaining;
                holdLine.SetPosition(0, new Vector3(remainingLength, 0f, 0f)); // shrinks from left
                holdLine.SetPosition(1, new Vector3(holdNoteLineOffset, 0f, 0f));
            }

            if (holdTimer >= data.duration && !holdCompleted)
            {
                holdCompleted = true;
                GameManager.Instance.HitNote();
                if (ChartGraph.Instance != null)
                    ChartGraph.Instance.AddPoint(
                        data.time,
                        data.key == KeyCode.D ? 3.5f :
                        data.key == KeyCode.F ? 1.5f :
                        data.key == KeyCode.J ? -1.5f : -3.5f,
                        data.key  // <-- pass the key so ChartGraph can use the Y overrides
                    );
                NoteManager.Instance.RemoveNote(this);
                Destroy(gameObject);
            }
        }
    }

    private void PlayHitAnimation()
    {
        if (animator == null) return;

        string trigger = data.key switch
        {
            KeyCode.D => "HitD",
            KeyCode.F => "HitF",
            KeyCode.J => "HitJ",
            KeyCode.K => "HitK",
            _ => null
        };

        if (trigger != null)
            animator.SetTrigger(trigger);
    }


    public void OnHit()
    {
        data.hit = true;
        PlayHitAnimation();

        if (IsHoldNote)
        {
            data.isBeingHeld = true;
            speed = 0f;
        }
        else
        {
            GameManager.Instance.HitNote();
            // add this line:
            if (ChartGraph.Instance != null) ChartGraph.Instance.AddPoint(data.time, transform.position.y); 
            NoteManager.Instance.RemoveNote(this);
            Destroy(gameObject, 0.4f);
        }
    }

    public void OnRelease()
    {
        if (!IsHoldNote || holdCompleted) return;

        data.isBeingHeld = false;

        if (holdTimer >= data.duration * 0.5f)
            GameManager.Instance.HitNote();
        else
            GameManager.Instance.MissNote();

        NoteManager.Instance.RemoveNote(this);
        Destroy(gameObject);
    }
}