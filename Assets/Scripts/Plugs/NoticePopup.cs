using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NoticePopup : BasePopup
{
    [SerializeField] Image m_NoticeDurationIcon;
    [SerializeField] Text m_Content;
    [SerializeField] Transform m_Frame;
    [SerializeField, Range(0, 3)] float m_NoticeOpenTime;
    [SerializeField, Range(0, 1)] float m_OpenCloseDuration = 0.3f;
    [SerializeField] AnimationCurve m_Curve;

    public static string content { get; set; }

    public override void OpenAsync(UnityAction done = null)
    {
        if (!string.IsNullOrEmpty(content)) { m_Content.text = content; }

        Core.audioManager.PlayUIAudio(AudioManager.UIType.POPUPOPEN);
        StartCoroutine(CoUtilize.VLerp((v) => m_Frame.localScale = v, Vector3.zero, Vector3.one, m_OpenCloseDuration, () => Opened(done), m_Curve));
    }

    public override void CloseAsync(UnityAction done = null)
    {
        StartCoroutine(CoUtilize.VLerp((v) => m_Frame.localScale = v, Vector3.one, Vector3.zero, m_OpenCloseDuration, () => Closed(done), m_Curve));
    }

    void Opened(UnityAction done)
    {
        done?.Invoke();
        StartCoroutine(NoticeOpenDuration());
    }

    void Closed(UnityAction done)
    {
        m_Content.text = null;
        content = null;
        gameObject.SetActive(false);
        done?.Invoke();
    }

    IEnumerator NoticeOpenDuration()
    {
        float elapsed = 0;

        while (elapsed < m_NoticeOpenTime)
        {
            elapsed += Time.deltaTime;
            m_NoticeDurationIcon.fillAmount = Mathf.Lerp(1, 0, m_Curve.Evaluate(elapsed / m_NoticeOpenTime));
            yield return null;
        }

		CloseAsync(() => removeOpenedPopup?.Invoke());
    }
}
