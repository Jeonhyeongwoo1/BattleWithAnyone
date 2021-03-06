using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public interface IPlugable
{
    string plugName { get; }
    void Open(UnityAction done = null);
    void Close(UnityAction done = null);
}

public class PlugDirector : MonoBehaviour
{
    private List<IPlugable> m_Loaded = new List<IPlugable>();

    public void Ensure<T>(UnityAction done = null) where T : IPlugable
    {
        string name = typeof(T).Name;
        Scene scene = SceneManager.GetSceneByName(name);
        if (scene.name != null || scene.isLoaded)
        {
            done?.Invoke();
            return;
        }

        OnLoadSceneAsync(name, done);
    }

    public void Load<T>(UnityAction done = null) where T : IPlugable
    {
        string name = typeof(T).Name;
        Scene scene = SceneManager.GetSceneByName(name);
        if (Has(name) || scene.isLoaded)
        {
            done?.Invoke();
            return;
        }

        OnLoadSceneAsync(name, done);
    }

    public void Unload<T>(UnityAction done = null) where T : IPlugable
    {
        string name = typeof(T).Name;
        if (!Has(name))
        {
            done?.Invoke();
            return;
        }

        UnloadSceneAsync(name, done);
    }

    public bool Has<T>() where T : IPlugable => Has(typeof(T).Name);
    public void Loaded(IPlugable plugable)
    {
        Log("On Loaded Plugable : " + plugable.plugName);
        m_Loaded.Add(plugable);
    }

    public void Unloaded(IPlugable plugable)
    {
        Log("On Unloaded plugable : " + plugable.plugName);
        m_Loaded.Remove(plugable);
    }

    public T Get<T>() where T : IPlugable
    {
        string name = typeof(T).Name;
        IPlugable plugable = m_Loaded.Find((v) => v.plugName == name);
        return (T)plugable;
    }

    public void DefaultEnsure()
    {
        Ensure<Popups>();
        Ensure<MapSettings>();
    //    Ensure<XTheme>();
    }

    public void OnLoadSceneAsync(string name, UnityAction done = null)
    {
        StartCoroutine(OnLoaidngSceneAsync(name, done));
    }

    public void UnloadSceneAsync(string name, UnityAction done = null)
    {
        StartCoroutine(UnloadingSceneAsync(name, done));
    }

    bool Has(string name)
    {
        IPlugable plugable = m_Loaded.Find((v) => v.plugName == name);
        return plugable != null;
    }

    void Log(string message)
    {
        if(XSettings.plugDirectorLog)
        {
            Debug.Log(message);
        }
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
