//////////////////////////////////////////////////
// Author:              Chris Murphy
// Date created:        03.11.19
// Date last edited:    19.05.24
//////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Retro3DShaderPack
{
    [RequireComponent(typeof(Camera))]
    // Allows the main camera to be rotated and 'flown' through space using the mouse and the WASDQE keys, as well as enabling and disabling the retro post-processing effects with the 'P' key.
    public class ExampleSceneCamera : MonoBehaviour
    {
        public PostProcessVolume RetroPostProcessVolume;
        public float MouseSensitivity = 1000.0f;
        public float VerticalClampAngle = 80.0f;
        public float MoveSpeed = 5.0f;

        private RetroPostProcessEffect _postProcessEffect = null;
        private Vector2 _mouseLookRotation; // Used to store the mouse look values of the previous frame for comparison.

        private void Awake()
        {
            Vector3 rotation = transform.localRotation.eulerAngles;
            _mouseLookRotation.x = rotation.x;
            _mouseLookRotation.y = rotation.y;

            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Start()
        {
            RetroPostProcessVolume.profile.TryGetSettings(out _postProcessEffect);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();

            UpdateMouseLook();
            UpdateMovement();
            UpdatePostProcessEffects();
        }

        private void UpdateMouseLook()
        {
            if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
                Cursor.lockState = CursorLockMode.Locked;

            Vector2 mousePos = new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"));
            _mouseLookRotation.x += mousePos.y * MouseSensitivity * Time.deltaTime;
            _mouseLookRotation.y += mousePos.x * MouseSensitivity * Time.deltaTime;
            _mouseLookRotation.x = Mathf.Clamp(_mouseLookRotation.x, -VerticalClampAngle, VerticalClampAngle);
            Quaternion localRotation = Quaternion.Euler(_mouseLookRotation.x, _mouseLookRotation.y, 0.0f);
            transform.rotation = localRotation;
        }

        private void UpdateMovement()
        {
            Vector3 moveDirection = Vector3.zero;
            if (Input.GetKey(KeyCode.W))
                moveDirection += Vector3.forward;
            if (Input.GetKey(KeyCode.S))
                moveDirection += Vector3.back;
            if (Input.GetKey(KeyCode.A))
                moveDirection += Vector3.left;
            if (Input.GetKey(KeyCode.D))
                moveDirection += Vector3.right;
            if (Input.GetKey(KeyCode.E))
                moveDirection += Vector3.up;
            if (Input.GetKey(KeyCode.Q))
                moveDirection += Vector3.down;

            float speedMultiplier = 1.0f;
            if (Input.GetKey(KeyCode.LeftShift))
                speedMultiplier = 2.0f;

            if (moveDirection != Vector3.zero)
                transform.Translate(moveDirection.normalized * MoveSpeed * speedMultiplier * Time.deltaTime);
        }

        private void UpdatePostProcessEffects()
        {
            if (Input.GetKeyDown(KeyCode.P))
                _postProcessEffect.enabled.value = !_postProcessEffect.enabled.value;
        }
    }
}