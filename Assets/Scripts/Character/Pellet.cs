using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;

public class Pellet : MonoBehaviourPunCallbacks
{
    public Rigidbody rb => m_Rigidbody;

    [SerializeField] bool m_TransforParent;
    [SerializeField] BulletCollisionEffect m_Effect;
    [SerializeField] Rigidbody m_Rigidbody;
    [SerializeField] LayerMask m_LayerMask;
    [SerializeField] float m_RayDist = 1f;

    Vector3 m_Direction;
    bool m_IsExplode;
    bool m_IsHit;
    int m_Damage;
    float m_CurLifeTime;
    float m_SecondColliderRange;
    UnityAction m_Despawn;
 
    public void Shoot(int damage, Vector3 direction, float lifeTime, float coliderRange, float range, float force, float scale, bool isExplode)
    {
        photonView.RPC(nameof(Shooting), RpcTarget.All, damage, direction, lifeTime, coliderRange, range, force, scale, isExplode);
    }

    public void Shoot(int damage, Vector3 direction, float lifeTime, Vector3 random, float force, UnityAction despawn, bool isExplode)
    {
        m_Despawn = despawn;
        photonView.RPC(nameof(Shooting), RpcTarget.All, damage, direction, random, lifeTime, force, isExplode);
    }

    [PunRPC]
    void Shooting(int damage, Vector3 direction, Vector3 random, float lifeTime, float force, bool isExplode)
    {
        m_Damage = damage;
        m_Direction = direction;
        m_CurLifeTime = lifeTime;
        m_IsExplode = isExplode;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        gameObject.SetActive(true);
        m_Rigidbody.AddForce(transform.forward * force + random, ForceMode.Impulse);
    }

    [PunRPC]
    void Shooting(int damage, Vector3 direction, float lifeTime, float coliderRange, float range, float force, float scale, bool isExplode)
    {
        Vector3 random = new Vector3(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range));
        transform.localScale = new Vector3(scale, scale, scale);
        m_Damage = damage;
        m_Direction = direction;
        m_CurLifeTime = lifeTime;
        m_SecondColliderRange = coliderRange;
        m_IsExplode = isExplode;
        gameObject.SetActive(true);
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        m_Rigidbody.useGravity = true;
        m_Rigidbody.AddForce(random * force, ForceMode.Impulse);
    }

    void OnHit(Transform target)
    {
        if (m_IsHit) { return; }
        m_IsHit = true;
        if (target.tag == "Player")
        {
            if (target.TryGetComponent<PhotonView>(out var view))
            {
                if (!view.IsMine)
                {
                    photonView.RPC(nameof(TakeDamange), RpcTarget.Others, m_Damage);
                }
            }
        }

       DisablePellet();
    }

    [PunRPC]
    void TakeDamange(int damage)
    {
        Transform player = PhotonNetwork.IsMasterClient ? Core.state.masterCharacter : Core.state.playerCharacter;

        if (player.TryGetComponent<PlayerController>(out var controller))
        {
            controller.TakeDamange(damage);
        }
    }

    void DisablePellet()
    {
        
        if (m_IsExplode)
        {
            if(!photonView.IsMine) { return;}

            GameObject go = PhotonNetwork.Instantiate(XSettings.bulletImpactPath + m_Effect.name, transform.position, transform.rotation, 0);
            if(go.transform.TryGetComponent<BulletCollisionEffect>(out var effect))
            {
                effect.PlayEffect(transform.position, transform.rotation);
            }
            photonView.RPC(nameof(NotifyObjDisappeared), RpcTarget.All, true);
        }
        else
        {
            m_Effect.PlayEffect(transform.position, transform.rotation, () => m_Despawn?.Invoke());
            photonView.RPC(nameof(NotifyObjDisappeared), RpcTarget.All, false);
        }
    }

    [PunRPC]
    void NotifyObjDisappeared(bool isDestroy)
    {
        if(isDestroy)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void Awake()
    {
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            m_Rigidbody = rb;
        }

        if (m_TransforParent)
        {
            IModel model = Core.models.Get();
            transform.SetParent(model.poolObjectCreatePoints);
        }
    }

    public override void OnDisable()
    {
        m_IsHit = false;
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;
        m_CurLifeTime = 0;
        transform.localPosition = Vector3.zero;
    }

    private void Update()
    {
        if (m_IsExplode)
        {
            Collider[] players = Physics.OverlapSphere(transform.position, m_SecondColliderRange, m_LayerMask);
            for (int i = 0; i < players.Length; i++)
            {
                OnHit(players[i].transform);
            }
        }
 
        if (m_CurLifeTime < 0)
        {
            DisablePellet();
        }

        m_CurLifeTime -= Time.deltaTime;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.tag == "Pellet") { return; }
        if (other.rigidbody != null)
        {
            other.rigidbody.velocity = Vector3.zero;
        }

        OnHit(other.transform);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Pellet") { return; }

        OnHit(other.transform);
    }


}
