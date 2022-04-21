using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public abstract class BulletBase : MonoBehaviour
{
    [SerializeField] protected BulletAttribute attribute;
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected ParticleSystem bulletEffect;
    [SerializeField, Range(0, 1)] protected float bounciness = 0.663f;
    [SerializeField] protected float rayCastRange;
    protected Vector3 direction;
    protected Transform shooter;
    protected float lifeTime;

    public abstract void Init(Vector3 dir, Transform shooter);
    public abstract void Shoot();

    
    protected void PlayCollisionEffect()
    {
        GameObject go = Core.poolManager.Spawn(nameof(BulletCollisionEffect));
        if (!go.TryGetComponent<BulletCollisionEffect>(out var effect)) { return; }

        effect.PlayEffect(transform.position, transform.rotation);
    }
}

public class Bullet : BulletBase
{

    public override void Init(Vector3 dir, Transform shooter)
    {
        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        direction = dir;
        this.shooter = shooter;
    }

    public override void Shoot()
    {
        switch (attribute.type)
        {
            case BulletAttribute.BulletType.Pistol:
                rb.AddForce(transform.forward * attribute.pistol.fwdForce, ForceMode.Impulse);
                break;
        }
    }

    void OnHit(Transform target)
    {
        switch (attribute.type)
        {
            case BulletAttribute.BulletType.Pistol:
                if (target.tag == "Player")
                {
                    if (target.TryGetComponent<PlayerController>(out var controller))
                    {
                        controller.TakeDamange(attribute.pistol.damage);
                    }
                }
                break;
        }

        PlayCollisionEffect();
        Core.poolManager.Despawn(nameof(Bullet), gameObject);
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
            Core.poolManager.Despawn(nameof(Bullet), gameObject);
        }

    }

    private void OnEnable()
    {
        lifeTime = attribute.maxLifeTime;
    }

    private void OnDisable()
    {
        lifeTime = 0;
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;
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
