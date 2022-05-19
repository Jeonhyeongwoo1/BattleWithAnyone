using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class ModelGroundType : MonoBehaviour
{
    public AudioManager.GroundType groundType => m_GroundType;

    [SerializeField] AudioManager.GroundType m_GroundType;

}
