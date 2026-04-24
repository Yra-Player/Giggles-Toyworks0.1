using UnityEngine;
using System.Collections;

public class PowerCable : MonoBehaviour
{
    public ScannerPower scannerToFix;    // Ссылка на сканер в сцене
    public Transform powerSource;   // Объект-розетка (Out)
    public LineRenderer cableLink;  // Линия, которая будет рисоваться

    private bool _isCircuitClosed = false;
    private Coroutine _cableRoutine;

    // Этот метод сработает, так как скрипт висит НА РУКЕ
    private void OnTriggerEnter(Collider other)
    {
        if (_isCircuitClosed) return;

        // Если рука коснулась розетки
        if (other.CompareTag("Out"))
        {
            if (_cableRoutine == null)
            {
                cableLink.enabled = true;
                _cableRoutine = StartCoroutine(TrackCableRoutine());
            }
        }

        // Если рука с проводом коснулась столба
        if (other.CompareTag("In") && _cableRoutine != null)
        {
            StopCoroutine(_cableRoutine);
            _cableRoutine = null;
            _isCircuitClosed = true;

            // Фиксируем провод между розеткой и столбом навсегда
            cableLink.SetPosition(0, powerSource.position);
            cableLink.SetPosition(1, other.transform.position);

            scannerToFix.Restore(); // Вызываем починку у сканера
        }
    }

    IEnumerator TrackCableRoutine()
    {
        while (true)
        {
            // Точка А всегда у розетки
            cableLink.SetPosition(0, powerSource.position);
            // Точка Б всегда следует за рукой (этим объектом)
            cableLink.SetPosition(1, transform.position);
            yield return null;
        }
    }
}
