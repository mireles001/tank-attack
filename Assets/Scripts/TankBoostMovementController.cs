using UnityEngine;

[DisallowMultipleComponent]
public class TankBoostMovementController : TankMovementController
{
    public bool IsHolding { get; set; }
    public float BoostPower { private set; get; }

    [Header("Speed Boost"), Tooltip("Move + (hold) Break to gain boost")]
    [SerializeField] private float _boostLimit;
    [SerializeField] private float _boostGainSpeed;
    [SerializeField] private float _boostLoseSpeed;
    [Header("Visual FX")]
    [SerializeField] private ParticleSystem _boostFx;

    private const float USE_FX_MULTIPLIER = 0.666f;
    private const float CAN_USE_FX_AGAIN_MULTIPLIER = 0.333f;

    private bool _canUseBoostFx;

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

    public override void Move(float moveInput)
    {
        MovementTarget.position += MovementTarget.forward * moveInput * (_movementSpeed + BoostPower);
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
