using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalSettings : MonoBehaviour
{
    public Slider volumeSlider;
    public static float volume { 
        get { 
            return PlayerPrefs.GetFloat("GlobalVolume", 1); 
        } 
        set {
            PlayerPrefs.SetFloat("GlobalVolume", value);
        } 
    }

    private void Start()
    {
        volumeSlider.SetValueWithoutNotify(volume);
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public static void SetVolume(float newVolume)
    {
        volume = newVolume;
    }
}
