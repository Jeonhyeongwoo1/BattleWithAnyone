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

	public void OnScenarioPrepare(UnityAction done)
    {
        if (!PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InRoom)
        {
			Debug.LogError("DisConnected");
			//Error 처리
		}
		
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

	// Start is called before the first frame update
	void Start()
	{
		Core.Ensure(() => Core.scenario.OnLoadedScenario(this));
	}

	private void Awake()
	{
		Core.Ensure(() => Core.scenario.OnScenarioAwaked(this));
	}

}
