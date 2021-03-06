using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.Events;
using ExitGames.Client.Photon;

public class GameTimer : MonoBehaviourPun, IPunObservable, IOnEventCallback
{
    [SerializeField] Text m_Timer;

    float m_RounTime = 0;

    public void StartTimer()
    {
        if (!PhotonNetwork.IsMasterClient) { return; }

        StartCoroutine(Timer(RoundEnd));
    }

    public void StopTimer()
    {
        StopAllCoroutines();
        m_Timer.text = "0";
        m_RounTime = 0;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && PhotonNetwork.IsMasterClient)
        {
            stream.SendNext(m_RounTime);
        }
        else
        {
            float roundTime = (float)stream.ReceiveNext();
            m_Timer.text = string.Format("{0}", Mathf.Floor(roundTime));
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case (byte)PhotonEventCode.ROUNDDONE:
                StopTimer();
                break;
        }
    }

    void RoundEnd()
    {
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
        PhotonNetwork.RaiseEvent((byte)PhotonEventCode.ROUNDDONE_TIMEOUT, null, raiseEventOptions, SendOptions.SendReliable);
    }

    IEnumerator Timer(UnityAction done)
    {
        m_RounTime = Core.state.mapPreferences.roundTime;
        while (m_RounTime >= 0)
        {
            m_RounTime -= Time.deltaTime;
            m_Timer.text = string.Format("{0}", (int)m_RounTime);
            yield return null;
        }

        m_Timer.text = "0";
        done.Invoke();
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

}
