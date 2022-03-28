using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class GameTimer : MonoBehaviourPun, IPunObservable
{
	[SerializeField] Text m_Timer;

	float m_RounTime = 0;

	public void StartTimer()
	{
		if (!PhotonNetwork.IsMasterClient) { return; }

		StartCoroutine(Timer());
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

	IEnumerator Timer()
	{
		m_RounTime = Core.state.mapPreferences.roundTime;
		while (m_RounTime >= 0)
		{
			m_RounTime -= Time.deltaTime;
			m_Timer.text = string.Format("{0}", (int)m_RounTime);
			yield return null;
		}
	}

}
