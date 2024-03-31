using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class TurretAttackController : MonoBehaviour
{
    public Transform TurretTransform
    {
        get
        {
            return _turretTransform;
        }
    }

    public bool OnCoolDown
    {
        get
        {
            return _onCooldown;
        }
    }

    [Header("Attack (Projectile shooting)")]
    [SerializeField] private Transform _projectileSpawner;
    [SerializeField] private ProjectileController _projectile;
    [Tooltip("Hold time between attacks")]
    [SerializeField] private float _attackIntervalTime;
    [Tooltip("After attacking rotation hold time")]
    [SerializeField] private float _rotationHoldDuration;
    [SerializeField] private ParticleSystem _attackFx;
    [Header("Turret Movement")]
    [SerializeField] private Transform _turretTransform;
    [SerializeField] private float _turretRotationSpeed;

    private bool _onCooldown;
    private bool _onRotationHold;
    private Collider[] _tankColliders;
    private Coroutine _attackIntervalCoroutine;
    private Coroutine _rotationHoldCoroutine;

    #region LIFE_CYCLE

    private void OnDisable()
    {
        KillAttackCoroutine();
        KillRotationHoldCoroutine();
    }

    private void Start()
    {
        _tankColliders = gameObject.GetComponentsInChildren<Collider>();
    }

    #endregion

    #region ROTATE

    public void RotateTurret(float rotationAngle)
    {
        if (_onRotationHold)
        {
            return;
        }

        Vector3 lerpedRotation = TurretTransform.eulerAngles;
        lerpedRotation.y = Mathf.MoveTowardsAngle(lerpedRotation.y, rotationAngle, _turretRotationSpeed);
        TurretTransform.eulerAngles = lerpedRotation;
    }

    #endregion

    #region ATTACK

    public void Attack()
    {
        if (_onCooldown)
        {
            return;
        }

        DoAttack();

        if (_attackIntervalTime > 0)
        {
            _attackIntervalCoroutine = StartCoroutine(AttackWait());
        }

        if (_rotationHoldDuration > 0)
        {
            _rotationHoldCoroutine = StartCoroutine(RotationHoldWait());
        }
    }

    private void DoAttack()
    {
        if (_projectile != null)
        {
            Instantiate(_projectile).StartUp(_tankColliders).SetPositionAndRotation(_projectileSpawner.position, _projectileSpawner.rotation);
        }

        if (_attackFx != null)
        {
            Instantiate(_attackFx).transform.SetPositionAndRotation(_projectileSpawner.position, _projectileSpawner.rotation);
        }
    }

    private IEnumerator AttackWait()
    {
        _onCooldown = true;

        yield return new WaitForSeconds(_attackIntervalTime);

        _onCooldown = false;

        KillAttackCoroutine();
    }

    private IEnumerator RotationHoldWait()
    {
        _onRotationHold = true;

        yield return new WaitForSeconds(_rotationHoldDuration);

        _onRotationHold = false;

        KillRotationHoldCoroutine();
    }

    private void KillAttackCoroutine()
    {
        if (_attackIntervalCoroutine == null)
        {
            return;
        }

        StopCoroutine(_attackIntervalCoroutine);
        _attackIntervalCoroutine = null;
    }

    private void KillRotationHoldCoroutine()
    {
        if (_rotationHoldCoroutine == null)
        {
            return;
        }

        StopCoroutine(_rotationHoldCoroutine);
        _rotationHoldCoroutine = null;
    }

    #endregion
}
