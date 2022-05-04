using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour, IInteractableItem
{
    public string interactableType => nameof(Teleport);

    public bool useTeleport
    {
        get => m_UseTeleport;
        private set => m_UseTeleport = value;
    }

    public Vector3 direction
    {
        get => m_Direction;
    }

    [SerializeField] Teleport m_ConnectedCircle;
    [SerializeField] ParticleSystem m_Effect;
    [SerializeField] Vector3 m_Direction;
    [SerializeField, Range(0, 1)] float m_WaitTime = 0.3f;

    WaitForSeconds m_WaitForSeconds;
    bool m_UseTeleport;
    bool m_DisconnectTeleport = false;

    public void Play()
    {
        m_DisconnectTeleport = false;
        m_Effect.Play();
    }

    public void Stop()
    {
        m_DisconnectTeleport = true;
        m_Effect.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (m_DisconnectTeleport) { return; }
        if (other.transform.CompareTag("Player"))
        {
            if (useTeleport) { return; }
            useTeleport = true;
            StartCoroutine(PassingThroughCircle(other.transform));
        }
    }

    IEnumerator PassingThroughCircle(Transform target)
    {
        yield return m_WaitForSeconds;
        target.position = new Vector3(m_ConnectedCircle.transform.position.x, target.position.y, m_ConnectedCircle.transform.position.z) + m_ConnectedCircle.direction;
        useTeleport = false;
    }

    private void OnEnable()
    {
        m_WaitForSeconds = new WaitForSeconds(m_WaitTime);
    }


}
