using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;
using Photon.Realtime;

public abstract class BulletBase : MonoBehaviourPunCallbacks
{
    public BulletAttribute.BulletType bulletType 
    {
        get => attribute.type;
    }

    public int damage
    {
        get
        {
            switch (attribute.type)
            {
                case BulletAttribute.BulletType.Pistol:
                    return attribute.pistol.damage;
                case BulletAttribute.BulletType.Cannon:
                    return attribute.cannon.damage;
                case BulletAttribute.BulletType.Shotgun:
                    return attribute.shotgun.damage;
                case BulletAttribute.BulletType.Fireball:
                    return attribute.fireball.damage;
                case BulletAttribute.BulletType.Junkrat:
                    return attribute.junkrat.damage;
                case BulletAttribute.BulletType.None:
                default:
                    return 0;
            }
        }
    }

    [SerializeField] protected BulletAttribute attribute;
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected ParticleSystem bulletEffect;
    [SerializeField, Range(0, 1)] protected float bounciness = 0.663f;
    [SerializeField] protected float rayCastRange;
    protected Vector3 direction;
    protected Transform shooter;
    protected float lifeTime;

    public abstract void Shoot(Vector3 dir, Vector3 position, Quaternion rotation);

    protected void PlayCollisionEffect()
    {
        if (!photonView.IsMine) { return; }

        GameObject go = Core.poolManager.Spawn(nameof(BulletCollisionEffect));
        if (go == null) { return; }
        if (!go.TryGetComponent<BulletCollisionEffect>(out var effect)) { return; }

        effect.PlayEffect(transform.position, transform.rotation);
    }

    public virtual void Awake()
    {
        photonView.RPC(nameof(SetBulletPoolObjParent), RpcTarget.All);
    }

    [PunRPC]
    public void SetBulletPoolObjParent()
    {
        IModel model = Core.models.Get();
        transform.SetParent(model.poolObjectCreatePoints);
        gameObject.SetActive(false);
    }
}

public class Bullet : BulletBase
{
    bool m_IsHit = false;

    public override void Shoot(Vector3 dir, Vector3 position, Quaternion rotation)
    {
        switch (attribute.type)
        {
            case BulletAttribute.BulletType.Pistol:
                photonView.RPC(nameof(Shooting), RpcTarget.All, dir, position, rotation);
                break;
        }
    }

    void OnHit(Transform target)
    {
        if (m_IsHit) { return; }
        m_IsHit = true;
        switch (attribute.type)
        {
            case BulletAttribute.BulletType.Pistol:
                if (target.CompareTag("Player"))
                {
                    if (target.TryGetComponent<PhotonView>(out var view))
                    {
                        if(!view.IsMine)
                        {
                            int damage = attribute.pistol.damage;
                            photonView.RPC(nameof(TakeDamange), RpcTarget.Others, damage);
                        }
                    }
                }
                break;
        }

        PlayCollisionEffect();
        BulletDisable();
    }

    void BulletDisable()
    {
        if (photonView.IsMine)
        {
            Core.poolManager.Despawn(nameof(Bullet), gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    [PunRPC]
    void Shooting(Vector3 dir, Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        direction = dir;
        gameObject.SetActive(true);
        rb.AddForce(transform.forward * attribute.pistol.fwdForce, ForceMode.Impulse);
    }

    [PunRPC]
    void TakeDamange(int damage)
    {
        Transform player = PhotonNetwork.IsMasterClient ? Core.state.masterCharacter : Core.state.playerCharacter;
        
        if(player.TryGetComponent<PlayerController>(out var controller))
        {
            controller.TakeDamange(damage);
        }
    }

    private void Start()
    {
        PhysicMaterial physicMaterial = new PhysicMaterial();
        physicMaterial.bounciness = bounciness;
        physicMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
        physicMaterial.bounceCombine = PhysicMaterialCombine.Maximum;
        GetComponent<Collider>().material = physicMaterial;
    }

    private void Update()
    {
        Ray ray = new Ray(transform.position, direction);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, rayCastRange, playerLayer))
        {
            OnHit(raycastHit.transform);
        }

        lifeTime -= Time.deltaTime;
        if (lifeTime < 0)
        {
            BulletDisable();
        }

    }

    public override void OnEnable()
    {
        lifeTime = attribute.maxLifeTime;
    }

    public override void OnDisable()
    {
        lifeTime = 0;
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;
        m_IsHit = false;
        transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other?.rigidbody != null)
        {
            other.rigidbody.velocity = Vector3.zero;
        }
        
        OnHit(other.transform);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attribute.cannon.hitRange);
    }

}
