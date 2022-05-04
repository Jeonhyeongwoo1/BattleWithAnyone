using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AttackableItem : MonoBehaviour, IInteractableItem
{
    public string prefabName => gameObject.name;
    public string interactableType => nameof(AttackableItem);
    [SerializeField] PlayParticle m_Effect;
    [SerializeField] float m_RepeatTime;
    [SerializeField] int m_Damage;

    public void Play()
    {
        InvokeRepeating(nameof(PlayEffect), 1, m_RepeatTime);
    }

    public void Stop()
    {
        CancelInvoke(nameof(Play));
        m_Effect.StopEffect();
    }

    void PlayEffect()
    {
        m_Effect.PlayEffect();
    }

    void OnHitPlayer(Transform target)
    {
        if (target.TryGetComponent<PlayerController>(out var player))
        {
            if (!player.photonView.IsMine) { return; }
            player.TakeDamange(m_Damage);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        m_Effect.hitCollision = OnHitPlayer;
    }
}
