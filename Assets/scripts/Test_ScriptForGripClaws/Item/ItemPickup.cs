using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public string gripClawsObjectName = "BuilversionGripClaws";
    public KeyCode pickupKey = KeyCode.E;

    private GameObject _gripClawsToEnable;
    private bool _canPickUp = false;

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что в зону вошел именно игрок
        if (other.CompareTag("Player"))
        {
            // Если мы еще не нашли ссылку на греппак, ищем её один раз
            if (_gripClawsToEnable == null)
            {
                _gripClawsToEnable = FindChildRecursive(other.transform, gripClawsObjectName);
            }

            _canPickUp = true;
            Debug.Log("Нажми E, чтобы подобрать греппак");
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
        // Вместо Update проверяем ввод в методе событий или OnGUI 
        // Но для KeyDown в связке с триггером лучше использовать простую логику:
        if (_canPickUp && Event.current.type == EventType.KeyDown && Event.current.keyCode == pickupKey)
        {
            PickUp();
        }
    }

    private void PickUp()
    {
        if (_gripClawsToEnable != null)
        {
            _gripClawsToEnable.SetActive(true);
            Debug.Log("Греппак активирован!");
            Destroy(gameObject);
        }
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
