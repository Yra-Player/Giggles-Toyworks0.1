using System;
using UnityEngine;

public static class EmergencyEventManager
{
    // Событие, вызываемое в момент поломки сканера и начала аварии
    public static event Action OnEmergencyStarted;

    public static void SendEmergencyStarted()
    {
        OnEmergencyStarted?.Invoke();
    }
}
