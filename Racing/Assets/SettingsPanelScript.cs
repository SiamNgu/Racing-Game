using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;
using System;
public class SettingsPanelScript : MonoBehaviour
{
    [SerializeField] private UnityEngine.UIElements.Slider volumeSlider;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private TMP_Dropdown steeringDeviceDropdown;
    private void OnEnable()
    {
        steeringDeviceDropdown.value = PlayerPrefs.GetInt("Steering Device", 0);
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
