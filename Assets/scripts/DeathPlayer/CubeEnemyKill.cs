using System.Collections;
using UnityEngine;

public class CubeEnemyKill : MonoBehaviour
{
    private bool standardKillTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !standardKillTriggered)
        {
            StartCoroutine(JumpscareSequence(other.gameObject));
        }
    }

    private IEnumerator JumpscareSequence(GameObject player)
    {
        standardKillTriggered = true;

        if (player.TryGetComponent(out FirstPersonMovement move)) move.enabled = false;
        FirstPersonLook look = player.GetComponentInChildren<FirstPersonLook>();
        if (look != null) look.enabled = false;

        Transform cameraTransform = Camera.main.transform;

        // Быстрый разворот камеры на куб
        float faceTimer = 0f;
        Vector3 targetDirection = (transform.position - cameraTransform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

        while (faceTimer < 0.15f)
        {
            faceTimer += Time.deltaTime;
            cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, targetRotation, faceTimer / 0.15f);
            yield return null;
        }

        // Рывок куба в лицо за 0.25 сек
        float attackTimer = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = cameraTransform.position + cameraTransform.forward * 0.4f;

        while (attackTimer < 0.25f)
        {
            attackTimer += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, endPos, attackTimer / 0.25f);
            yield return null;
        }

        if (DeathScreenManager.Instance != null) DeathScreenManager.Instance.TriggerDeath();
    }
}
