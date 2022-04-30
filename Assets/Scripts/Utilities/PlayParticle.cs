using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayParticle : MonoBehaviour
{
    public UnityAction<Transform> hitCollision
    {
        private get => m_HitCollision;
        set => m_HitCollision = value;
    }

    [SerializeField] ParticleSystem m_Effect;
    [SerializeField] float m_Duration;
    UnityAction<Transform> m_HitCollision;

    public void PlayEffect(UnityAction done = null)
    {
        m_Effect.Play();
        StartCoroutine(PlayingEffect(done));
    }

    public void StopEffect(bool immediately)
    {
        if(immediately)
        {
            m_Effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        else
        {
            m_Effect.Stop();
        }
    }

    IEnumerator PlayingEffect(UnityAction done)
    {
        while (m_Effect.isPlaying) { yield return null; }
        m_Effect.Stop();
        done?.Invoke();
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Player"))
        {
            hitCollision?.Invoke(other.transform);
        }
    }

    private void Start()
    {
        if (m_Effect == null)
        {
            if (m_Effect.TryGetComponent<ParticleSystem>(out var particle))
            {
                m_Effect = particle;
                ParticleSystem.MainModule main = m_Effect.main;
                main.duration = m_Duration;
            }
        }
    }

}
