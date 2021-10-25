using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MakeRadarObject : MonoBehaviour
{
    public Image image;

    private void Start()
    {
        Radar.RegisterRadarObject(gameObject, image);
    }

    private void OnDestroy()
    {
        Radar.RemoveRadarObject(gameObject);
    }
}
