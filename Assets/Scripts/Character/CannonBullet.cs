using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CannonBullet : BulletBase
{
    [SerializeField] Collider m_Collider;

    BulletAttribute.Cannon m_CannonAttirbute;
    float m_CurCollision = 0;
    float m_CollisionDetectionWaitTime = 0f;

    public override void Init(Vector3 dir, Transform shooter)
    {
        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        direction = dir;
        this.shooter = shooter;
    }

    public override void Shoot()
    {
        rb.AddForce(transform.forward * m_CannonAttirbute.fwdForce, ForceMode.Impulse);
        rb.AddForce(transform.up * m_CannonAttirbute.upForce, ForceMode.Impulse);


        if (bulletEffect != null)
            bulletEffect?.Play();
    }

    BulletAttribute.Cannon GetCannonAttribute()
    {
        switch (attribute.type)
        {
            case BulletAttribute.BulletType.Cannon:
                return attribute.cannon;
            case BulletAttribute.BulletType.Junkrat:
                return attribute.junkrat;
        }

        return null;
    }

    void Explosion(PlayerController player = null)
    {
        bool isHit = false;
        if (player)
        {
            OnHit(player);
            isHit = true;
        }
        else
        {
            Collider[] players = Physics.OverlapSphere(transform.position, rayCastRange, playerLayer);
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].transform == shooter) { continue; }

                if (players[i].transform.TryGetComponent<PlayerController>(out var component))
                {
                    OnHit(component);
                    isHit = true;
                }
            }
        }

        if (!isHit && attribute.junkrat.isSecondBall)
        {
            Pellet ball = attribute.junkrat.secondBall;
            float range = attribute.junkrat.secondBallRandomRange;
            int count = attribute.junkrat.secondBallCount;
            for (int i = 0; i < count; i++)
            {
                Vector3 random = new Vector3(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range));
                Pellet pellet = Instantiate<Pellet>(ball, transform.position, Quaternion.identity, transform.parent);
                pellet.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                pellet.Init(attribute.junkrat.secondBallDamage, direction, attribute.junkrat.maxSecondLifeTime, attribute.junkrat.secondColliderRange, null, true);
                pellet.gameObject.SetActive(true);
                pellet.rb.useGravity = true;
                pellet.rb.AddForce(random * attribute.junkrat.secondBallForce, ForceMode.Impulse);
            }

        }

        PlayCollisionEffect();
        Core.poolManager.Despawn(nameof(Bullet), gameObject);
    }


    void OnHit(PlayerController player)
    {
        float damage = m_CannonAttirbute.damage;
        player.TakeDamange(damage);
    }

    IEnumerator WaitTime(float time, UnityAction done)
    {
        yield return new WaitForSeconds(time);
        done?.Invoke();
    }

    private void OnEnable()
    {
        if (attribute.type == BulletAttribute.BulletType.None)
        {
            Debug.LogError("Select attribute Type!!");
        }

        m_CannonAttirbute = GetCannonAttribute();
        lifeTime = attribute.maxLifeTime;
    }

    private void Update()
    {
        if (lifeTime < 0)
        {
            Explosion();
        }

        float timeBeforeExplosion = m_CannonAttirbute.timeBeforeExplosion;
        Collider[] players = Physics.OverlapSphere(transform.position, rayCastRange, playerLayer);
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].transform == shooter) { continue; }
            rb.velocity = Vector3.zero;
            m_Collider.isTrigger = true;

            if (players[i].transform.TryGetComponent<PlayerController>(out var p)) { }
            StartCoroutine(WaitTime(timeBeforeExplosion, () => Explosion(p)));
        }

        if (m_CannonAttirbute.maxCollision != 0 && m_CurCollision >= m_CannonAttirbute.maxCollision)
        {
            StartCoroutine(WaitTime(timeBeforeExplosion, () => Explosion()));
            return;
        }

        float deltaTime = Time.deltaTime;
        lifeTime -= deltaTime;
        m_CollisionDetectionWaitTime -= deltaTime;

    }

    private void Start()
    {
        PhysicMaterial physicMaterial = new PhysicMaterial();
        physicMaterial.bounciness = bounciness;
        physicMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
        physicMaterial.bounceCombine = PhysicMaterialCombine.Maximum;
        GetComponent<Collider>().material = physicMaterial;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.transform == shooter) { return; }
        if (other.rigidbody != null) //Debug
        {
            other.rigidbody.velocity = Vector3.zero;
        }

        if (m_CannonAttirbute.maxCollision == 0)
        {
            Explosion();
            return;
        }

        if (m_CollisionDetectionWaitTime > 0) { return; }

        m_CollisionDetectionWaitTime = m_CannonAttirbute.collisionDetectionWaitTime;
        m_CurCollision++;

        rb.velocity = rb.velocity * m_CannonAttirbute.deceleration;
        rb.AddForce(Vector3.down * m_CannonAttirbute.downForce, ForceMode.VelocityChange);
        bulletEffect.Stop();
    }

    private void OnDisable()
    {
        m_Collider.isTrigger = false;
        lifeTime = 0;
        m_CollisionDetectionWaitTime = 0;
        m_CurCollision = 0;
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;
        transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rayCastRange);
    }
}
