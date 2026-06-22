using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateTrigger : MonoBehaviour
{
    public GameObject TriggerSafe;
    private bool WasActive = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !WasActive)
        {
            TriggerSafe.SetActive(true);
            Debug.Log("Скрежет ворот");
            WasActive = true;
            Debug.Log("Скрипт больше не сработает");
        }
    }
}
