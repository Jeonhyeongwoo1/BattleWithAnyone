using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class GamePlayLoading : MonoBehaviour
{
    [SerializeField] Text m_Title;
    [SerializeField] Text m_Master;
    [SerializeField] Text m_Player;
    [SerializeField] Slider m_MasterBar;
    [SerializeField] Slider m_PlayerBar;
    [SerializeField] TextAnimator m_TextAnimator;
    [SerializeField, Range(0, 5)] float m_MinLoadingDuration;

    float m_TotalLoadingCount = 2;
    public Transform testCharacter;

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

        Core.models.Load(map.mapName, () => loadedCount++);

        XTheme theme = Core.plugs.Get<XTheme>();
        theme?.Open(() =>
        {
            theme.SetGameInfo(map.roundTime.ToString(), map.numberOfRound.ToString(), m_Master.text, m_Player.text);
            loadedCount++;
        });

        while (elapsed < m_MinLoadingDuration)
        {
            elapsed += Time.deltaTime;
            m_MasterBar.value = Mathf.Lerp(0, 1, elapsed / m_MinLoadingDuration);
            yield return null;
        }

        done?.Invoke();
    }

    private void Start()
    {
        m_MasterBar.value = 0;
        m_PlayerBar.value = 0;
    }

}
