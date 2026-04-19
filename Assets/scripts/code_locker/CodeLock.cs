using UnityEngine;
using UnityEngine.Events;

public class CodeLock : MonoBehaviour
{
    public string correctCode = "1111"; // Правильный код
    public UnityEvent onCodeCorrect;   // Сюда в инспекторе перетащим дверь

    private string _currentInput = "";

    // Метод, который вызывают кнопки
    public void AddDigit(string digit)
    {
        if (_currentInput.Length >= 4) ResetCode(); // Сброс, если ввели лишнее

        _currentInput += digit;
        Debug.Log("Введено: " + _currentInput);

        if (_currentInput.Length == 4)
        {
            CheckCode();
        }
    }

    private void CheckCode()
    {
        if (_currentInput == correctCode)
        {
            Debug.Log("Код верен!");
            onCodeCorrect?.Invoke(); // Вызываем открытие двери
        }
        else
        {
            Debug.Log("Неверный код!");
            ResetCode();
        }
    }

    public void ResetCode()
    {
        _currentInput = "";
    }
}
