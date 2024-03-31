using UnityEngine;
using UnityEngine.Events;

public class EnemyController : BaseTankController
{
    [Header("Enemy movement variables")]
    [SerializeField] private float _moveToDistance;
    [SerializeField] private float _moveToMaxRadius;
    [Header("Enemy attack variables")]
    [SerializeField] private AggroController _aggroController;
    [Tooltip("BoxCast halved dimension to validate if turret is aimgin to target")]
    [SerializeField] private Vector3 _lineOfSightBoxHalfExtents;
    [SerializeField] private bool _ignoreBlockedLineOfSight;

    [Space, Header("Execute events on destroy"), Space]
    [SerializeField] private UnityEvent _onDestroyEvents;

    private bool _isTargetInSight;

    #region LIFE_CYCLE

    protected override void OnDisable()
    {
        base.OnDisable();

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
        Move();
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

    protected override void OnTankDestroyed()
    {
        base.OnTankDestroyed();

        _onDestroyEvents?.Invoke();
        Destroy(gameObject);
    }

    private void Move()
    {
        if (_aggroController.AggroTarget == null)
        {

        }
        else
        {
            float targetDistance = Vector3.Distance(transform.position, _aggroController.AggroTarget.position);
            float maxDistance = Mathf.Abs(_moveToDistance);
            float maxRadius = Mathf.Abs(_moveToMaxRadius);
            if (targetDistance > maxDistance + maxRadius)
            {
                //Debug.Log("Away");
            }
        }
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
}
