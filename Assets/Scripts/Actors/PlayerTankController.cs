using UnityEngine;

namespace Shibidubi.TankAttack
{
    public class PlayerTankController : ActorController
    {
        public Transform MovementTarget
        {
            get
            {
                if (_movementController == null)
                {
                    return null;
                }

                return _movementController.MovementTarget;
            }
        }
        public Vector3 CameraTarget
        {
            get
            {
                if (_forwardPointDamp <= 0 || _forwardPointDistance <= 0)
                {
                    return MovementTarget.position;
                }

                return _forwardPoint;
            }
        }

        [SerializeField] private TankMovementController _movementController;
        [SerializeField] private TurretAttackController _attackController;
        [Header("Camera focus")]
        [SerializeField] private Camera _camera;
        [SerializeField] private float _forwardPointDistance;
        [SerializeField] private float _forwardPointDamp;
        [Header("Speed Boost"), Tooltip("Move + (hold) Break to gain boost")]
        [SerializeField] private float _boostLimit;
        [SerializeField] private float _boostGainSpeed;
        [SerializeField] private float _boostLoseSpeed;
        [Header("Visual FX")]
        [SerializeField] private ParticleSystem _boostFx;
        [Space, Header("Input Labels")]
        [SerializeField] private string _horizontalAxis = "Horizontal";
        [SerializeField] private string _verticalAxis = "Vertical";
        [SerializeField] private string _attackButton = "Attack";
        [SerializeField] private string _breakButton = "Break";
        [SerializeField] private string _rightStickHorizontalAxis = "StickHorizontal";
        [SerializeField] private string _rightStickVerticalAxis = "StickVertical";

        private const float MIN_INPUT_AXIS_VALUE = 0.19f;
        private const float LOOK_AT_DISTANCE = 1.5f;
        private const float MOUSE_LOOK_DEATHZONE = 0.9f;
        private const float USE_FX_MULTIPLIER = 0.666f;
        private const float CAN_USE_FX_AGAIN_MULTIPLIER = 0.333f;

        private bool _isUsingJoystick;
        private bool _isCameraTargetFreezed;
        private bool _canUseBoostFx;
        private bool _isHolding;
        private float _boostPower;
        private Vector3 _lastMouseInput;
        private Vector3 _forwardPoint;
        private Vector3 _forwardPointVelocity;
        private Vector3 _lookAtDirection;

        #region LIFE_CYCLE

        protected override void Start()
        {
            base.Start();

#if !UNITY_EDITOR
        Cursor.lockState = CursorLockMode.Confined;
#endif

            _forwardPointDistance = Mathf.Max(0, _forwardPointDistance);
            _forwardPointDamp = Mathf.Max(0, _forwardPointDamp);

            _lookAtDirection = MovementTarget.forward * LOOK_AT_DISTANCE;

            _canUseBoostFx = true;
        }

        private void Update()
        {
            if (DoNotUpdate())
            {
                return;
            }

            _isHolding = Input.GetButton(_breakButton);
            MoveTank(Time.deltaTime);
            Attack();
            MoveTurret(Time.deltaTime);
        }

        private void LateUpdate()
        {
            if (_isHolding)
            {
                return;
            }

            if (_boostPower <= 0 || _boostLoseSpeed <= 0 || _boostLimit <= 0)
            {
                return;
            }

            if (!_canUseBoostFx && _boostPower <= _boostLimit * CAN_USE_FX_AGAIN_MULTIPLIER)
            {
                _canUseBoostFx = true;
            }

            _boostPower = Mathf.Max(_boostPower - _boostLoseSpeed * Time.deltaTime, 0);
        }

        public void OnTriggerEnter(Collider other)
        {
            if (DoNotUpdate())
            {
                return;
            }

            LevelExitController exit = other.gameObject.GetComponent<LevelExitController>();
            if (exit != null && !exit.IsLocked)
            {
                LevelManager.Instance.End();
            }
        }

        #endregion

        #region MOVEMENT

        private void MoveTank(float timeDelta)
        {
            Vector3 direction = new Vector3(Input.GetAxis(_horizontalAxis), 0, Input.GetAxis(_verticalAxis));

            MoveForwardPointGoToPosition(direction);

            if (direction.magnitude < MIN_INPUT_AXIS_VALUE)
            {
                return;
            }

            MovePosition(direction, timeDelta);

            MoveRotation(direction);
        }

