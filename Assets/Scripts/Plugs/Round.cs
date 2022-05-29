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
	[SerializeField] Text m_RoundTxt;
	[SerializeField, Range(0, 1)] float m_OpenCloseDuration = 0.3f;
	[SerializeField] AnimationCurve m_Curve;
	[SerializeField] Vector3 m_TextShowingScale;

	public override void OpenAsync(UnityAction done = null)
	{
		m_RoundTxt.text = "READY";
		StartCoroutine(CoUtilize.Lerp((v) => m_CanvasGroup.alpha = v, 0, 1, m_OpenCloseDuration, done, m_Curve));
	}

	public override void CloseAsync(UnityAction done = null)
	{
		StartCoroutine(CoUtilize.Lerp((v) => m_CanvasGroup.alpha = v, 1, 0, m_OpenCloseDuration, () => Closed(done), m_Curve));
	}

    public void Close(UnityAction done = null)
    {
        m_CanvasGroup.alpha = 0;
        Closed(done);
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
			m_RoundTxt.text = m_RoundTxts[i];
			Core.audioManager.PlayUIAudio(AudioManager.UIType.ROUND);
			yield return CoUtilize.VLerp((v) => m_RoundTxt.transform.localScale = v, m_TextShowingScale, Vector3.one, 1, null, m_Curve);
		}

		done?.Invoke();
	}

	void Awake()
	{
		m_CanvasGroup.alpha = 0;
	}

}
