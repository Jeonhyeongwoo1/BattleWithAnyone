using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : BulletBase
{
    int m_RecoupPellet = 0;

    public override void Init(Vector3 dir, Transform shooter)
    {
        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        direction = dir;
    }

    public override void Shoot()
    {
        Vector3 angle = attribute.shotgun.vAngle;
        Pellet[] pellets = attribute.shotgun.pellets;
        for (int i = 0; i < pellets.Length; i++)
        {
            Vector3 random = new Vector3(Random.Range(-angle.x, angle.x), Random.Range(-angle.y, angle.y), Random.Range(-angle.z, angle.z));
            pellets[i].Init(attribute.shotgun.damage, direction, attribute.maxLifeTime, 0, RecoupPellet, false);
            pellets[i].gameObject.SetActive(true);
            pellets[i].transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            pellets[i].rb.AddForce(transform.forward * attribute.shotgun.fwdForce + random, ForceMode.Impulse);
        }
    }

    void RecoupPellet()
    {
        m_RecoupPellet++;
        if (attribute.shotgun.pellets.Length == m_RecoupPellet)
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
    }

    private void Update()
    {
        lifeTime -= Time.deltaTime;

        if (lifeTime < 0)
        {
            Core.poolManager.Despawn(nameof(Bullet), gameObject);
        }
    }

}
