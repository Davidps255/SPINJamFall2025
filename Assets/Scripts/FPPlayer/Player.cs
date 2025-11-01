using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace Farmer {
    [RequireComponent(typeof(FPController))]
    public class Player : MonoBehaviour {
        [Header("Components")]
        [SerializeField] FPController FPController;


        #region Input Handling

        private void OnMove(InputValue value) {
            FPController.MoveInput = value.Get<Vector2>();
        }

        private void OnLook(InputValue value) {
            FPController.LookInput = value.Get<Vector2>();
        }

        private void OnSprint(InputValue value) {
            FPController.SprintInput = value.isPressed;
        }

        private void OnJump(InputValue value) {
            if (value.isPressed) {
                FPController.TryJump();
            }
        }

        private void OnMask(InputValue value)
        {
            FPController.Mask();
        }
        
        private void OnFlashlight(InputValue value)
        {
            if (value.isPressed)
            {
                FPController.ToggleFlashlight();
            }
        }

        #endregion



        #region Unity Methods

        private void OnValidate() {
            if (FPController == null) FPController = GetComponent<FPController>();
        }

        private void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            FPController.FlashlightActive = false;
            FPController.FlashlightLight.gameObject.SetActive(false);
            FPController.BatteryIndicator.color = Color.green;
        }
        
        private void Update()
        {
            if (FPController.FlashlightActive)
            {
                FPController.BatteryCurrent = Mathf.Clamp(FPController.BatteryCurrent - FPController.BatteryDrain, 0f, FPController.BatteryMax);
                if (FPController.BatteryCurrent > FPController.LowBatteryThreshold)
                {
                   FPController.BatteryIndicator.color = Color.green;
                }
                else if (FPController.BatteryCurrent > 0 && !FPController.DeadBattery)
                {
                    FPController.BatteryIndicator.color = Color.yellow;
                }
                if (FPController.BatteryCurrent <= 0)
                {
                    FPController.OutOfBattery();
                    FPController.BatteryIndicator.color = Color.red;
                }
            }
            else
            {
                FPController.BatteryCurrent = Mathf.Clamp(FPController.BatteryCurrent + FPController.BatteryRecharge, 0f, FPController.BatteryMax);
            }
            if (FPController.DeadBattery == true)
            {
                if (FPController.BatteryCurrent >= FPController.BatteryThreshold)
                {
                    FPController.DeadBattery = false;
                }
            }
        }

        #endregion
    }
}