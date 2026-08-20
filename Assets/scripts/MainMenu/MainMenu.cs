using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using TMPro; // ОБЯЗАТЕЛЬНО: Добавляем библиотеку для работы с TextMeshPro

public class MainMenu : MonoBehaviour
{
    [Header("Настройки сцен")]
    public string gameSceneName = "MainGame";
    public Button continueButton;

    [Header("Панели интерфейса")]
    [SerializeField] private GameObject buttonPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject gameTitle;

    [Header("Динамический список чекпоинтов")]
    [SerializeField] private GameObject checkpointPanel;
    [SerializeField] private GameObject slotsContainer;
    [SerializeField] private GameObject slotPrefab;

    [Header("Картинки локаций (По порядку их открытия в игре)")]
    [SerializeField] private Sprite[] locationSprites;

    [Header("Настройки Интро (MP4)")]
    [SerializeField] private GameObject videoScreen;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Button skipVideoButton;

    void Start()
    {
        if (buttonPanel != null) buttonPanel.SetActive(true);
        if (gameTitle != null) gameTitle.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (checkpointPanel != null) checkpointPanel.SetActive(false);
        if (videoScreen != null) videoScreen.SetActive(false);
        if (skipVideoButton != null) skipVideoButton.gameObject.SetActive(false);

        if (videoPlayer != null) videoPlayer.loopPointReached += OnVideoFinished;

        if (PlayerPrefs.GetInt("HasAnySave", 0) == 1)
        {
            if (continueButton != null) continueButton.interactable = true;
        }
        else
        {
            if (continueButton != null) continueButton.interactable = false;
        }
    }

    public void NewGame()
    {
        int totalSlots = PlayerPrefs.GetInt("MaxSavedSlotsCount", 20);
        for (int i = 1; i <= totalSlots; i++)
        {
            string p = "Slot_" + i + "_";
            PlayerPrefs.DeleteKey(p + "CheckpointX");
            PlayerPrefs.DeleteKey(p + "CheckpointY");
            PlayerPrefs.DeleteKey(p + "CheckpointZ");
            PlayerPrefs.DeleteKey(p + "LocationName");
            PlayerPrefs.DeleteKey(p + "SaveDate");
            PlayerPrefs.DeleteKey(p + "PlayTime");
            PlayerPrefs.DeleteKey(p + "Exists");
        }

        PlayerPrefs.DeleteKey("HasAnySave");
        PlayerPrefs.DeleteKey("LastSelectedSlot");
        PlayerPrefs.DeleteKey("SelectedSlotToLoad");
        PlayerPrefs.DeleteKey("MaxSavedSlotsCount");
        PlayerPrefs.Save();

        if (videoPlayer != null && videoScreen != null)
        {
            if (buttonPanel != null) buttonPanel.SetActive(false);
            if (gameTitle != null) gameTitle.SetActive(false);
            videoScreen.SetActive(true);
            if (skipVideoButton != null) skipVideoButton.gameObject.SetActive(true);
            videoPlayer.Play();
        }
        else
        {
            StartGameLoading();
        }
    }

    public void ContinueGame()
    {
        if (buttonPanel != null) buttonPanel.SetActive(false);
        if (gameTitle != null) gameTitle.SetActive(false);
        if (checkpointPanel != null) checkpointPanel.SetActive(true);

        BuildCheckpointList();
    }

    private void BuildCheckpointList()
    {
        foreach (Transform child in slotsContainer.transform)
        {
            Destroy(child.gameObject);
        }

        int lastSlot = PlayerPrefs.GetInt("LastSelectedSlot", 0);
        if (lastSlot == 0) return;

        if (lastSlot > PlayerPrefs.GetInt("MaxSavedSlotsCount", 0))
        {
            PlayerPrefs.SetInt("MaxSavedSlotsCount", lastSlot);
            PlayerPrefs.Save();
        }

        for (int i = lastSlot; i >= 1; i--)
        {
            string prefix = "Slot_" + i + "_";

            if (PlayerPrefs.GetInt(prefix + "Exists", 0) == 1)
            {
                GameObject newSlot = Instantiate(slotPrefab, slotsContainer.transform);

                // ИСПРАВЛЕНО: Теперь скрипт ищет компоненты по их точному типу и иерархии
                Image slotImage = newSlot.transform.Find("Image").GetComponent<Image>();
                TMP_Text slotText = newSlot.transform.Find("InfoText").GetComponent<TMP_Text>();

                string locName = PlayerPrefs.GetString(prefix + "LocationName", "Локация");
                string saveDate = PlayerPrefs.GetString(prefix + "SaveDate", "--.--.--");
                float playTimeSeconds = PlayerPrefs.GetFloat(prefix + "PlayTime", 0f);

                System.TimeSpan t = System.TimeSpan.FromSeconds(playTimeSeconds);
                string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);

                if (slotText != null)
                {
                    slotText.text = $"ЛОКАЦИЯ: {locName}\nДАТА: {saveDate}\nВРЕМЯ: {formattedTime}";
                }

                if (slotImage != null && locationSprites != null && (i - 1) < locationSprites.Length)
                {
                    slotImage.sprite = locationSprites[i - 1];
                }

                int indexForButton = i;
                Button btn = newSlot.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() => LoadSlotAndStart(indexForButton));
                }
            }
        }
    }

    private void LoadSlotAndStart(int slotIndex)
    {
        PlayerPrefs.SetInt("SelectedSlotToLoad", slotIndex);
        PlayerPrefs.Save();
        StartGameLoading();
    }

    public void CloseCheckpointPanel()
    {
        if (checkpointPanel != null) checkpointPanel.SetActive(false);
        if (buttonPanel != null) buttonPanel.SetActive(true);
        if (gameTitle != null) gameTitle.SetActive(true);
    }

    private void CheckTitle() { if (gameTitle != null) gameTitle.SetActive(true); }

    private void OnVideoFinished(VideoPlayer vp) => StartGameLoading();
    public void SkipVideo() { if (videoPlayer != null && videoPlayer.isPlaying) { videoPlayer.Stop(); StartGameLoading(); } }
    private void StartGameLoading() => SceneLoader.Load(gameSceneName);
    public void OpenSettings() { buttonPanel.SetActive(false); gameTitle.SetActive(false); settingsPanel.SetActive(true); }
    public void CloseSettings() { settingsPanel.SetActive(false); buttonPanel.SetActive(true); gameTitle.SetActive(true); }
    public void ExitGame() => Application.Quit();
}
