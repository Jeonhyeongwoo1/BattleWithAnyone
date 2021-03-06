using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

public class ExplosiveItem : MonoBehaviourPunCallbacks, IInteractableItem
{
    public string prefabName => gameObject.name;
    public string interactableType => nameof(ExplosiveItem);

    [SerializeField] PlayParticle m_Effect;
    [SerializeField] int m_Health = 50;
    [SerializeField] float m_Radius = 2;
    [SerializeField] LayerMask m_TargetLayer;
    [SerializeField] int m_Damage = 20;
    [SerializeField] int m_CurHealth;

    bool isInteractable = false;

    public void Play()
    {
        if(!transform.parent.gameObject.activeSelf)
        {
            transform.parent.gameObject.SetActive(true);
        }

        isInteractable = true;
    }

    public void Stop()
    {
        m_Effect.StopEffect();
        transform.parent.gameObject.SetActive(false);
        isInteractable = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!isInteractable) { return; }
        Transform target = other.transform;
        if (target.TryGetComponent<PhotonView>(out var view))
        {
            if (!view.IsMine) { return; }
        }

        if (target.CompareTag("Bullet") || target.CompareTag("Pellet"))
        {
            if (target.TryGetComponent<BulletBase>(out var bullet))
            {
                photonView.RPC(nameof(NotifyTookDamage), RpcTarget.All, bullet.damage);
            }
            else if (target.TryGetComponent<Pellet>(out var pellet))
            {
                photonView.RPC(nameof(NotifyTookDamage), RpcTarget.All, pellet.damage);
            }

        }
    }

    [PunRPC]
    void NotifyTookDamage(int value)
    {
        m_CurHealth -= value;

        if (m_CurHealth <= 0)
        {
            Explosion();
        }
    }

    void Explosion()
    {

        Collider[] players = Physics.OverlapSphere(transform.position, m_Radius, m_TargetLayer);
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].TryGetComponent<PlayerController>(out var player))
            {
                if (!player.photonView.IsMine) { return; }
                player.TakeDamange(m_Damage);
            }
        }

        if (m_Effect)
        {
            m_Effect.PlayEffect(() => Stop());
            gameObject.SetActive(false);
        }
        else
        {
            Stop();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, m_Radius);
    }

    public override void OnEnable()
    {
        m_CurHealth = m_Health;
    }

    public override void OnDisable()
    {
        m_CurHealth = 0;
    }
}
