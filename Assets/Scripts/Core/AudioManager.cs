using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public enum GroundType { NONE, DIRTY, WOOD, CONCRETE, SAND, GRASS }
    public enum UIType { NONE, BUTTON, ENTERROOM, LOGIN, CLOSE, MESSAGE, GAMESTART, SELECT, POPUPOPEN, POPUPCLOSE, ROUND }
    public enum BackgroundType { NONE, LOGIN, HOME, ROOM, WIN, LOSE }

    [Serializable]
    public struct FootStepAudioData
    {
        public GroundType groundType;
        public AudioClip audio;
    }

    [Serializable]
    public struct UIAudioData
    {
        public UIType uIType;
        public AudioClip audio;
    }

    [Serializable]
    public struct BackgroundAudio
    {
        public BackgroundType backgroundType;
        public AudioClip audio;
    }

    [SerializeField] FootStepAudioData[] m_FootStep;
    [SerializeField] UIAudioData[] m_UIAudio;
    [SerializeField] BackgroundAudio[] m_BackgroundAudio;
    [SerializeField] AudioSource m_UIAudioSource;
    [SerializeField] AudioSource m_BackgroundSource;
    [SerializeField] AudioClip m_DefaultFootStep;

    public void PlayBackground(BackgroundType type)
    {
        if(BackgroundType.NONE == type) { return;}

        int count = m_BackgroundAudio.Length;
        for(int i = 0; i < count; i++)
        {
            if(m_BackgroundAudio[i].backgroundType == type)
            {
                AudioClip audioClip = m_BackgroundAudio[i].audio;
                if (audioClip == null) { break; }
                m_BackgroundSource.clip = audioClip;
                m_BackgroundSource.Play();
            }
        }
    }

    public void StopBackground()
    {
        if(m_BackgroundSource.isPlaying)
        {
            m_BackgroundSource.Stop();
        }
    }

    public void PlayUIAudio(UIType uIType)
    {
        if (uIType == UIType.NONE) { return; }

        int count = m_UIAudio.Length;
        for(int i = 0; i < count; i++)
        {
            if(uIType == m_UIAudio[i].uIType)
            {
                if (m_UIAudio[i].audio == null) { break; }
                m_UIAudioSource.PlayOneShot(m_UIAudio[i].audio);
                break;
            }
        }
    }

    public AudioClip GetFootStepAudio(GroundType ground)
    {
        if (ground == GroundType.NONE) { return m_DefaultFootStep; }

        int count = m_FootStep.Length;
        for(int i = 0; i < count; i++)
        {
            if(m_FootStep[i].groundType == ground)
            {
                return m_FootStep[i].audio;
            }
        }

        return m_DefaultFootStep;
    }
    


}
