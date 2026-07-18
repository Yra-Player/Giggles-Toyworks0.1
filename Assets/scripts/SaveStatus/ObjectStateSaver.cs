using UnityEngine;
using UnityEngine.Events;

public class ObjectStateSaver : MonoBehaviour
{
    [Header("Событие при загрузке сохраненного состояния")]
    [Tooltip("Что должно произойти, если этот объект уже был активирован до перезапуска? (Например: открыть дверь, выключить кодовый замок)")]
    public UnityEvent OnAlreadyActivated;

    [Header("Системные настройки (Не трогать)")]
    [SerializeField][HideInInspector] private string uniqueID; // GUID сохраняется прямо в меш объекта

    private void OnValidate()
    {
        // Автоматически генерируем вечный ID прямо в редакторе Unity, если его еще нет
        if (string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = System.Guid.NewGuid().ToString();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    private void Start()
    {
        // Теперь ID железно стабилен и не зависит от позиции объекта на сцене
        if (LevelStateManager.GetState(uniqueID) == 1)
        {
            Debug.Log($"<color=green>[СОХРАНЕНИЕ] Объект {gameObject.name} успешно восстановлен из сейва!</color>");
            OnAlreadyActivated?.Invoke();
        }
    }

    // ВЫЗОВИ ЭТОТ МЕТОД из скрипта твоего цифрового замка в момент, когда код введен ПРАВИЛЬНО!
    public void MarkAsActivated()
    {
        LevelStateManager.SaveState(uniqueID, 1);
        Debug.Log($"<color=yellow>[СОХРАНЕНИЕ] Состояние объекта {gameObject.name} записано в память.</color>");
    }
}
