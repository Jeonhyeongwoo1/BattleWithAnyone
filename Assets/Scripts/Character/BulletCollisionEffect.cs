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

    public void PlayEffect(Vector3 position, Quaternion rotation, UnityAction done = null)
    {
        transform.SetPositionAndRotation(position, rotation);
        gameObject.SetActive(true);
        StartCoroutine(PlayingCollisionEffect(done));
    }

    [PunRPC]
    public void SetBulletCollisionPoolObj()
    {
        IModel model = Core.models.Get();
        transform.SetParent(model.poolObjectCreatePoints);
        gameObject.SetActive(m_DisableState);
    }

    private void Awake()
    {
        if(m_TransforParent)
        {
            photonView.RPC(nameof(SetBulletCollisionPoolObj), RpcTarget.All);
        }
    }

    IEnumerator PlayingCollisionEffect(UnityAction done)
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
                Core.poolManager.Despawn(nameof(BulletCollisionEffect), gameObject);
                break;
            case Type.Disable:
                gameObject.SetActive(false);
                break;
        }

        done?.Invoke();
    }

}
