using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Scenes")]
    public string levelToLoad;

    [Header("Windows")]
    public GameObject settingsWindow;
    
    public void StartGame()
    {
        if (!string.IsNullOrEmpty(levelToLoad))
            FadeScreen.LoadSceneWithFadeAsync(levelToLoad);
        else
            Debug.LogWarning("[MainMenu] levelToLoad non configuré.");
    }

    public void SettingsButton()
    {
        if (settingsWindow != null)
            settingsWindow.SetActive(true);
    }

    public void CloseSettingsWindow()
    {
        if (settingsWindow != null)
            settingsWindow.SetActive(false);
    }

    public void LoadCreditsScene()
    {
        FadeScreen.LoadSceneWithFadeAsync("Credits");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
