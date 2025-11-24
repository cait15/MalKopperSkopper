using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsmenuStuff : MonoBehaviour
{
    [Header("Slider")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    
    [Header("Sounds")]
  
    [SerializeField] private AudioSource uiAudioSource;
    
    private void Start()
    {
        
        float musicVol = musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.3f);// saves the players settings, i just put it as 0.3 so the music dont blast yah ears off
        float sfxVol =  sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        musicSlider.value = musicVol;
        sfxSlider.value = sfxVol;
        AudioStuff.instance.SetMusicVolume(musicVol);
        AudioStuff.instance.SetSFXVolume(sfxVol);
        musicSlider.onValueChanged.AddListener(volume => AudioStuff.instance.SetMusicVolume(volume));
        sfxSlider.onValueChanged.AddListener(volume =>
        {
            AudioStuff.instance.SetSFXVolume(volume);
            PlaySfx();
        });
    }

    private void PlaySfx()
    {
        if ( uiAudioSource != null)
        {
            uiAudioSource.Play();
        }
    }
}