//////////////////////////////////////////////////
// Author:              LEAKYFINGERS
// Date created:        04.11.19
// Date last edited:    04.11.19
//////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public Vector3 RotationSpeed;


    private void Update()
    {
        transform.Rotate(RotationSpeed * Time.deltaTime);
    }
}