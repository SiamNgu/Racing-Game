using UnityEngine;

public class InstantiateCarButtonsScript : MonoBehaviour
{
    [SerializeField] private CarDatasScriptableObject carDatas;
    [SerializeField] private GameObject carUI;

    private void Awake()
    {
        for (int i = 0; i < carDatas.cars.Length; i++)
        {
            CarUIReferences instantiatedMapUI = Instantiate(carUI, transform).GetComponent<CarUIReferences>();
            instantiatedMapUI.car = carDatas.cars[i];
        }
    }
}
