using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ModelSkybox : MonoBehaviour
{
    public string modelName;
    public Material skybox;

    public bool useBlend;
    public bool useRepeat;
    public float initBlendValue;
    public float transitionDuration; 
    public float transitionDirection; //0, 1
    public float transitionStartTime;

    public bool useRotation;
    [Range(0, 360)] public float rotation;
    public float rotationSpeed;

    public bool useFog;
    public bool useAysncFog;
    public bool useRenderFog;
    public float fogActivationDuration;
    public float fogActivationStartTime;
    public float fogActivatingDuration;
    [Range(0, 1)] public float fogIntensity;
    [Range(0, 1)] public float fogHeight;
    [Range(0, 1)] public float fogSmoothness;
    [Range(0, 1)] public float fogFill;
    public float fogPosition;
    public Color fogColor;

    public Material defaultSkybox;
    public AnimationCurve blendCurve;
    public AnimationCurve fogActivatingCurve;

    public void SetupSkybox(bool returnOriginalValue)
    {
        if (string.IsNullOrEmpty(modelName)) { return; }
        if (skybox == null)
        {
            Debug.LogError("There isn't Skybox material");
            RenderSettings.skybox = defaultSkybox;
            return;
        }

        RenderSettings.skybox = returnOriginalValue ? defaultSkybox : skybox;

        if(useBlend)
        {
            skybox.SetFloat("_CubemapTransition", returnOriginalValue ? 0 : initBlendValue);

            if(!returnOriginalValue)
            {
                StartCoroutine(BlendingCubemap());
            }

        }

        if (useRotation)
        {
            skybox.SetFloat("_EnableRotation", returnOriginalValue ? 0 : 1);
            skybox.SetFloat("_Rotation", returnOriginalValue ? 0 : rotation);
            skybox.SetFloat("_RotationSpeed", returnOriginalValue ? 0 : rotationSpeed);
        }

        
        if (useFog)
        {
            RenderSettings.fog = useRenderFog;
            RenderSettings.fogColor = fogColor;

            skybox.SetFloat("_EnableFog", returnOriginalValue ? 0 : 1);
            skybox.SetFloat("_FogHeight", returnOriginalValue ? 0 : fogHeight);
            skybox.SetFloat("_FogSmoothness", returnOriginalValue ? 0 : fogSmoothness);
            skybox.SetFloat("_FogFill", returnOriginalValue ? 0 : fogFill);

            if (useAysncFog && !returnOriginalValue)
            {
                StartCoroutine(ActivatingFog());
            }
            else
            {
                skybox.SetFloat("_EnableFog", returnOriginalValue ? 0 : 1);
                skybox.SetFloat("_FogIntensity", returnOriginalValue ? 0 : fogIntensity);
            }
        }

    }

    IEnumerator ActivatingFog()
    {
        skybox.SetFloat("_FogIntensity", 0);
        skybox.SetFloat("_EnableFog", 0);

        yield return new WaitForSeconds(fogActivationStartTime);

        skybox.SetFloat("_EnableFog", 1);
        float elapsed = 0;
        float start = 0;
        float end = fogIntensity;

        while(elapsed < fogActivatingDuration)
        {
            elapsed += Time.deltaTime;
            //Intensity
            float value = Mathf.Lerp(start, end, fogActivatingCurve.Evaluate(elapsed / fogActivatingDuration));
            skybox.SetFloat("_FogIntensity", value);
            yield return null;
        }

    }

    IEnumerator BlendingCubemap()
    {

        WaitForSeconds waitForSeconds = new WaitForSeconds(transitionStartTime);
        float elapsed = 0;
        float start = initBlendValue;
        float end = transitionDirection;

        do
        {
            yield return waitForSeconds;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float value = Mathf.Lerp(start, end, blendCurve.Evaluate(elapsed / transitionDuration));
                skybox.SetFloat("_CubemapTransition", value);
                yield return null;
            }

            elapsed = 0;
            start = end;
            end = end == initBlendValue ? transitionDirection : initBlendValue;
        } while (useRepeat);

    }

}
