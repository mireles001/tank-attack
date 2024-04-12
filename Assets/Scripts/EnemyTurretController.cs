using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Shibidubi.TankAttack
{
    public class EnemyTurretController : MonoBehaviour
    {
        [SerializeField] private ActorHealthController _healthController;
        [SerializeField] protected TurretAttackController _attackController;
        [Header("Aggressive")]
        [SerializeField] private AggroController _aggroController;
        [Tooltip("Attack target even if there is not clear line of sight")]
        [SerializeField] private bool _attackThrought;
        [SerializeField] private float _resetAggroWaitTime;
        [Header("On Damage")]
        [Tooltip("On damage aggro check radius will multiply by this")]
        [SerializeField] private float _aggroRangeMultiplier = 1f; // 1 == Same check radius
        [Tooltip("Duration of modified aggro check radius before going back to normal")]
        [SerializeField] private float _aggroRangeDuration;
        [Space, Header("Execute events OnTankDestroyed"), Space]
        [SerializeField] private UnityEvent _onDestroyEvents;

        private readonly Vector3 LOS_BOXCAST_HALF_EXTENDS = new Vector3(0.3f, 0.25f, 0.25f);

        private bool _isAlive;
        private bool _isTargetInSight;
        private float _aggroCheckRadius;
        private Coroutine _onDamagedCoroutine;
        private Coroutine _resetAggroCoroutine;

        #region LIFE_CYCLE

        private void OnDisable()
        {
            KillOnDamagedWait();
            KillResetAggro();

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RemoveEnemy();
                LevelManager.Instance.LevelStart -= OnLevelStart;
                LevelManager.Instance.LevelEnd -= OnLevelEnd;
                LevelManager.Instance.PlayerDefeat -= OnPlayerDefeat;
            }

            if (_healthController != null)
            {
                _healthController.HealthModified -= OnDamaged;
                _healthController.HealthDepleted -= OnTurretDestroyed;
            }
        }

        private void Start()
        {
            _isAlive = true;

            if (_healthController != null)
            {
                _healthController.HealthModified += OnDamaged;
                _healthController.HealthDepleted += OnTurretDestroyed;
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

            _isTargetInSight = CheckLineOfSight(_attackController.TurretTransform, _aggroController.AggroTarget, LOS_BOXCAST_HALF_EXTENDS, _attackThrought);
            Attack();
            MoveTurret(Time.deltaTime);
        }

        #endregion

        private void OnLevelStart()
        {
            _aggroController.SetIgnoreDestructible(new IDestructible[] { _healthController }).FirstAggroCheckTic();
        }

        private void OnLevelEnd()
        {
            OnTurretDestroyed();
        }

        private void OnDamaged()
        {
            if (_aggroController == null || _aggroRangeDuration <= 0)
            {
                return;
            }

            _aggroController.SetAggroCheckRadius(_aggroCheckRadius * _aggroRangeMultiplier).AggroCheckTic();

            _onDamagedCoroutine = StartCoroutine(OnDamagedWait());

        }

        private void OnPlayerDefeat() { }

        private void OnTurretDestroyed()
        {
            _isAlive = false;
            _onDestroyEvents?.Invoke();
            Destroy(gameObject);
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

            Vector3 lookAtDirection = _aggroController.AggroTargetPosition - _attackController.TurretTransform.position;
            _attackController.RotateTurret(lookAtDirection, deltaTime);
        }

        private IEnumerator OnDamagedWait()
        {
            yield return new WaitForSeconds(_aggroRangeDuration);

            _aggroController.SetAggroCheckRadius(_aggroCheckRadius);
            KillOnDamagedWait();
        }

        private void KillOnDamagedWait()
        {
            if (_onDamagedCoroutine == null)
            {
                return;
            }

            StopCoroutine(_onDamagedCoroutine);
            _onDamagedCoroutine = null;
        }

        private IEnumerator ResetAggro()
        {
            yield return new WaitForSeconds(_resetAggroWaitTime);
            KillResetAggro();
        }

        private void KillResetAggro()
        {
            if (_resetAggroCoroutine == null)
            {
                return;
            }

            StopCoroutine(_resetAggroCoroutine);
            _resetAggroCoroutine = null;
        }

        private static bool CheckLineOfSight(Transform source, Transform target, Vector3 halfExtents, bool attackThrought)
        {
            bool result = false;
            if (target == null)
            {
                return result;
            }

            float targetDistance = Vector3.Distance(source.position, target.position);
            if (attackThrought)
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
}
