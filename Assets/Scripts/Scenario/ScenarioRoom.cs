using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class ScenarioRoom : MonoBehaviour, IScenario
{
	public string scenarioName => typeof(ScenarioRoom).Name;

	public Button m_Button;

	public void OnScenarioPrepare(UnityAction done)
	{


		done?.Invoke();
	}

	public void OnScenarioStandbyCamera(UnityAction done)
	{
		done?.Invoke();
	}

	public void OnScenarioStart(UnityAction done)
	{
		done?.Invoke();
	}

	public void OnScenarioStop(UnityAction done)
	{
		done?.Invoke();
	}


	void GoHome()
	{
		PhotonNetwork.LeaveRoom();
		Core.scenario.OnLoadScenario(nameof(ScenarioHome));
	}

	// Start is called before the first frame update
	void Start()
	{
		m_Button.onClick.AddListener(() => GoHome());
		Core.Ensure(() => Core.scenario.OnLoadedScenario(this));
	}

	private void Awake()
	{
		Core.Ensure(() => Core.scenario.OnScenarioAwaked(this));
	}

}
