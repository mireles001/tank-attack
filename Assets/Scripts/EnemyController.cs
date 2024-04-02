using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EnemyController : BaseTankController
{
    [Header("Enemy movement variables")]
    [SerializeField] private float _aggroMovementMultiplier = 1f;
    [SerializeField] private float _aggressiveGoToDistance;
    [SerializeField] private float _lostTargetWaitTime;
    [SerializeField] private float _patrolMoveMaxDistance;
    [SerializeField] private Vector2 _patrolMoveIntervalRange;
    [Header("Enemy attack variables")]
    [SerializeField] private AggroController _aggroController;
    [Tooltip("BoxCast halved dimension to validate if turret is aimgin to target")]
    [SerializeField] private Vector3 _lineOfSightBoxHalfExtents;
    [SerializeField] private bool _ignoreBlockedLineOfSight;
    [Header("On Damage")]
    [SerializeField] private float _aggroRangeDuration;
    [SerializeField] private float _aggroRangeMultiplier = 1f;

    [Space, Header("Execute events on destroy"), Space]
    [SerializeField] private UnityEvent _onDestroyEvents;

    private readonly float MIN_DISTANCE_CHECK = 0.1f;

    private bool _isPatrolIntervalActive;
    private bool _isTargetInSight;
    private float _aggroCheckRadius;
    private Vector3 _goTo;
    private Coroutine _aggroRangeOnDamageCoroutine;
    private Coroutine _lostTargetWaitCoroutine;
    private Coroutine _patrolIntervalCoroutine;

    #region LIFE_CYCLE

    protected override void OnDisable()
    {
        base.OnDisable();

        if (_healthController != null)
        {
            _healthController.TankHealthModified -= OnDamaged;
        }

        KillAggroRangeOnDamageCoroutine();
        KillPatrolIntervalCoroutine();
        KillLostTargetCoroutine();

        if (LevelManager.Instance == null)
        {
            return;
        }

        LevelManager.Instance.RemoveEnemy();
        LevelManager.Instance.LevelStart -= OnLevelStart;
        LevelManager.Instance.LevelEnd -= OnLevelEnd;
        LevelManager.Instance.PlayerDefeat -= OnPlayerDefeat;
    }

    protected override void Start()
    {
        base.Start();

        if (_healthController != null)
        {
            _healthController.TankHealthModified += OnDamaged;
        }

        if (_aggroController != null)
        {
            _aggroCheckRadius = _aggroController.AggroCheckRadius;
        }

        if (LevelManager.Instance == null)
        {
            return;
        }

        LevelManager.Instance.AddEnemy();
        LevelManager.Instance.LevelStart += OnLevelStart;
        LevelManager.Instance.LevelEnd += OnLevelEnd;
        LevelManager.Instance.PlayerDefeat += OnPlayerDefeat;

        if (LevelManager.Instance.ActiveGameplay)
        {
            _aggroController.SetIgnoreDestructible(new IDestructible[] { _healthController }).FirstAggroCheckTic();
        }
    }

    private void Update()
    {
        if (!_isAlive || (LevelManager.Instance != null && !LevelManager.Instance.ActiveGameplay))
        {
            return;
        }

        _isTargetInSight = CheckLineOfSight(_attackController.TurretTransform, _aggroController.AggroTarget, _lineOfSightBoxHalfExtents, _ignoreBlockedLineOfSight);
        Move(Time.deltaTime);
        Attack();
        MoveTurret();
    }

    #endregion

    private void OnLevelStart()
    {
        _aggroController.SetIgnoreDestructible(new IDestructible[] { _healthController }).FirstAggroCheckTic();
    }

    private void OnLevelEnd()
    {
        OnTankDestroyed();
    }

    private void OnPlayerDefeat() { }

    private void OnDamaged()
    {
        if (_aggroController == null || _aggroRangeDuration <= 0)
        {
            return;
        }

        _aggroController.SetAggroCheckRadius(_aggroCheckRadius * _aggroRangeMultiplier).AggroCheckTic();

        _aggroRangeOnDamageCoroutine = StartCoroutine(AggroRangeOnDamageDuration());

    }

    protected override void OnTankDestroyed()
    {
        base.OnTankDestroyed();

        _onDestroyEvents?.Invoke();
        Destroy(gameObject);
    }

    private void Move(float timeDelta)
    {
        bool updatePosition = false;

        if (_aggroController.AggroTarget == null)
        {
            if (_lostTargetWaitCoroutine != null)
            {
                updatePosition = Vector3.Distance(_goTo, _movementController.MovementTarget.position) > MIN_DISTANCE_CHECK;
            }
            else
            {
                if (!_isPatrolIntervalActive)
                {
                    _patrolIntervalCoroutine = StartCoroutine(PatrolIntervalWait());
                    Vector2 randomPositionRaw = Random.insideUnitCircle * _patrolMoveMaxDistance;
                    _goTo = new Vector3(randomPositionRaw.x, 0, randomPositionRaw.y) + _movementController.MovementTarget.position;
                }

                updatePosition = _isPatrolIntervalActive && Vector3.Distance(_goTo, _movementController.MovementTarget.position) > MIN_DISTANCE_CHECK;
            }
        }
        else
        {
            KillPatrolIntervalCoroutine();

            float targetDistance = Vector3.Distance(_movementController.MovementTarget.position, _aggroController.AggroTarget.position);
            float maxDistance = Mathf.Abs(_aggressiveGoToDistance);
            if (targetDistance > maxDistance)
            {
                Vector3 goToDirection = (_aggroController.AggroTarget.position - _movementController.MovementTarget.position).normalized;
                _goTo = goToDirection * _aggressiveGoToDistance + _movementController.MovementTarget.position;
                updatePosition = true;
            }

            KillLostTargetCoroutine();
            _lostTargetWaitCoroutine = StartCoroutine(LostTargetWait());
        }

        if (updatePosition)
        {
            MoveTank(timeDelta);
        }
    }

    private void MoveTank(float timeDelta)
    {
        Vector3 direction = (_goTo - _movementController.MovementTarget.position).normalized;

        float movementInput = Mathf.Max(Mathf.Abs(direction.x), Mathf.Abs(direction.z)) * timeDelta;

        if (_lostTargetWaitCoroutine != null)
        {
            movementInput *= _aggroMovementMultiplier;
        }
        _movementController.Move(movementInput);

        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        _movementController.Rotate(targetAngle);
    }

    private void Attack()
    {
        if (!_isTargetInSight || _attackController.OnCoolDown)
        {
            return;
        }

        _attackController.Attack();
    }

    private void MoveTurret()
    {
        if (_aggroController.AggroTarget == null)
        {
            return;
        }

        Vector3 lookAtDirection = _aggroController.AggroTarget.position - _movementController.MovementTarget.position;
        float targetAngle = Mathf.Atan2(lookAtDirection.x, lookAtDirection.z) * Mathf.Rad2Deg;
        _attackController.RotateTurret(targetAngle);
    }

    private IEnumerator AggroRangeOnDamageDuration()
    {
        yield return new WaitForSeconds(_aggroRangeDuration);

        _aggroController.SetAggroCheckRadius(_aggroCheckRadius);
        KillAggroRangeOnDamageCoroutine();
    }

    private void KillAggroRangeOnDamageCoroutine()
    {
        if (_aggroRangeOnDamageCoroutine == null)
        {
            return;
        }

        StopCoroutine(_aggroRangeOnDamageCoroutine);
        _aggroRangeOnDamageCoroutine = null;
    }

    private IEnumerator PatrolIntervalWait()
    {
        _isPatrolIntervalActive = true;
        yield return new WaitForSeconds(Random.Range(_patrolMoveIntervalRange.x, _patrolMoveIntervalRange.y));
        KillPatrolIntervalCoroutine();
    }

    private void KillPatrolIntervalCoroutine()
    {
        _isPatrolIntervalActive = false;

        if (_patrolIntervalCoroutine == null)
        {
            return;
        }

        StopCoroutine(_patrolIntervalCoroutine);
        _patrolIntervalCoroutine = null;
    }

    private IEnumerator LostTargetWait()
    {
        yield return new WaitForSeconds(_lostTargetWaitTime);
        KillLostTargetCoroutine();
    }

    private void KillLostTargetCoroutine()
    {
        if (_lostTargetWaitCoroutine == null)
        {
            return;
        }

        StopCoroutine(_lostTargetWaitCoroutine);
        _lostTargetWaitCoroutine = null;
    }

    private static bool CheckLineOfSight(Transform source, Transform target, Vector3 halfExtents, bool ignoreBlocked)
    {
        bool result = false;
        if (target == null)
        {
            return result;
        }

        float targetDistance = Vector3.Distance(source.position, target.position);
        if (ignoreBlocked)
        {
            RaycastHit[] hits = Physics.BoxCastAll(source.position, halfExtents, source.forward, source.rotation, targetDistance);

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject.transform == target)
                {
                    result = true;
                    break;
                }
            }
        }
        else
        {
            if (Physics.BoxCast(source.position, halfExtents, source.forward, out RaycastHit hit, source.rotation, targetDistance))
            {
                if (hit.collider.gameObject.transform == target)
                {
                    result = true;
                }
            }
        }

        return result;
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Gizmos.color = _aggroController.AggroTarget == null ? Color.green : Color.red;
        Gizmos.DrawWireSphere(_goTo, 0.25f);
    }

#endif
}
