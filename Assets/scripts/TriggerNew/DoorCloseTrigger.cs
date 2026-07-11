using UnityEngine;

public class DoorCloseTrigger : MonoBehaviour
{
    public TestDoor door; // Перетащите сюда объект вашей двери из иерархии

    private void OnTriggerExit(Collider other)
    {
        // Проверяем, что через триггер прошёл именно игрок (по тегу)
        if (other.CompareTag("Player"))
        {
            door.StartClosing();

             
        }
    }
}
