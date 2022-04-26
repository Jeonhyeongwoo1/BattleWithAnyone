using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameEndUI : MonoBehaviour
{
	[SerializeField] Text m_Count;
	[SerializeField] Transform m_VictoryForm;
	[SerializeField] Transform m_LoseForm;
	[SerializeField, Range(0, 10)] float m_GameEndWaitTime = 5;

	public const string Victory = "UI.Victory";
	public const string Lose = "UI.Lose";

	public void OpenCloseVictory(bool open)
	{
		m_VictoryForm.gameObject.SetActive(open);
		m_Count.gameObject.SetActive(open);
		if (open)
		{
			StartCoroutine(WaitingTime());
		}
	}

	public void OpenCloseLose(bool open)
	{
		m_LoseForm.gameObject.SetActive(open);
		m_Count.gameObject.SetActive(open);
		if (open)
		{
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
