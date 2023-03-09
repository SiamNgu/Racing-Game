using UnityEngine;

public class InstantiateMapButtonsScript : MonoBehaviour
{
    [SerializeField] private MapsScriptableObject mapsData;
    [SerializeField] private GameObject mapUI;

    private void Awake()
    {
        for (int i = 0; i < mapsData.maps.Length; i++)
        {
            MapUIReferences instantiatedMapUI = Instantiate(mapUI, transform).GetComponent<MapUIReferences>();
            instantiatedMapUI.map = mapsData.maps[i];
        }
    }
}
