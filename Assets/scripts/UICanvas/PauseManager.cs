using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject buttonContainer;
    [SerializeField] private GameObject imageLogo;
    [SerializeField] private RectTransform greenPanel;

    [Header("Animation Settings")]
    [SerializeField] private float animationSpeed = 5f;
    private float hiddenXPosition;
    private Vector2 targetPosition;
    private Coroutine animationCoroutine;

    [Header("Settings Panel Integration")]
    [SerializeField] private UI_SettingsPanel settingsPanel;

    [Header("Checkpoint Panel Integration")]
    [SerializeField] private UI_CheckpointPanel checkpointPanel; // Ссылка на наш скрипт панели чекпоинтов!

    [Header("Player References")]
    [SerializeField] private FirstPersonMovement movementScript;
    [SerializeField] private FirstPersonLook cameraLookScript;

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);

        if (greenPanel != null)
        {
            targetPosition = greenPanel.anchoredPosition;
            hiddenXPosition = -greenPanel.rect.width - 50f;
        }

        // Подписка на закрытие настроек
        if (settingsPanel != null)
        {
            settingsPanel.OnPanelClosed.RemoveAllListeners();
            settingsPanel.OnPanelClosed.AddListener(RestorePauseMenu);
        }

        // ПОДПИСКА НА ЧЕКПОИНТЫ: Ровно по аналогии с настройками!
        if (checkpointPanel != null)
        {
            checkpointPanel.gameObject.SetActive(false);
            checkpointPanel.OnPanelClosed.RemoveAllListeners();
            checkpointPanel.OnPanelClosed.AddListener(RestorePauseMenu);
        }
    }

    public void PauseGame()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.gameObject.SetActive(false);
        if (checkpointPanel != null) checkpointPanel.gameObject.SetActive(false);

        if (greenPanel != null)
        {
            greenPanel.gameObject.SetActive(true);
            if (buttonContainer != null) buttonContainer.SetActive(true);
            if (imageLogo != null) imageLogo.SetActive(true);

            greenPanel.anchoredPosition = new Vector2(hiddenXPosition, greenPanel.anchoredPosition.y);
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (movementScript != null) movementScript.enabled = false;
        if (cameraLookScript != null) cameraLookScript.enabled = false;

        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimateIn());
    }

    public void ResumeGame()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.gameObject.SetActive(false);
        if (checkpointPanel != null) checkpointPanel.gameObject.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (movementScript != null) movementScript.enabled = true;
        if (cameraLookScript != null) cameraLookScript.enabled = true;

        if (GameManager.Instance != null) GameManager.Instance.isGamePaused = false;
    }

    private IEnumerator AnimateIn()
    {
        while (Vector2.Distance(greenPanel.anchoredPosition, targetPosition) > 0.1f)
        {
            greenPanel.anchoredPosition = Vector2.Lerp(greenPanel.anchoredPosition, targetPosition, animationSpeed * Time.unscaledDeltaTime);
            yield return null;
        }
        greenPanel.anchoredPosition = targetPosition;
    }

    public void OpenSettingsFromPause()
    {
        ToggleMainPauseVisuals(false);

        if (settingsPanel != null)
        {
            settingsPanel.gameObject.SetActive(true);
            settingsPanel.Open();
        }
    }

    // МЕТОД ДЛЯ КНОПКИ "ЗАГРУЗИТЬ" В МЕНЮ ПАУЗЫ
    public void OpenCheckpointFromPause()
    {
        ToggleMainPauseVisuals(false);

        if (checkpointPanel != null)
        {
            checkpointPanel.Open(); // Открываем и запускаем генерацию списка
        }
    }

    public void RestorePauseMenu()
    {
        ToggleMainPauseVisuals(true);
        if (greenPanel != null) greenPanel.anchoredPosition = targetPosition;
    }

    private void ToggleMainPauseVisuals(bool isVisible)
    {
        if (buttonContainer != null) buttonContainer.SetActive(isVisible);
        if (imageLogo != null) imageLogo.SetActive(isVisible);
        if (greenPanel != null) greenPanel.gameObject.SetActive(isVisible);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuRemake");
    }
}
