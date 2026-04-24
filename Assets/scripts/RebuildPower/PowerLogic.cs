using UnityEngine;

public class PowerLogic : MonoBehaviour
{
    public string outTag = "Out";      // Тег источника питания
    public ScannerPower scannerToFix;      // Ссылка на ваш скрипт сканера

    private bool _isHoldingPower = false;

    // Этот метод вызывается, когда рука касается "розетки" (Out)
    // Если на руке уже висит скрипт захвата, можно интегрировать туда
    private void OnTriggerEnter(Collider other)
    {
        // 1. Проверяем, коснулись ли мы источника (Out)
        if (other.CompareTag(outTag))
        {
            _isHoldingPower = true;
            Debug.Log("Энергия захвачена! Несите её к разъему In.");
        }

        // 2. Проверяем, вошли ли мы в зону In (этот объект)
        // Если у этого объекта тег "In", и мы несем энергию:
        if (gameObject.CompareTag("In") && _isHoldingPower)
        {
            if (scannerToFix != null)
            {
                scannerToFix.Restore();
                _isHoldingPower = false; // Энергия потрачена
            }
        }
    }
}
