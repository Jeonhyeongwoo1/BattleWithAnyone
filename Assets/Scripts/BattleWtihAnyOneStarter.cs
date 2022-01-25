using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleWtihAnyOneStarter : MonoBehaviour
{
	private void Start()
	{
		Core.Ensure(()=> OnLoadScenarioLoading());
	}

	void OnLoadScenarioLoading()
	{
		if (ScenarioDirector.scenarioReady)
		{
			Debug.Log("Scnesario is Loaded");
			return;
		}
		Debug.LogError(ScenarioDirector.scenarioReady);
		Core.scenario.OnLoadScenario(nameof(ScenarioLoading));
	}

}
