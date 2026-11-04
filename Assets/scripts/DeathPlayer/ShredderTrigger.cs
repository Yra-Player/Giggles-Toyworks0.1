using System.Collections;
using UnityEngine;

public class ShredderTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(ShredderSequence(other.gameObject));
        }
    }

    private IEnumerator ShredderSequence(GameObject player)
    {
        if (player.TryGetComponent(out FirstPersonMovement move)) move.enabled = false;
        yield return new WaitForSeconds(1.0f); // 1 секунда паники перед смертью

        if (DeathScreenManager.Instance != null) DeathScreenManager.Instance.TriggerShredderDeath();
    }
}
