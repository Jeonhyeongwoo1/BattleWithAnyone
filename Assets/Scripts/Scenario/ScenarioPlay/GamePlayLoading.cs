using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon.Pun;
using System;

public class GamePlayLoading : MonoBehaviourPunCallbacks
{

    [Serializable]
    public struct Backgrounds
    {
        public string name;
        public Sprite sprite;
    }

    [SerializeField] Text m_Title;
    [SerializeField] Text m_Master;
    [SerializeField] Text m_Player;
    [SerializeField] Slider m_MasterBar;
    [SerializeField] Slider m_PlayerBar;
    [SerializeField] TextAnimator m_TextAnimator;
    [SerializeField] Image m_Background;
    [SerializeField, Range(0, 5)] float m_MinLoadingDuration;
    [SerializeField] Backgrounds[] m_BackgroundDatas;

    float m_OtherPlayerLoadingProcess = 0f;
    float m_TotalCompletedCount = 4;
    public Transform masterTestCharacter;
    public Transform playerTestCharacter;

    public void Prepare()
    {
        string mapName = Core.state.mapPreferences?.mapName;
        m_Title.text = mapName;
        m_Master.text = PhotonNetwork.MasterClient.NickName;
        m_Player.text = Core.gameManager.playerName;
        m_MasterBar.value = 0;
        m_PlayerBar.value = 0;
        SetBackground(mapName);

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        BattleWtihAnyOneStarter.GetLoading()?.StopLoading();
        BattleWtihAnyOneStarter.GetBlockSkybox()?.gameObject.SetActive(false);
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
            OnCreateCharacters(() => loadedCount++);
            b.ReadyCamera(PhotonNetwork.IsMasterClient, () => loadedCount++);
        });

        Core.plugs.Load<XTheme>(() => loadedCount++);

        float value = 0;
        Slider progress = PhotonNetwork.IsMasterClient ? m_MasterBar : m_PlayerBar;
        bool isUpdated = false;
        int maxWaitTime = 10;
        while (value < 1)
        {
            elapsed += Time.deltaTime;
            value = MathF.Min((elapsed / m_MinLoadingDuration), (loadedCount / m_TotalCompletedCount));
            progress.value = Mathf.Lerp(0, 1, value);

            if ((0.5f < value && value < 0.7f) && !isUpdated)
            {
                photonView.RPC(nameof(UpdateLoadingProgress), RpcTarget.Others, 0.5f);
                isUpdated = true;
            }

            if (elapsed > maxWaitTime)
            {
                Photon.Pun.PhotonNetwork.Disconnect();
            }

            yield return null;
        }

        Transform player = PhotonNetwork.IsMasterClient ? Core.state.masterCharacter : Core.state.playerCharacter;
        if (player.TryGetComponent<PlayerController>(out var controller))
        {
            controller.rotationSpeed = Core.state.playerRotSensitivity;
            controller.OnVolumChanged(nameof(Core.state.playerSound), Core.state.playerSound);
            controller.OnSoundMute(nameof(Core.state.playerSoundMute), Core.state.playerSoundMute);
            controller.CreateBullet();
            controller.CreateCollisionEffect();
            controller.photonView.RPC(nameof(controller.SetParent), RpcTarget.All, PhotonNetwork.IsMasterClient);
        }

        //completed
        photonView.RPC(nameof(UpdateLoadingProgress), RpcTarget.Others, 1.0f);
        elapsed = 0;
        while (m_OtherPlayerLoadingProcess < 1)
        {
            if (elapsed > maxWaitTime)
            {
                Core.gameManager.OnOtherPlayerDisconnectedDuringLoading();
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(1f);
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

        //Only Test			
        if (Core.state.masterCharacter == null)
        {
            Core.state.masterCharacter = masterTestCharacter;
        }

        if (Core.state.playerCharacter == null)
        {
            Core.state.playerCharacter = playerTestCharacter;
        }

        string name = PhotonNetwork.IsMasterClient ? Core.state.masterCharacter.name : Core.state.playerCharacter.name;
        Transform[] createPoints = model.playerCreatePoints;
        int index = PhotonNetwork.IsMasterClient ? 0 : 1;
        Transform player = PhotonNetwork.Instantiate(XSettings.chracterPath + name, createPoints[index].position, createPoints[index].rotation, 0).transform;
        if (PhotonNetwork.IsMasterClient)
        {
            Core.state.masterCharacter = player;
        }
        else
        {
            Core.state.playerCharacter = player;
        }

        done?.Invoke();
    }

    void SetBackground(string mapName)
    {
        foreach (Backgrounds background in m_BackgroundDatas)
        {
            if (background.name == mapName)
            {
                m_Background.sprite = background.sprite;
                break;
            }
        }
    }

    [PunRPC]
    void UpdateLoadingProgress(float value)
    {
        Slider slider = PhotonNetwork.IsMasterClient ? m_PlayerBar : m_MasterBar;
        slider.value = value;
        m_OtherPlayerLoadingProcess = value;
    }
}
