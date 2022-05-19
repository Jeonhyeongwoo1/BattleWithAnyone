using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

public class Fireball : BulletBase
{
    BulletAttribute.MagicBullet magicBullet;

    public override void Shoot(Vector3 dir, Vector3 position, Quaternion rotation)
    {
        photonView.RPC(nameof(Shooting), RpcTarget.All, dir, position);
    }

    private void Update()
    {
        lifeTime -= Time.deltaTime;

        if (lifeTime < 0)
        {
            photonView.RPC(nameof(NotifyObjDisappeared), RpcTarget.All);   
        }
    }

    public override void OnEnable()
    {
        if (attribute.type == BulletAttribute.BulletType.None)
        {
            Debug.LogError("Select attribute Type!!");
        }

        lifeTime = attribute.maxLifeTime;
        magicBullet = GetMagicBullet();
    }

    public override void OnDisable()
    {
        lifeTime = 0;
    }

    void OnHit(Transform target)
    {
        if (target.TryGetComponent<PhotonView>(out var view))
        {
            if (!view.IsMine)
            {
                BulletAttribute.Fireball f = attribute.fireball;
                Core.state.totalTakeDamange += f.damage;
                Core.state.totalBulletHitCount++;
                photonView.RPC(nameof(TakeDamange), RpcTarget.Others, attribute.fireball.damage, f.residualfireDuration, f.residualfireDamage, f.residualfireDamageInterval);
            }
        }
    }

    [PunRPC]
    void TakeDamange(int damage, float duration, int fireDamage, float damageInterval)
    {
        Transform player = PhotonNetwork.IsMasterClient ? Core.state.masterCharacter : Core.state.playerCharacter;
        if (player.TryGetComponent<PlayerController>(out var controller))
        {
            controller.TakeDamange(damage);
            if (attribute.type == BulletAttribute.BulletType.Fireball)
            {
                controller.TakeDamagedByResidualFire(duration, fireDamage, damageInterval);
            }
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.tag == "Player")
        {
            OnHit(other.transform);
        }

        StartCoroutine(PlayingParticle(() => photonView.RPC(nameof(NotifyObjDisappeared), RpcTarget.All)));
    }
    
    BulletAttribute.MagicBullet GetMagicBullet()
    {
        switch (attribute.type)
        {
            case BulletAttribute.BulletType.Fireball:
                return attribute.fireball;
        }

        return null;
    }

    [PunRPC]
    void NotifyObjDisappeared()
    {
        attribute.fireball.rootParticle.Stop();
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
    void Shooting(Vector3 dir, Vector3 position)
    {
        transform.SetPositionAndRotation(position, Quaternion.LookRotation(dir, Vector3.up));
        gameObject.SetActive(true);
        magicBullet.rootParticle.Stop();
        var main = magicBullet.rootParticle.main;
        main.startSpeed = magicBullet.fwdForce;
        main.duration = magicBullet.effectDuration;
        magicBullet.rootParticle.Play();
    }

    IEnumerator PlayingParticle(UnityAction done)
    {
        ParticleSystem p = magicBullet.rootParticle;
        while (p.isPlaying) { yield return null; }
        p.Stop();
        done?.Invoke();
    }

}
