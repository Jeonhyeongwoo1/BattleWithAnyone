using System.Collections;
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
		if (Core.networkManager.member != null && PhotonNetwork.IsConnected)
		{
			Core.networkManager.WaitStateToConnectedToMasterServer(() => PhotonNetwork.JoinLobby());
			done?.Invoke();
			return;
		}

		Core.networkManager.ConnectPhotonNetwork(() => Core.networkManager.WaitStateToConnectedToMasterServer(JoinLobby));
		done?.Invoke();
	}

    //Local 
    void JoinLobby()
    {
        Core.networkManager.member = MemberFactory.Get();
        PhotonNetwork.JoinLobby();
    }

    public void OnScenarioStop(UnityAction done)
    {
        StopAllCoroutines();
        done?.Invoke();
    }

    public override void OnJoinedLobby()
    {
        BattleWtihAnyOneStarter.GetLoading()?.StopLoading();
        roomMenu.gameObject.SetActive(true);
        userInfo.SetUserInfo(Core.networkManager.member.mbr_id);
        userInfo.gameObject.SetActive(true);
        m_Exit.gameObject.SetActive(true);
    }

    private void Start()
    {
        Core.Ensure(() => Core.scenario.OnLoadedScenario(this));
    }

    private void Awake()
    {
        Core.Ensure(() => Core.scenario.OnScenarioAwaked(this));
        m_Exit.onClick.AddListener(() => Application.Quit());
    }

}