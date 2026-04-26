using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangePosition : MonoBehaviour
{
    public Transform PositionKota;
    public Transform newPosition; 

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            PositionKota.position = newPosition.position;
            PositionKota.LookAt(other.transform);
            Debug.Log("Слышен странный звук");
        }
    }
    
}
