using UnityEngine;

public class SettingsButtonScript : MonoBehaviour
{
    [SerializeField] private GameObject settings;

    public void ToggleSettings()
    {
        settings.SetActive(!settings.activeSelf);
        PlayerPrefs.Save();
    }
}
