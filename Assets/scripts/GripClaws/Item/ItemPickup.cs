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

    [Header("Защита пазла")]
    [Tooltip("Перетащи сюда рычаг. Игрок не сможет взять предмет, пока рычаг не опустит его!")]
    public LeftLever requiredLever;

    private GameObject _rootObject;
    private GameObject _handObject;
    private bool _canPickUp = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 💡 ПРОВЕРКА 1: Проверяем, нет ли у игрока уже этой руки в памяти
            if (itemType == PickupType.RightHandOnly && PlayerPrefs.GetInt("HasRightHand", 0) == 1)
            {
                Debug.Log("[Pickup] Игрок уже имеет Правую руку, подбор заблокирован.");
                return;
            }
            if (itemType == PickupType.LeftHandWithGrappack && PlayerPrefs.GetInt("HasLeftHand", 0) == 1)
            {
                Debug.Log("[Pickup] Игрок уже имеет Левую руку, подбор заблокирован.");
                return;
            }

            // 💡 ПРОВЕРКА 2: Защита от хитрецов. Если рычаг привязан, но ЕЩЕ НЕ активирован — брать нельзя!
            if (requiredLever != null && !requiredLever.IsActivated())
            {
                Debug.Log("[Pickup] Предмет слишком высоко! Сначала опусти рычаг.");
                return;
            }

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
        if (_rootObject == null || _handObject == null)
        {
            Debug.LogError($"[Pickup] Ошибка: Не найдены объекты на игроке!");
            return;
        }

        _rootObject.SetActive(true);
        _handObject.SetActive(true);

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
        Destroy(gameObject); // Уничтожаем предмет на крюке
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
