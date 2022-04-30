using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class InteractableItem : MonoBehaviourPunCallbacks
{
    public enum Type { Health, Speed, Random}

    [SerializeField] Type m_ItemType;
    [SerializeField] ParticleSystem m_Effect;
    [SerializeField] int value;
    [SerializeField] int duration;
    [SerializeField] int restartWaitTime;

    [PunRPC]
    void SetItemParent()
    {
        IModel model = Core.models.Get();
        if (model != null)
        {
            transform.SetParent(model.itemCreatePoint);
        }
    }

    [PunRPC]
    void NotifyStopEffect()
    {
        m_Effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        StartCoroutine(ReStartEffectWaitTime());
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerController>(out var player))
            {
                if (!player.photonView.IsMine) { return; }
                switch (m_ItemType)
                {
                    case Type.Health:
                        player.UpdateHealth(value);
                        break;
                    case Type.Speed:
                        player.IncreaseSpeedByItem(value, duration);
                        break;
                    case Type.Random:
                        int r = Random.Range(0, 100);
                        player.UpdateHealth(r <= 60 ? -value : value);
                        break;
                }

                photonView.RPC(nameof(NotifyStopEffect), RpcTarget.All);
            }
        }
    }

    private void Awake()
    {
        photonView.RPC(nameof(SetItemParent), RpcTarget.All);
    }

    IEnumerator ReStartEffectWaitTime()
    {
        float elapsed = 0;
        while (elapsed < restartWaitTime)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        m_Effect.Play();
    }
}
