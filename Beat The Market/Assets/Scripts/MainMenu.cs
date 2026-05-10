using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    
    public void OpenWindow()
    {
        Debug.Log("Opening window...");
        LeanTween.moveY(gameObject, 200, 0.5f).setEase(LeanTweenType.easeOutBack);
        this.enabled = false; 
    }

    private void Update()
    {
        if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            OpenWindow();
        }
    }

    public void QuitWindow()
    {
        Application.Quit();
        Debug.Log("Quitting application...");
    }

    public void Options()
    {
        Debug.Log("Opening options...");
    }

    public void OpenVolume(GameObject theSlider)
    {
        bool isOpen = theSlider.activeSelf;

        if (isOpen)
        {
            theSlider.SetActive(false);
            return;
        } else
        {
            theSlider.SetActive(true);
        }
    }
}

