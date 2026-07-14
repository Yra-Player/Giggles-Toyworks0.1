using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplaySettingsManager : MonoBehaviour
{
    [Header("Компоненты UI Дисплея")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown screenModeDropdown; // Ссылка на ваш Dropdown режимов экрана
    [SerializeField] private Slider fpsSlider;
    [SerializeField] private TextMeshProUGUI fpsValueText;    // Ссылка на статичный заголовок Text_FPSTitle
    [SerializeField] private TMP_InputField fpsInputField;    // Ссылка на поле ввода InputField_FPSValue
    [SerializeField] private Toggle vsyncToggle;

    private Resolution[] resolutions;

    // Массив аппаратных режимов Unity, строго соответствующих вашему списку: FULLSCREEN, BORDERLESS, WINDOWED
    private readonly FullScreenMode[] screenModes = new FullScreenMode[]
    {
        FullScreenMode.ExclusiveFullScreen, // FULLSCREEN
        FullScreenMode.FullScreenWindow,    // BORDERLESS
        FullScreenMode.Windowed             // WINDOWED
    };

    void Start()
    {
        // Инициализируем режим экрана ДО разрешения, чтобы разрешение знало, в каком режиме запускаться
        InitScreenMode();
        InitResolution();
        InitFPS();
        InitVSync();
    }

    // --- 1. НАСТРОЙКА РАЗРЕШЕНИЯ ЭКРАНА ---
    private void InitResolution()
    {
        if (resolutionDropdown == null) return;

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            // Форматируем под капс и ретро-терминал: "1920 X 1080 @ 60HZ"
            string option = $"{resolutions[i].width} X {resolutions[i].height} @ {resolutions[i].refreshRateRatio.value:F0}HZ";
            options.Add(option.ToUpper());

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        // Загружаем сохраненный индекс разрешения или ставим текущий монитора
        int savedRes = PlayerPrefs.GetInt("Retro_ResIndex", currentResolutionIndex);
        if (savedRes >= resolutions.Length) savedRes = currentResolutionIndex;

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = savedRes;
        resolutionDropdown.RefreshShownValue();

        ApplyResolution(savedRes);
        resolutionDropdown.onValueChanged.AddListener(ApplyResolution);
    }

    private void ApplyResolution(int index)
    {
        if (index < 0 || index >= resolutions.Length) return;
        Resolution res = resolutions[index];

        // Получаем текущий сохраненный режим экрана, чтобы не сбрасывать его при смене разрешения
        int savedMode = PlayerPrefs.GetInt("Retro_ScreenMode", 0);
        FullScreenMode activeMode = screenModes[savedMode];

        // Меняем разрешение с учетом выбранного режима экрана!
        Screen.SetResolution(res.width, res.height, activeMode);

        Debug.Log($"[DISPLAY] Движок применил разрешение: {res.width}x{res.height} в режиме {activeMode}");

        PlayerPrefs.SetInt("Retro_ResIndex", index);
        PlayerPrefs.Save();
    }

    // --- 2. НАСТРОЙКА РЕЖИМА ЭКРАНА ---
    private void InitScreenMode()
    {
        if (screenModeDropdown == null) return;

        // Загружаем сохраненный режим (по умолчанию 0 — FULLSCREEN)
        int savedMode = PlayerPrefs.GetInt("Retro_ScreenMode", 0);
        screenModeDropdown.value = savedMode;
        screenModeDropdown.RefreshShownValue();

        ApplyScreenMode(savedMode);
        screenModeDropdown.onValueChanged.AddListener(ApplyScreenMode);
    }

    private void ApplyScreenMode(int index)
    {
        if (index < 0 || index >= screenModes.Length) return;
        FullScreenMode selectedMode = screenModes[index];

        // Применяем режим экрана аппаратно
        Screen.fullScreenMode = selectedMode;

        // Из-за особенностей Unity, при смене режима экрана стоит обновить и разрешение
        if (resolutions != null && resolutionDropdown != null)
        {
            int currentResIndex = resolutionDropdown.value;
            if (currentResIndex >= 0 && currentResIndex < resolutions.Length)
            {
                Resolution res = resolutions[currentResIndex];
                Screen.SetResolution(res.width, res.height, selectedMode);
            }
        }

        Debug.Log($"[DISPLAY] Движок переключил режим монитора на: {selectedMode}");

        PlayerPrefs.SetInt("Retro_ScreenMode", index);
        PlayerPrefs.Save();
    }

    // --- 3. НАСТРОЙКА ОГРАНИЧЕНИЯ FPS (ДВУСТОРОННЯЯ СВЯЗЬ) ---
    private void InitFPS()
    {
        if (fpsSlider == null || fpsInputField == null) return;

        // Загружаем сохраненный лимит FPS, по умолчанию 60
        int savedFPS = PlayerPrefs.GetInt("Retro_MaxFPS", 60);

        // Настраиваем ограничения слайдера под рамки интерфейса
        fpsSlider.minValue = 30;
        fpsSlider.maxValue = 300;
        fpsSlider.value = savedFPS;

        // Выводим стартовое значение исключительно в поле ввода, заголовок остается статичным
        fpsInputField.text = savedFPS.ToString();

        ApplyFPS(savedFPS);

        // Слушатель 1: Двигаем ползунок -> цифры в поле ввода плавно меняются в реальном времени
        fpsSlider.onValueChanged.AddListener((val) =>
        {
            int intVal = (int)val;
            fpsInputField.text = intVal.ToString();
            ApplyFPS(intVal);
        });

        // Слушатель 2: Пишем руками -> ползунок прыгает на место при нажатии Enter
        fpsInputField.onEndEdit.AddListener((text) =>
        {
            // Защита от пустого поля
            if (string.IsNullOrEmpty(text)) text = fpsSlider.minValue.ToString();

            if (int.TryParse(text, out int resultValue))
            {
                // Ограничиваем ввод рамками слайдера
                resultValue = Mathf.Clamp(resultValue, (int)fpsSlider.minValue, (int)fpsSlider.maxValue);

                fpsInputField.text = resultValue.ToString();
                fpsSlider.value = resultValue;
                ApplyFPS(resultValue);
            }
        });
    }

    private void ApplyFPS(int value)
    {
        // Задаем лимит кадров в секунду для Unity
        Application.targetFrameRate = value;

        Debug.Log($"[DISPLAY] Лимит FPS установлен: {value} (Внимание: если VSync включен, этот лимит игнорируется движком)");

        PlayerPrefs.SetInt("Retro_MaxFPS", value);
        PlayerPrefs.Save();
    }

    // --- 4. НАСТРОЙКА ВЕРТИКАЛЬНОЙ СИНХРОНИЗАЦИИ ---
    private void InitVSync()
    {
        if (vsyncToggle == null) return;

        // Загружаем сохраненное состояние (0 — выкл, 1 — вкл)
        bool savedVSync = PlayerPrefs.GetInt("Retro_VSync", 0) == 1;
        vsyncToggle.isOn = savedVSync;
        ApplyVSync(savedVSync);

        vsyncToggle.onValueChanged.AddListener(ApplyVSync);
    }

    private void ApplyVSync(bool isOn)
    {
        // 0 — выключен, 1 — включен (синхронизация с герцовкой монитора)
        QualitySettings.vSyncCount = isOn ? 1 : 0;

        Debug.Log($"[DISPLAY] Вертикальная синхронизация (VSync) изменена на: {(isOn ? "ВКЛ (1)" : "ВЫКЛ (0)")}");

        PlayerPrefs.SetInt("Retro_VSync", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }
}
