using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class ItemSpawnManager : MonoBehaviour
{
    public Transform itemCreatePoint => m_ItemCreatePoint;

    [SerializeField] Transform m_ItemCreatePoint;
    [SerializeField] Transform[] m_SpawnPoints;
    [SerializeField] string m_HealthItem; //40
    [SerializeField] string m_SpeedItem; //40
    [SerializeField] string m_RandomItem; //20

    public void CreateItem(bool usePhotonNetwork)
    {
        if (!PhotonNetwork.IsMasterClient) { return; }
        
        for (int i = 0; i < m_SpawnPoints.Length; i++)
        {
            int value = UnityEngine.Random.Range(0, 100);
            GameObject item = null;
            string itemName = "";
            if (value <= 40)
            {
                itemName = m_HealthItem;
            }
            else if (value > 40 && value <= 80)
            {
                itemName = m_SpeedItem;
            }
            else if (value > 80 && value <= 100)
            {
                itemName = m_RandomItem;
            }

            if(usePhotonNetwork)
            {
                item = PhotonNetwork.Instantiate(XSettings.itemPath + itemName, m_SpawnPoints[i].position, m_SpawnPoints[i].rotation, 0);
            }
            else
            {
                //
            }
        }
    }

    public int GetItemCount() => m_ItemCreatePoint.childCount;
    public bool HasItem() => m_ItemCreatePoint.childCount > 0;

    public void SetActiveItems(bool active)
    {
        int count = m_ItemCreatePoint.childCount;
        for (int i = 0; i < count; i++)
        {
            m_ItemCreatePoint.GetChild(i).gameObject.SetActive(active);
        }
    }

    [ContextMenu("Destory")]
    public void DestoryItems()
    {
        int count = m_ItemCreatePoint.childCount;
        for(int i = 0; i < count; i++)
        {
            Destroy(m_ItemCreatePoint.GetChild(i).gameObject);
        }
    }

}
