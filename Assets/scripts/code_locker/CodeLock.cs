using UnityEngine;
using UnityEngine.Events;
using TMPro; // 1. Добавляем пространство имен для работы с текстом

public class CodeLock : MonoBehaviour
{
    public TextMeshProUGUI displayField; // 2. Ссылка на экранчик (перетащить в инспекторе)
    public string correctCode = "1111";
    public UnityEvent onCodeCorrect;

    private string _currentInput = "";
    private bool _isUnlocked = false;
    private float _lastInputTime;

    public void AddDigit(string digit)
    {
        if (_isUnlocked) return;

        if (Time.time - _lastInputTime < 0.2f) return;
        _lastInputTime = Time.time;

        _currentInput += digit;

        // 3. ОБНОВЛЯЕМ ЭКРАН после каждого нажатия
        if (displayField != null)
        {
            displayField.text = _currentInput;
        }

        if (_currentInput.Length >= 4)
        {
            if (_currentInput == correctCode)
            {
                _isUnlocked = true;
                displayField.text = "OPEN"; // Визуальный отклик
                onCodeCorrect?.Invoke();
            }
            else
            {
                _currentInput = "";
                // Можно добавить задержку перед очисткой или вывести "ERR"
                displayField.text = "";
            }
        }
    }
}
