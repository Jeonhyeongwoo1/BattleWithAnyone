using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Events;

public class DoorOpenCloseControl : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] bool m_IsOpen;
    [SerializeField] Transform m_LDoor;
    [SerializeField] Transform m_RDoor;
    [SerializeField] Vector3 m_OpenPosition; //pivot left
    [SerializeField] float m_OpenCloseDuration = 0.5f;
    [SerializeField] float m_CloseWaitTime = 3;
    [SerializeField] AnimationCurve m_Curve;

    public void OpenCloseDoor(UnityAction done = null)
    {
        StopAllCoroutines();
        StartCoroutine(SwitchingState(done));
    }

    IEnumerator SwitchingState(UnityAction done)
    {
        float elapsed = 0;
        Vector3 end = m_IsOpen ? Vector3.zero : m_OpenPosition;

        m_IsOpen = !m_IsOpen;

        while (elapsed < m_OpenCloseDuration)
        {
            elapsed += Time.deltaTime;
            float e = elapsed / m_OpenCloseDuration;
            m_LDoor.localPosition = Vector3.Lerp(m_LDoor.localPosition, end, m_Curve.Evaluate(e));
            m_RDoor.localPosition = Vector3.Lerp(m_RDoor.localPosition, -end, m_Curve.Evaluate(e));
            yield return null;
        }

        done?.Invoke();
    }

    IEnumerator WaitTime(UnityAction done, float time)
    {
        yield return new WaitForSeconds(time);
        done?.Invoke();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(m_LDoor.localPosition);
            stream.SendNext(m_RDoor.localPosition);
        }
        else
        {
            Vector3 LPos = (Vector3)stream.ReceiveNext();
            Vector3 RPos = (Vector3)stream.ReceiveNext();
            m_LDoor.localPosition = LPos;
            m_RDoor.localPosition = RPos;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            if (m_IsOpen)
            {
                StopAllCoroutines();
                return;
            }

            OpenCloseDoor();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            StartCoroutine(WaitTime(() => OpenCloseDoor(), m_CloseWaitTime));
        }
    }
}
