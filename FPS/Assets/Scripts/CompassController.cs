using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompassController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private Waypoint[] waypoints;
    [SerializeField] private GameObject pointer;
    [SerializeField] private RectTransform compassLine;
    private static int currentWaypointIndex = 0;
    private static Waypoint[] _waypoints;
    private RectTransform rect;

    private void Start()
    {
        rect = pointer.GetComponent<RectTransform>();
        _waypoints = waypoints;
    }

    public static void WaypointReached(Waypoint sourceWaypoint)
    {
        if (sourceWaypoint != _waypoints[currentWaypointIndex]) return;
        
        currentWaypointIndex = currentWaypointIndex > _waypoints.Length
            ? _waypoints.Length
            : currentWaypointIndex += 1;
        
        // Debug.Log($"Current waypoint index: {currentWaypointIndex}, waypointsLength: {waypointsLength}");
    }

    private void Update()
    {
        if (GameStats.gameOver) return;
        
        Vector3[] corners = new Vector3[4];
        compassLine.GetLocalCorners(corners);
        float pointerScale = Vector3.Distance(corners[1], corners[2]);
        Vector3 direction = waypoints[currentWaypointIndex].gameObject.transform.position - player.transform.position;
        float angleToTarget = Vector3.SignedAngle(player.transform.forward, direction, player.transform.up);
        angleToTarget = Mathf.Clamp(angleToTarget, -90, 90) / 180.0f * pointerScale;
        rect.localPosition = new Vector3(angleToTarget, rect.localPosition.y, rect.localPosition.z);
    }
}
