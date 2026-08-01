using UnityEngine;

public class ElectroRoomLightingController : MonoBehaviour
{
    [Header("Настройки освещения")]
    [SerializeField] private GameObject lightsRoot;

    // Ключ для сохранения в PlayerPrefs
    private const string SaveKey = "ElectroRoom_HasTriggered";
    private bool _hasTriggered = false;

    private void Awake()
    {
        // Загружаем сохраненное состояние (0 - false, 1 - true)
        _hasTriggered = PlayerPrefs.GetInt(SaveKey, 0) == 1;

        if (lightsRoot != null)
        {
            // Если событие уже срабатывало ранее, свет должен быть включен сразу
            lightsRoot.SetActive(_hasTriggered);
        }
    }

    private void OnEnable()
    {
        // Если событие уже было, подписка не нужна
        if (!_hasTriggered)
        {
            EmergencyEventManager.OnEmergencyStarted += EnableRoomPower;
        }
    }

    private void OnDisable()
    {
        EmergencyEventManager.OnEmergencyStarted -= EnableRoomPower;
    }

    private void EnableRoomPower()
    {
        if (_hasTriggered) return;

        _hasTriggered = true;

        // Сохраняем состояние в память устройства
        PlayerPrefs.SetInt(SaveKey, 1);
        PlayerPrefs.Save();

        if (lightsRoot != null)
        {
            lightsRoot.SetActive(true);
            Debug.Log("[ElectroRoom] Аварийное освещение активировано навсегда и сохранено.");
        }

        EmergencyEventManager.OnEmergencyStarted -= EnableRoomPower;
    }
}
