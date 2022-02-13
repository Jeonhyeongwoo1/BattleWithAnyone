using System.Collections;
using UnityEngine;

public class LoginBackground : MonoBehaviour
{
    public bool StopChangingBackground = false;

    [SerializeField] CanvasGroup m_Moutain;
    [SerializeField] CanvasGroup m_Desert;
    [SerializeField] CanvasGroup m_Graveyard;
    [SerializeField, Range(0, 10)] float m_ChangeDuration;
    [SerializeField, Range(0, 10)] float m_WaitTime;
    [SerializeField] AnimationCurve m_Curve;

    public void LoginBackgroundAnimation()
    {
        StartCoroutine(ChangeBackground());
    }

    IEnumerator ChangeBackground()
    {
        while (!StopChangingBackground)
        {
            yield return new WaitForSeconds(m_WaitTime);
            yield return CoUtilize.Lerp((v) => m_Desert.alpha = v, 0, 1, m_ChangeDuration, () => m_Moutain.alpha = 0, m_Curve);
            yield return new WaitForSeconds(m_WaitTime);
            yield return CoUtilize.Lerp((v) => m_Graveyard.alpha = v, 0, 1, m_ChangeDuration, () => { m_Desert.alpha = 0; m_Moutain.alpha = 1; }, m_Curve);
            yield return new WaitForSeconds(m_WaitTime);
            yield return CoUtilize.Lerp((v) => m_Graveyard.alpha = v, 1, 0, m_ChangeDuration, null, m_Curve);
            yield return new WaitForSeconds(m_WaitTime);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        LoginBackgroundAnimation();
    }
}
