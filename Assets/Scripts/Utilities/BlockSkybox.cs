using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BlockSkybox : MonoBehaviour
{
	[SerializeField] CanvasGroup m_CanvasGroup;
	[SerializeField, Range(0, 1)] float m_Duration;
	[SerializeField] AnimationCurve m_Curve;

	public void SetAlpha(float value)
	{
		m_CanvasGroup.alpha = value;
	}

	public void UseBlockSkybox(bool block, UnityAction done = null)
	{
		if (block && m_CanvasGroup.alpha == 1) { return; }
		if (!block && m_CanvasGroup.alpha == 0) { return; }

		int s = 0, e = 1;
		s = block ? 0 : 1;
		e = block ? 1 : 0;

		StartCoroutine(CoUtilize.Lerp((v) => m_CanvasGroup.alpha = v, s, e, m_Duration, done, m_Curve));
	}
}
