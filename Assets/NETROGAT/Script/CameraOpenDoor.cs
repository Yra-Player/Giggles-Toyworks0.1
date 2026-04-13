using UnityEngine;

namespace CameraDoorScript
{
    public class CameraOpenDoor : MonoBehaviour
    {
        public float DistanceOpen = 3f;
        public GameObject interactionText; // Лучше давать понятные имена

        void Update()
        {
            RaycastHit hit;
            // Пускаем луч из позиции камеры вперед
            if (Physics.Raycast(transform.position, transform.forward, out hit, DistanceOpen))
            {
                // Пытаемся получить компонент двери один раз за попадание луча
                var door = hit.transform.GetComponent<DoorScript.Door>();

                if (door != null)
                {
                    if (interactionText != null) interactionText.SetActive(true);

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        door.OpenDoor();
                    }
                }
                else
                {
                    HideText();
                }
            }
            else
            {
                HideText();
            }
        }

        private void HideText()
        {
            if (interactionText != null && interactionText.activeSelf)
            {
                interactionText.SetActive(false);
            }
        }
    }
}
