using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Video;

public class DeathScreenManager : MonoBehaviour
{
    public static DeathScreenManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject deathCanvasPanel;
    [SerializeField] private TextMeshProUGUI mainQuoteText;
    [SerializeField] private TextMeshProUGUI subCommandText;
    [SerializeField] private Image flashAndFadeOverlay;

    [Header("Звуковые эффекты")]
    [SerializeField] private AudioClip textClickSound;

    [Header("Настройки замедления щелчков (Pitch)")]
    [Range(0.1f, 1.0f)]
    [SerializeField] private float targetMinPitch = 0.5f;
    [SerializeField] private float pitchSlowdownSpeed = 1.0f;

    [Header("Эффекты финала (Poppy Style)")]
    [SerializeField] private AudioClip beepSound;
    [SerializeField] private Color finalScreenColor = Color.white;

    [Header("Тайминги экрана")]
    [SerializeField] private float whiteFlashDuration = 0.65f;
    [SerializeField] private float delayBeforeFirstText = 0.35f;
    [SerializeField] private float delayBeforeSecondText = 1.0f;
    [SerializeField] private float textFadeSpeed = 1.5f;
    [SerializeField] private float delayBeforeRestart = 4.0f;

    


    [Header("Цитаты смерти")]
    [SerializeField]
    private string[] observerQuotes = new string[]
    {
        "ТЕБЕ ЕЩЁ РАНО!",
        "ДИРИЖЁР НЕ ЗАКОНЧИТ СВОИ...",
        "ТЫ НАМ НУЖЕН",
        "ТЫ ШЕСТЕРЁНКА, СОШЕДШАЯ С МЕСТА",
        "ЭТО НЕ ТВОЙ ФИНАЛ",
        "РАНО СКЛАДЫВАТЬ РУКИ",
        "ТВОЙ ФИНАЛ, МОЁ НАЧАЛО",
        "Фгув бпинг аяи спърсшяпг",
        "КАК Я БЕЗ ТЕБЯ?",
        "08.08.202????"
    };

    [Header("Постоянная фраза")]
    [SerializeField] private string fixedCommand = "ВСТАВАЙ";

    private bool isDying = false;

    // РАЗДЕЛЬНЫЕ КАНАЛЫ ЗВУКА:
    private AudioSource clickAudioSource; // Канал 1: Только для четких щелчков текста
    private AudioSource noiseAudioSource; // Канал 2: Только для фонового белого шума в ушах

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (deathCanvasPanel != null) deathCanvasPanel.SetActive(false);

        // Инициализируем два независимых источника звука
        clickAudioSource = gameObject.AddComponent<AudioSource>();
        clickAudioSource.playOnAwake = false;
        clickAudioSource.spatialBlend = 0f;