        private void MovePosition(Vector3 direction, float timeDelta)
        {
            if (_isHolding)
            {
                if (_boostGainSpeed <= 0 || _boostLimit <= 0 || timeDelta == 0)
                {
                    return;
                }

                _boostPower = Mathf.Min(_boostPower + _boostGainSpeed * timeDelta, _boostLimit);
                return;
            }

            if (_canUseBoostFx && _boostPower > _boostLimit * USE_FX_MULTIPLIER)
            {
                _canUseBoostFx = false;

                if (_boostFx != null)
                {
                    Instantiate(_boostFx).transform.SetPositionAndRotation(MovementTarget.position, MovementTarget.rotation);
                }
            }

            float baseMoveInput = Mathf.Max(Mathf.Abs(direction.x), Mathf.Abs(direction.z));
            if (_boostPower > 0)
            {
                baseMoveInput += _boostPower;
            }

            _movementController.Move(baseMoveInput * timeDelta);
        }

        private void MoveRotation(Vector3 direction)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _camera.transform.eulerAngles.y;
            _movementController.Rotate(targetAngle);
        }

        private void MoveForwardPointGoToPosition(Vector3 direction)
        {
            if (_isCameraTargetFreezed || _forwardPointDamp <= 0 || _forwardPointDistance <= 0)
            {
                return;
            }

            _forwardPoint = Vector3.SmoothDamp(_forwardPoint, MovementTarget.position + GetRelativeDirection(direction, _camera.transform) * _forwardPointDistance, ref _forwardPointVelocity, _forwardPointDamp);
        }

        #endregion

        #region ATTACK

        private void Attack()
        {
            if (_attackController.OnCoolDown)
            {
                return;
            }

            bool attackInput = Input.GetButton(_attackButton);

            if (!attackInput)
            {
                attackInput = Input.GetAxisRaw(_attackButton) != 0;
            }

            if (!attackInput)
            {
                return;
            }

            _attackController.Attack();
        }

        private void MoveTurret(float deltaTime)
        {
            Vector3 targetCurrentLookAtDirection = _attackController.TurretTransform.forward * LOOK_AT_DISTANCE;

            Vector3 mouseInput = Input.mousePosition;
            Vector3 joystickInput = new Vector3(Input.GetAxis(_rightStickHorizontalAxis), 0, Input.GetAxis(_rightStickVerticalAxis));

            SetInputType(mouseInput, joystickInput);

            if (_isUsingJoystick)
            {
                _lookAtDirection = joystickInput != Vector3.zero ? GetRelativeDirection(joystickInput, _camera.transform).normalized * LOOK_AT_DISTANCE : targetCurrentLookAtDirection;
            }
            else
            {
                bool validMousePosition = false;
                Vector3 mouseDirection = Vector3.zero;
                Ray mouseRay = _camera.ScreenPointToRay(mouseInput);
                Plane mathematicalPlane = new(Vector3.up, MovementTarget.position);
                if (mathematicalPlane.Raycast(mouseRay, out float hitDistance))
                {
                    Vector3 hitPoint = mouseRay.GetPoint(hitDistance);

                    if (Vector3.Distance(MovementTarget.position, hitPoint) > MOUSE_LOOK_DEATHZONE)
                    {
                        validMousePosition = true;
                        mouseDirection = hitPoint - MovementTarget.position;
                    }
                }

                _lookAtDirection = validMousePosition ? mouseDirection : targetCurrentLookAtDirection;
            }

            _attackController.RotateTurret(_lookAtDirection, deltaTime);
        }

        private void SetInputType(Vector3 mouse, Vector3 joystick)
        {
            if (_isUsingJoystick)
            {
                if (_lastMouseInput != mouse)
                {
                    _isUsingJoystick = false;
                    Cursor.visible = true;
                }
            }
            else if (joystick != Vector3.zero)
            {
                _isUsingJoystick = true;
                Cursor.visible = false;
            }

            _lastMouseInput = mouse;
        }

        #endregion

        private static Vector3 GetRelativeDirection(Vector3 direction, Transform cameraTransform)
        {
            if (direction.magnitude > 1)
            {
                direction.Normalize();
            }

            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0;
            Vector3 cameraRight = cameraTransform.right;
            cameraRight.y = 0;

            return direction.z * cameraForward + direction.x * cameraRight;
        }

        protected override void OnActorDestroyed()
        {
            base.OnActorDestroyed();

            // Disable camera-target position updates
            _isCameraTargetFreezed = true;

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.Defeat();
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_boostPower > 0)
            {
                Vector3 boostPowerDebugLine = MovementTarget.position + Vector3.up * 0.5f;
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(boostPowerDebugLine, boostPowerDebugLine + MovementTarget.forward * _boostPower);
            }

            if (MovementTarget == null)
            {
                return;
            }

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(MovementTarget.position, CameraTarget);
            Gizmos.DrawWireSphere(CameraTarget, 0.25f);

            Gizmos.color = _isUsingJoystick ? Color.blue : Color.cyan;
            Gizmos.DrawSphere(MovementTarget.position + _lookAtDirection, 0.15f);
        }
#endif
    }
}
