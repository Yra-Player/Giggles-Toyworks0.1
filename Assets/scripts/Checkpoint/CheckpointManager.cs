using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointManager : MonoBehaviour
{
    [Header("Настройки Игрока")]
    public Transform playerTransform; // Сюда в инспекторе перетащите Plaeyry_test

    void Start()
    {
        // Загрузка позиции игрока из памяти ПК
        if (PlayerPrefs.HasKey("CheckpointX"))
        {
            float x = PlayerPrefs.GetFloat("CheckpointX");
            float y = PlayerPrefs.GetFloat("CheckpointY");
            float z = PlayerPrefs.GetFloat("CheckpointZ");

            Vector3 savedPosition = new Vector3(x, y, z);

            // Временно отключаем физику, чтобы игрок не провалился под пол при спавне
            Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            playerTransform.position = savedPosition;

            if (rb != null) rb.isKinematic = false;

            Debug.Log("Позиция игрока успешно загружена из памяти: " + savedPosition);
        }
    }

    // Этот метод вызывается из CheckpointTrigger
    public static void SavePosition(Vector3 newPos)
    {
        PlayerPrefs.SetFloat("CheckpointX", newPos.x);
        PlayerPrefs.SetFloat("CheckpointY", newPos.y);
        PlayerPrefs.SetFloat("CheckpointZ", newPos.z);
        PlayerPrefs.Save();

        Debug.Log("Позиция игрока успешно сохранена на жесткий диск: " + newPos);
    }

    // Метод для перезапуска сцены (для триггера смерти)
    public void RestartScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }
}
