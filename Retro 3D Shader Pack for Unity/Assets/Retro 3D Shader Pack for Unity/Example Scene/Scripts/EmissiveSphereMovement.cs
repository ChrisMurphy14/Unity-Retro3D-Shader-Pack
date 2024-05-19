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
    // Continually lerps the transform to an offset destination before teleporting it back to its starting point.
    public class EmissiveSphereMovement : MonoBehaviour
    {
        public Vector3 TargetPosOffset;
        public float TravelDuration;

        private Vector3 _initialPos;
        private float _travelTimer = 0.0f;

        private void Awake()
        {
            _initialPos = transform.position;
        }

        private void Update()
        {
            transform.position = Vector3.Lerp(_initialPos, _initialPos + TargetPosOffset, _travelTimer / TravelDuration);

            _travelTimer += Time.deltaTime;
            if (_travelTimer > TravelDuration)
                _travelTimer = 0.0f;
        }
    }
}