        noiseAudioSource = gameObject.AddComponent<AudioSource>();
        noiseAudioSource.playOnAwake = false;
        noiseAudioSource.spatialBlend = 0f;
    }

    private void StartDeathAudio()
    {
        if (noiseAudioSource != null && beepSound != null)
        {
            noiseAudioSource.pitch = 1.0f;
            noiseAudioSource.clip = beepSound;
            noiseAudioSource.volume = 0f; // Стартуем в абсолютной тишине в первый кадр вспышки
            noiseAudioSource.loop = true;
            noiseAudioSource.Play();
        }
    }

    private void DisablePlayerControls()
    {
        CheckpointManager checkpointManager = FindFirstObjectByType<CheckpointManager>();
        if (checkpointManager != null && checkpointManager.playerObject != null)
        {
            GameObject player = checkpointManager.playerObject;

            CharacterController cc = player.GetComponent<CharacterController>();
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (cc != null) cc.enabled = false;
            if (rb != null) rb.isKinematic = true;

            MonoBehaviour movementScript = player.GetComponent("FirstPersonMovement") as MonoBehaviour;
            MonoBehaviour lookScript = player.GetComponentInChildren<Camera>().GetComponent<MonoBehaviour>();
            if (lookScript == null) lookScript = player.GetComponent("FirstPersonLook") as MonoBehaviour;

            if (movementScript != null) movementScript.enabled = false;
            if (lookScript != null) lookScript.enabled = false;

            Debug.Log("[DeathScreenManager] Управление игрока заблокировано");
        }
    }

    public void TriggerDeath()
    {
        if (isDying) return;
        DisablePlayerControls();
        StartDeathAudio();
        StartCoroutine(DefaultDeathSequence());
    }

    private IEnumerator DefaultDeathSequence()
    {
        isDying = true;
        yield return StartCoroutine(InstantWhiteFlash());

        flashAndFadeOverlay.color = Color.black;
       

        StartCoroutine(SlowdownClicksPitchSequence());
        yield return StartCoroutine(ClickTextSequence());

        yield return new WaitForSeconds(delayBeforeRestart);
        RestartCurrentScene();
    }

    public void TriggerShredderDeath()
    {
        if (isDying) return;
        DisablePlayerControls();
        StartDeathAudio();
        StartCoroutine(ShredderDeathSequence());
    }

    private IEnumerator ShredderDeathSequence()
    {
        isDying = true;
        yield return StartCoroutine(InstantWhiteFlash());

        float timer = 0f;
        bool toggle = true;
        while (timer < 1.0f)
        {
            flashAndFadeOverlay.color = toggle ? Color.black : Color.white;
            toggle = !toggle;
            timer += 0.08f;
            yield return new WaitForSeconds(0.08f);
        }

        flashAndFadeOverlay.color = new Color(0.05f, 0.05f, 0.05f, 1f);
        

        StartCoroutine(SlowdownClicksPitchSequence());
        yield return StartCoroutine(ClickTextSequence());

        yield return new WaitForSeconds(delayBeforeRestart);
        RestartCurrentScene();
    }

    private IEnumerator InstantWhiteFlash()
    {
        deathCanvasPanel.SetActive(true);

        if (mainQuoteText != null) mainQuoteText.color = new Color(mainQuoteText.color.r, mainQuoteText.color.g, mainQuoteText.color.b, 0f);
        if (subCommandText != null) subCommandText.color = new Color(subCommandText.color.r, subCommandText.color.g, subCommandText.color.b, 0f);

        flashAndFadeOverlay.color = Color.white;
        yield return new WaitForSeconds(whiteFlashDuration);
    }

    // Замедляет только канал щелчков, не ломая фоновый шум
    private IEnumerator SlowdownClicksPitchSequence()
    {
        clickAudioSource.pitch = 1.0f;
        while (clickAudioSource.pitch > targetMinPitch)
        {
            clickAudioSource.pitch -= Time.deltaTime * pitchSlowdownSpeed;
            if (clickAudioSource.pitch < targetMinPitch) clickAudioSource.pitch = targetMinPitch;
            yield return null;
        }
    }

    private IEnumerator ClickTextSequence()
    {
        yield return new WaitForSeconds(delayBeforeFirstText);

        // --- ЩЕЛЧОК 1 ---
        if (mainQuoteText != null && observerQuotes.Length > 0)
        {
            mainQuoteText.text = $"\"{observerQuotes[Random.Range(0, observerQuotes.Length)]}\"";
            if (clickAudioSource != null && textClickSound != null)
                clickAudioSource.PlayOneShot(textClickSound);

            float alpha = 0f;
            while (alpha < 1f)
            {
                alpha += Time.deltaTime * textFadeSpeed;
                mainQuoteText.color = new Color(mainQuoteText.color.r, mainQuoteText.color.g, mainQuoteText.color.b, alpha);
                yield return null;
            }
        }

        yield return new WaitForSeconds(delayBeforeSecondText);

        // --- ЩЕЛЧОК 2 ---
        if (subCommandText != null)
        {
            subCommandText.text = $"\"{fixedCommand}\"";
            if (clickAudioSource != null && textClickSound != null)
                clickAudioSource.PlayOneShot(textClickSound);

            float alpha = 0f;
            while (alpha < 1f)
            {
                alpha += Time.deltaTime * textFadeSpeed;
                subCommandText.color = new Color(subCommandText.color.r, subCommandText.color.g, subCommandText.color.b, alpha);
                yield return null;
            }
        }

        

        float elapsed = 0f;
        while (elapsed < delayBeforeRestart)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / delayBeforeRestart;

            // Плавно возвращаем оверлей из черного в БЕЛЫЙ цвет поверх работающего видео
            flashAndFadeOverlay.color = Color.Lerp(Color.black, finalScreenColor, normalizedTime);




            if (noiseAudioSource != null)
            {
                // Честная волна синуса громкости: от 0 до 1 к середине и строго в 0 к рестарту!
                noiseAudioSource.volume = normalizedTime;

            }

            yield return null;
        }

        
    }

    private void RestartCurrentScene()
    {
        CheckpointManager checkpointManager = FindFirstObjectByType<CheckpointManager>();

        if (checkpointManager != null)
        {
            // 1. Запускаем бесшовную телепортацию игрока на сохраненный чекпоинт
            checkpointManager.LoadLastSave();

            // 2. Безопасно снимаем эффект контузии через SendMessage
            if (checkpointManager.playerObject != null)
            {
                checkpointManager.playerObject.SendMessage("ResetContusion", SendMessageOptions.DontRequireReceiver);
            }

            // 3. СБРОС СОСТОЯНИЯ ЭКРАНА СМЕРТИ (Вызовется только ПОСЛЕ успешного рестарта)
            ResetDeathScreenState();

            Debug.Log("[DeathScreenManager] Игрок успешно воскрешен. Управление восстановлено, каша в звуках зачищена.");
        }
        else
        {
            Debug.LogError("[DeathScreenManager] Критическая ошибка: Не найден CheckpointManager на сцене! Откат к жесткой перезагрузке.");
            Time.timeScale = 1f;
            isDying = false;
            if (noiseAudioSource != null) noiseAudioSource.Stop();
            if (clickAudioSource != null) clickAudioSource.pitch = 1.0f;

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    // Полный сброс всех элементов и жесткая остановка кассеты при спавне
    private void ResetDeathScreenState()
    {
        if (deathCanvasPanel != null) deathCanvasPanel.SetActive(false);

        if (mainQuoteText != null) mainQuoteText.color = new Color(mainQuoteText.color.r, mainQuoteText.color.g, mainQuoteText.color.b, 0f);
        if (subCommandText != null) subCommandText.color = new Color(subCommandText.color.r, subCommandText.color.g, subCommandText.color.b, 0f);
        if (flashAndFadeOverlay != null) flashAndFadeOverlay.color = new Color(0f, 0f, 0f, 0f);

        if (noiseAudioSource != null)
        {
            noiseAudioSource.Stop();
            noiseAudioSource.volume = 0f;
        }
        if (clickAudioSource != null)
        {
            clickAudioSource.Stop();
            clickAudioSource.pitch = 1.0f;
        }

        Time.timeScale = 1f;
        isDying = false;
    }

}
