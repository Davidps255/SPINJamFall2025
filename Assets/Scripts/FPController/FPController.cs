using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.VisualScripting;

namespace Farmer {
    [RequireComponent(typeof(CharacterController))]
    public class FPController : MonoBehaviour {
        [Header("Movement Parameters")]
        public float MaxSpeed => SprintInput ? SprintSpeed : WalkSpeed;
        public float Acceleration = 15f;

        [SerializeField] float WalkSpeed = 3.5f;
        [SerializeField] float SprintSpeed = 8f;

        [Space(15)]
        [Tooltip("This is how high the character can jump.")]
        [SerializeField] float JumpHeight = 2f;

        [Header("Looking Parameters")]
        public Vector2 LookSensitivty = new Vector2(0.1f, 0.1f);
        public float PitchLimit = 85f;
        [SerializeField] float currentPitch = 0f;

        public float CurrentPitch
        {
            get => currentPitch;

            set {
                currentPitch = Mathf.Clamp(value, -PitchLimit, PitchLimit);
            }
        }

        public bool Sprinting {
            get {
                return SprintInput && CurrentSpeed > 0.1f;
            }
        }

        [Header("Camera Parameters")]
        [SerializeField] float CameraNormalFOV = 60f;
        [SerializeField] float CameraSprintFOV = 80f;
        [SerializeField] float CameraFOVSmoothing = 1f;

        float TargetCameraFOV {
            get {
                return Sprinting ? CameraSprintFOV : CameraNormalFOV;
            }
        }

        [Header("Physics Parameters")]
        [SerializeField] float GravityScale = 3f;

        public float VerticalVelocity = 0f;
        public Vector3 CurrentVelocity { get; private set; }
        public float CurrentSpeed { get; private set; }
        public bool IsGrounded => characterController.isGrounded;

        [Header("Input")]
        public Vector2 MoveInput;
        public Vector2 LookInput;
        public bool SprintInput;
        public bool Masked;
        private bool MaskDebounce = false;

        [Header("Gameplay Parameters")]
        [Tooltip("This is how long it takes to mask as well as delay you from swapping")]
        public float MaskTime = 3.0f;
        public float Corruption = 0f;
        public float MaskingCorruption = 10f;
        public float CorruptionRate = 1f;

        [Header("Components")]
        [SerializeField] CinemachineCamera fpCamera;
        [SerializeField] CharacterController characterController;



        #region Unity Methods

        private void OnValidate() {
            if (characterController == null) {
                characterController = GetComponent<CharacterController>();
            }
        }

        private void Update() {
            MoveUpdate();
            LookUpdate();
            CameraUpdate();
            if (Masked) {
                Corruption += CorruptionRate * Time.deltaTime;
            }
        }

        #endregion



        #region Controller Methods

        public void TryJump() {
            if (IsGrounded == false) {
                return;
            }

            VerticalVelocity = Mathf.Sqrt(JumpHeight * -2 * Physics.gravity.y * GravityScale);
        }

        IEnumerator MyCoroutine() {
            if (!MaskDebounce) {
                MaskDebounce = true;
                if (Masked) {
                    print("Take mask off");
                    Masked = false;
                    yield return new WaitForSeconds(MaskTime);
                } else {
                    yield return new WaitForSeconds(MaskTime);
                    print("Put mask on");
                    Corruption += MaskingCorruption;
                    Masked = true;
                }
                MaskDebounce = false;
            }
        }

        public void Mask() {
            StartCoroutine("MyCoroutine");
        }

        private void MoveUpdate() {
            Vector3 motion = transform.forward * MoveInput.y + transform.right * MoveInput.x;
            motion.y = 0f;
            motion.Normalize();

            if (motion.sqrMagnitude >= 0.01f) {
                CurrentVelocity = Vector3.MoveTowards(CurrentVelocity, motion * MaxSpeed, Acceleration * Time.deltaTime);
            } else {
                CurrentVelocity = Vector3.MoveTowards(CurrentVelocity, Vector3.zero, Acceleration * Time.deltaTime);
            }

            if (IsGrounded && VerticalVelocity <= 0.0f) {
                VerticalVelocity = -3f;
            } else {
                VerticalVelocity += Physics.gravity.y * GravityScale * Time.deltaTime;
            }
            
            Vector3 fullVelocity = new Vector3(CurrentVelocity.x, VerticalVelocity, CurrentVelocity.z);

            CollisionFlags flags = characterController.Move(fullVelocity * Time.deltaTime);
            if ((flags & CollisionFlags.Above) != 0) {
                VerticalVelocity = 0f;
            }

            CurrentSpeed = CurrentVelocity.magnitude; // Updating speed
        }

        private void LookUpdate() {
            Vector2 input = new Vector2(LookInput.x * LookSensitivty.x, LookInput.y * LookSensitivty.y);

            // Looking up and down
            CurrentPitch -= input.y;
            fpCamera.transform.localRotation = Quaternion.Euler(CurrentPitch, 0f, 0f);

            // Looking left and right
            transform.Rotate(Vector3.up * input.x);
        }

        private void CameraUpdate() {
            float targetFOV = CameraNormalFOV;

            if (Sprinting) {
                float speedRatio = CurrentSpeed / SprintSpeed;

                targetFOV = Mathf.Lerp(CameraNormalFOV, CameraSprintFOV, speedRatio);
            }

            fpCamera.Lens.FieldOfView = Mathf.Lerp(fpCamera.Lens.FieldOfView, targetFOV, CameraFOVSmoothing * Time.deltaTime);
        }

        #endregion
    }
}