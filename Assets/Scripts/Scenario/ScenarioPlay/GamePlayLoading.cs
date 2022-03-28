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

	float m_TotalCompletedCount = 4;
	public Transform masterTestCharacter;
	public Transform playerTestCharacter;

	public void Prepare()
	{
		m_Title.text = Core.state.mapPreferences?.mapName;
		m_Master.text = PhotonNetwork.MasterClient.NickName;
		m_Player.text = PhotonNetwork.IsMasterClient ? PhotonNetwork.MasterClient.NickName : PhotonNetwork.NickName;
		m_MasterBar.value = 0;
		m_PlayerBar.value = 0;

		if (!gameObject.activeSelf)
		{
			gameObject.SetActive(true);
		}

		//test
		if (String.IsNullOrEmpty(m_Player.text))
		{
			m_Player.text = "TESTer";
		}

		if (String.IsNullOrEmpty(m_Master.text))
		{
			m_Master.text = MemberFactory.Get().mbr_id;
		}
	}

	public void StartLoading(UnityAction done)
	{
		StartCoroutine(Loading(done));
	}

	public void EndLoading()
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
		if (Core.state.mapPreferences == null)
		{
			Debug.LogError("Map is null");
			yield break;
		}

		m_TextAnimator.StartAnimation();

		Core.models.Load(Core.state.mapPreferences.mapName, () =>
		{
			loadedCount++;
			IModel b = Core.models.Get();

			//Only Test			
			if (Core.state.masterCharacter == null)
			{
				Core.state.masterCharacter = masterTestCharacter;
			}

			if (Core.state.playerCharacter == null)
			{
				Core.state.playerCharacter = playerTestCharacter;
			}

			OnCreateCharacters(() => loadedCount++);
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

			if ((0.5f < value && value < 0.7f) && !isUpdated)
			{
				photonView.RPC("UpdateLoadingProgress", RpcTarget.Others, 0.5f);
				isUpdated = true;
			}

			yield return null;
		}

		photonView.RPC("UpdateLoadingProgress", RpcTarget.Others, 1.0f);

		yield return new WaitForSeconds(2f);
		done?.Invoke();
	}

	void OnCreateCharacters(UnityAction done = null)
	{
		IModel model = Core.models.Get();
		if (model == null)
		{
			Debug.LogError("Map isn't Loaded!!");
			return;
		}

		Transform[] createPoints = model.playerCreatePoints;
		string name = PhotonNetwork.IsMasterClient ? Core.state.masterCharacter.name : Core.state.playerCharacter.name;
		int playerIndex = PhotonNetwork.IsMasterClient ? 0 : 1;
		Transform tr = null;
		tr = PhotonNetwork.Instantiate(XSettings.chracterPath + name, createPoints[playerIndex].position, createPoints[playerIndex].rotation, 0).transform;
		tr.SetParent(createPoints[playerIndex].parent);

		if (PhotonNetwork.IsMasterClient)
		{
			Core.state.masterCharacter = tr;
		}
		else
		{
			Core.state.playerCharacter = tr;
		}

		photonView.RPC(nameof(CreatedPlayerCharacter), RpcTarget.Others);
		done?.Invoke();
	}

	[PunRPC]
	public void CreatedPlayerCharacter()
	{
		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		foreach (var player in players)
		{
			PhotonView photonView = player.GetPhotonView();
			if (!photonView.IsMine)
			{
				if (PhotonNetwork.IsMasterClient)
				{
					Core.state.playerCharacter = player.transform;
				}
				else
				{
					Core.state.masterCharacter = player.transform;
				}

				IModel model = Core.models.Get();
				int playerIndex = PhotonNetwork.IsMasterClient ? 1 : 0;
				player.transform.SetParent(model.playerCreatePoints[playerIndex].parent);
			}
		}
	}

	[PunRPC]
	void UpdateLoadingProgress(float value)
	{
		Slider slider = PhotonNetwork.IsMasterClient ? m_PlayerBar : m_MasterBar;
		slider.value = value;
	}
}
