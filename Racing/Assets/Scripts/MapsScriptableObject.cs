using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Maps", menuName = "ScriptableObjects/MapsScriptableObject", order = 1)]
public class MapsScriptableObject : ScriptableObject
{
    public MapData[] maps;
}

[Serializable]
public class MapData
{
    [Serializable] public struct Placement
    {
        public Vector3 position;
        public Vector3 eulerAngles;
    };
    public string name;
    public GameObject prefab;
    public Vector3 spawnPosition;
    public Placement finishLine;
    public Sprite image;
}
