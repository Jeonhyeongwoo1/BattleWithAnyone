using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BattleWtihAnyOneStarter : MonoBehaviour
{
	public XSettings.Profile profile;

	[SerializeField] BlockSkybox m_BlockSkybox;
	[SerializeField] LoadingAnimation m_LoadingAnimation;

	static BlockSkybox blockSkybox;
	static LoadingAnimation loadingAnimation;

	public static LoadingAnimation GetLoading()
	{
		if (!loadingAnimation)
		{
			loadingAnimation = FindObjectOfType<LoadingAnimation>();
		}

		return loadingAnimation;
	}

	public static BlockSkybox GetBlockSkybox()
	{
		if (!blockSkybox)
		{
			blockSkybox = FindObjectOfType<BlockSkybox>();
		}

		return blockSkybox;
	}

	private void Start()
	{
		Core.Ensure(() => OnLoadScenarioLoading());
	}

	private void Awake()
	{
		Ensure();
		GetBlockSkybox()?.gameObject.SetActive(false);
		GetLoading()?.gameObject.SetActive(false);
	}

	T CopyTo<T>(ref T component) where T : MonoBehaviour
	{
		return component as T;
	}

	void Ensure()
	{
		blockSkybox = CopyTo<BlockSkybox>(ref m_BlockSkybox);
		loadingAnimation = CopyTo<LoadingAnimation>(ref m_LoadingAnimation);
	}

	void OnLoadScenarioLoading()
	{
		Core.settings.profile = profile;

		if (ScenarioDirector.scenarioReady)
		{
			Debug.Log("Scnesario is Loaded");
			return;
		}

		blockSkybox.SetAlpha(1);
		blockSkybox.gameObject.SetActive(true);
		Core.scenario.OnLoadScenario(nameof(ScenarioLoading));
	}
}
