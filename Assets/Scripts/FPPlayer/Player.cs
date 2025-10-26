using UnityEngine;
using UnityEngine.InputSystem;

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

        #endregion



        #region Unity Methods

        private void OnValidate() {
            if (FPController == null) FPController = GetComponent<FPController>();
        }

        private void Start() {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        #endregion
    }
}