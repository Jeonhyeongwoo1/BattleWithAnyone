using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class GameEndUI : MonoBehaviour, IOnEventCallback
{
	[SerializeField] Text m_Count;
	[SerializeField] Transform m_VictoryForm;
	[SerializeField] Transform m_LoseForm;
	[SerializeField] Transform m_GameValeInfoForm;

	[SerializeField] Text m_WinCount;
	[SerializeField] Text m_DamageReceived;
	[SerializeField] Text m_TakeDamange;
	[SerializeField, Range(0, 10)] float m_GameEndWaitTime = 5;

	public void OpenCloseVictory(bool open)
	{
		m_VictoryForm.gameObject.SetActive(open);
		m_Count.gameObject.SetActive(open);
		m_GameValeInfoForm.gameObject.SetActive(open);
		if (open)
		{
			Core.audioManager.PlayBackground(AudioManager.BackgroundType.WIN, true);
			SetGameInfo();
			StartCoroutine(WaitingTime());
		}
	}

	public void OpenCloseLose(bool open)
	{
		m_LoseForm.gameObject.SetActive(open);
		m_Count.gameObject.SetActive(open);
        m_GameValeInfoForm.gameObject.SetActive(open);
		if (open)
        {
            Core.audioManager.PlayBackground(AudioManager.BackgroundType.LOSE, true);
            SetGameInfo();
			StartCoroutine(WaitingTime());
		}
	}

    public void OnEvent(EventData photonEvent)
	{
        if (photonEvent.Code == (byte)PhotonEventCode.GAME_END)
        {
			int winCount = PhotonNetwork.IsMasterClient ? Core.state.masterWinCount : Core.state.playerWinCount;
            if(winCount == Core.state.mapPreferences.numberOfRound)
			{
                OpenCloseVictory(true);
			}
			else
			{
				OpenCloseLose(true);
			}
        }
	}

    void SetGameInfo()
    {
        m_WinCount.text = string.Format(Core.language.GetNotifyMessage("game.end.wincount"), PhotonNetwork.IsMasterClient ? Core.state.masterWinCount : Core.state.playerWinCount);
        m_DamageReceived.text = string.Format(Core.language.GetNotifyMessage("game.end.damangereceived"), Core.state.totalDamangeReceived);
        m_TakeDamange.text = string.Format(Core.language.GetNotifyMessage("game.end.takedamage"), Core.state.totalTakeDamange);
    }

    IEnumerator WaitingTime()
	{
		float time = m_GameEndWaitTime;
		while (time > 0)
		{
			time -= Time.deltaTime;
			m_Count.text = string.Format("{0}", (int)time);
			yield return null;
		}

		//GameEnd
		Core.gameManager.state = (GamePlayManager.State.GameEnd);
	}

	void OnEnable()
	{
        PhotonNetwork.AddCallbackTarget(this);
	}

	void OnDisable()
	{
		PhotonNetwork.RemoveCallbackTarget(this);
	}

}
