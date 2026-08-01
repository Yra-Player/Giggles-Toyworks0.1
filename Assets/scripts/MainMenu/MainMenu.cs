using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video; // Обязательно для работы с видеоплеером

public class MainMenu : MonoBehaviour
{
    [Header("Настройки сцен")]
    public string gameSceneName = "MainGame"; // Имя вашей главной сцены с игрой
    public Button continueButton;             // Ссылка на кнопку "ПРОДОЛЖИТЬ"

    [Header("Панели интерфейса")]
    [SerializeField] private GameObject buttonPanel;   // Панель с главными кнопками
    [SerializeField] private GameObject settingsPanel; // Панель настроек
    [SerializeField] private GameObject gameTitle;     // Объект с названием игры (Giggles ToyWorks)

    [Header("Настройки Интро (MP4)")]
    [SerializeField] private GameObject videoScreen;   // Объект Raw Image, где будет играть видео
    [SerializeField] private VideoPlayer videoPlayer;   // Компонент Video Player
    [SerializeField] private Button skipVideoButton;   // Кнопка "Пропустить" (если захотите её сделать)

    void Start()
    {
        // При старте включаем меню и выключаем всё лишнее
        if (buttonPanel != null) buttonPanel.SetActive(true);
        if (gameTitle != null) gameTitle.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (videoScreen != null) videoScreen.SetActive(false);
        if (skipVideoButton != null) skipVideoButton.gameObject.SetActive(false);

        // Подписываемся на событие автоматического окончания видеоролика
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }

        // Проверяем наличие сохранений для активации кнопки "ПРОДОЛЖИТЬ"
        if (PlayerPrefs.HasKey("CheckpointX"))
        {
            if (continueButton != null) continueButton.interactable = true;
        }
        else
        {
            if (continueButton != null) continueButton.interactable = false;
        }
    }

    // Этот метод должен быть привязан к кнопке "НОВАЯ ИГРА"
    public void NewGame()
    {
        // Очищаем старый прогресс и протезы рук (ваш оригинальный код)
        PlayerPrefs.DeleteKey("CheckpointX");
        PlayerPrefs.DeleteKey("CheckpointY");
        PlayerPrefs.DeleteKey("CheckpointZ");
        PlayerPrefs.SetInt("HasLeftHand", 0);
        PlayerPrefs.SetInt("HasRightHand", 0);
        PlayerPrefs.Save();

        // Проверяем, настроен ли видеоплеер в инспекторе
        if (videoPlayer != null && videoScreen != null)
        {
            // Скрываем интерфейс меню, чтобы он не накладывался на видео
            if (buttonPanel != null) buttonPanel.SetActive(false);
            if (gameTitle != null) gameTitle.SetActive(false);

            // Включаем экран с видео и кнопку пропуска
            videoScreen.SetActive(true);
            if (skipVideoButton != null) skipVideoButton.gameObject.SetActive(true);

            // Запускаем воспроизведение MP4
            videoPlayer.Play();
        }
        else
        {
            // Если видео забыли добавить, просто сразу переходим к загрузке
            StartGameLoading();
        }
    }

    // Этот метод должен быть привязан к кнопке "ПРОДОЛЖИТЬ"
    public void ContinueGame()
    {
        // При продолжении интро не нужно, сразу включаем экран загрузки
        StartGameLoading();
    }

    // Метод сработает автоматически, когда MP4 ролик досмотрится до самого конца
    private void OnVideoFinished(VideoPlayer vp)
    {
        StartGameLoading();
    }

    // Этот метод можно привязать к кнопке "Пропустить" поверх видео
    public void SkipVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
            StartGameLoading();
        }
    }

    // Переход на экран загрузки через наш невидимый класс-мост
    private void StartGameLoading()
    {
        SceneLoader.Load(gameSceneName);
    }

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
