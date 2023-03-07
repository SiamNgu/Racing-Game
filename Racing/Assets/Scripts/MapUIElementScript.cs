using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MapUIElementScript : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Image image;

    public Map map;

    private void Start()
    {
        text.text = map.name;
        image.sprite = map.image;
    }

    public void SetSelectedMap()
    {
        DataBetweenScenes.mapSelected = map;
    }
}
