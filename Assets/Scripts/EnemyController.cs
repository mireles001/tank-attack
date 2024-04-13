using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Shibidubi.TankAttack
{
    public abstract class EnemyController : ActorController
    {
        [Header("Aggro")]
        [SerializeField] private AggroController _aggroController;
        [Tooltip("Attack target even if there is not clear line of sight")]
        [SerializeField] private bool _attackThrought;
        [SerializeField] private float _resetAggroWaitTime;
        [Header("OnDamage")]
        [Tooltip("On damage aggro check radius will multiply by this")]
        [SerializeField] private float _aggroRangeMultiplier = 1f; // 1 == Same check radius
        [Tooltip("Duration of modified aggro check radius before going back to normal")]
        [SerializeField] private float _aggroRangeDurationTime; 
        [Space, Header("OnDestroyed"), Space]
        [SerializeField] private UnityEvent _onDestroyEvents;

        private float _aggroCheckRadius;
        private Coroutine _onDamageCoroutine;
        private Coroutine _resetAggroCoroutine;

        protected bool _isTargetInSight;

        protected override void OnDisable()
        {
            base.OnDisable();

            KillOnDamageWait();
            KillResetAggro();

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RemoveEnemy();
                LevelManager.Instance.LevelStart -= OnLevelStart;
                LevelManager.Instance.LevelEnd -= OnLevelEnd;
                LevelManager.Instance.PlayerDefeat -= OnPlayerDefeat;
            }
        }

        protected override void Start()
        {
            base.Start();

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
                OnLevelStart();
            }
        }

        private void OnLevelStart()
        {
            _aggroController.SetIgnoreDestructible(new IDestructible[] { _healthController }).FirstAggroCheckTic();
        }

        private void OnLevelEnd()
        {
            OnActorDestroyed();
        }

        protected virtual void OnPlayerDefeat() { }

        protected override void OnActorDamaged()
        {
            if (_aggroController == null || _aggroRangeDurationTime <= 0)
            {
                return;
            }

            _aggroController.SetAggroCheckRadius(_aggroCheckRadius * _aggroRangeMultiplier).AggroCheckTic();

            _onDamageCoroutine = StartCoroutine(OnDamageWait());
        }

        protected override void OnActorDestroyed()
        {
            base.OnActorDestroyed();

            _onDestroyEvents?.Invoke();
            Destroy(gameObject);
        }

        private IEnumerator OnDamageWait()
        {
            yield return new WaitForSeconds(_aggroRangeDurationTime);

            _aggroController.SetAggroCheckRadius(_aggroCheckRadius);
            KillOnDamageWait();
        }

        private void KillOnDamageWait()
        {
            if (_onDamageCoroutine == null)
            {
                return;
            }

            StopCoroutine(_onDamageCoroutine);
            _onDamageCoroutine = null;
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

        protected static bool CheckLineOfSight(Transform source, Transform target, Vector3 halfExtents, bool attackThrought)
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