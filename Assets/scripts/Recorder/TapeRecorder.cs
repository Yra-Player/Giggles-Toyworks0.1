using UnityEngine;

public class TapeRecorder : MonoBehaviour
{
    [Header("Настройка аудио")]
    [Tooltip("Аудио запись для магнитафона")]
    public AudioClip voiceLog;

    [Header("СОбытие для спуска")]
    public System.Action OnLeverActivated;

    private AudioSource audioSource;
    private bool isPlaying = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        // Настраиваем звук как объемный 3D хоррор-звук
        audioSource.spatialBlend = 1.0f;
        audioSource.playOnAwake = false;
    }

    // МЕТОД ДЛЯ ИГРОКА: Вызывается из PlayerInteraction при нажатии 'E' на магнитофон
    public void InsertTape()
    {
        if (isPlaying) return;

        // Сюда можно добавить твою проверку инвентаря: if (!inventory.HasTape()) return;

        isPlaying = true;
        Debug.Log($"<color=yellow>[МАГНИТОФОН] Кассета вставлена в {gameObject.name}. Проигрывание... </color>");

        // Включаем голос из динамиков
        if (voiceLog != null)
        {
            audioSource.clip = voiceLog;
            audioSource.Play();
        }

        // 💡 МАГИЯ СИНХРОНИЗАЦИИ: Кричим в эфир тот же сигнал, что и рычаг!
        OnLeverActivated?.Invoke();
    }

    public bool IsActivated() => isPlaying;
}
