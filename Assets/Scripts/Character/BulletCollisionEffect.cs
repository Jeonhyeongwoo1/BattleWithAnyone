using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Realtime;
using Photon.Pun;

public class BulletCollisionEffect : MonoBehaviourPunCallbacks
{
    public enum Type { Destory, Despawn, Disable }

    public Type type { get => m_Type; set => m_Type = value; }

    [SerializeField] bool m_TransforParent;
    [SerializeField] bool m_DisableState;
    [SerializeField] Type m_Type;
    [SerializeField] ParticleSystem m_Effect;

    UnityAction m_EffectPlayingDone = null;

    public void PlayEffect(Vector3 position, Quaternion rotation, UnityAction done = null)
    {
        m_EffectPlayingDone = done;
        photonView.RPC(nameof(NotfiyPlayEffect), RpcTarget.All, position, rotation);
    }

    [PunRPC]
    void NotfiyPlayEffect(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
        gameObject.SetActive(true);
        StartCoroutine(PlayingCollisionEffect());
    }

    [PunRPC]
    void SetBulletCollisionPoolObj()
    {
        IModel model = Core.models.Get();
        transform.SetParent(model.poolObjectCreatePoints);
        gameObject.SetActive(m_DisableState);
    }

    private void Awake()
    {
        if (m_TransforParent)
        {
            photonView.RPC(nameof(SetBulletCollisionPoolObj), RpcTarget.All);
        }
    }

    IEnumerator PlayingCollisionEffect()
    {
        m_Effect.Play();
        while (m_Effect.isPlaying) { yield return null; }
        m_Effect.Stop();

        switch (m_Type)
        {
            case Type.Destory:
                Destroy(gameObject);
                break;
            case Type.Despawn:
                if (photonView.IsMine)
                {
                    Core.poolManager.Despawn(nameof(BulletCollisionEffect), gameObject);
                }
                else
                {
                    gameObject.SetActive(false);
                }
                break;
            case Type.Disable:
                gameObject.SetActive(false);
                break;
        }

        m_EffectPlayingDone?.Invoke();
    }
}
