using UnityEngine;
using UnityEngine.Events;

public class ObjectStateSaver : MonoBehaviour
{
    [Header("Событие при загрузке сохраненного состояния")]
    [Tooltip("Что должно произойти, если этот объект уже был активирован до перезапуска?")]
    public UnityEvent OnAlreadyActivated;

    private string uniqueID;

    private void Start()
    {
        uniqueID = "Object_" + gameObject.name + "_" + transform.position.ToString();

        // Если объект уже был активирован ранее
        if (LevelStateManager.GetState(uniqueID) == 1)
        {
            // Вызываем привязанные функции (например: выключить триггер подбора, включить зеленую лампочку)
            OnAlreadyActivated?.Invoke();
        }
    }

    // Вызовите этот метод из вашего скрипта Сканера/Кнопки в момент успешной активации!
    public void MarkAsActivated()
    {
        LevelStateManager.SaveState(uniqueID, 1);
    }
}
