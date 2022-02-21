using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ScenarioPlay : MonoBehaviour, IScenario
{
	public string scenarioName => typeof(ScenarioPlay).Name;

	public void OnScenarioPrepare(UnityAction done)
	{
		BattleWtihAnyOneStarter.GetBlockSkybox()?.gameObject.SetActive(false);
		BattleWtihAnyOneStarter.GetLoading()?.StopLoading();

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

	private void Start()
	{
		Core.Ensure(() => Core.scenario.OnLoadedScenario(this));
	}

	private void Awake()
	{
		Core.Ensure(() => Core.scenario.OnScenarioAwaked(this));
	}

}
