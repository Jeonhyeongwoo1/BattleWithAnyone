using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public interface IInteractableItem
{
    public string interactableType { get; }
    public void Play();
    public void Stop();
}

public class InteractableItemControl : MonoBehaviour
{
    public Transform itemCreatePoint => m_ItemCreatePoint;

    [SerializeField] Transform m_ItemCreatePoint;
    [SerializeField] Transform[] m_SpawnPoints;
    [SerializeField] AttackableItem[] m_AttackableItems;
    [SerializeField] ExplosiveItem[] m_ExplosiveItems;
    [SerializeField] Transform m_TeleportParent;

    List<HelpableItem> m_CreatedHelpableItem = new List<HelpableItem>();

    private readonly string healthItem = "Health";
    private readonly string speedItem = "Speed";
    private readonly string randomItem = "Random";
    private readonly int healthItemDropValue = 45;
    private readonly int speedItemDropValue = 35;
    private readonly int randomItemDropValue = 20;

    public void PlayInteractableItem()
    {

        int aCount = m_AttackableItems.Length;
        for (int i = 0; i < aCount; i++)
        {
            m_AttackableItems[i].Play();
        }

        int eCount = m_ExplosiveItems.Length;
        for (int i = 0; i < eCount; i++)
        {
            m_ExplosiveItems[i].Play();
        }
    }

    public void StopInteractableItem()
    {

        int aCount = m_AttackableItems.Length;
        for (int i = 0; i < aCount; i++)
        {
            m_AttackableItems[i].Stop();
        }

        int eCount = m_ExplosiveItems.Length;
        for (int i = 0; i < eCount; i++)
        {
            m_ExplosiveItems[i].Stop();
        }
    }

    public bool HasAttackableItem() => m_AttackableItems.Length > 0;
    public bool HasExplosiveItem() => m_ExplosiveItems.Length > 0;
    public bool HasHelpableItem() => m_CreatedHelpableItem.Count > 0;

    public void ConnectTeleport()
    {
        int count = m_TeleportParent.childCount;
        Teleport[] teleports = m_TeleportParent.GetComponentsInChildren<Teleport>();
        for (int i = 0; i < count; i++)
        {
            teleports[i].Play();
        }
    }

    public void DisconnectTeleport()
    {
        int count = m_TeleportParent.childCount;
        Teleport[] teleports = m_TeleportParent.GetComponentsInChildren<Teleport>();
        for (int i = 0; i < count; i++)
        {
            teleports[i].Stop();
        }
    }

    public void CreateHelpableItems()
    {
        if (!PhotonNetwork.IsMasterClient) { return; }

        int hValue = healthItemDropValue;
        int sValue = speedItemDropValue + healthItemDropValue;
        int rValue = randomItemDropValue + sValue;
        int max = healthItemDropValue + speedItemDropValue + randomItemDropValue;

        for (int i = 0; i < m_SpawnPoints.Length; i++)
        {
            int value = UnityEngine.Random.Range(0, max);
            GameObject item = null;
            string itemName = "";
            if (value <= hValue)
            {
                itemName = healthItem;
            }
            else if (value > hValue && value <= sValue)
            {
                itemName = speedItem;
            }
            else if (value > sValue && value <= rValue)
            {
                itemName = randomItem;
            }

            item = PhotonNetwork.Instantiate(XSettings.itemPath + itemName, m_SpawnPoints[i].position, m_SpawnPoints[i].rotation, 0);
            if (item.TryGetComponent<HelpableItem>(out var v))
            {
                m_CreatedHelpableItem.Add(v);
            }
        }
    }

    [ContextMenu(nameof(DestoryHelpableItems))]
    public void DestoryHelpableItems()
    {
        int count = m_CreatedHelpableItem.Count;
        for (int i = 0; i < count; i++)
        {
            Destroy(m_CreatedHelpableItem[i].gameObject);
        }
    }

    [ContextMenu(nameof(AllDestoryInteractableItems))]
    public void AllDestoryInteractableItems()
    {
        int count = m_ItemCreatePoint.childCount;
        Transform[] items = m_ItemCreatePoint.GetComponentsInChildren<Transform>();
        for (int i = 0; i < count; i++)
        {
            Destroy(items[i].gameObject);
        }
    }

    private void Awake()
    {
    }


}
