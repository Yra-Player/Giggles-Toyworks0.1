using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public enum PickupType { LeftHandWithGrappack, RightHandOnly }

    [Header("Настройки предмета")]
    public PickupType itemType;

    [Tooltip("Имя корневого объекта Греппака на игроке (BuilversionGripClaws)")]
    public string rootObjectName = "BuilversionGripClaws";

    [Tooltip("Имя конкретной руки в иерархии персонажа, которую нужно включить")]
    public string handObjectName = "ИМЯ_РУКИ_В_ИЕРАРХИИ";

    public KeyCode pickupKey = KeyCode.E;

    private GameObject _rootObject;
    private GameObject _handObject;
    private bool _canPickUp = false;



    
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Находим корень Греппака
            if (_rootObject == null)
            {
                _rootObject = FindChildRecursive(other.transform, rootObjectName);
            }

            // Находим конкретную руку (левую или правую)
            if (_handObject == null)
            {
                _handObject = FindChildRecursive(other.transform, handObjectName);
            }

            _canPickUp = true;
            Debug.Log($"Нажми [{pickupKey}], чтобы подобрать предмет: {itemType}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _canPickUp = false;
        }
    }

    private void OnGUI()
    {
        if (_canPickUp && Event.current.type == EventType.KeyDown && Event.current.keyCode == pickupKey)
        {
            PickUp();
        }
    }

    private void PickUp()
    {
        // Проверяем, нашли ли мы корневой Греппак
        if (_rootObject == null)
        {
            Debug.LogError($"[Pickup] Ошибка: Не найден корневой объект '{rootObjectName}' на игроке!");
            return;
        }

        // Проверяем, нашли ли мы саму клешню
        if (_handObject == null)
        {
            Debug.LogError($"[Pickup] Ошибка: Внутри игрока не найден объект руки с именем '{handObjectName}'!");
            return;
        }

        // Активируем корень (актуально для первой подборки) и саму руку
        _rootObject.SetActive(true);
        _handObject.SetActive(true);

        // Записываем сохранение в зависимости от типа
        if (itemType == PickupType.LeftHandWithGrappack)
        {
            PlayerPrefs.SetInt("HasLeftHand", 1);
            Debug.Log("[Pickup] Успешно подобраны Греппак и Левая рука!");
        }
        else if (itemType == PickupType.RightHandOnly)
        {
            PlayerPrefs.SetInt("HasRightHand", 1);
            Debug.Log("[Pickup] Успешно подобрана Правая рука!");
        }

        PlayerPrefs.Save();
        Destroy(gameObject); // Уничтожаем предмет на полу
    }

    private GameObject FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name) return child.gameObject;
        }
        return null;
    }
}
