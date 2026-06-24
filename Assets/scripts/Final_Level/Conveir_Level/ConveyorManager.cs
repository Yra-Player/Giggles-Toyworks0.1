using System.Collections.Generic;
using UnityEngine;

public class ConveyorManager : MonoBehaviour
{
    [Header("Список всех конвейеров на сцене")]
    public List<ConveyorBelt> conveyorBelts = new List<ConveyorBelt>();

    private bool isAnyConveyorTriggered = false;

    // Этот метод активирует все конвейеры из списка одновременно
    public void ActivateAllConveyors()
    {
        if (isAnyConveyorTriggered) return;
        isAnyConveyorTriggered = true;

        Debug.Log("Менеджер: Игрок коснулся конвейера! Запускаем всю систему...");

        foreach (ConveyorBelt belt in conveyorBelts)
        {
            if (belt != null)
            {
                belt.ActivateConveyor();
            }
        }
    }
}
