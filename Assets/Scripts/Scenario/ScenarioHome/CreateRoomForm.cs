using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

[Serializable]
public class MapInfo
{
    public string mapTitle;
    public string mapInfo;
    public string imageName;
    public Sprite sprite;
}

[Serializable]
public class MapList
{
    public MapInfo[] data;
}

public class CreateRoomForm : MonoBehaviour
{
    public Transform mapPrefab;
    [SerializeField] Transform m_MapContentView;

    public MapList mapList;

    public void SetMapListInfo()
    {
        mapList = Core.settings.LoadMapList();
        if (mapList == null) { return; }

        int c = mapList.data.Length;
        for (int i = 0; i < c; i++)
        {
            MapInfo mapInfo = mapList.data[i];
            mapInfo.sprite = LoadMapImage(mapInfo.imageName);
            Transform map = Instantiate<Transform>(mapPrefab, Vector3.zero, Quaternion.identity, m_MapContentView);
            
        }

    }

    public Sprite LoadMapImage(string imageName)
    {
        string path = Core.settings.mapImagePath + "/" + imageName;
        Sprite image = Resources.Load<Sprite>(path);
    
        if (image == null)
        {
            Debug.LogError("Failed to Load Image");
            return null;
        }

        return image;
    }


    private void Start()
    {
        SetMapListInfo();

    }

}
