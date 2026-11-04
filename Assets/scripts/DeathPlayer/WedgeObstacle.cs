using UnityEngine;

public class WedgeObstacle : MonoBehaviour
{
    [SerializeField] private float lethalSpeed = 22f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                if (playerRb.linearVelocity.magnitude >= lethalSpeed)
                {
                    if (DeathScreenManager.Instance != null) DeathScreenManager.Instance.TriggerDeath();
                }
            }
        }
    }
}
