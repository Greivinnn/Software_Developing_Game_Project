using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject settingMenu;
    public void PlayGame()
    {
        SceneManager.LoadScene("01_Prototype");
    }

    public void Setting()
    {
        settingMenu.SetActive(true);
    }

    public void CloseSetting()
    {
        settingMenu.SetActive(false);
    }

    public void QuitGame()
    {
         Application.Quit();
         Debug.Log("Quit Game");
    }
}
