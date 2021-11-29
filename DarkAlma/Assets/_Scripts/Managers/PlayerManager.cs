using System;
using System.Collections;
using System.Collections.Generic;
using JB;
using UnityEngine;

namespace JB
{
    public class PlayerManager : CharacterManager
    {
        private PlayerAnimatorManager playerAnimatorManager;
        private InputHandler _inputHandler;
        private Animator anim;
        private CameraHandler cameraHandler;
        private PlayerLocomotion playerLocomotion;
        private PlayerStats playerStats;
        
        private InteractableUI interactableUI;
        public GameObject interactableUIGameObject;
        public GameObject itemInteractableGameObject;
        
        public bool isInteracting;
        [Header("Player Flags")] 
        public bool isSprinting;
        public bool isInAir;
        public bool isGrounded;
        public bool canDoCombo;
        public bool isUsingRightHand;
        public bool isUsingLeftHand;
        public bool isInvulnerable;

        private void Awake()
        {
            cameraHandler = FindObjectOfType<CameraHandler>();
            backStabCollider = GetComponentInChildren<CriticalDamageCollider>();
            playerAnimatorManager = GetComponentInChildren<PlayerAnimatorManager>();
            playerStats = GetComponent<PlayerStats>();
            _inputHandler = GetComponent<InputHandler>();
            anim = GetComponentInChildren<Animator>();
            playerLocomotion = GetComponent<PlayerLocomotion>();
            interactableUI = FindObjectOfType<InteractableUI>();
        }

        private void Update()
        {
            float delta = Time.deltaTime;
            
            isInteracting = anim.GetBool("isInteracting");
            canDoCombo = anim.GetBool("canDoCombo");
            anim.SetBool("isInAir", isInAir);
            anim.SetBool("isBlocking", isBlocking);
            isUsingRightHand = anim.GetBool("isUsingRightHand");
            isUsingLeftHand = anim.GetBool("isUsingLeftHand");
            isInvulnerable = anim.GetBool("isInvulnerable");
            anim.SetBool("isDead", playerStats.isDead);
            playerAnimatorManager.canRotate = anim.GetBool("canRotate");
            
            _inputHandler.TickInput(delta);
            playerLocomotion.HangleRollingAndSprinting(delta);
            playerLocomotion.HandleJumping();
            playerStats.RegenerateStamina();

            CheckForInteractableObject();
        }
        
        private void FixedUpdate()
        {
            float delta = Time.fixedDeltaTime;
            
            playerLocomotion.HandleMovement(delta);
            playerLocomotion.HandleFalling(delta, playerLocomotion.moveDirection);
            playerLocomotion.HandleRotation(delta);
        }

        private void LateUpdate()
        {
            _inputHandler.rollFlag = false;
            _inputHandler.rb_Input = false;
            _inputHandler.rt_Input = false;
            _inputHandler.lt_Input = false;
            _inputHandler.d_Pad_Down = false;
            _inputHandler.d_Pad_Left = false;
            _inputHandler.d_Pad_Right = false;
            _inputHandler.d_Pad_Up = false;
            _inputHandler.a_Input = false;
            _inputHandler.jump_Input = false;
            _inputHandler.inventory_Input = false;

            float delta = Time.deltaTime;
            if (cameraHandler != null)
            {
                cameraHandler.FollowTarget(delta);
                cameraHandler.HandleCameraRotation(delta, _inputHandler.mouseX, _inputHandler.mouseY);
            }
            
            if (isInAir)
            {
                playerLocomotion.inAirTimer += Time.deltaTime;
            }
        }

        #region Interactions

        public void OpenChestInteraction(Transform playerStandsHereWhenOpeningChest)
        {
            playerLocomotion.rigidbody.velocity = Vector3.zero;
            transform.position = playerStandsHereWhenOpeningChest.position;
            playerAnimatorManager.PlayTargetAnimation("Open Chest", true);
        }
        
        public void CheckForInteractableObject()
        {
            RaycastHit hit;

            if (Physics.SphereCast(transform.position, 0.6f, transform.forward, out hit, 1f,
                cameraHandler.ignoreLayers))
            {
                if (hit.collider.CompareTag("Interactable"))
                {
                    Interactable interactableObject = hit.collider.GetComponent<Interactable>();

                    if (interactableObject != null)
                    {
                        string interactableText = interactableObject.interactableText;
                        interactableUI.interactableText.text = interactableText;
                        interactableUIGameObject.SetActive(true);
                        
                        if (_inputHandler.a_Input)
                        {
                            hit.collider.GetComponent<Interactable>().Interact(this);
                        }
                    }
                }
            }
            else
            {
                if (interactableUIGameObject != null)
                {
                    interactableUIGameObject.SetActive(false);
                }

                if (itemInteractableGameObject != null && _inputHandler.a_Input == true)
                {
                    itemInteractableGameObject.SetActive(false);
                }
            }
        }

        #endregion
    }
}

