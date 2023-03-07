using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateMapButtonsScript : MonoBehaviour
{
    [SerializeField] private MapsScriptableObject mapsData;
    [SerializeField] private GameObject mapUI;

    private void Awake()
    {
        for (int i = 0; i < mapsData.maps.Length; i++)
        {
            MapUIElementScript instantiatedMapUI = Instantiate(mapUI, transform).GetComponent<MapUIElementScript>();
            instantiatedMapUI.map = mapsData.maps[i];
        }
    }
}
