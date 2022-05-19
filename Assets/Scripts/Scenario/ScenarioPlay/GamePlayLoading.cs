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

            if(elapsed > maxWaitTime)
            {
                Photon.Pun.PhotonNetwork.Disconnect();
            }

            yield return null;
        }

        Transform player = PhotonNetwork.IsMasterClient ? Core.state.masterCharacter : Core.state.playerCharacter;
        if (player.TryGetComponent<PlayerController>(out var controller))
        {
            controller.CreateBullet();
            controller.CreateCollisionEffect();
            controller.photonView.RPC(nameof(controller.SetParent), RpcTarget.All, PhotonNetwork.IsMasterClient);
        }

        photonView.RPC(nameof(UpdateLoadingProgress), RpcTarget.Others, 1.0f);

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

    [PunRPC]
    void UpdateLoadingProgress(float value)
    {
        Slider slider = PhotonNetwork.IsMasterClient ? m_PlayerBar : m_MasterBar;
        slider.value = value;
    }
}
