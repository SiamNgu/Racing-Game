using UnityEngine;

public class MinimapCameraScript : MonoBehaviour
{
    [SerializeField] private Transform follow;
    [SerializeField] private float height;
    

    private void Update()
    {
        transform.rotation = Quaternion.Euler(90, follow.eulerAngles.y, 0);
        transform.position = follow.position + Vector3.up * height;
    }
}
