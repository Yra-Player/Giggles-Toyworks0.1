using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    private bool isTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        // Проверяем тег игрока и то, что триггер еще не срабатывал в этой сессии
        if (!isTriggered && other.CompareTag("Player"))
        {
            CheckpointManager.SavePosition(transform.position);
            isTriggered = true; // Блокируем триггер, чтобы он больше не сейвил при спавне игрока
        }
    }
}
