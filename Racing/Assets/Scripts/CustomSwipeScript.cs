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

    private void Awake()
    {
        scrollbar= GetComponent<Scrollbar>();
        positions = new float[content.childCount];
        for (int i=0; i < positions.Length; i++)
        {
            positions[i] = i * 1f / (content.childCount-1f);
        }
        currentIndex= 0;
    }

    private void Update()
    {
        scrollbar.value= Mathf.Lerp(scrollbar.value, positions[currentIndex], Time.deltaTime * 10);
    }

    public void Swipe(int dir)
    {
        if (currentIndex == 0 && dir == -1)
        {
            currentIndex = positions.Length -1;
        }
        else if (currentIndex == positions.Length - 1 && dir == 1)
        {
            currentIndex = 0;
        }
        else currentIndex += dir > 0 ? 1 : -1;
    }
}
