using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;

public class CannonBullet : BulletBase
{
    [SerializeField] Collider m_Collider;

    BulletAttribute.Cannon m_CannonAttirbute;
    float m_CurCollision = 0;
    float m_CollisionDetectionWaitTime = 0f;

    public override void Shoot(Vector3 dir, Vector3 position, Quaternion rotation)
    {
        photonView.RPC(nameof(Shooting), Photon.Pun.RpcTarget.All, dir, position, rotation);
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
                if (players[i].transform.TryGetComponent<PlayerController>(out var component))
                {
                    OnHit(component);
                    isHit = true;
                }
            }
        }

        if (!isHit && attribute.junkrat.isSecondBall && photonView.IsMine)
        {
            float range = attribute.junkrat.secondBallRandomRange;
            int count = attribute.junkrat.secondBallCount;
            float scale = attribute.junkrat.secondBallScale;
            for (int i = 0; i < count; i++)
            {
                GameObject p = PhotonNetwork.Instantiate(XSettings.bulletPath + attribute.junkrat.secondBall.name, transform.position, Quaternion.identity, 0);
                if (p.TryGetComponent<Pellet>(out var pellet))
                {
                    pellet.Shoot(attribute.junkrat.secondBallDamage, direction, attribute.junkrat.maxSecondLifeTime, attribute.junkrat.secondColliderRange, range, attribute.junkrat.secondBallForce, scale, true);
                }
            }
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


    void OnHit(PlayerController player)
    {
        if (player.TryGetComponent<PhotonView>(out var view))
        {
            if (!view.IsMine)
            {
                int damage = m_CannonAttirbute.damage;
                photonView.RPC(nameof(TakeDamange), RpcTarget.Others, damage);
            }
        }
    }

    [PunRPC]
    void Shooting(Vector3 dir, Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        direction = dir;
        gameObject.SetActive(true);
        rb.AddForce(transform.forward * m_CannonAttirbute.fwdForce, ForceMode.Impulse);
        rb.AddForce(transform.up * m_CannonAttirbute.upForce, ForceMode.Impulse);
        if (bulletEffect != null)
            bulletEffect?.Play();
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

    IEnumerator WaitTime(float time, UnityAction done)
    {
        yield return new WaitForSeconds(time);
        done?.Invoke();
    }

    public override void OnEnable()
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
            if (players[i].transform.TryGetComponent<PhotonView>(out var view))
            {
                if (view.IsMine) { continue; }
            }

            if (!players[i].transform.TryGetComponent<PlayerController>(out var p)) { continue; }

            rb.velocity = Vector3.zero;
            m_Collider.isTrigger = true;
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
        if (other.transform.TryGetComponent<PhotonView>(out var view))
        {
            if (view.IsMine) { return; }
        }

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

    public override void OnDisable()
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
