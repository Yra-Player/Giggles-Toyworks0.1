using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public string gameSceneName = "MainGame";
    public Button continueButton;
    [SerializeField] private GameObject buttonPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject gameTitle;

    void Start()
    {
        if (buttonPanel != null) buttonPanel.SetActive(true);
        if (gameTitle != null) gameTitle.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Проверяем наличие сохранений для активации кнопки "ПРОДОЛЖИТЬ"
        if (PlayerPrefs.HasKey("CheckpointX"))
        { if (continueButton != null) continueButton.interactable = true; }
        else
        { if (continueButton != null) continueButton.interactable = false; }
    }

    public void NewGame()
    {
        // Очищаем старый прогресс и протезы рук
        PlayerPrefs.DeleteKey("CheckpointX");
        PlayerPrefs.DeleteKey("CheckpointY");
        PlayerPrefs.DeleteKey("CheckpointZ");
        PlayerPrefs.SetInt("HasLeftHand", 0);
        PlayerPrefs.SetInt("HasRightHand", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene(gameSceneName);
    }

    public void ContinueGame() => SceneManager.LoadScene(gameSceneName);

    public void OpenSettings()
    {
        if (buttonPanel != null) buttonPanel.SetActive(false);
        if (gameTitle != null) gameTitle.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (buttonPanel != null) buttonPanel.SetActive(true);
        if (gameTitle != null) gameTitle.SetActive(true);
    }

    public void ExitGame() => Application.Quit();
}
