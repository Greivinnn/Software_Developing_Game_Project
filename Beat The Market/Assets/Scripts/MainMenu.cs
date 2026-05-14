using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    Animator sceneAnim;
    public void OpenWindow()
    {
        Debug.Log("Opening window...");

        RectTransform rect = GetComponent<RectTransform>();
        LeanTween.moveY(rect, 900f, 0.5f).setEase(LeanTweenType.easeInBack);

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

    public void PlayGame(string sceneName)
    {
        StartCoroutine(SceneLoad(sceneName));
    }

    IEnumerator SceneLoad(string sceneName)
    {
        sceneAnim.Play("Transition");
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(sceneName);
    }
}

