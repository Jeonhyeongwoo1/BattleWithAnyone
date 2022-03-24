using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class Round : BasePopup
{
    [SerializeField] string[] m_RoundTxts;
    [SerializeField] CanvasGroup m_CanvasGroup;
    [SerializeField] TextAnimator m_Text;
    [SerializeField, Range(0, 1)] float m_OpenCloseDuration = 0.3f;
    [SerializeField] AnimationCurve m_Curve;

    public override void OpenAsync(UnityAction done = null)
    {
        m_Text.animatorData.textmesh.text = "READY";
        StartCoroutine(CoUtilize.Lerp((v) => m_CanvasGroup.alpha = v, 0, 1, m_OpenCloseDuration, done, m_Curve));
    }

    public override void CloseAsync(UnityAction done = null)
    {
        StartCoroutine(CoUtilize.Lerp((v) => m_CanvasGroup.alpha = v, 1, 0, m_OpenCloseDuration, ()=> Closed(done), m_Curve));
    }

    public void ShowRoundInfo(UnityAction done)
    {
        StartCoroutine(ShowingInfo(() => CloseAsync(done)));
    }

    void Closed(UnityAction done)
    {
        removeOpenedPopup?.Invoke();
        done?.Invoke();
		gameObject.SetActive(false);
    }

    IEnumerator ShowingInfo(UnityAction done)
    {
        for (int i = 0; i < m_RoundTxts.Length; i++)
        {
            m_Text.animatorData.textmesh.text = m_RoundTxts[i];
            yield return new WaitForSeconds(1f);
        }

        done?.Invoke();
    }

    void Awake()
    {
        m_CanvasGroup.alpha = 0;
    }

}
