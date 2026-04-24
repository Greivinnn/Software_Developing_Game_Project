using UnityEngine;

public class InputManager : MonoBehaviour
{
    public NoteManager noteManager;
    public float hitWindow = 1.5f;
    void Update()
    {
        CheckKey(KeyCode.D);
        CheckKey(KeyCode.F);
        CheckKey(KeyCode.J);
        CheckKey(KeyCode.K);
    }

    void CheckKey(KeyCode key)
    {
        if (Input.GetKeyDown(key))
        {
            NoteObject note = noteManager.GetClosestNote(key);

            if (note != null)
            {
                float timing = Mathf.Abs(note.transform.position.x);

                if (timing < hitWindow) 
                {
                    note.OnHit();
                }
                else
                {
                    GameManager.Instance.MissNote();
                }
            }
        }
    }
}
