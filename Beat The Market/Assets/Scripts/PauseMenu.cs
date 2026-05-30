using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject MenuUI;
    private bool isPaused = false;

    void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    private void Pause()
    {
        isPaused = true;
        MenuUI.SetActive(true);
        Time.timeScale = 0f;

        // Stop audio exactly where it is
        if (SongManager.Instance != null && SongManager.Instance.audioSource != null)
            SongManager.Instance.audioSource.Pause();

        // Freeze note spawning
        if (NoteManager.Instance != null)
            NoteManager.Instance.enabled = false;

        // Freeze the chart line
        if (ChartGraph.Instance != null)
            ChartGraph.Instance.enabled = false;
    }

    public void Resume()
    {
        isPaused = false;
        MenuUI.SetActive(false);
        Time.timeScale = 1f;

        // Resume audio from where it stopped
        if (SongManager.Instance != null && SongManager.Instance.audioSource != null)
            SongManager.Instance.audioSource.UnPause();

        // Resume note spawning
        if (NoteManager.Instance != null)
            NoteManager.Instance.enabled = true;

        // Resume the chart line
        if (ChartGraph.Instance != null)
            ChartGraph.Instance.enabled = true;
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Home()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
}