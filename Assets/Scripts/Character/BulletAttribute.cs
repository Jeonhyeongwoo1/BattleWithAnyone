using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class BulletAttribute
{
    public enum BulletType { None, Cannon, Pistol, Shotgun, Fireball, Junkrat }

    /*
    1. 단발성 총알 pistol
    2. 대포형 cannon
    3. 레이저 형 (보류)
    4. 여러개 동시에 발사 junkrat
    5. 폭탄 형 cannon(maxCollsion)
    6. 샷건 Shotgun
    7. 파이어 볼 Fireball
    */

    [Serializable]
    public class BaseItem
    {
        public float fwdForce;
        public float damage;
    }

    [Serializable]
    public class Cannon : BaseItem
    {
        public float upForce;
        public float hitRange;
        public float maxCollision;
        public float downForce;
        public float timeBeforeExplosion;
        public float collisionDetectionWaitTime;
        public float deceleration = 0.3f;
    }

    [Serializable]
    public class Shotgun : BaseItem
    {
        public Vector3 vAngle;
        public Pellet[] pellets;
    }

    [Serializable]
    public class MagicBullet : BaseItem
    {
        public float effectDuration;
        public ParticleSystem rootParticle;
    }

    [Serializable]
    public class Fireball : MagicBullet
    {
        public float residualfireDamageInterval;
        public float residualfireDuration;
        public float residualfireDamage;
    }

    [Serializable]
    public class JunkratBall : Cannon
    {
        public bool isSecondBall;
        public float maxSecondLifeTime;
        public float secondBallRandomRange;
        public float secondColliderRange;
        public float secondBallForce;
        public float secondBallDamage;
        public int secondBallCount;
        public Pellet secondBall;
    }

    [Serializable]
    public class Pistol : BaseItem { }

    public BulletType type;
    public Cannon cannon;
    public Pistol pistol;
    public Shotgun shotgun;
    public Fireball fireball;
    public JunkratBall junkrat;
    public float maxLifeTime;

}
