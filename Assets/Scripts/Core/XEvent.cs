using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void Event(string key, object o);

public class XEvent : MonoBehaviour
{
    Dictionary<string, Listener> m_Listeners = new Dictionary<string, Listener>();

    public class Listener
    {
        event Event listener;
        object eventValue;

        public void AddListener(string key, Event e)
        {
            listener += e;
        }

        public void RemoveListener(string key, Event e)
        {
            listener -= e;
        }

        public void Call(string key, object o)
        {
            eventValue = o;
            listener?.Invoke(key, o);
        }

        public object GetEventValue() => eventValue;
    }
    
    public Listener GetListener(string key)
    {
        Listener listener = null;
        if(!m_Listeners.TryGetValue(key, out listener))
        {
            listener = new Listener();
            m_Listeners.Add(key, listener);
        }

        return listener;
    }

    public void Watch(string key, Event e)
    {  
        GetListener(key)?.AddListener(key, e);
    }
    
    public void Stop(string key, Event e)
    {
        GetListener(key)?.RemoveListener(key, e);
    }

    public void Raise(string key, object o)
    {
        GetListener(key)?.Call(key, o);
    }

}
