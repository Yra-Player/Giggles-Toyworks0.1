using UnityEngine;
using UnityEngine.Events;

public class CodeLock : MonoBehaviour 
{
    public string correctCode = "1111";
    public UnityEvent onCodeCorrect;

    private string _currentInput = "";
    private bool _isUnlocked = false;
    private float _lastInputTime; // Время последнего нажатия

    public void AddDigit(string digit) 
    {
        if (_isUnlocked) return; 

        // ЗАЩИТА: Если с момента последнего нажатия прошло меньше 0.2 сек — игнорируем
        if (Time.time - _lastInputTime < 0.2f) return;
        _lastInputTime = Time.time;

        // Теперь выводим сначала кнопку, потом результат
        Debug.Log("Нажата клавиша: " + digit);
        
        _currentInput += digit;
        Debug.Log("Введено: " + _currentInput);

        if (_currentInput.Length >= 4) 
        {
            if (_currentInput == correctCode) 
            {
                _isUnlocked = true;
                Debug.Log("КОД ВЕРЕН!");
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
