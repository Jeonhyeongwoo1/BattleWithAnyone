using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Photon.Pun;

public class ScenarioHome : MonoBehaviourPunCallbacks, IScenario
{
	public string scenarioName => typeof(ScenarioHome).Name;
	public UserInfo userInfo;
	public RoomMenu roomMenu;

	[SerializeField] Button m_Exit;
	[SerializeField] RectTransform m_LoginForm;
	[SerializeField] InputField m_Id;
	[SerializeField] InputField m_Password;
	[SerializeField] Button m_LoginBtn;

	public void OnScenarioPrepare(UnityAction done)
	{
		BattleWtihAnyOneStarter.GetBlockSkybox()?.gameObject.SetActive(false);
		BattleWtihAnyOneStarter.GetLoading()?.StartLoading();
		Core.plugs.DefaultEnsure();
		done?.Invoke();
	}

	public void OnScenarioStandbyCamera(UnityAction done)
	{
		done?.Invoke();
	}

	public void OnScenarioStart(UnityAction done)
	{
        Debug.LogError(PhotonNetwork.NetworkClientState);
		Core.networkManager.ConnectPhotonNetwork(() => ConnectedPhotonNetwork(null));
		done?.Invoke();
	}

	void ConnectedPhotonNetwork(UnityAction done)
	{
		BattleWtihAnyOneStarter.GetLoading()?.StopLoading();
		m_Exit.gameObject.SetActive(true);
		roomMenu.gameObject.SetActive(true);
		if (!Core.networkManager.isLogined)
		{
			m_LoginForm.gameObject.SetActive(true);
		}
		else
		{

			PhotonNetwork.JoinLobby();
		}

		done?.Invoke();
	}

	public void OnScenarioStop(UnityAction done)
	{
		StopAllCoroutines();
		done?.Invoke();
	}

	void OnLogin()
	{
		string id = m_Id.text;
		string password = m_Password.text;

		if (string.IsNullOrEmpty(id))
		{
			m_Id.ActivateInputField();
			Debug.Log("Input id");
			return;
		}

		if (string.IsNullOrEmpty(password))
		{
			m_Password.ActivateInputField();
			Debug.Log("Input password");
			return;
		}

		Core.networkManager.ReqLogin(id, password, LoginSuccessed, LoginFailed);

	}

	void LoginSuccessed(string data)
	{
		if (string.IsNullOrEmpty(data))
		{
			LoginFailed(null);
			return;
		}

		Member member = JsonUtility.FromJson<Member>(data);

		Core.networkManager.SetPlayerName(member.mbr_id);
		userInfo.SetUserInfo(member.mbr_id);
		Core.networkManager.isLogined = true;
		PhotonNetwork.JoinLobby();
	}

	public override void OnJoinedLobby()
	{
		roomMenu.OnEnableRoomMenu();
		userInfo.gameObject.SetActive(true);
		m_LoginForm.gameObject.SetActive(false);
	}

	void LoginFailed(string error)
	{
		//Error
		m_Id.text = null;
		m_Password.text = null;

	}

	private void Start()
	{
		Core.Ensure(() => Core.scenario.OnLoadedScenario(this));
	}

	private void Awake()
	{
		Core.Ensure(() => Core.scenario.OnScenarioAwaked(this));
		m_LoginBtn.onClick.AddListener(OnLogin);
	}

}