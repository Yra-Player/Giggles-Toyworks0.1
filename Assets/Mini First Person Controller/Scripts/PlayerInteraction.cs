using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Настройки луча")]
    public float interactDist = 4f;

    [Header("Настройки прицела")]
    public Image cursorDot;
    public Color normalColor = Color.white;
    public Color interactColor = Color.red;

    [Header("Настройки GripClaws (Правая рука)")]
    public GameObject rightArmShoulder;
    public GameObject rightHandModel;

    void Start()
    {
        if (rightArmShoulder != null) rightArmShoulder.SetActive(false);
        if (rightHandModel != null) rightHandModel.SetActive(false);

        StartCoroutine(InteractionRoutine());
    }

    IEnumerator InteractionRoutine()
    {
        while (true)
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            bool hitSomething = Physics.Raycast(ray, out hit, interactDist);

            LockButton button = null;
            ButtonSequence soundButton = null;
            DoorControl door = null;
            bool isPickableKey = false;
            bool isRightHandItem = false;

            if (hitSomething)
            {
                button = hit.collider.GetComponent<LockButton>();
                soundButton = hit.collider.GetComponent<ButtonSequence>();
                door = hit.collider.GetComponent<DoorControl>();

                if (hit.collider.CompareTag("KeyStaffOnly") || hit.collider.CompareTag("KeyForMiniMarket"))
                {
                    isPickableKey = true;
                }

                if (hit.collider.CompareTag("Item_Right_hand"))
                {
                    isRightHandItem = true;
                }
            }

            if (button != null || soundButton != null || door != null || isPickableKey || isRightHandItem)
            {
                if (cursorDot != null) cursorDot.color = interactColor;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    Inventory inventory = GetComponent<Inventory>();

                    if (isRightHandItem)
                    {
                        if (rightArmShoulder != null) rightArmShoulder.SetActive(true);
                        if (rightHandModel != null) rightHandModel.SetActive(true);

                        Destroy(hit.collider.gameObject);
                        Debug.Log("Правая рука подобрана и активирована!");
                    }
                    else if (isPickableKey)
                    {
                        if (inventory != null) inventory.AddKey(hit.collider.gameObject);
                    }
                    else if (door != null)
                    {
                        if (inventory != null) door.TryOpen(inventory);
                    }
                    else if (button != null)
                    {
                        button.PressButton();
                    }
                    else if (soundButton != null)
                    {
                        soundButton.OnButtonPress();
                    }
                }
            }
            else
            {
                if (cursorDot != null) cursorDot.color = normalColor;
            }

            yield return null;
        }
    }
}
