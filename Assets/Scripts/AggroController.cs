using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class AggroController : MonoBehaviour
{
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
    [SerializeField] private bool _targetPlayer;
    [SerializeField] private bool _targetEnemies;
    [SerializeField] private bool _targetOthers;

    private const float CHECK_TIC_TIMER = 0.333f;
    private readonly Color TARGET_EXISTS = Color.red;
    private readonly Color TARGET_NULL = Color.gray;

    private IDestructible[] _ignoreDestructibles;
    private Transform _currentTarget;
    private LevelSettings _settings;
    private Coroutine _aggroCheckTic;

    private void OnDisable()
    {
        KillAggroCheck();
    }

    public AggroController SetIgnoreDestructible(IDestructible[] ignoreDestructibles)
    {
        _ignoreDestructibles = ignoreDestructibles;
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

    private void AggroCheckTic()
    {
        KillAggroCheck();
        AggroCheck();
        _aggroCheckTic = StartCoroutine(AggroCheckWait(CHECK_TIC_TIMER));
    }

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

        Handles.color = _currentTarget == null ? TARGET_NULL : TARGET_EXISTS;
        Handles.DrawWireDisc(GetCenterPoint(_aggroCheckSource, _aggroCheckOffset), Vector3.up, _aggroCheckRadius);

        if (_currentTarget != null)
        {
            Gizmos.color = _currentTarget == null ? TARGET_NULL : TARGET_EXISTS;
            Gizmos.DrawLine(_aggroCheckSource.position, _currentTarget.position);
        }
    }
#endif
}
