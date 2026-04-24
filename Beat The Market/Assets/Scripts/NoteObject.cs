using UnityEngine;

public class NoteObject : MonoBehaviour
{
    public NoteData data;
    public float speed = 5f;
    public float hitPositionX = 0f;

    private void Update()
    {
        // move the note object to the left
        transform.position += Vector3.left * speed * Time.deltaTime;
        // auto miss if passed hit zone
        if (transform.position.x <= hitPositionX - 1f && !data.hit)
        {
            GameManager.Instance.MissNote();
            NoteManager.Instance.RemoveNote(this);
            Destroy(gameObject);
        }
    }

    public void OnHit()
    {
        data.hit = true;
        GameManager.Instance.HitNote();
        NoteManager.Instance.RemoveNote(this);
        Destroy(gameObject);
    }
}

