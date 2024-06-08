using System.Collections;
using UnityEngine;

namespace Shibidubi.TankAttack
{
    public class EnemyTankController : EnemyController
    {
        [Space, Space, SerializeField] private TankMovementController _movementController;
        [Header("Attacking (Aggressive)")]
        [SerializeField] private TurretAttackController _attackController;
        [Tooltip("Multiply movement speed if has target")]
        [SerializeField] private float _aggroMovementMultiplier = 1f; // 1 == Same movement speed
        [Tooltip("Move up to this distance from target")]
        [SerializeField] private float _aggroGoToDistance;
        [Header("Patroling (Non-aggressive)")]
        [SerializeField] private float _patrolingMaxDistance;
        [SerializeField] private Vector2 _patrolingIntervalRange;
        [SerializeField] private float _forwardCheckDistance;
        [SerializeField] private Vector3 _forwardCheckOffset;
        [SerializeField] private float _forwardCheckAngleTolerance;

        private readonly float MIN_DISTANCE_CHECK = 0.1f;
        private readonly Vector3 LOS_BOXCAST_HALF_EXTENDS = new(0.25f, 0.25f, 0.25f);

        private bool _isPatroling;
        private Vector3 _goTo;
        private Coroutine _patrolingIntervalCoroutine;

        protected override void OnDisable()
        {
            base.OnDisable();

            KillPatrolingInterval();
        }

        private void Update()
        {
            if (DoNotUpdate() || _aggroController == null)
            {
                return;
            }

            _isTargetInSight = CheckLineOfSight(_attackController.TurretTransform, _aggroController.AggroTarget, LOS_BOXCAST_HALF_EXTENDS, _attackThrought);
            MoveTank(Time.deltaTime);
            Attack();
            MoveTurret(Time.deltaTime);
        }

        private void MoveTank(float timeDelta)
        {
            if (_aggroController.AggroTarget == null ? MoveTankPatroling() : MoveTankAggressive())
            {
                Vector3 direction = (_goTo - _movementController.MovementTarget.position).normalized;

                float movementInput = Mathf.Max(Mathf.Abs(direction.x), Mathf.Abs(direction.z)) * timeDelta;

                if (_resetAggroCoroutine != null)
                {
                    movementInput *= _aggroMovementMultiplier;
                }
                _movementController.Move(movementInput);

                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                _movementController.Rotate(targetAngle);
            }
        }

        private bool MoveTankPatroling()
        {
            bool updatePosition = false;

            // In case target was recently lost...
            if (_resetAggroCoroutine != null)
            {
                // Resume moving or not to last GoTo (last target position)
                updatePosition = Vector3.Distance(_goTo, _movementController.MovementTarget.position) > MIN_DISTANCE_CHECK;
            }
            // Patroling logic...
            else
            {
                Transform target = _movementController.MovementTarget;
                if (!_isPatroling)
                {
                    // Only once we define the next GoTo position
                    _patrolingIntervalCoroutine = StartCoroutine(PatrolingInterval());
                    Vector2 randomPositionRaw = Random.insideUnitCircle * _patrolingMaxDistance;
                    _goTo = new Vector3(randomPositionRaw.x, 0, randomPositionRaw.y) + target.position;
                }

                updatePosition = _isPatroling && Vector3.Distance(_goTo, target.position) > MIN_DISTANCE_CHECK;

                Vector3 forwardCheckOrigin = GetForwardCheckOrigin(target, _forwardCheckOffset);
                // Check if we are facing/near something up front
                if (Physics.Raycast(forwardCheckOrigin, target.forward, out RaycastHit hit, _forwardCheckDistance))
                {
                    // Stop updating position if there is a non-trigger collider in front
                    updatePosition = hit.collider.isTrigger;

                    // But we let it move if the GoTo is behind (so it rotates and get unstucked)
                    if (!updatePosition)
                    {
                        updatePosition = IsGoToPositionValid(target, _goTo, _forwardCheckAngleTolerance);
                    }
                }
            }

            return updatePosition;
        }

        private bool MoveTankAggressive()
        {
            bool updatePosition = false;

            KillPatrolingInterval();

            float targetDistance = Vector3.Distance(_movementController.MovementTarget.position, _aggroController.AggroTarget.position);
            float maxDistance = Mathf.Abs(_aggroGoToDistance);
            if (targetDistance > maxDistance)
            {
                Vector3 goToDirection = (_aggroController.AggroTarget.position - _movementController.MovementTarget.position).normalized;
                _goTo = goToDirection * _aggroGoToDistance + _movementController.MovementTarget.position;
                updatePosition = true;
            }

            KillAggroReset();
            _resetAggroCoroutine = StartCoroutine(BeginAggroReset());

            return updatePosition;
        }

        private void Attack()
        {
            if (!_isTargetInSight || _attackController.OnCoolDown)
            {
                return;
            }

            _attackController.Attack();
        }

        private void MoveTurret(float deltaTime)
        {
            if (_aggroController.AggroTarget == null)
            {
                return;
            }

            Vector3 lookAtDirection = _aggroController.AggroTargetPosition - _movementController.MovementTarget.position;
            _attackController.RotateTurret(lookAtDirection, deltaTime);
        }

        private IEnumerator PatrolingInterval()
        {
            _isPatroling = true;
            yield return new WaitForSeconds(Random.Range(_patrolingIntervalRange.x, _patrolingIntervalRange.y));
            KillPatrolingInterval();
        }

        private void KillPatrolingInterval()
        {
            _isPatroling = false;

            if (_patrolingIntervalCoroutine == null)
            {
                return;
            }

            StopCoroutine(_patrolingIntervalCoroutine);
            _patrolingIntervalCoroutine = null;
        }

        private static Vector3 GetForwardCheckOrigin(Transform origin, Vector3 offset)
        {
            return origin.right * offset.x + origin.up * offset.y + origin.forward * offset.z + origin.position;
        }

        /// <summary>
        /// Check if GoTo position is behind current position
        /// </summary>
        /// <param name="target"></param>
        /// <param name="goTo"></param>
        /// <param name="angleTolerance"></param>
        /// <returns></returns>
        private static bool IsGoToPositionValid(Transform target, Vector3 goTo, float angleTolerance)
        {
            Vector3 goToRelativeDirection = goTo - target.position;
            float targetAngle = Vector3.Angle(target.forward, goToRelativeDirection);
            return targetAngle > angleTolerance;
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (_aggroController != null)
            {
                Gizmos.color = _aggroController.AggroTarget == null ? Color.green : Color.red;
                Gizmos.DrawWireSphere(_goTo, 0.25f);
            }

            if (_isPatroling && _movementController != null)
            {
                Transform target = _movementController.MovementTarget;
                Vector3 origin = GetForwardCheckOrigin(target, _forwardCheckOffset);
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(origin, origin + target.forward * _forwardCheckDistance);
            }
        }

#endif
    }
}
