using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using TMPro;

public class UI_CheckpointPanel : MonoBehaviour
{
    [Header("Настройки сцены")]
    [SerializeField] private string gameSceneName = "MainGame"; 

    [Header("Ссылки на элементы (как в Главном Меню)")]
    [SerializeField] private GameObject slotsContainer; 
    [SerializeField] private GameObject slotPrefab;     
    [SerializeField] private Sprite[] locationSprites;
    [Header("Интерфейсные События")]
    public UnityEvent OnPanelClosed;

    public void Open()
    {
        gameObject.SetActive(true);
        BuildCheckpointList();
    }

    private void BuildCheckpointList()
    {
        if (slotsContainer == null) return;

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

        // 2. Точно такая же логика создания карточек, как в твоем Главном Меню
        for (int i = lastSlot; i >= 1; i--)
        {
            string prefix = "Slot_" + i + "_";

            if (PlayerPrefs.GetInt(prefix + "Exists", 0) == 1)
            {
                GameObject newSlot = Instantiate(slotPrefab, slotsContainer.transform);

                Transform imgTransform = newSlot.transform.Find("Image");
                Transform txtTransform = newSlot.transform.Find("InfoText");

                Image slotImage = imgTransform != null ? imgTransform.GetComponent<Image>() : null;
                TMP_Text slotText = txtTransform != null ? txtTransform.GetComponent<TMP_Text>() : null;

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
        Time.timeScale = 1f;

        PlayerPrefs.SetInt("SelectedSlotToLoad", slotIndex);
        PlayerPrefs.Save();

        SceneManager.LoadScene(gameSceneName);
    }
    public void Close()
    {
        gameObject.SetActive(false);
        OnPanelClosed?.Invoke();
    }
}
