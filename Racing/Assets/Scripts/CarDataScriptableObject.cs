using UnityEngine;

[CreateAssetMenu(fileName = "CarData", menuName = "ScriptableObjects/CarDataScriptableObject", order = 1)]
public class CarDataScriptableObject : ScriptableObject
{
    public float brakeForce;
    public float topSpeed;
    public float acceleration;

    public AudioClip engineSound;
}
