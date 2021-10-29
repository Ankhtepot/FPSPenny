using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToMainMenuDelayed : MonoBehaviour
{
    [SerializeField] private AudioSource sound;

    private IEnumerator Start()
    {
        sound.Play();
        yield return new WaitWhile(() => sound.isPlaying);

        SceneManager.LoadScene("MainMenu");
    }
}