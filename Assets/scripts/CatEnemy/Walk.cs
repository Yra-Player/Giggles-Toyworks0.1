using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walk : MonoBehaviour
{
    public Transform Player;
    
    
    public float speedEnemy = 6.5f;
    public float speedRunEnemy = 14.5f;
    public float DistanceSeePlayer;

    private bool isFollowing = false;

    private FirstPersonMovement playerScript;
    

    void Start()
    {
        if (Player != null)
            playerScript = Player.GetComponent<FirstPersonMovement>();
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, Player.position);

        if (distance < DistanceSeePlayer && !isFollowing)
        {
            isFollowing = true;
            StartCoroutine(FollowRoutine());
        }
        

    }

        IEnumerator FollowRoutine()
        {
            while (isFollowing)
            {
                // ? означает Тогда, а : означет Иначе
                float currentMonsterSpeed = (playerScript.speed > 7f) ? speedRunEnemy : speedEnemy;

            //.normalized превращает длину вектора в 1 чистое направление. Без этого враг летел
            Vector3 direction = (Player.position - transform.position).normalized;

            direction.y = 0;
            transform.LookAt(transform.position + direction);

            transform.position += direction * currentMonsterSpeed * Time.deltaTime;


                
                yield return null;
            }
        }
}