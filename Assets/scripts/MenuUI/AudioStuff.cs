using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioStuff : MonoBehaviour
{
    [Header("Audio shit")] public static AudioStuff instance;
    [SerializeField] private AudioMixer audioMixer;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);


            float music = PlayerPrefs.GetFloat("MusicVolume", 0.3f);
            float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

            SetMusicVolume(music);
            SetSFXVolume(sfx);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Clamp(value, 0.001f, 1f)) * 20);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Clamp(value, 0.001f, 1f)) * 20);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }
}