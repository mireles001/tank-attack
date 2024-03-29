using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    public Transform MovementTarget
    {
        get
        {
            return _movementController.MovementTarget;
        }
    }
    public Vector3 CameraTarget
    {
        get
        {
            if (_forwardPointhDamp <= 0 || _forwardPointhDistance <= 0)
            {
                return MovementTarget.position;
            }

            return _forwardPoint;
        }
    }

    [SerializeField] private TankHealthController _healthController;
    [SerializeField] private TankMovementController _movementController;
    [SerializeField] private TankAttackController _attackController;
    [SerializeField] private Camera _camera;
    [SerializeField] private float _forwardPointhDistance;
    [SerializeField] private float _forwardPointhDamp;
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

    private bool _isUsingJoystick;
    private bool _isCameraTargetFreezed;
    private Vector3 _lastMouseInput;
    private Vector3 _forwardPoint;
    private Vector3 _forwardPointVelocity;
    private Vector3 _lookAtDirection;

    #region LIFE_CYCLE

    private void OnEnable()
    {
        if (_healthController == null)
        {
            return;
        }

        _healthController.TankDestroyed += OnTankDestroyed;
    }

    private void OnDisable()
    {
        if (_healthController == null)
        {
            return;
        }

        _healthController.TankDestroyed -= OnTankDestroyed;
    }

    private void Start()
    {
#if !UNITY_EDITOR
        Cursor.lockState = CursorLockMode.Confined;
#endif

        _forwardPointhDistance = Mathf.Max(0, _forwardPointhDistance);
        _forwardPointhDamp = Mathf.Max(0, _forwardPointhDamp);

        _lookAtDirection = MovementTarget.forward * LOOK_AT_DISTANCE;
    }

    private void Update()
    {
        if (LevelManager.Instance != null && LevelManager.Instance.IsPlayerInputLocked)
        {
            return;
        }

        _movementController.IsHolding = Input.GetButton(_breakButton);
        MoveTank(Time.deltaTime);
        Attack();
        MoveCannon();
    }

    public void OnTriggerEnter(Collider other)
    {
        LevelExitController exit = other.gameObject.GetComponent<LevelExitController>();
        if (LevelManager.Instance != null && !LevelManager.Instance.IsPlayerInputLocked && exit != null && !exit.IsLocked)
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

        _movementController.Move(Mathf.Max(Mathf.Abs(direction.x), Mathf.Abs(direction.z)) * timeDelta, timeDelta);

        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _camera.transform.eulerAngles.y;
        _movementController.Rotate(targetAngle);
    }

    private void MoveForwardPointGoToPosition(Vector3 direction)
    {
        if (_isCameraTargetFreezed || _forwardPointhDamp <= 0 || _forwardPointhDistance <= 0)
        {
            return;
        }

        _forwardPoint = Vector3.SmoothDamp(_forwardPoint, MovementTarget.position + GetRelativeDirection(direction, _camera.transform) * _forwardPointhDistance, ref _forwardPointVelocity, _forwardPointhDamp);
    }

    #endregion

    #region ATTACK

    private void Attack()
    {
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

    private void MoveCannon()
    {
        Vector3 targetCurrentLookAtDirection = _attackController.CannonTransform.forward * LOOK_AT_DISTANCE;

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

        float targetAngle = Mathf.Atan2(_lookAtDirection.x, _lookAtDirection.z) * Mathf.Rad2Deg;

        _attackController.RotateCannon(targetAngle);
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

    private void OnTankDestroyed()
    {
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
