using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class TankAttackController : MonoBehaviour
{
    public Transform CannonTransform
    {
        get
        {
            return _cannonTransform;
        }
    }

    [Header("Attack (Projectile shooting)")]
    [SerializeField] private Transform _projectileSpawner;
    [SerializeField] private ProjectileController _projectile;
    [SerializeField] private float _attackIntervalTime;
    [SerializeField] private ParticleSystem _attackFx;
    [Header("Cannon Movement")]
    [SerializeField] private Transform _cannonTransform;
    [SerializeField] private float _cannonRotationSpeed;

    private bool _onCooldown;
    private Collider[] _tankColliders;
    private Coroutine _attackIntervalCoroutine;

    #region LIFE_CYCLE

    private void OnDisable()
    {
        KillAttackCoroutine();
    }

    private void Start()
    {
        _tankColliders = gameObject.GetComponentsInChildren<Collider>();
    }

    #endregion

    #region ROTATE

    public void RotateCannon(float rotationAngle)
    {
        Vector3 lerpedRotation = CannonTransform.eulerAngles;
        lerpedRotation.y = Mathf.MoveTowardsAngle(lerpedRotation.y, rotationAngle, _cannonRotationSpeed);
        CannonTransform.eulerAngles = lerpedRotation;
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

        _attackIntervalCoroutine = StartCoroutine(AttackWait());
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

    private void KillAttackCoroutine()
    {
        if (_attackIntervalCoroutine == null)
        {
            return;
        }

        StopCoroutine(_attackIntervalCoroutine);
        _attackIntervalCoroutine = null;
    }

    #endregion
}
