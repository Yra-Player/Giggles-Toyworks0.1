using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangePosition : MonoBehaviour
{
    public GameObject Enemy; 
    public GameObject BlackVoid;
    public GameObject Activate;
    private bool WasActive = false;



    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !WasActive) 
        {
            BlackVoid.SetActive(true);
            Enemy.SetActive(true);
            Activate.SetActive(true);
            Debug.Log("Слышен странный звук");
            WasActive = true;
            Debug.Log("Скрипт больше не сработает");
        }
        if (Enemy == null)
        {
            Debug.Log("Где то чудище которое должно следовать за тобой?");
        }
    }
    
}
