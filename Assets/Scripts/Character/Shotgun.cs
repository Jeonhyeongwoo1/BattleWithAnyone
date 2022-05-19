using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Shotgun : BulletBase
{
    int m_RecoupPellet = 0;
    List<bool> m_PelletHit = new List<bool>();

    public override void Shoot(Vector3 dir, Vector3 position, Quaternion rotation)
    {
       photonView.RPC(nameof(Shooting), RpcTarget.All, dir, position, rotation);
    }

    void RecoupPellet(bool isHit)
    {
        m_RecoupPellet++;
        m_PelletHit.Add(isHit);
        if (attribute.shotgun.pellets.Length == m_RecoupPellet)
        {
            print("TESt");
            bool ishit = m_PelletHit.Find((v)=> v == true);
            if(ishit)
            {
                print(ishit);
                Core.state.totalBulletHitCount++;
            }

            photonView.RPC(nameof(NotifyObjDisappeared), RpcTarget.All);
        }
    }

    [PunRPC]
    void Shooting(Vector3 dir, Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
        direction = dir;
        gameObject.SetActive(true);

        Vector3 angle = attribute.shotgun.vAngle;
        Pellet[] pellets = attribute.shotgun.pellets;
        int damage = attribute.shotgun.damage;
        float maxLife = attribute.shotgun.pelletLifeTime;
        float force = attribute.shotgun.fwdForce;
        
        for (int i = 0; i < pellets.Length; i++)
        {
            Vector3 random = new Vector3(Random.Range(-angle.x, angle.x), Random.Range(-angle.y, angle.y), Random.Range(-angle.z, angle.z));
            pellets[i].Shoot(damage, direction, maxLife, random, force, RecoupPellet, false);
        }
    }

    [PunRPC]
    void NotifyObjDisappeared()
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

    public override void OnEnable()
    {
        lifeTime = attribute.maxLifeTime;
    }

    public override void OnDisable()
    {
        lifeTime = 0;
    }

    private void Update()
    {
        lifeTime -= Time.deltaTime;

        if (lifeTime < 0)
        {
           // photonView.RPC(nameof(NotifyObjDisappeared), RpcTarget.All);
        }
    }

}
