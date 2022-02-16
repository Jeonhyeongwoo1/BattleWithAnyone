using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class BasePopup : MonoBehaviour
{
	public UnityAction removeOpenedPopup;
    public abstract void OpenAsync(UnityAction done = null);
    public abstract void CloseAsync(UnityAction done = null);
}

public class Popups : MonoBehaviour, IPlugable
{
    public string plugName => nameof(Popups);
    [SerializeField] List<BasePopup> m_Popups = new List<BasePopup>();
    [SerializeField] List<BasePopup> m_IsOpenedPopups = new List<BasePopup>();

    public void Open(UnityAction done = null)
    {
        gameObject.SetActive(true);
        done?.Invoke();
    }

    public void Close(UnityAction done = null) => done?.Invoke();

    public bool HasPopup<T>() where T : BasePopup
    {
        foreach (BasePopup p in m_Popups)
        {
            if (p is T) { return true; }
        }
        return false;
    }

    public T Get<T>() where T : BasePopup
    {
        foreach (BasePopup p in m_Popups)
        {
            if (p is T) { return (T)p; }
        }
        return null;
    }

    public bool IsOpened<T>() where T : BasePopup
    {
        foreach (BasePopup p in m_Popups)
        {
            if (p is T) { return true; }
        }
        return false;
    }

    public void OpenPopupAsync<T>(UnityAction done = null) where T : BasePopup
    {
        foreach (BasePopup p in m_Popups)
        {
            if (p is T)
            {
                if (!p.gameObject.activeSelf) { p.gameObject.SetActive(true); }

                p.OpenAsync(() =>
				{
					m_IsOpenedPopups.Add(p);
					p.removeOpenedPopup = () => m_IsOpenedPopups.Remove(p);
                    done?.Invoke();
                });
                return;
            }
        }

        Debug.LogError("There is no Popup : " + typeof(T).Name);
        done?.Invoke();
    }

    public void ClosePopupAsync<T>(UnityAction done = null) where T : BasePopup
    {
        foreach (BasePopup p in m_Popups)
        {
            if (p is T)
            {
                p.CloseAsync(() =>
                {
                    m_IsOpenedPopups.Remove(p);
                    p.gameObject.SetActive(false);
                    done?.Invoke();
                });
                return;
            }
        }

        Debug.LogError("There is no Popup : " + typeof(T).Name);
        done?.Invoke();
    }

    // Start is called before the first frame update
    void Start()
    {
        Core.Ensure(() => Core.plugs.Loaded(this));
    }

    private void OnDestroy()
    {
        Core.Ensure(() => Core.plugs.Unloaded(this));
    }

}
