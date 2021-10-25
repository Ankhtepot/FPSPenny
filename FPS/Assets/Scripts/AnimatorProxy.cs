using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorProxy : MonoBehaviour
{
    [SerializeField] private AudioSource shot;

    public void PlayShot()
    {
        shot.Play();
    }

    public void AllowCanShoot()
    {
        GameStats.canShoot = true;
    }
}
