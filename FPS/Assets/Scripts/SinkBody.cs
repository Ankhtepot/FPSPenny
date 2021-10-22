using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinkBody : MonoBehaviour
{
    [SerializeField] private float delay = 10;
    private float destroyHeight;
    private bool isRagDoll;

    private void Start()
    {
        isRagDoll = gameObject.CompareTag("RagDoll");

        if (isRagDoll)
        {
            Invoke(nameof(StartSink), 5);
        }
    }

    public void StartSink()
    {
        destroyHeight = Terrain.activeTerrain.SampleHeight(transform.position) - 5;

        foreach (var c in transform.GetComponentsInChildren<Collider>())
        {
            Destroy(c);
        }
        
        InvokeRepeating(nameof(SinkIntoGround), delay, 0.1f);
    }
    
    void SinkIntoGround()
    {
        transform.Translate(0, -0.001f, 0);

        if (transform.position.y < destroyHeight)
        {
            Destroy(gameObject);
        }
    }
}
