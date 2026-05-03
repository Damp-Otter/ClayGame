using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Data
{

    [CreateAssetMenu(menuName = "Data/MapSelectionData", fileName = "MapSelectionData")]

    public class MapSelectionData : ScriptableObject
    {

        public List<MapInfo> maps;

    }
}

[Serializable]
public struct MapInfo
{
    public string mapName;
    public string sceneName;
}