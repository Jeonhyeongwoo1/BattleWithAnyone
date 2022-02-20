using UnityEngine;
using UnityEngine.UI;
using System;

public class RoomMapInfo : MonoBehaviour
{
    [Serializable]
    public struct MapSprtie
    {
        public string name;
        public Sprite sprite;
    }

    [SerializeField] MapSprtie[] m_MapSprties;
    [SerializeField] Image m_MapImage;
    [SerializeField] Text m_MapTitle;
    [SerializeField] Text m_MapRoundNumber;
    [SerializeField] Text m_MapGameTime;

    public void SetInfo(string mapTitle, int round, int time)
    {
        foreach (MapSprtie v in m_MapSprties)
        {
            if (mapTitle == v.name)
            {
                m_MapImage.sprite = v.sprite;
            }
        }

        m_MapTitle.text = mapTitle;
        m_MapGameTime.text = round.ToString();
        m_MapRoundNumber.text = time.ToString();

        Core.gameManager.SetMapPreference(mapTitle, round, time);
    }

}
