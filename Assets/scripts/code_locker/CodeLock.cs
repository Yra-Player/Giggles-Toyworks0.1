using UnityEngine;
using UnityEngine.Events;

public class CodeLock : MonoBehaviour
{
    public string correctCode = "1111";
    public UnityEvent onCodeCorrect;

    [Header("Настройки сохранения (Уникальный ID для этой комнаты)")]
    public string lockUniqueID = "Room_GripClaws_Lock";

    private string _currentInput = "";
    private bool _isUnlocked = false;
    private float _lastInputTime;

    void Start()
    {
        // Проверяем: если этот замок уже открывали до перезапуска
        if (LevelStateManager.GetState(lockUniqueID) == 1)
        {
            _isUnlocked = true;
            Debug.Log($"<color=green>[ЗАМОК] {gameObject.name} уже был открыт ранее. Пропускаем игрока.</color>");

            // Находим дверь на сцене и открываем её МГНОВЕННО без анимаций
            Dla_Dveri door = GetComponentInParent<Dla_Dveri>();
            if (door == null) door = FindFirstObjectByType<Dla_Dveri>(); // Для Unity 6

            if (door != null)
            {
                door.InstantOpen(); // Мгновенное открытие на старте
            }

            onCodeCorrect?.Invoke(); // Активируем остальные события (свет, интерфейс), если нужно
        }
    }

    public void AddDigit(string digit)
    {
        if (_isUnlocked) return;

        if (Time.time - _lastInputTime < 0.2f) return;
        _lastInputTime = Time.time;

        _currentInput += digit;
        Debug.Log("Введено: " + _currentInput);

        if (_currentInput.Length >= 4)
        {
            if (_currentInput == correctCode)
            {
                _isUnlocked = true;
                Debug.Log("КОД ВЕРЕН!");

                // ЗАПИСЫВАЕМ В СЕЙВ: Код введен верно!
                LevelStateManager.SaveState(lockUniqueID, 1);

                onCodeCorrect?.Invoke();
            }
            else
            {
                Debug.Log("Неверный код! Сброс.");
                _currentInput = "";
            }
        }
    }
}
