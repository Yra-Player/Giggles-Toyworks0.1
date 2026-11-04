using System.Collections;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    [Header("Ссылка на игрока")]
    public GameObject playerObject;

    private float _timeOffset = 0f;
    private string prefix;

    private void Awake()
    {
        InitSlotPrefix();

        // Переносим проверку в Awake, чтобы сработать РАНЬШЕ старта скриптов движения
        if (PlayerPrefs.HasKey(prefix + "CheckpointX"))
        {
            _timeOffset = PlayerPrefs.GetFloat(prefix + "PlayTime", 0f);
            StartCoroutine(TeleportWithDelay());
        }
        else
        {
            Debug.Log("[CheckpointManager] Сохранений нет. Новая игра.");
        }
    }

    private void InitSlotPrefix()
    {
        int slotToLoad = PlayerPrefs.GetInt("SelectedSlotToLoad", PlayerPrefs.GetInt("LastSelectedSlot", 1));
        prefix = "Slot_" + slotToLoad + "_";
    }

    // 🔥 НОВЫЙ МЕТОД: Публичная функция для вызова из DeathScreenManager при возрождении
    public void LoadLastSave()
    {
        InitSlotPrefix(); //на всякий случай

        if (PlayerPrefs.HasKey(prefix + "CheckpointX"))
        {
            Debug.Log("[CheckpointManger] Загрузка чекпоинта после смерти");
            StartCoroutine(TeleportWithDelay());
        }
        else
        {
            Debug.LogWarning("[CheckpointManager] Попытка загрузки");
        }
    }

    //Возвращает общее время то что набежало раньше + сколкьо прошлов этой сессии
    public float GetCurrentPlayTime()
    {
        return _timeOffset + Time.timeSinceLevelLoad;
    }

    private IEnumerator TeleportWithDelay()
    {
        if (playerObject == null)
        {
            Debug.LogError("[CheckpointManager] Ошибка: Не привязана ссылка на Player Object!");
            yield break;
        }

        yield return new WaitForEndOfFrame();

        // Считываем координаты
        float x = PlayerPrefs.GetFloat(prefix + "CheckpointX");
        float y = PlayerPrefs.GetFloat(prefix +"CheckpointY");
        float z = PlayerPrefs.GetFloat(prefix + "CheckpointZ");
        Vector3 targetPosition = new Vector3(x, y, z);

        // Находим сам триггер чекпоинта на сцене, чтобы узнать, куда направлен его "перед"
        // (Ищем ближайший объект со скриптом Checkpoint к точке спавна)
        Quaternion targetRotation = Quaternion.identity;
        Checkpoint closestCheckpoint = null;
        float closestDistance = Mathf.Infinity;

        foreach (Checkpoint cp in FindObjectsByType<Checkpoint>(FindObjectsSortMode.None)) // [2026 Unity Safe Find]
        {
            float dist = Vector3.Distance(cp.transform.position, targetPosition);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestCheckpoint = cp;
            }
        }

        // Если нашли чекпоинт — берем его направление "вперед" и зануляем наклон по вертикали
        if (closestCheckpoint != null)
        {
            Vector3 forwardDirection = closestCheckpoint.transform.forward;
            forwardDirection.y = 0f; // Железно гасим взгляд в пол/потолок
            targetRotation = Quaternion.LookRotation(forwardDirection);
        }

        // Отключаем физику и компоненты движения
        CharacterController cc = playerObject.GetComponent<CharacterController>();
        Rigidbody rb = playerObject.GetComponent<Rigidbody>();
        MonoBehaviour movementScript = playerObject.GetComponent("FirstPersonMovement") as MonoBehaviour;
        MonoBehaviour lookScript = playerObject.GetComponentInChildren<Camera>().GetComponent<MonoBehaviour>();
        if (lookScript == null) lookScript = playerObject.GetComponent("FirstPersonLook") as MonoBehaviour;

        if (cc != null) cc.enabled = false;
        if (rb != null) rb.isKinematic = true;
        if (movementScript != null) movementScript.enabled = false;
        if (lookScript != null) lookScript.enabled = false;

        // Железная фиксация позиции и взгляда вперед
        for (int i = 0; i < 3; i++)
        {
            playerObject.transform.position = targetPosition;
            playerObject.transform.rotation = targetRotation; // Игрок смотрит строго вперед по направлению куба!

            Camera playerCam = playerObject.GetComponentInChildren<Camera>();
            if (playerCam != null)
            {
                playerCam.transform.localRotation = Quaternion.identity; // Камера смотрит ровно по горизонту
            }
            yield return null;
        }

        // Синхронизируем внутренние углы мыши с новым направлением
        if (lookScript != null)
        {
            lookScript.SendMessage("InitRotation", SendMessageOptions.DontRequireReceiver);

            lookScript.enabled = true;
        }

        if (cc != null) cc.enabled = true;
        if (rb != null) rb.isKinematic = false;
        if (movementScript != null) movementScript.enabled = true;

        Debug.Log("[CheckpointManager] Игрок успешно развернут строго вперед по направлению чекпоинта.");
    }

    // Оставляем старый метод для совместимости
    public static void SavePosition(Vector3 pos)
    {
        PlayerPrefs.SetFloat("CheckpointX", pos.x);
        PlayerPrefs.SetFloat("CheckpointY", pos.y);
        PlayerPrefs.SetFloat("CheckpointZ", pos.z);
        PlayerPrefs.Save();
    }
}
