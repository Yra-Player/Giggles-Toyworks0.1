using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walk : MonoBehaviour
{
    public Transform Player;
    
    
    public float speedEnemy = 6.5f;
    public float speedRunEnemy = 14.5f;
    public float DistanceSeePlayer;

    //private bool isFollowing = false;
    private Rigidbody Rb;

    private FirstPersonMovement playerScript;
    

    void Start()
    {
        Rb = GetComponent<Rigidbody>();
        if (Player != null)
        {
            playerScript = Player.GetComponent<FirstPersonMovement>();
        }
    }

   void FixedUpdate()
    {
        if (Player == null) return;
        float distance = Vector3.Distance(transform.position, Player.position);
        if (distance < DistanceSeePlayer)
        {

            Vector3 derectional = Player.position - transform.position;
            derectional.y = 0;
            derectional.Normalize();
            transform.LookAt(transform.position + derectional);
            Rb.MovePosition(Rb.position + derectional * speedEnemy * Time.fixedDeltaTime);
        }

    }
}