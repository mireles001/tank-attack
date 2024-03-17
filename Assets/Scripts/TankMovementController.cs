using UnityEngine;

public class TankMovementController : MonoBehaviour
{
    public bool IsHolding { get; set; }
    public float BoostPower { private set; get; }
    public Transform MovementTarget
    {
        get
        {
            return transform;
        }
    }

    [Header("Movement and Rotation")]
    [SerializeField] private float _movementSpeed;
    [SerializeField] private float _rotationSpeed;
    [Header("Speed Boost"), Tooltip("Move + (hold) Break to gain boost")]
    [SerializeField] private float _boostLimit;
    [SerializeField] private float _boostGainSpeed;
    [SerializeField] private float _boostLoseSpeed;
    [Header("Visual FX")]
    [SerializeField] private ParticleSystem _boostFx;

    private const float USE_FX_MULTIPLIER = 0.666f;
    private const float CAN_USE_FX_AGAIN_MULTIPLIER = 0.333f;

    private bool _canUseBoostFx;
    private float _rotationSmoothVelocity;

    #region LIFE_CYCLE

    private void Start()
    {
        _canUseBoostFx = true;
    }

    private void LateUpdate()
    {
        if (IsHolding)
        {
            return;
        }

        if (BoostPower <= 0 || _boostLoseSpeed <= 0 || _boostLimit <= 0)
        {
            return;
        }

        if (!_canUseBoostFx && BoostPower <= _boostLimit * CAN_USE_FX_AGAIN_MULTIPLIER)
        {
            _canUseBoostFx = true;
        }

        BoostPower = Mathf.Max(BoostPower - _boostLoseSpeed * Time.deltaTime, 0);
    }

    #endregion

    #region UNITY_EVENTS

    public void OnTriggerEnter(Collider other)
    {
        var interactable = other.gameObject.GetComponent<IInteractable>();
        interactable?.StartInteractionHandler(transform);
    }

    public void OnTriggerStay(Collider other)
    {
        var interactable = other.gameObject.GetComponent<IInteractable>();
        interactable?.InteractionHandler(transform);
    }

    public void OnTriggerExit(Collider other)
    {
        var interactable = other.gameObject.GetComponent<IInteractable>();
        interactable?.StopInteractionHandler(transform);
    }

    #endregion

    public void Move(float moveInput, float timeDelta)
    {
        if (IsHolding)
        {
            if (_boostGainSpeed <= 0 || _boostLimit <= 0 || timeDelta == 0)
            {
                return;
            }

            BoostPower = Mathf.Min(BoostPower + _boostGainSpeed * timeDelta, _boostLimit);
            return;
        }

        if (_canUseBoostFx && BoostPower > _boostLimit * USE_FX_MULTIPLIER)
        {
            _canUseBoostFx = false;

            if (_boostFx != null)
            {
                Instantiate(_boostFx).transform.SetPositionAndRotation(MovementTarget.position, MovementTarget.rotation);
            }
        }

        Move(moveInput);
    }

    public void Move(float moveInput)
    {
        MovementTarget.position += MovementTarget.forward * moveInput * (_movementSpeed + BoostPower);
    }

    public void Rotate(float rotationAngle)
    {
        float smoothRotationAngle = Mathf.SmoothDampAngle(MovementTarget.eulerAngles.y, rotationAngle, ref _rotationSmoothVelocity, _rotationSpeed);

        MovementTarget.rotation = Quaternion.Euler(0, smoothRotationAngle, 0);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (BoostPower <= 0)
        {
            return;
        }

        Vector3 boostPowerDebugLine = MovementTarget.position + Vector3.up * 0.5f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(boostPowerDebugLine, boostPowerDebugLine + MovementTarget.forward * BoostPower);
    }
#endif
}
