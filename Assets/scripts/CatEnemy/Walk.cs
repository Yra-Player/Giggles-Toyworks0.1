using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walk : MonoBehaviour
{
    public GameObject player;
    public GameObject enemy;
    public float speedEnemy = 6.5f;
    public float speedRunEnemy = 14.5f;
    private bool isFollowing = false;
    private FirstPersonMovement playerScript;

    void Start()
    {
        if (player != null)
            playerScript = player.GetComponent<FirstPersonMovement>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isFollowing)
        {
            if (enemy != null)
            {
                isFollowing = true;
                StartCoroutine(FollowRoutine());
            }
            else
            {
                Debug.LogWarning("Забыл назначить врага");
            }
        }
    }

        IEnumerator FollowRoutine()
        {
            while (isFollowing)
            {
                // ? означает Тогда, а : означет Иначе
                float currentMonsterSpeed = (playerScript.speed > 7f) ? speedRunEnemy : speedEnemy;

            //.normalized превращает длину вектора в 1 чистое направление. Без этого враг летел
            Vector3 direction = (player.transform.position - transform.position).normalized;
                enemy.transform.position += direction * currentMonsterSpeed * Time.deltaTime;


                //enemy.transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));
                yield return null;
            }
        }
}