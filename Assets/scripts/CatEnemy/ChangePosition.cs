using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangePosition : MonoBehaviour
{
    public Transform PositionKota;
    public Transform newPosition; 
    public GameObject BlackVoid;
    public GameObject TriggerWalk;
    

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            PositionKota.position = newPosition.position;
            PositionKota.LookAt(other.transform);
            BlackVoid.SetActive(true);
            TriggerWalk.SetActive(true);
            
            Debug.Log("Слышен странный звук");
        }

    }
    
}
