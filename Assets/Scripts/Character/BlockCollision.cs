using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCollision : MonoBehaviour
{
    [SerializeField] Collider m_Target;
    [SerializeField] Collider m_TargetBlock;

    // Start is called before the first frame update
    void Start()
    {
        Physics.IgnoreCollision(m_Target, m_TargetBlock, true);
    }

}
