using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace JB
{
    public class PlayerLocomotion : MonoBehaviour
    {
        private CameraHandler cameraHandler;
        private PlayerManager playerManager;
        private PlayerStats playerStats;
        private Transform cameraObject;
        private InputHandler _inputHandler;
        public Vector3 moveDirection;

        [HideInInspector] public Transform myTransform;
        [FormerlySerializedAs("animatorHandler")] [HideInInspector] public PlayerAnimatorManager playerAnimatorManager;
        public new Rigidbody rigidbody;
        public GameObject normalCamera;

        [Header("Ground & Air Detection Stats")] 
        [SerializeField] private float groundDetectionRayStartPoint = 0.5f;
        [SerializeField] private float minimumDistanceNeededToBeginFall = 1f;
        [SerializeField] private float groundDirectionRayDistance = 0.2f;
        private LayerMask ignoreForGroundCheck;
        public float inAirTimer;

        [Header("Movement Stats")] 
        [SerializeField] private float walkingSpeed = 1;
        [SerializeField] private float movementSpeed = 5;
        [SerializeField] private float sprintSpeed = 7;
        [SerializeField] private float rotationSpeed = 10;
        [SerializeField] private float fallingSpeed = 45;

        [Header("Stamina Costs")] 
        [SerializeField] private int rollStaminaCost = 15;
        [SerializeField] private int backstepStaminaCost = 12;
        [SerializeField] private int sprintStaminaCost = 1;
        
        public CapsuleCollider characterCollider;
        public CapsuleCollider characterCollisionBlockerCollider;

        private void Awake()
        {
            playerStats = GetComponent<PlayerStats>();
            playerManager = GetComponent<PlayerManager>();
            rigidbody = GetComponent<Rigidbody>();
            _inputHandler = GetComponent<InputHandler>();
            playerAnimatorManager = GetComponentInChildren<PlayerAnimatorManager>();
            cameraHandler = FindObjectOfType<CameraHandler>();
        }

        private void Start()
        {
            cameraObject = Camera.main.transform;
            myTransform = transform;
            playerAnimatorManager.Initialize();

            playerManager.isGrounded = true;
            ignoreForGroundCheck = ~(1 << 8 | 1 << 11);
            Physics.IgnoreCollision(characterCollider, characterCollisionBlockerCollider, true);
        }

        #region Movement
        private Vector3 normalVector;
        private Vector3 targetPosition;

        public void HandleRotation(float delta)
        {
            if (playerAnimatorManager.canRotate)
            {
                if (_inputHandler.lockOnFlag)
                {
                    if (_inputHandler.sprintFlag || _inputHandler.rollFlag)
                    {
                        Vector3 targetDirection = Vector3.zero;
                        targetDirection = cameraHandler.cameraTransform.forward * _inputHandler.vertical;
                        targetDirection += cameraHandler.cameraTransform.right * _inputHandler.horizontal;
                        targetDirection.Normalize();
                        targetDirection.y = 0;

                        if (targetDirection == Vector3.zero)
                        {
                            targetDirection = transform.forward;
                        }

                        Quaternion tr = Quaternion.LookRotation(targetDirection);
                        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, rotationSpeed * Time.deltaTime);

                        transform.rotation = targetRotation;
                    }
                    else
                    {
                        Vector3 rotationDirection = moveDirection;
                        rotationDirection = cameraHandler.currentLockOnTarget.transform.position - transform.position;
                        rotationDirection.y = 0;
                        rotationDirection.Normalize();
                        Quaternion tr = Quaternion.LookRotation(rotationDirection);
                        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, rotationSpeed * Time.deltaTime);
                        transform.rotation = targetRotation;
                    }
                }
                else
                {
                    Vector3 targetDirection = Vector3.zero;
                    float moveOverride = _inputHandler.moveAmount;

                    targetDirection = cameraObject.forward * _inputHandler.vertical;
                    targetDirection += cameraObject.right * _inputHandler.horizontal;

                    targetDirection.Normalize();
                    targetDirection.y = 0;

                    if (targetDirection == Vector3.zero)
                    {
                        targetDirection = myTransform.forward;
                    }

                    float rs = rotationSpeed;
                    Quaternion tr = Quaternion.LookRotation(targetDirection);
                    Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation, tr, rs * delta);

                    myTransform.rotation = targetRotation;
                }
            }
        }

        public void HandleMovement(float delta)
        {
            if (_inputHandler.rollFlag)
            {
                return;
            }

            if (playerManager.isInteracting)
            {
                return;
            }
            
            moveDirection = cameraObject.forward * _inputHandler.vertical;
            moveDirection += cameraObject.right * _inputHandler.horizontal;
            moveDirection.Normalize();
            moveDirection.y = 0;

            float speed = movementSpeed;

            if (_inputHandler.sprintFlag && _inputHandler.moveAmount > 0.5f)
            {
                speed = sprintSpeed;
                playerManager.isSprinting = true;
                moveDirection *= speed;
                playerStats.TakeStaminaDamage(sprintStaminaCost);
            }
            else
            {
                if (_inputHandler.moveAmount < 0.5f)
                {
                    moveDirection *= walkingSpeed;
                    playerManager.isSprinting = false;

                }
                else
                {
                    moveDirection *= speed;
                    playerManager.isSprinting = false;
                }
            }

            Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, normalVector);
            rigidbody.velocity = projectedVelocity;

            if (_inputHandler.lockOnFlag && !_inputHandler.sprintFlag)
            {
                playerAnimatorManager.UpdateAnimatorValues(_inputHandler.vertical, _inputHandler.horizontal, playerManager.isSprinting);
            }
            else
            {
                playerAnimatorManager.UpdateAnimatorValues(_inputHandler.moveAmount, 0, playerManager.isSprinting);
            }
        }

        public void HangleRollingAndSprinting(float delta)
        {
            if (playerAnimatorManager.anim.GetBool("isInteracting"))
            {
                return;
            }

            if (playerStats.currentStamina <= 0)
            {
                return;
            }
            
            if (_inputHandler.rollFlag)
            {
                moveDirection = cameraObject.forward * _inputHandler.vertical;
                moveDirection += cameraObject.right * _inputHandler.horizontal;

                if (_inputHandler.moveAmount > 0)
                {
                    playerAnimatorManager.PlayTargetAnimation("Rolling", true);
                    moveDirection.y = 0;
                    Quaternion rollRotation = Quaternion.LookRotation(moveDirection);
                    myTransform.rotation = rollRotation;
                    playerStats.TakeStaminaDamage(rollStaminaCost);
                }
                else
                {
                    playerAnimatorManager.PlayTargetAnimation("Stepback", true);
                    playerStats.TakeStaminaDamage(backstepStaminaCost);
                }
            }
        }

        public void HandleFalling(float delta, Vector3 moveDirection)
        {
            playerManager.isGrounded = false;
            RaycastHit hit;
            Vector3 origin = myTransform.position;
            origin.y += groundDetectionRayStartPoint;

            if (Physics.Raycast(origin, myTransform.forward, out hit, 0.4f))
            {
                moveDirection = Vector3.zero;
            }

            if (playerManager.isInAir)
            {
                rigidbody.AddForce(-Vector3.up * fallingSpeed);
                rigidbody.AddForce(moveDirection * fallingSpeed / 6f);
            }

            Vector3 dir = moveDirection;
            dir.Normalize();
            origin = origin + dir * groundDirectionRayDistance;

            targetPosition = myTransform.position;
            Debug.DrawRay(origin, -Vector3.up * minimumDistanceNeededToBeginFall, Color.red, 0.1f, false);
            if (Physics.Raycast(origin, -Vector3.up, out hit, minimumDistanceNeededToBeginFall, ignoreForGroundCheck))
            {
                normalVector = hit.normal;
                Vector3 tp = hit.point;
                playerManager.isGrounded = true;
                targetPosition.y = tp.y;

                if (playerManager.isInAir)
                {
                    if (inAirTimer > 0.5f)
                    {
                        Debug.Log("You were in the air for " + inAirTimer);
                        playerAnimatorManager.PlayTargetAnimation("Land", true);
                        inAirTimer = 0;
                    }
                    else
                    {
                        playerAnimatorManager.PlayTargetAnimation("Empty", false);
                        inAirTimer = 0;
                    }

                    playerManager.isInAir = false;
                }
            }
            else
            {
                if (playerManager.isGrounded)
                {
                    playerManager.isGrounded = false;
                }

                if (playerManager.isInAir == false)
                {
                    if (playerManager.isInteracting == false)
                    {
                        playerAnimatorManager.PlayTargetAnimation("Falling", true);
                    }

                    Vector3 vel = rigidbody.velocity;
                    vel.Normalize();
                    rigidbody.velocity = vel * (movementSpeed / 2);
                    playerManager.isInAir = true;
                }
            }

            if (playerManager.isGrounded)
            {
                if (playerManager.isInteracting || _inputHandler.moveAmount > 0)
                {
                    myTransform.position = Vector3.Lerp(myTransform.position, targetPosition, Time.deltaTime);
                }
                else
                {
                    myTransform.position = targetPosition;
                }
            }

            if (playerManager.isInteracting || _inputHandler.moveAmount > 0)
            {
                myTransform.position = Vector3.Lerp(myTransform.position, targetPosition, Time.deltaTime / 0.1f);
            }
            else
            {
                myTransform.position = targetPosition;
            }
        }

        public void HandleJumping()
        {
            if (playerManager.isInteracting)
            {
                return;
            }
            
            if (playerStats.currentStamina <= 0)
            {
                return;
            }

            if (_inputHandler.jump_Input)
            {
                if (_inputHandler.moveAmount > 0)
                {
                    moveDirection = cameraObject.forward * _inputHandler.vertical;
                    moveDirection += cameraObject.right * _inputHandler.horizontal;
                    playerAnimatorManager.PlayTargetAnimation("Jump", true);
                    moveDirection.y = 0;
                    Quaternion jumpRotation = Quaternion.LookRotation(moveDirection);
                    myTransform.rotation = jumpRotation;
                }
            }
        }
        
        #endregion
    }
}

