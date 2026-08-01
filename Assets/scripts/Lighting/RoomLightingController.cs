using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomLightingController : MonoBehaviour
{
    [Header("Основной свет")]
    public Light[] mainLights;
    public float dimSpeed = 600f; // Установлено из инспектора: 600
    public float restoreSpeed = 300f; // Установлено из инспектора: 300

    [Header("Аварийный")]
    public Light redSpotlight;
    public float redIntensifySpeed = 300f; // Установлено из инспектора: 300
    public float targetRedIntensity = 300f; // Установлено из инспектора: 300

    // === НАСТРОЙКИ СОХРАНЕНИЯ ===
    private const string SaveKey = "RoomLighting_State";
    // ============================

    private List<float> _originalIntensities = new List<float>();
    private Coroutine _lightingCoroutine;

    private void Awake()
    {
        if (mainLights != null)
        {
            foreach (Light lt in mainLights)
            {
                if (lt != null)
                {
                    _originalIntensities.Add(lt.intensity);
                }
                else
                {
                    _originalIntensities.Add(0f);
                }
            }
        }

        // Загружаем состояние: 0 - Нормальный свет, 1 - Аварийный свет
        int savedState = PlayerPrefs.GetInt(SaveKey, 0);
        ApplyLoadedState(savedState);
    }

    private void ApplyLoadedState(int state)
    {
        if (state == 1)
        {
            for (int i = 0; i < mainLights.Length; i++)
            {
                if (mainLights[i] != null) mainLights[i].intensity = 0f;
            }

            if (redSpotlight != null)
            {
                redSpotlight.enabled = true;
                redSpotlight.intensity = targetRedIntensity;
            }
        }
        else
        {
            for (int i = 0; i < mainLights.Length; i++)
            {
                if (mainLights[i] != null) mainLights[i].intensity = _originalIntensities[i];
            }

            if (redSpotlight != null)
            {
                redSpotlight.intensity = 0f;
                redSpotlight.enabled = false;
            }
        }
    }

    public void TriggerPowerOutage()
    {
        EmergencyEventManager.SendEmergencyStarted();

        PlayerPrefs.SetInt(SaveKey, 1);
        PlayerPrefs.Save();

        if (_lightingCoroutine != null) StopCoroutine(_lightingCoroutine);
        _lightingCoroutine = StartCoroutine(PowerOutageRoutine());
    }

    public void TriggerPowerRestore()
    {
        PlayerPrefs.SetInt(SaveKey, 0);
        PlayerPrefs.Save();

        if (_lightingCoroutine != null) StopCoroutine(_lightingCoroutine);
        _lightingCoroutine = StartCoroutine(PowerRestoreRoutine());
    }

    private IEnumerator PowerOutageRoutine()
    {
        if (redSpotlight != null) redSpotlight.enabled = true;

        bool mainLightsStillDimming = true;
        bool redLightStillIntensifying = true;

        while (mainLightsStillDimming || redLightStillIntensifying)
        {
            mainLightsStillDimming = false;
            redLightStillIntensifying = false;

            for (int i = 0; i < mainLights.Length; i++)
            {
                if (mainLights[i] != null && mainLights[i].intensity > 0f)
                {
                    mainLights[i].intensity = Mathf.MoveTowards(mainLights[i].intensity, 0f, dimSpeed * Time.deltaTime);
                    if (mainLights[i].intensity > 0f) mainLightsStillDimming = true;
                }
            }

            if (redSpotlight != null)
            {
                if (redSpotlight.intensity < targetRedIntensity)
                {
                    redSpotlight.intensity = Mathf.MoveTowards(redSpotlight.intensity, targetRedIntensity, redIntensifySpeed * Time.deltaTime);
                    if (redSpotlight.intensity < targetRedIntensity) redLightStillIntensifying = true;
                }
            }

            yield return null;
        }

        Debug.Log("[Менеджер света] Хоррор-атмосфера успешно включена полностью!");
    }

    private IEnumerator PowerRestoreRoutine()
    {
        bool mainLightsStillRestoring = true;
        bool redLightStillDimming = true;

        while (mainLightsStillRestoring || redLightStillDimming)
        {
            mainLightsStillRestoring = false;
            redLightStillDimming = false;

            for (int i = 0; i < mainLights.Length; i++)
            {
                if (mainLights[i] != null)
                {
                    float targetIntensity = _originalIntensities[i];
                    if (mainLights[i].intensity < targetIntensity)
                    {
                        mainLights[i].intensity = Mathf.MoveTowards(mainLights[i].intensity, targetIntensity, restoreSpeed * Time.deltaTime);
                        if (mainLights[i].intensity < targetIntensity) mainLightsStillRestoring = true;
                    }
                }
            }

            if (redSpotlight != null && redSpotlight.intensity > 0f)
            {
                redSpotlight.intensity = Mathf.MoveTowards(redSpotlight.intensity, 0f, redIntensifySpeed * Time.deltaTime);
                if (redSpotlight.intensity > 0f) redLightStillDimming = true;
            }

            yield return null;
        }

        if (redSpotlight != null) redSpotlight.enabled = false;
        Debug.Log("[Менеджер света] Обычное освещение успешно восстановлено!");
    }
}
