using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("Game");
    }
    public void Exit()
    {
        Application.Quit();
        Debug.Log("Application is quitting.");
    }
    public void OpenGacha()
    {
        SceneManager.LoadScene("Gacha");
    }
    public void OpenCollection()
    {
        SceneManager.LoadScene("Collection");
    }
    public void OpenShop()
    {
        SceneManager.LoadScene("Shop");
    }
    public void Back()
    {
        SceneManager.LoadScene("MainMenu");
    }

}
