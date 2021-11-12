using System;
using System.Collections;
using System.Collections.Generic;
using JB;
using UnityEngine;

namespace JB
{
    public class PlayerManager : MonoBehaviour
    {
        private InputHandler _inputHandler;
        private Animator anim;
        private CameraHandler cameraHandler;
        private PlayerLocomotion playerLocomotion;
        
        public bool isInteracting;
        [Header("Player Flags")] 
        public bool isSprinting;
        public bool isInAir;
        public bool isGrounded;
        public bool canDoCombo;

        private void Awake()
        {
            cameraHandler = FindObjectOfType<CameraHandler>();
        }
        
        private void Start()
        {
            _inputHandler = GetComponent<InputHandler>();
            anim = GetComponentInChildren<Animator>();
            playerLocomotion = GetComponent<PlayerLocomotion>();
        }

        private void Update()
        {
            float delta = Time.deltaTime;
            isInteracting = anim.GetBool("isInteracting");
            canDoCombo = anim.GetBool("canDoCombo");

            _inputHandler.TickInput(delta);
            playerLocomotion.HandleMovement(delta);
            playerLocomotion.HangleRollingAndSprinting(delta);
            playerLocomotion.HandleFalling(delta, playerLocomotion.moveDirection);
        }
        
        private void FixedUpdate()
        {
            float delta = Time.fixedDeltaTime;
            if (cameraHandler != null)
            {
                cameraHandler.FollowTarget(delta);
                cameraHandler.HandleCameraRotation(delta, _inputHandler.mouseX, _inputHandler.mouseY);
            }
        }

        private void LateUpdate()
        {
            _inputHandler.rollFlag = false;
            _inputHandler.sprintFlag = false;
            _inputHandler.rb_Input = false;
            _inputHandler.rt_Input = false;
            _inputHandler.d_Pad_Down = false;
            _inputHandler.d_Pad_Left = false;
            _inputHandler.d_Pad_Right = false;
            _inputHandler.d_Pad_Up = false;
            
            if (isInAir)
            {
                playerLocomotion.inAirTimer += Time.deltaTime;
            }
        }
    }
}

