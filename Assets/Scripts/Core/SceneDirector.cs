using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public interface IPlayable
{
}

public class SceneDirector<XType> : MonoBehaviour where XType : IPlayable
{
    private List<XType> m_Loaded = new List<XType>();

    public void Ensure<T>(UnityAction done = null) where T : XType
    {
        string name = typeof(T).Name;

        Scene scene = SceneManager.GetSceneByName(name);
        if (scene.name == null || scene.isLoaded)
        {
            Debug.Log("X Type :" + name + " Is Loaded");
            return;
        }


        OnLoadSceneAsync(name, done);
    }

    public bool Has(string name)
    {
        XType xType = m_Loaded.Find((v) => v.ToString() == name);
        return xType != null ? true : false;
    }

    public XType GetXType(string name)
    {
        return m_Loaded.Find((v) => v.ToString() == name);
    }
    
    public void OnLoadSceneAsync(string name, UnityAction done = null)
    {
        StartCoroutine(OnLoaidngSceneAsync(name, done));
    }

    public void UnloadSceneAsync(string name, UnityAction done = null)
    {
        StartCoroutine(UnloadingSceneAsync(name, done));
    }

    IEnumerator UnloadingSceneAsync(string name, UnityAction done = null)
    {
        AsyncOperation async = SceneManager.UnloadSceneAsync(name);
        while (!async.isDone) { yield return null; }

        done?.Invoke();
    }

    IEnumerator OnLoaidngSceneAsync(string name, UnityAction done = null)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
        while (!async.isDone) { yield return null; }

        done?.Invoke();
    }

}
