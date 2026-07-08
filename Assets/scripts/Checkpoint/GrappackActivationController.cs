using UnityEngine;

public class GrappackActivationController : MonoBehaviour
{
    [Header("Ссылки на предметы")]
    [SerializeField] private GameObject grappackObject;
    [SerializeField] private GameObject leftHand;
    [SerializeField] private GameObject rightHand;


    private void Start()
    {
        InitializeGrappackState();
    }

    private void InitializeGrappackState()
    {
        //if have not save

        if (!PlayerPrefs.HasKey("CheckpointX"))
        {
            DeactivateFullGrappack();
            Debug.Log("[GameManager] Новая игра. Всё выключено.");
            return;
        }

        //if have any save

        int leftActive = PlayerPrefs.GetInt("HasLeftHand", 0); //0 default
        int rightActive = PlayerPrefs.GetInt("HasRightHand", 0);

        if (leftActive == 1 || rightActive == 1)
        {
            if(grappackObject  != null) grappackObject.SetActive(true);

            // Включаем только то, что было сохранено
            if (leftHand != null) leftHand.SetActive(leftActive == 1);
            if (rightHand != null) rightHand.SetActive(rightActive == 1);
            Debug.Log($"[GameManager] Загрузка рук из сейва. Левая: {leftActive}, Правая: {rightActive}");
        }

        else
        {
            // Сейв есть (например, в самой первой комнате), но рук еще нет по сюжету
            DeactivateFullGrappack();
            Debug.Log("[GameManager] Загрузка без экипировки. Греппак выключен.");
        }
    }


    private void DeactivateFullGrappack()
    {
        if (grappackObject != null) grappackObject.SetActive(false);
        if (leftHand != null) leftHand.SetActive(false);
        if (rightHand != null) rightHand.SetActive(false);
    }
}
