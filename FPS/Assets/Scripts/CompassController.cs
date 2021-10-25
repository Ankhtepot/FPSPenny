using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompassController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject target;
    [SerializeField] private GameObject pointer;
    [SerializeField] private RectTransform compassLine;
    private RectTransform rect;

    private void Start()
    {
        rect = pointer.GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (GameStats.gameOver) return;
        
        Vector3[] corners = new Vector3[4];
        compassLine.GetLocalCorners(corners);
        float pointerScale = Vector3.Distance(corners[1], corners[2]);
        Vector3 direction = target.transform.position - player.transform.position;
        float angleToTarget = Vector3.SignedAngle(player.transform.forward, direction, player.transform.up);
        angleToTarget = Mathf.Clamp(angleToTarget, -90, 90) / 180.0f * pointerScale;
        rect.localPosition = new Vector3(angleToTarget, rect.localPosition.y, rect.localPosition.z);
    }
}
