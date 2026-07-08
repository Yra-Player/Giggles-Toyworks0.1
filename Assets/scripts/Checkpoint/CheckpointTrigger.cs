using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Точка спавна игрока")]
    [Tooltip("Создайте внутри чекпоинта пустой объект (безопасное место на полу) и перетащите сюда")]
    public Transform spawnPoint;

    private bool _isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!_isTriggered && other.CompareTag("Player"))
        {
            if (spawnPoint != null)
            {
                // Сохраняем ТОЛЬКО координаты
                PlayerPrefs.SetFloat("CheckpointX", spawnPoint.position.x);
                PlayerPrefs.SetFloat("CheckpointY", spawnPoint.position.y);
                PlayerPrefs.SetFloat("CheckpointZ", spawnPoint.position.z);
                PlayerPrefs.Save();

                _isTriggered = true;
                Debug.Log($"[Checkpoint] Координаты сохранены в точке: {spawnPoint.position}");
            }
            else
            {
                Debug.LogError($"[Checkpoint] Ошибка: На объекте {gameObject.name} не назначен Spawn Point!");
            }
        }
    }
}
