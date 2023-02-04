using UnityEngine;

public class WheelRotIndicatorScript : MonoBehaviour
{
    [SerializeField] private Transform frontCarWheel;

    private void Update()
    {
        transform.rotation = Quaternion.Euler(Vector3.forward * -frontCarWheel.localEulerAngles.y);
    }
}
