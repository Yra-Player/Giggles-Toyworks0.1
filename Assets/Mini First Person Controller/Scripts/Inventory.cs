using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<GameObject> staffKeys = new List<GameObject>(); // Ключи персонала
    public List<GameObject> marketKeys = new List<GameObject>(); // Ключи маркета

    public void AddKey(GameObject keyObject)
    {
        if (keyObject.CompareTag("KeyStaffOnly"))
        {
            staffKeys.Add(keyObject);
            Debug.Log("Подобран ключ персонала (Staff Only)");
        }
        else if (keyObject.CompareTag("KeyForMiniMarket"))
        {
            marketKeys.Add(keyObject);
            Debug.Log("Подобран ключ от Минимаркета");
        }

        Destroy(keyObject);
    }

    public bool HasStaffKey() => staffKeys.Count > 0;
    public bool HasMarketKey() => marketKeys.Count > 0;

    public void UseStaffKey() { if (HasStaffKey()) staffKeys.RemoveAt(0); }
    public void UseMarketKey() { if (HasMarketKey()) marketKeys.RemoveAt(0); }
}
