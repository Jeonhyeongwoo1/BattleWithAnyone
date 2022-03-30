using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public delegate void State(string key, object o);

public class States : MonoBehaviour
{
    Dictionary<string, Observer> m_Observers = new Dictionary<string, Observer>();

    public class Observer
    {
        event State observer;
        object stateValue;

        public void Listen(string key, State state)
        {
            observer += state;
        }

        public void Stop(string key, State state)
        {
            observer -= state;
        }

        public void Set(string key, object o)
        {
            stateValue = o;
            observer?.Invoke(key, o);
        }

    }

    public Observer GetObserver(string key)
    {
        Observer state = null;
        if (!m_Observers.TryGetValue(key, out state))
        {
            state = new Observer();
            m_Observers.Add(key, state);
        }

        return state;
    }

    public void Set(string key, object o)
    {
        GetObserver(key)?.Set(key, o);
    }

    public void Stop(string key, State state)
    {
        GetObserver(key)?.Stop(key, state);
    }

    public void Listen(string key, State state)
    {
        GetObserver(key)?.Listen(key, state);
    }
}

public partial class XState : States
{

    [Serializable]
    public class MapPreferences
    {
        public string mapName;
        public int numberOfRound = 3; //게임 라운드 횟수
        public int roundTime = 150;

        public MapPreferences(string mapName, int numberOfRound, int roundTime)
        {
            this.mapName = mapName;
            this.numberOfRound = numberOfRound;
            this.roundTime = roundTime;
        }
    }

    MapPreferences m_MapPreferences;
    Transform m_MasterCharacter;
    Transform m_PlayerCharecter;

    int m_MasterWinCount;
    int m_PlayerWinCount;
    Vector2 m_PlayerPosNormalized;

    void SilentListen()
    {
        Debug.Log("Silent Listen!!!!");
    
    }

    private void Start()
    {
        SilentListen();
    }

}
