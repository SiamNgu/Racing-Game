using UnityEngine;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.UI;

public class SettingsPanelScript : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Dropdown steeringDeviceDropdown;
    private void OnEnable()
    {
        steeringDeviceDropdown.value = PlayerPrefs.GetInt("Steering Device", 0);
        float volume;
        bool isExist = audioMixer.GetFloat("Volume", out volume);
        if (isExist) 
        {
            Debug.Log(volume);
            volumeSlider.value = volume;
        }
    }
    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("Volume", volume);
    }

    public void SetSteeringDevice(int value)
    {
        PlayerPrefs.SetInt("Steering Device", value);
    }
}
