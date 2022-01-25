using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class XSettings : MonoBehaviour
{
    public readonly string mapListPath = "Jsons/MapList";
    public readonly string mapImagePath = "Image/Maps";


    public MapList LoadMapList()
    {
        TextAsset mapList = Resources.Load<TextAsset>(mapListPath);
        if (mapList == null)
        {
            Debug.LogError("Failed to Load Map List");
            return null;
        }

        MapList map = JsonUtility.FromJson<MapList>(mapList.text);
        if (map == null)
        {
            Debug.LogError("Failed to JsonParse MapList");
            return null;
        }

        return map;
    }

}
