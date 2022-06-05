using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public interface IModel
{
    string Name { get; }
    Transform[] playerCreatePoints { get; }
    Transform poolObjectCreatePoints { get; }
    Transform itemCreatePoint { get; }
    void LoadedModel(UnityAction done = null);
    void UnLoadModel(UnityAction done = null);
    void ReadyCamera(bool isMaster, UnityAction done = null);
    IEnumerator ShootingCamera(bool isMaster, UnityAction done);
}

public class ModelDirector : MonoBehaviour
{
    //Model은 한가지만 로드가 가능하다.
    IModel m_LoadedModel = null;
    UnityAction<Scene, LoadSceneMode> m_SceneLoaded;

    void Log(string message)
    {
        if(XSettings.modelDirectorLog)
        {
            Debug.Log(message);
        }
    }

    public void Ensure(IModel model, UnityAction done = null)
    {
        string name = nameof(model);

        Scene scene = SceneManager.GetSceneByName(name);
        if (scene.isLoaded)
        {
            Log("Model :" + name + " Is Loaded");
            return;
        }

        OnLoadSceneAsync(name, done);
    }

    public void OnLoaded(IModel model)
    {
        Log("Model : " + model + "loaded");
        SceneManager.sceneLoaded -= m_SceneLoaded;
        m_LoadedModel = model;
        model.LoadedModel();
    }

    public void Unloaded(IModel model)
    {
        Log("Model : " + model + "Unloaded");
        m_LoadedModel = null;
        model.UnLoadModel();
    }

    public void Load<T>(UnityAction done = null) where T : IModel
    {
        string name = typeof(T).Name;
        if (m_LoadedModel != null || m_LoadedModel?.Name == name)
        {
            Debug.LogError("Already Loaded Model.. Close previous Loaded Model");
            UnloadScene(m_LoadedModel.Name);
        }

		OnLoadSceneAsync(name, done);
    }

    public void Unload<T>(UnityAction done = null) where T : IModel
    {
        string name = typeof(T).Name;
        Scene scene = SceneManager.GetSceneByName(name);
        if (!scene.isLoaded)
        {
            Log("Model :" + name + " Is Unloaded");
            done?.Invoke();
            return;
        }

		UnloadSceneAsync(name, done);
    }

    public void Load(string name, UnityAction done = null)
    {
        if (m_LoadedModel != null || m_LoadedModel?.Name == name)
        {
            Debug.LogWarning("Already Loaded Model.. Close previous Loaded Model");
            UnloadScene(m_LoadedModel.Name);
        }

		OnLoadSceneAsync(name, done);
    }

    public void Unload(string name, UnityAction done = null)
    {
        Scene scene = SceneManager.GetSceneByName(name);
        if (!scene.isLoaded)
        {
            Log("Model :" + name + " Is Unloaded");
            return;
        }

		UnloadSceneAsync(name, done);
    }

    public bool HasModel<T>() where T : IModel
    {
        return typeof(T).Name == m_LoadedModel.Name;
    }

    public bool Has() => m_LoadedModel != null;

    public IModel Get() => m_LoadedModel;

    void UnloadScene(string name, UnityAction done = null)
    {
        if (done != null)
        {
            SceneManager.sceneLoaded += m_SceneLoaded = (s, o) => done?.Invoke();
        }

        StartCoroutine(UnloadingSceneAsync(name, done));
    }

    void OnLoadScene(string name, UnityAction done = null)
    {
        if (done != null)
        {
            SceneManager.sceneLoaded += m_SceneLoaded = (s, o) => done?.Invoke();
        }

        SceneManager.LoadScene(name, LoadSceneMode.Additive);
    }

    void OnLoadSceneAsync(string name, UnityAction done = null)
    {
        StartCoroutine(OnLoaidngSceneAsync(name, done));
    }

    void UnloadSceneAsync(string name, UnityAction done = null)
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
