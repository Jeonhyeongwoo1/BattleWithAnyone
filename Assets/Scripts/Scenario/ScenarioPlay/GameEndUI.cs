using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class GameEndUI : MonoBehaviour
{
	[SerializeField] Text m_Count;
	[SerializeField] Transform m_VictoryForm;
	[SerializeField] Transform m_LoseForm;
	[SerializeField] Transform m_GameValeInfoForm;

	[SerializeField] Text m_WinCount;
	[SerializeField] Text m_DamageReceived;
	[SerializeField] Text m_AccuracyRate;
	[SerializeField] Text m_TakeDamange;
	[SerializeField, Range(0, 10)] float m_GameEndWaitTime = 5;

	public const string Victory = "UI.Victory";
	public const string Lose = "UI.Lose";

	void SetGameInfo()
	{
        float accuracyRate = Mathf.Round(Core.state.totalBulletHitCount * 100 / Core.state.totalShootBulletCount);
        if(accuracyRate <= 0)
		{
			accuracyRate = 0;
		}
		
		m_AccuracyRate.text = string.Format(MessageCommon.Get("game.end.accuracyrate"), accuracyRate);	
		m_WinCount.text = string.Format(MessageCommon.Get("game.end.wincount"), PhotonNetwork.IsMasterClient ? Core.state.masterWinCount : Core.state.playerWinCount);
		m_DamageReceived.text = string.Format(MessageCommon.Get("game.end.damangereceived"), Core.state.totalDamangeReceived);
		m_TakeDamange.text = string.Format(MessageCommon.Get("game.end.takedamage"), Core.state.totalTakeDamange);
	}

	public void OpenCloseVictory(bool open)
	{
		m_VictoryForm.gameObject.SetActive(open);
		m_Count.gameObject.SetActive(open);
		m_GameValeInfoForm.gameObject.SetActive(open);
		if (open)
		{
			Core.audioManager.PlayBackground(AudioManager.BackgroundType.WIN);
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
            Core.audioManager.PlayBackground(AudioManager.BackgroundType.LOSE);
            SetGameInfo();
			StartCoroutine(WaitingTime());
		}
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

	void OpenCloseGameEndUI(string key, object o)
	{
		bool value = (bool)o;
		switch (key)
		{
			case Victory:
				OpenCloseVictory(value);
				break;
			case Lose:
				OpenCloseLose(value);
				break;
		}
	}

	void OnEnable()
	{
		Core.xEvent?.Watch(Victory, OpenCloseGameEndUI);
		Core.xEvent?.Watch(Lose, OpenCloseGameEndUI);
	}

	void OnDisable()
	{
		Core.xEvent?.Stop(Victory, OpenCloseGameEndUI);
		Core.xEvent?.Stop(Lose, OpenCloseGameEndUI);
	}

}
