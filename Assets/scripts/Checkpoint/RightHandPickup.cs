using UnityEngine;

public class RightHandPickup : MonoBehaviour
{
    [Header("Ссылки из иерархии Игрока")]
    [Tooltip("Перетащите сюда саму ПРАВУЮ руку из иерархии вашего игрока")]
    public GameObject rightHandOnPlayer;

    [Header("Клавиша подбора")]
    public KeyCode pickupKey = KeyCode.E;

    private bool _canPickUp = false;

    // Работаем через физическое столкновение (без галочки Is Trigger)
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            _canPickUp = true;
            Debug.Log($"[Pickup] Игрок коснулся руки. Нажми [{pickupKey}], чтобы подобрать!");
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            _canPickUp = false;
        }
    }

    private void OnGUI()
    {
        if (_canPickUp && Event.current.type == EventType.KeyDown && Event.current.keyCode == pickupKey)
        {
            PickUpRightHand();
        }
    }

    private void PickUpRightHand()
    {
        if (rightHandOnPlayer != null)
        {
            rightHandOnPlayer.SetActive(true);

            PlayerPrefs.SetInt("HasRightHand", 1);
            PlayerPrefs.Save();

            Debug.Log("[Pickup] УСПЕХ! Правая рука активирована и сохранена!");
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("[Pickup] Ошибка: Не привязана правая рука игрока в инспекторе!");
        }
    }
}
