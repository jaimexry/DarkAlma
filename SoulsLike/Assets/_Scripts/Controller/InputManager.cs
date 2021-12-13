using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JB
{
    public class InputManager : MonoBehaviour
    {
        public Controller controller;
        public Transform camTransform;

        //Triggers & bumpers
        private bool Rb;
        private bool Rt;
        private bool Lb;
        private bool Lt;
        private bool isAttacking;
        
        //Inventory
        private bool inventoryInput;
        
        //Prompts
        private bool b_Input;
        private bool y_Input;
        private bool x_Input;
        
        //Dpad
        private bool leftArrow;
        private bool rightArrow;
        private bool upArrow;
        private bool downArrow;

        public PlayerProfile playerProfile;
        private void Start()
        {
            //TODO: check if you have the controller assigned, if not, Instantiate it
            camTransform = Camera.main.transform;

            ResourcesManager rm = Settings.ResourcesManager;
            for (int i = 0; i < playerProfile.startingClothes.Length; i++)
            {
                Item item = rm.GetItem(playerProfile.startingClothes[i]);
                if (item is ClothItem)
                {
                    controller.startingCloth.Add((ClothItem)item);
                }
            }
            
            controller.Init();

            controller.SetWeapons(rm.GetItem(playerProfile.rightHandWeapon), rm.GetItem(playerProfile.leftHandWeapon));
        }

        private void Update()
        {
            if (controller == null)
            {
                return;
            }
            HandleInput();
        }

        private void HandleInput()
        {
            bool retVal = false;
            isAttacking = false;
            
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Rb = Input.GetButton("RB");
            Rt = Input.GetButton("RT");
            Lb = Input.GetButton("LB");
            Lt = Input.GetButton("LT");
            inventoryInput = Input.GetButton("Inventory");
            b_Input = Input.GetButton("B");
            y_Input = Input.GetButtonDown("Y");
            x_Input = Input.GetButton("X");
            leftArrow = Input.GetButton("Left");
            rightArrow = Input.GetButton("Right");
            upArrow = Input.GetButton("Up");
            downArrow = Input.GetButton("Down");
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            if (!controller.isInteracting)
            {
                if (!retVal)
                {
                    retVal = HandleRolls();
                }
            }

            if (!retVal)
            {
                retVal = HandleAttacking();
            }
            
            if (Input.GetKeyDown(KeyCode.F))
            {
                /*if (controller.lockOn)
                {
                    controller.OnClearLookOverride();
                }
                else
                {
                    controller.target = controller.FindLockableTarget();
                    if (controller.target != null)
                    {
                        controller.OnAssignLookOverride(controller.target);
                    }
                }*/
            }

            Vector3 rotateDirection = camTransform.right * horizontal;
            rotateDirection += camTransform.forward * vertical;
            
            controller.MoveCharacter(vertical, horizontal, rotateDirection);
        }

        private bool HandleAttacking()
        {
            AttackInputs attackInput = AttackInputs.none;
            
            if (Rb || Rt || Lb || Lt)
            {
                isAttacking = true;

                if (Rb)
                {
                    attackInput = AttackInputs.rb;
                }
                if (Rt)
                {
                    attackInput = AttackInputs.rt;
                }
                if (Lb)
                {
                    attackInput = AttackInputs.lb;
                }
                if (Lt)
                {
                    attackInput = AttackInputs.lt;
                }
            }

            if (y_Input)
            {
               
            }

            if (attackInput != AttackInputs.none)
            {
                if (!controller.isInteracting)
                {
                    controller.PlayTargetItemAction(attackInput);
                }
                else
                {
                    if (controller.canDoCombo)
                    {
                        controller.DoCombo(attackInput);
                    }
                }
            }

            return isAttacking;
        }

        private bool HandleRolls()
        {
            if (b_Input)
            {
                /*Vector3 targetDir = Vector3.zero;
                targetDir = controller.camera.transform.forward * controller.vertical;
                targetDir += controller.camera.transform.right * controller.horizontal;

                if (targetDir.z != 0)
                {
                    targetDir.z = targetDir.z > 0 ? 1 : -1;
                }
                
                if (targetDir.x != 0)
                {
                    targetDir.x = targetDir.x > 0 ? 1 : -1;
                }

                if (targetDir != Vector3.zero)
                {
                    controller.rollDirection = targetDir;

                    controller.mTransform.rotation = Quaternion.LookRotation(controller.rollDirection);
                    controller.PlayTargetAnimation("Roll", true);
                    controller.ChangeState(controller.rollStateId);
                    controller.isRolling = false;
                }
                else
                {
                    controller.rollDirection = Vector3.zero;
                    
                    controller.PlayTargetAnimation("Step", true);
                    controller.ChangeState(controller.attackStateId);
                }
                controller.vertical = 0;
                controller.horizontal = 0;
                controller.moveAmount = 0;*/
                return true;
            }
            return false;
        }
    }
}
