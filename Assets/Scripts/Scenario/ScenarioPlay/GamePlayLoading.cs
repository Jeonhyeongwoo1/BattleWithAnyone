using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon.Pun;
using System;

public class GamePlayLoading : MonoBehaviourPunCallbacks
{
	[SerializeField] Text m_Title;
	[SerializeField] Text m_Master;
	[SerializeField] Text m_Player;
	[SerializeField] Slider m_MasterBar;
	[SerializeField] Slider m_PlayerBar;
	[SerializeField] TextAnimator m_TextAnimator;
	[SerializeField, Range(0, 5)] float m_MinLoadingDuration;

	float m_TotalCompletedCount = 3;
	public Transform masterTestCharacter;
	public Transform playerTestCharacter;

	public void SetInfo(string title, string masterName, string playerName)
	{
		m_Title.text = title;
		m_Master.text = masterName;
		m_Player.text = playerName;
	}

	public void StartGameLoading(UnityAction done)
	{
		StartCoroutine(Loading(done));
	}

	public void StopGameLoading()
	{
		StopAllCoroutines();
		m_TextAnimator.StopAllCoroutines();
		gameObject.SetActive(false);
	}

	IEnumerator Loading(UnityAction done)
	{
		float elapsed = 0;
		float loadedCount = 0;

		//Model, Theme
		GamePlayManager.MapPreferences map = Core.gameManager.GetMapPreference();
		if (map == null)
		{
			Debug.LogError("Map is null");
			yield break;
		}

		m_TextAnimator.StartAnimation();

		Core.models.Load(map.mapName, () =>
		{
			loadedCount++;
			IModel b = Core.models.Get();
			//TEST
			if (Core.gameManager.GetPlayersCharacter() == (null, null))
			{
				Core.gameManager.SetPlayersCharacter(masterTestCharacter, playerTestCharacter);
			}

			b.CreateCharacter(Core.gameManager.GetPlayersCharacter().Item1, Core.gameManager.GetPlayersCharacter().Item2);
			b.ReadyCamera(PhotonNetwork.IsMasterClient, () => loadedCount++);
		});

		Core.plugs.Load<XTheme>(() => loadedCount++);

		float value = 0;
		Slider progress = PhotonNetwork.IsMasterClient ? m_MasterBar : m_PlayerBar;
		bool isUpdated = false;
		while (value < 1)
		{
			elapsed += Time.deltaTime;
			value = MathF.Min((elapsed / m_MinLoadingDuration), (loadedCount / m_TotalCompletedCount));
			progress.value = Mathf.Lerp(0, 1, value);

			if((0.5f < value && value < 0.7f) && !isUpdated)
			{
				photonView.RPC("UpdateLoadingProgress", RpcTarget.Others, 0.5f);
				isUpdated = true;
			}

			yield return null;
		}

		photonView.RPC("UpdateLoadingProgress", RpcTarget.Others, 1.0f);

		yield return new WaitForSeconds(5f);
		done?.Invoke();
	}

	[PunRPC]
	public void UpdateLoadingProgress(float value)
	{
		Slider slider = PhotonNetwork.IsMasterClient ? m_PlayerBar : m_MasterBar;
		slider.value = value;
	}

	private void Start()
	{
		m_MasterBar.value = 0;
		m_PlayerBar.value = 0;
	}

}
