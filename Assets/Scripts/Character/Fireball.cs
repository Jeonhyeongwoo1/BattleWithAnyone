using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Fireball : BulletBase
{
    BulletAttribute.MagicBullet magicBullet;

    public override void Init(Vector3 dir, Transform shooter)
    {
        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        direction = dir;
        this.shooter = shooter;
    }

    public override void Shoot()
    {
        if (magicBullet.rootParticle.isPlaying || magicBullet.rootParticle.isEmitting)
        {
            magicBullet.rootParticle.Stop();
        }

        var main = magicBullet.rootParticle.main;
        main.startSpeed = magicBullet.fwdForce;
        main.duration = magicBullet.effectDuration;
        magicBullet.rootParticle.Play();
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

    private void Update()
    {
        lifeTime -= Time.deltaTime;

        if (lifeTime < 0)
        {
            attribute.fireball.rootParticle.Stop();
            Core.poolManager.Despawn(nameof(Bullet), gameObject);
        }
    }

    private void OnEnable()
    {
        if (attribute.type == BulletAttribute.BulletType.None)
        {
            Debug.LogError("Select attribute Type!!");
        }

        lifeTime = attribute.maxLifeTime;
        magicBullet = GetMagicBullet();
    }

    private void OnDisable()
    {
        lifeTime = 0;
    }

    void OnHit(Transform target)
    {
        if (target.TryGetComponent<PlayerController>(out var player))
        {
            player.TakeDamange(magicBullet.damage);
            SpeicalAttack(player);
        }
    }

    void SpeicalAttack(PlayerController player)
    {
        if (attribute.type == BulletAttribute.BulletType.Fireball)
        {
            BulletAttribute.Fireball f = attribute.fireball;
            player.TakeDamagedByResidualFire(f.residualfireDuration, f.residualfireDamage, f.residualfireDamageInterval);
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.tag == "Player")
        {
            OnHit(other.transform);
        }

        StartCoroutine(PlayingParticle(() => Core.poolManager.Despawn(nameof(Bullet), gameObject)));
    }

    IEnumerator PlayingParticle(UnityAction done)
    {
        ParticleSystem p = magicBullet.rootParticle;
        while (p.isPlaying) { yield return null; }
        p.Stop();
        done?.Invoke();
    }

}
