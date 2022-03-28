using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System;
using Photon.Pun;

public class GamePlayManager : MonoBehaviourPunCallbacks
{
    public enum Status { None, Prepare, Prepared, RoundPlaying, RoundDone, GameDone, GameEnd }

    public string roomName { get; set; }

    Status m_Status;

    public Status GetState() => m_Status;
    public void SetState(Status status) => m_Status = status;

    public void Log(string message)
    {
        Debug.Log(message);
    }

    //GamePlay Routine
    public void OnGamePrepare()
    {
        Log("Getting ready to play");
        Popups popups = Core.plugs.Get<Popups>();
        popups.OpenPopupAsync<Round>(() =>
        {
            Round round = popups.Get<Round>();
            round.ShowRoundInfo(OnGamePrepared);
        });
    }

    void OnGamePrepared()
    {
        Log("Game ready");
        XState.MapPreferences map = Core.state.mapPreferences;
        XTheme theme = Core.plugs.Get<XTheme>();
        theme.SetGameInfo(map.roundTime.ToString(), map.numberOfRound.ToString(),
                                                    PhotonNetwork.MasterClient.NickName,
                                                    PhotonNetwork.IsMasterClient ? PhotonNetwork.MasterClient.NickName : PhotonNetwork.NickName);
        Core.plugs.Get<XTheme>().Open();
        theme.gameTimer.StartTimer();
        StartCoroutine(GamePlaying(OnRoundDone));
    }

    IEnumerator GamePlaying(UnityAction done)
    {
        Log("Game Playing");

        while (m_Status == Status.RoundPlaying) { yield return null; }

        done?.Invoke();
    }

    void OnRoundDone()
    {
        Log("Round Done");
    }

    void OnGameDone()
    {

    }

    void GameEnd()
    {

    }

}
