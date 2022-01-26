using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleWtihAnyOneStarter : MonoBehaviour
{
	public static BlockSkybox GetBlockSkybox()
	{
		return FindObjectOfType<BlockSkybox>();
	}

	private void Start()
	{
		Core.Ensure(() => OnLoadScenarioLoading());
	}

	void OnLoadScenarioLoading()
	{
		if (ScenarioDirector.scenarioReady)
		{
			Debug.Log("Scnesario is Loaded");
			return;
		}

		GetBlockSkybox()?.SetAlpha(1);
		Core.scenario.OnLoadScenario(nameof(ScenarioLoading));
	}
}
