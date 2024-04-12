using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Shibidubi.TankAttack
{
    public class AggroController : MonoBehaviour
    {
        public float AggroCheckRadius
        {
            get
            {
                return _aggroCheckRadius;
            }
        }
        public Vector3 AggroTargetPosition
        {
            get
            {
                return _currentTargetPosition;
            }
        }
        public Transform AggroTarget
        {
            get
            {
                return _currentTarget;
            }
        }

        [SerializeField] private Transform _aggroCheckSource;
        [SerializeField] private Vector3 _aggroCheckOffset;
        [SerializeField] private float _aggroCheckRadius;
        [Space, Header("Targeting:"), Space]
        [Tooltip("Will not update target until current is lost")]
        [SerializeField] private bool _lockOnTarget;
        [Space]
        [SerializeField] private bool _targetPlayer = true;
        [SerializeField] private bool _targetEnemies;
        [SerializeField] private bool _targetOthers;

        private const float CHECK_TIC_TIMER = 0.333f;
        private readonly Color HAS_TARGET = Color.red;
        private readonly Color NULL_TARGET = Color.gray;

        private IDestructible[] _ignoreDestructibles;
        private Transform _currentTarget;
        private Vector3 _currentTargetPosition;
        private LevelSettings _settings;
        private Coroutine _aggroCheckTic;

        private void OnDisable()
        {
            KillAggroCheck();
        }

        #region PUBLIC_FUNCTIONS

        public AggroController SetIgnoreDestructible(IDestructible[] ignoreDestructibles)
        {
            _ignoreDestructibles = ignoreDestructibles;
            return this;
        }

        public AggroController SetAggroCheckRadius(float aggroCheckRadius)
        {
            _aggroCheckRadius = aggroCheckRadius;
            return this;
        }

        public void FirstAggroCheckTic()
        {
            if (LevelManager.Instance != null)
            {
                _settings = LevelManager.Instance.Settings;
            }

            AggroCheck();
            _aggroCheckTic = StartCoroutine(AggroCheckWait(UnityEngine.Random.Range(0, CHECK_TIC_TIMER) + 0.1f));
        }

        public void AggroCheckTic()
        {
            KillAggroCheck();
            AggroCheck();
            _aggroCheckTic = StartCoroutine(AggroCheckWait(CHECK_TIC_TIMER));
        }

        #endregion

        private void AggroCheck()
        {
            if (_settings == null || _aggroCheckSource == null || _aggroCheckRadius <= 0 || (!_targetPlayer && !_targetEnemies && !_targetOthers))
            {
                if (_currentTarget != null)
                {
                    _currentTarget = null;
                }

                return;
            }

            bool skipUpdateTarget = false;
            Transform newTarget = null;
            Collider[] colliders = Physics.OverlapSphere(GetCenterPoint(_aggroCheckSource, _aggroCheckOffset), _aggroCheckRadius);
            foreach (Collider collider in colliders)
            {
                if (collider.isTrigger)
                {
                    continue;
                }

                IDestructible destructible = collider.gameObject.GetComponent<IDestructible>();
                if (destructible == null || (_ignoreDestructibles != null && _ignoreDestructibles.Length > 0 && Array.IndexOf(_ignoreDestructibles, destructible) > -1))
                {
                    continue;
                }

                bool isPlayer = collider.gameObject.tag.Equals(_settings.PlayerTag);
                bool isEnemy = collider.gameObject.tag.Equals(_settings.EnemyTag);

                if (!((_targetPlayer && isPlayer) || (_targetEnemies && isEnemy) || (_targetOthers && !isPlayer && !isEnemy)))
                {
                    continue;
                }

                Transform colliderTransform = collider.gameObject.transform;
                if (_lockOnTarget && colliderTransform == _currentTarget)
                {
                    skipUpdateTarget = true;
                    break;
                }

                if (newTarget == null)
                {
                    newTarget = colliderTransform;
                }
                else if (Vector3.Distance(_aggroCheckSource.position, colliderTransform.position) < Vector3.Distance(_aggroCheckSource.position, newTarget.position))
                {
                    newTarget = colliderTransform;
                }

                _currentTargetPosition = collider.bounds.center;
            }

            if (skipUpdateTarget)
            {
                return;
            }

            _currentTarget = newTarget;
        }

        private IEnumerator AggroCheckWait(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            AggroCheckTic();
        }

        private void KillAggroCheck()
        {
            if (_aggroCheckTic == null)
            {
                return;
            }

            StopCoroutine(_aggroCheckTic);
            _aggroCheckTic = null;
        }

        private static Vector3 GetCenterPoint(Transform sourceTransform, Vector3 offset)
        {
            return sourceTransform.position + sourceTransform.right * offset.x + sourceTransform.up * offset.y + sourceTransform.forward * offset.z;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_aggroCheckSource == null)
            {
                return;
            }

            Handles.color = _currentTarget == null ? NULL_TARGET : HAS_TARGET;
            Handles.DrawWireDisc(GetCenterPoint(_aggroCheckSource, _aggroCheckOffset), Vector3.up, _aggroCheckRadius);

            if (_currentTarget != null)
            {
                Gizmos.color = _currentTarget == null ? NULL_TARGET : HAS_TARGET;
                Gizmos.DrawLine(_aggroCheckSource.position, _currentTarget.position);
            }
        }
#endif
    }
}
