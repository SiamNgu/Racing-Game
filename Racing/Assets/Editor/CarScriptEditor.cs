using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(CarScript))]
[ExecuteInEditMode]
public class CarScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CarScript car  = (CarScript)target;
        base.OnInspectorGUI();
        if (GUILayout.Button("Spawn Wheels"))
        {
            car.SpawnCar();
        }
    }
}
