using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeBlockingObject : MonoBehaviour
{
    public enum FadeMode
    {
        Transparent,
        Fade
    }

    [SerializeField] FadeMode m_FadeMode = FadeMode.Fade;
    [SerializeField] LayerMask m_BlockLayerMask;
    [SerializeField] float m_MaxDistance = 10;
    [SerializeField] float m_FadeAlpha = 0.3f;
    [SerializeField] float m_FadeSpeed;
    [SerializeField] float m_FPS = 60f;
    [SerializeField] AnimationCurve m_Curve;

    List<FadingModel> m_BlockViewModels = new List<FadingModel>();
    Dictionary<FadingModel, Coroutine> m_RunningFadingModels = new Dictionary<FadingModel, Coroutine>();
    RaycastHit[] raycastHits = new RaycastHit[10];

    public void StartCheckBlockingObject()
    {
    //    StartCoroutine(CheckingBlockingObject());
    }

    public void StopCheckBlockingObject()
    {
        StopAllCoroutines();
    }

    IEnumerator CheckingBlockingObject()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);

        while(true)
        {
            int hits = Physics.RaycastNonAlloc(transform.position, transform.forward, raycastHits, m_MaxDistance, m_BlockLayerMask);
            if (hits > 0)
            {
                for (int i = 0; i < raycastHits.Length; i++)
                {
                    if (raycastHits[i].transform == null) { continue; }
                    if (!raycastHits[i].transform.TryGetComponent<FadingModel>(out var fadingModel)) { continue; }
                    if (fadingModel.materials == null) { continue; }
                    if (m_BlockViewModels.Contains(fadingModel)) { continue; }
                    if (m_RunningFadingModels.ContainsKey(fadingModel)) { continue; }

                    m_BlockViewModels.Add(fadingModel);
                    m_RunningFadingModels.Add(fadingModel, StartCoroutine(Fadeout(fadingModel)));

                }
            }

            for (int i = 0; i < m_BlockViewModels.Count; i++)
            {
                bool isHitting = false;
                for (int j = 0; j < hits; j++)
                {
                    if (raycastHits[j].transform == null) { continue; }
                    if (!raycastHits[j].transform.TryGetComponent<FadingModel>(out var fadingModel)) { continue; }
                    if (m_BlockViewModels[i] == fadingModel)
                    {
                        isHitting = true;
                        break;
                    }
                }

                if (!isHitting)
                {
                    FadingModel fadingModel = m_BlockViewModels[i];
                    if (m_RunningFadingModels.ContainsKey(fadingModel))
                    {
                        StopCoroutine(m_RunningFadingModels[fadingModel]);
                        m_RunningFadingModels.Remove(fadingModel);
                    }

                    if (fadingModel.materials[0].HasProperty("_Color"))
                    {
                        int materialsCount = fadingModel.materials.Count;
                        for (int k = 0; k < materialsCount; k++)
                        {
                            Material mat = fadingModel.materials[k];
                            if (mat.HasProperty("_Color"))
                            {
                                mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 1);
                            }
                        }

                        for (int k = 0; k < materialsCount; k++)
                        {
                            Material mat = fadingModel.materials[k];
                            if (m_FadeMode == FadeMode.Fade)
                            {
                                mat.DisableKeyword("_ALPHABLEND_ON");
                            }
                            else
                            {
                                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                            }

                            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                            mat.SetInt("_ZWrite", 1); // re-enable Z Writing
                            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                        }
                    }

                    m_BlockViewModels.RemoveAt(i);
                }
            }

            //raycastHits.Initialize();
            ClearHits();
            yield return waitForSeconds;
        }
    
    }

    IEnumerator Fadeout(FadingModel fadingModel)
    {
        float fValue = 1 / m_FPS;
        int materialsCount = fadingModel.materials.Count;
        for (int i = 0; i < materialsCount; i++)
        {
            Material mat = fadingModel.materials[i];
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha); // affects both "Transparent" and "Fade" options
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha); // affects both "Transparent" and "Fade" options
            mat.SetInt("_ZWrite", 0); // disable Z writing
            if (m_FadeMode == FadeMode.Fade)
            {
                mat.EnableKeyword("_ALPHABLEND_ON");
            }
            else
            {
                mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            }

            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        if(fadingModel.materials[0].HasProperty("_Color"))
        {
            int alpha = 1;
            while(alpha > m_FadeAlpha)
            {
                for (int i = 0; i < materialsCount; i++)
                {
                    Material mat = fadingModel.materials[i];
                    if (!mat.HasProperty("_Color")) { continue; }
                    float value = Mathf.Lerp(mat.color.a, m_FadeAlpha, m_Curve.Evaluate(fValue * m_FadeSpeed));
                    mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, value);

                }

                yield return fValue;
            }
        }

        if(m_RunningFadingModels.ContainsKey(fadingModel))
        {
            m_RunningFadingModels.Remove(fadingModel);
        }
    }


    private void ClearHits()
    {
        RaycastHit hit = new RaycastHit();
        for (int i = 0; i < raycastHits.Length; i++)
        {
            raycastHits[i] = hit;
        }
    }

}
