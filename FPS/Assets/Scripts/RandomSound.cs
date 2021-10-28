using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomSound : MonoBehaviour
{
    [SerializeField] private AudioSource sound;
    [SerializeField] private float firstPlay;
    [SerializeField] private float randomMin;
    [SerializeField] private float randomMax;

    private void Start()
    {
        InvokeSound(firstPlay);
    }

    private void InvokeSound(float delay = 0)
    {
        Invoke(nameof(PlaySound), delay == 0 ? Random.Range(randomMin, randomMax) : delay);
    }

    private void PlaySound()
    {
        var newAS = sound.CreateCopyAsGameObject(gameObject.transform);
        newAS.GetComponent<AudioSource>().Play();
        Destroy(newAS, sound.clip.length);
        InvokeSound();
    }
}
