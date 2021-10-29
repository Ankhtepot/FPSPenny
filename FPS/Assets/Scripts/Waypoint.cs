using System;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Waypoint reached.");
            CompassController.WaypointReached(this);
        }
    }
}