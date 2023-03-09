using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomSwipeScript : MonoBehaviour
{
    private Scrollbar scrollbar;
    private float[] positions;
    [SerializeField] private Transform content;
    [SerializeField] private int currentIndex;
    [SerializeField] private CarDatasScriptableObject carDatas;
    [SerializeField] private MapsScriptableObject mapDatas;

    private void Awake()
    {
        scrollbar = GetComponent<Scrollbar>();
        positions = new float[content.childCount];
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = i * 1f / (content.childCount-1f);
        }
        currentIndex = 0;
    }

    private void Update()
    {
        scrollbar.value = Mathf.Lerp(scrollbar.value, positions[currentIndex], Time.deltaTime * 10);
    }

    public void Swipe(int amount)
    {
        int indexToSet = currentIndex + amount;
        if (indexToSet < 0) indexToSet = positions.Length - 1;
        if (indexToSet >= positions.Length - 1) indexToSet = 0;
        currentIndex = indexToSet;
    }

    public void UpdateSelectedCar()
    {
        DataBetweenScenes.carSelected = carDatas.cars[currentIndex];
    }

    public void UpdateSelectedMap()
    {
        DataBetweenScenes.mapSelected = mapDatas.maps[currentIndex];
    }
}
