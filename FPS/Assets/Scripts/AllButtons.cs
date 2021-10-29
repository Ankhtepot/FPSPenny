using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AllButtons : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    private AudioSource gcAudioSource;
    
    private void Awake()
    {
        if (musicSlider)
        {
            gcAudioSource = GameObject.Find("GameController")?.GetComponent<AudioSource>();
            if (!gcAudioSource ) return;
            musicSlider.value = gcAudioSource.volume;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ExitToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    
    public void PlayGame()
    {
        SceneManager.LoadScene("GameLevel");
    }

    public void ChangeVolume(float volume)
    {
        if (!gcAudioSource) return;
        GameObject.Find("GameController").GetComponent<AudioSource>().volume = volume;
    }
}
