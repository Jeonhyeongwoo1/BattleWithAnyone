using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AttackableItem : MonoBehaviour
{

    [SerializeField] PlayParticle m_Effect;
    [SerializeField] float m_RepeatTime;
    [SerializeField] int m_Damage;

    public void PlayEffect()
    {
        m_Effect.PlayEffect();
    }

    public void StopEffect(bool immediately)
    {
        m_Effect.StopEffect(immediately);
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
        InvokeRepeating(nameof(PlayEffect), 1, m_RepeatTime);
    }
}
