using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class Round : BasePopup
{
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

	}


	void Awake()
	{
		m_CanvasGroup.alpha = 0;
	}

}
