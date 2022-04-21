using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Pellet : MonoBehaviour
{
    public Rigidbody rb => m_Rigidbody;

    [SerializeField] BulletCollisionEffect m_Effect;
    [SerializeField] Rigidbody m_Rigidbody;
    [SerializeField] LayerMask m_LayerMask;
    [SerializeField] float m_RayDist = 1f;

    Vector3 m_Direction;
    bool m_IsExplode;
    float m_Damage;
    float m_CurLifeTime;
    float m_SecondColliderRange;
    UnityAction m_Despawn;

    public void Init(float damage, Vector3 direction, float lifeTime, float coliderRange, UnityAction despawn, bool isExplode = false)
    {
        m_Damage = damage;
        m_Direction = direction;
        m_CurLifeTime = lifeTime;
        m_SecondColliderRange = coliderRange;
        m_IsExplode = isExplode;
        m_Despawn = despawn;
    }

    void OnHit(Transform target)
    {
        if (target.tag == "Player")
        {
            if (target.TryGetComponent<PlayerController>(out var player))
            {
                player.TakeDamange(m_Damage);
            }
        }

       DisablePellet();
    }

    void DisablePellet()
    {
        
        if (m_IsExplode)
        {
            m_Despawn?.Invoke();
            BulletCollisionEffect effect = Instantiate<BulletCollisionEffect>(m_Effect, transform.position, transform.rotation, transform.parent);
            effect.PlayEffect(transform.position, transform.rotation);
            Destroy(gameObject);
        }
        else
        {
            m_Effect.PlayEffect(transform.position, transform.rotation, ()=> m_Despawn.Invoke());
            gameObject.SetActive(false);
        }
    }

    private void Awake()
    {
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            m_Rigidbody = rb;
        }
    }

    private void OnDisable()
    {
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;
        m_CurLifeTime = 0;
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
        else
        {
            Ray ray = new Ray(transform.position, m_Direction);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, m_RayDist, m_LayerMask))
            {
                OnHit(raycastHit.transform);
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
