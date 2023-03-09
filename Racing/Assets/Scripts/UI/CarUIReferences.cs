using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CarUIReferences : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Image image;

    public CarData car;

    private void Start()
    {
        text.text = car.name;
        image.sprite = car.image;
    }
}
