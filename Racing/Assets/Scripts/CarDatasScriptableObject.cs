using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "CarDatas", menuName = "ScriptableObjects/CarDatasScriptableObject", order = 1)]
public class CarDatasScriptableObject : ScriptableObject
{
    public CarData[] cars;
}

[System.Serializable]
public class CarData
{
    public string name;
    public Sprite image;
    public float brakeForce;
    public float topSpeed;
    public float acceleration;
    public AudioClip engineSound;
    public Vector3 RLWheelPos;
    public Vector3 FRWheelPos;
    public GameObject RWheel;
    public GameObject LWheel;
    public GameObject car;
    public Vector3 colliderSize;
    public float springHeight;
}
