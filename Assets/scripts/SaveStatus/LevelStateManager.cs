using UnityEngine;

public class LevelStateManager : MonoBehaviour
{
    public static LevelStateManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Метод для сохранения состояния (0 - заперто/выключено, 1 - открыто/включено)
    public static void SaveState(string objectID, int state)
    {
        PlayerPrefs.SetInt("State_" + objectID, state);
        PlayerPrefs.Save();
        Debug.Log($"[LevelState] Сохранено: {objectID} -> {state}");
    }

    // Метод для получения состояния
    public static int GetState(string objectID)
    {
        return PlayerPrefs.GetInt("State_" + objectID, 0); // По умолчанию 0
    }

    // Вызывайте этот метод в MainMenu.cs при старте Новой Игры!
    public static void ResetAllStates()
    {
        // Удаляем все ключи состояний, которые начинаются с префикса State_
        // В PlayerPrefs встроенного метода очистки по маске нет, поэтому при Новой игре проще сделать DeleteAll,
        // либо сбрасывать конкретные ID, если DeleteAll использовать нельзя.
        PlayerPrefs.DeleteAll();
        Debug.Log("[LevelState] Все состояния уровня полностью сброшены для Новой игры.");
    }
}
