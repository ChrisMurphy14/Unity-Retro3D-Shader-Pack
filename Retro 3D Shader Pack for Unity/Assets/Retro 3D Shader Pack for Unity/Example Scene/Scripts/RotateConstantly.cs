//////////////////////////////////////////////////
// Author:              Chris Murphy
// Date created:        04.11.19
// Date last edited:    19.05.24
//////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Retro3DShaderPack
{
    public class RotateConstantly : MonoBehaviour
    {
        public Vector3 RotationSpeed;

        private void Update()
        {
            transform.Rotate(RotationSpeed * Time.deltaTime);
        }
    }
}