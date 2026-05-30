using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject MenuUI;

    // Update is called once per frame
    void Update()
    {
        // if p key pressed
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            MenuUI.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Resume()
    {
        MenuUI.SetActive(false);
        Time.timeScale = 1;
    }

    public void Home()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
}
