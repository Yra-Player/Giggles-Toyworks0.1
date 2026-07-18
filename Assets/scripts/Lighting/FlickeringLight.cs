using UnityEngine;
using System.Collections;

public class FlickeringLight : MonoBehaviour
{
    private Light mainLight;
    private float mainBaseIntensity;

    [Header("Синхронизация (Для потолка)")]
    [Tooltip("Перетащи сюда нижний Spot Light, который светит на потолок")]
    public Light ceilingSpotLight;
    private float ceilingBaseIntensity;

    [Header("Настройка мигания")]
    [Tooltip("Минимальная яркость (0 = полная темнота)")]
    public float minFlickerPercent = 0.0f;
    [Tooltip("Макс яркость мигания (1 = базовая яркость)")]
    public float maxFlickerPercent = 1.0f;
    [Tooltip("Пауза вспышек (в секундах)")]
    public float flickDelay = 0.05f;

    void Start()
    {
        mainLight = GetComponent<Light>();

        if (mainLight != null)
        {
            mainBaseIntensity = mainLight.intensity;
        }

        // Запоминаем базовую яркость нижнего спотлайта, если он привязан
        if (ceilingSpotLight != null)
        {
            ceilingBaseIntensity = ceilingSpotLight.intensity;
        }

        if (mainLight != null)
        {
            StartCoroutine(FlickerRoutine());
        }
    }

    private IEnumerator FlickerRoutine()
    {
        while (true)
        {
            // Генерируем ОДИН общий случайный коэффициент для обеих ламп
            float randomFactor = Random.Range(minFlickerPercent, maxFlickerPercent);

            // Мигаем верхней лампой
            if (mainLight != null)
            {
                mainLight.intensity = mainBaseIntensity * randomFactor;
            }

            // Синхронно мигаем нижним спотлайтом на потолке
            if (ceilingSpotLight != null)
            {
                // Если хочешь, чтобы в моменты темноты потолок чуть-чуть светился (остаточный свет),
                // можно вместо randomFactor написать Mathf.Max(0.1f, randomFactor)
                ceilingSpotLight.intensity = ceilingBaseIntensity * randomFactor;
            }

            yield return new WaitForSeconds(flickDelay);
        }
    }
}
