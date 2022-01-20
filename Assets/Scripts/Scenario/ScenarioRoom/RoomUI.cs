using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class RoomUI : MonoBehaviour
{

    [SerializeField] Text m_Title;
    [SerializeField] Button m_Close;
    [SerializeField] Button m_GameStart;

    public void SetRoomInfo(string title)
    {
        m_Title.text = title;

    }

    void GoHome()
    {
        //Open Popup
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

        Core.scenario.OnLoadScenario(nameof(ScenarioHome));
    }

    // Start is called before the first frame update
    void Start()
    {
        m_Close.onClick.AddListener(GoHome);
    }
}
