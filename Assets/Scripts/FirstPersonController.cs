using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && INPUT_ASSET_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && INPUT_ASSET_PACKAGES_CHECKED
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class FirstPersonController : MonoBehaviour
    {
        #region Public Variables
        [Header("Movement Settings")]
        public float MoveSpeed = 4.0f;
        public float SprintSpeed = 6.0f;
        public float RotationSpeed = 1.0f;
        public float JumpHeight = 1.2f;
        public float SprintChangeRate = 10.0f;

        [Header("Global Settings")]
        public float Gravity = -15.0f;

        [Header("Timeouts")]
        public float JumpTimeout = 0.1f;
        public float FallTimeout = 0.14f;

        [Header("Grounded Settings")]
        public bool Grounded = true;
        public float GroundOffset = 0.2f;
        public float GroundRadius = 0.5f;
        public LayerMask GroundMask;

        [Header("CinemaMachine Camera")]
        public GameObject Cinemachine;
        public float TopPitch = 90f;
        public float BottomPitch = -90f;
        #endregion

        #region Private Variables

        //default variables
        private float _speed;
        private float _verticalVelocity;
        private float _rotationSpeed;
        private float _jumpTimeout;
        private float _fallTimeout;

        //cinemachine
        private float _cinemachineCameraPitch;

        //Scripts
        private PlayerInput _player;
        private PlayerInputAsset _input;
        private CharacterController _controller;
        private GameObject _mainCamera;

        //const variables
        private const float _threshold = 0.1f;
        private const float _terminalVelocity = 54.0f;
        private const float _speedOffset = 0.1f;

        #endregion

        private void Awake()
        {
            if (!_mainCamera)
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
        private void Start()
        {
            _player = GetComponent<PlayerInput>();
            _input = GetComponent<PlayerInputAsset>();
            _controller = GetComponent<CharacterController>();

            _fallTimeout = FallTimeout;
            _jumpTimeout = JumpTimeout;
        }
        public bool IsUsingMouseWithKeyboard => _player.currentControlScheme == "Keyboard Mouse";

        private void Update()
        {
            Move();
            JumpAndGravity();
            GroundCheck();
        }
        private void LateUpdate()
        {
            CameraRotaion();
        }
        private void Move()
        {
            float _targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
            if (_input.move == Vector2.zero)
                _targetSpeed = 0.0f;

            float _currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0, _controller.velocity.y).magnitude;
            float _inputMagnitude = _input.isAnalogMovementOn ? _input.move.magnitude : 1.0f;
            if (_currentHorizontalSpeed < _targetSpeed - _speedOffset || _currentHorizontalSpeed > _targetSpeed + _speedOffset)
            {
                _speed = Mathf.Lerp(_speed, _targetSpeed * _inputMagnitude, SprintChangeRate * Time.deltaTime);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
                _speed = _targetSpeed;

            Vector3 _inputDirection = new Vector3(_input.move.x, 0, _input.move.y).normalized;
            if (_input.move != Vector2.zero)
                _inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;

            _controller.Move(_inputDirection.normalized *(_speed * Time.deltaTime) + new Vector3 (0, _verticalVelocity, 0) * Time.deltaTime);
        }
        private void JumpAndGravity()
        {
            if (Grounded)
            {
                _fallTimeout = FallTimeout;

                if (_jumpTimeout >= 0.0f)
                    _jumpTimeout -= Time.deltaTime;

                if (_verticalVelocity <= 0.0f)
                    _verticalVelocity = -2.0f;

                if (_input.jump && _jumpTimeout <= 0.0f)
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2.0f * Gravity);
                
            }
            else
            {
                _jumpTimeout = JumpTimeout;

                if (_fallTimeout >= 0.0f)
                    _fallTimeout -= Time.deltaTime;

                if (_verticalVelocity < _terminalVelocity && _fallTimeout <= 0.0f)
                    _verticalVelocity += Gravity * Time.deltaTime;

                _input.jump = false;
            }
        }
        private void GroundCheck()
        {
            Vector3 _spherePosition = new Vector3(transform.position.x, transform.position.y - GroundOffset, transform.position.z);
            Grounded = Physics.CheckSphere(_spherePosition, GroundRadius, GroundMask, QueryTriggerInteraction.Ignore);
        }
        private void CameraRotaion()
        {
            if (_input.look.sqrMagnitude > _threshold)
            {
                float _Multiplyer = IsUsingMouseWithKeyboard ? 1.0f : Time.deltaTime;
                _cinemachineCameraPitch += _input.look.y * RotationSpeed * _Multiplyer;
                _rotationSpeed = _input.look.x * RotationSpeed * _Multiplyer;

                _cinemachineCameraPitch = ClampAngle(_cinemachineCameraPitch, BottomPitch, TopPitch);

                Cinemachine.transform.localRotation = Quaternion.Euler(_cinemachineCameraPitch, 0, 0);
                transform.Rotate(Vector3.up * _rotationSpeed);
            }
        }
        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle <= -360f) angle = 360f;
            if (angle >= 360f) angle = -360f;
            return Mathf.Clamp(angle, min, max);
        }
        private void OnDrawGizmosSelected()
        {
            Color _red = new Color(0.0f, 1.0f, 0.0f, 0.2f);
            Color _green = new Color(1.0f, 0.0f, 0.0f, 0.2f);
            if (Grounded)
                Gizmos.color = _red;
            else
                Gizmos.color = _green;
            Gizmos.DrawSphere(new Vector3 (transform.position.x, transform.position.y - GroundOffset, transform.position.z), GroundRadius);
        }
    }
}

