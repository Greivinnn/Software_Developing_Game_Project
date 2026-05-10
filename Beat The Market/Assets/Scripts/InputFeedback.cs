using UnityEngine;

public class InputFeedback : MonoBehaviour
{
    public SpriteRenderer feedbackSprite;
    public float displayDuration = 0.2f;

    private float timer = 0f;
    private bool isShowing = false;

    public void ShowHit()
    {
        feedbackSprite.color = Color.green;
        feedbackSprite.enabled = true;
        timer = displayDuration;
        isShowing = true;
    }

    public void ShowMiss()
    {
        feedbackSprite.color = Color.red;
        feedbackSprite.enabled = true;
        timer = displayDuration;
        isShowing = true;
    }

    private void Update()
    {
        if (!isShowing) return;
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            feedbackSprite.enabled = false;
            isShowing = false;
        }
    }
}