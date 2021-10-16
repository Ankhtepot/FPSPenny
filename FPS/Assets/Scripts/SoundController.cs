using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    [SerializeField] private AudioSource shot;

    public void PlayShot()
    {
        shot.Play();
    }
}
