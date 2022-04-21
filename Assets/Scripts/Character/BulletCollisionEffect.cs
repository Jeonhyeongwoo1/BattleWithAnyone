using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BulletCollisionEffect : MonoBehaviour
{
    public enum Type { Destory, Despawn, Disable}

    public Type type { get => m_Type; set => m_Type = value; }

    [SerializeField] Type m_Type;
    [SerializeField] bool m_IsDestory = false;
    [SerializeField] ParticleSystem m_Effect;

    public void PlayEffect(Vector3 position, Quaternion rotation, UnityAction done = null)
    {
        transform.SetPositionAndRotation(position, rotation);
        gameObject.SetActive(true);
        StartCoroutine(PlayingCollisionEffect(done));
    }

    IEnumerator PlayingCollisionEffect(UnityAction done)
    {
        m_Effect.Play();
        while (m_Effect.isPlaying) { yield return null; }
        m_Effect.Stop();
        done?.Invoke();

        switch (m_Type)
        {
            case Type.Destory:
                Destroy(gameObject);
            break;
            case Type.Despawn:
                Core.poolManager.Despawn(nameof(BulletCollisionEffect), gameObject);
            break;
            case Type.Disable:
                gameObject.SetActive(false);
            break;
        }

    }

}
