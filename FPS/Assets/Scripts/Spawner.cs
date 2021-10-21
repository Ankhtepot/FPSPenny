using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private int number;
    [SerializeField] private float spawnRadius;
    [SerializeField] private bool spawnOnStart = true;

    private void Start()
    {
        if (spawnOnStart)
        {
            Spawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!spawnOnStart && other.CompareTag("Player"))
        {
            Spawn();
        }
    }

    private void Spawn()
    {
        for (int i = 0; i < number; i++)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * spawnRadius;
            float newY = Terrain.activeTerrain.SampleHeight(randomPoint);
            randomPoint.y = newY;
            if (NavMesh.SamplePosition(randomPoint, out var hit, spawnRadius, NavMesh.AllAreas))
            {
                Instantiate(zombiePrefab, randomPoint, Quaternion.identity);
            }
            else
            {
                i--;
            }
        }
    }
}
