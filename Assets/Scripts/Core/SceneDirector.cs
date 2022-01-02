using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Model : SceneDirector<Model>, IPlayable
{

}

public interface IPlayable
{

}

public class SceneDirector<XType> : MonoBehaviour where XType : IPlayable
{
    List<XType> m_Loaded = new List<XType>();

    public void Ensure(XType xType, UnityAction done = null)
    {
        string name = nameof(xType);

        Scene scene = SceneManager.GetSceneByName(name);
        if (scene.name == null || scene.isLoaded)
        {
            Debug.Log(xType + " Scene is Loaded");
            return;
        }

        OnLoadSceneAsync(xType, done);
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
    
    public void OnLoadSceneAsync(XType xType, UnityAction done = null)
    {
        m_Loaded.Add(xType);
        StartCoroutine(OnLoaidngSceneAsync(nameof(xType), done));
    }

    public void UnloadSceneAsync(XType xType, UnityAction done = null)
    {
        m_Loaded.Remove(xType);
        StartCoroutine(UnloadingSceneAsync(nameof(xType), done));
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
