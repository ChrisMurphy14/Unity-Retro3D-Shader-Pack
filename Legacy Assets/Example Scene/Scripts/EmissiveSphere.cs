//////////////////////////////////////////////////
// Author:              LEAKYFINGERS
// Date created:        04.11.19
// Date last edited:    04.11.19
//////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EmissiveSphere : MonoBehaviour
{
    public Vector3 TargetPosOffset;
    public float TravelDuration;


    private Vector3 initialPos;
    private float travelTimer = 0.0f;

    private void Awake()
    {
        initialPos = transform.position;
    }

    private void Update()
    {       
        transform.position = Vector3.Lerp(initialPos, initialPos + TargetPosOffset, travelTimer / TravelDuration);

        travelTimer += Time.deltaTime;
        if (travelTimer > TravelDuration)
            travelTimer = 0.0f;
    }
}