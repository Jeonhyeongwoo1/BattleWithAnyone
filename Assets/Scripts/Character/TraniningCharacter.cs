using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TraniningCharacter : MonoBehaviour
{
    [SerializeField] Animator m_Animator;
    [SerializeField] float m_AttackCycle;
    [SerializeField] bool m_IsAttack;
    [SerializeField] bool m_IsMoving;
    [SerializeField] int m_BulletCreateCount = 10;
    [SerializeField] BulletBase m_Bullet;
    [SerializeField] Transform m_ShootPoint;

    [SerializeField] GameObject point1;
    [SerializeField] GameObject point2;

    IEnumerator Attack()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(m_AttackCycle);
        yield return waitForSeconds;
        while (m_IsAttack)
        {
            m_Animator.SetTrigger("Attack");
            GameObject go = Core.poolManager.Spawn(nameof(Bullet));
            if (go.TryGetComponent<BulletBase>(out var bullet))
            {
                go.SetActive(true);
                bullet.transform.SetPositionAndRotation(m_ShootPoint.position, Quaternion.Euler(m_ShootPoint.eulerAngles));
                Vector3 dir = transform.forward;
                bullet.Init(dir, transform);
                bullet.Shoot();
            }

            yield return waitForSeconds;
        }
    }

    IEnumerator Moving()
    {
        //point2
        float elapsed = 0;
        while(m_IsMoving)
        {
            transform.position = Vector3.Lerp(transform.position, point2.transform.position, Time.deltaTime /2 );
            yield return null;
        }

        

    }

    private void OnCollisionEnter(Collision other) {
    }

    private void OnEnable()
    {
        //StartCoroutine(Moving());
        StartCoroutine(Attack());
    }

    private void Start() {
        if (!Core.poolManager.Has(nameof(Bullet)))
        {
            ObjectPool objectPool = new ObjectPool(m_BulletCreateCount, m_BulletCreateCount * 2, transform.parent, m_Bullet.gameObject);
            Core.poolManager.Add(nameof(Bullet), objectPool);
            Core.poolManager.Initialize(nameof(Bullet));
        }
    }

    private void Awake()
    {

    }
}
