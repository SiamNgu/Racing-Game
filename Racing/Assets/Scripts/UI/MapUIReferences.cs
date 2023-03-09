using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MapUIReferences : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Image image;

    public MapData map;

    private void Start()
    {
        text.text = map.name;
        image.sprite = map.image;
    }
}
