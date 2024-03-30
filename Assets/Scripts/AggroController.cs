using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class AggroController : MonoBehaviour
{
    [SerializeField] private Transform _aggroCheckSource;
    [SerializeField] private Vector3 _aggroCheckOffset;
    [SerializeField] private float _aggroCheckRadius;

    private IDestructible _thisDestructible;
    private Transform _currentTarget;
    private const float CHECK_TIC_TIMER = 0.5f;
    private readonly Color TARGET_EXISTS = Color.red;
    private readonly Color TARGET_NULL = Color.gray;

    private Coroutine _aggroCheckTic;

    private void OnDisable()
    {
        KillAggroCheck();
    }

    public void OnLevelStart(IDestructible thisDestructible = null)
    {
        _thisDestructible = thisDestructible;
        _aggroCheckTic = StartCoroutine(AggroCheckWait());
    }

    private void AggroCheck()
    {
        KillAggroCheck();

        if (_aggroCheckSource != null && _aggroCheckRadius > 0)
        {
            Vector3 centerPoint = _aggroCheckSource.position + _aggroCheckSource.right * _aggroCheckOffset.x + _aggroCheckSource.up * _aggroCheckOffset.y + _aggroCheckSource.forward * _aggroCheckOffset.z;

            Transform newTarget = null;
            Collider[] colliders = Physics.OverlapSphere(centerPoint, _aggroCheckRadius);
            foreach (Collider collider in colliders)
            {
                if (collider.isTrigger)
                {
                    continue;
                }

                IDestructible destructible = collider.gameObject.GetComponent<IDestructible>();
                if (destructible == null || destructible == _thisDestructible)
                {
                    continue;
                }

                Transform colliderTransform = collider.gameObject.transform;
                if (newTarget == null)
                {
                    newTarget = colliderTransform;
                }
                else if (Vector3.Distance(_aggroCheckSource.position, colliderTransform.position) < Vector3.Distance(_aggroCheckSource.position, newTarget.position))
                {
                    newTarget = colliderTransform;
                }
            }

            _currentTarget = newTarget;
        }

        _aggroCheckTic = StartCoroutine(AggroCheckWait());
    }

    private IEnumerator AggroCheckWait()
    {
        yield return new WaitForSeconds(CHECK_TIC_TIMER);

        AggroCheck();
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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_aggroCheckSource == null || _aggroCheckRadius <= 0)
        {
            return;
        }

        Vector3 centerPoint = _aggroCheckSource.position + _aggroCheckSource.right * _aggroCheckOffset.x + _aggroCheckSource.up * _aggroCheckOffset.y + _aggroCheckSource.forward * _aggroCheckOffset.z;


        Handles.color = _currentTarget == null ? TARGET_NULL : TARGET_EXISTS;
        Handles.DrawWireDisc(centerPoint, Vector3.up, _aggroCheckRadius);

        if (_currentTarget != null)
        {
            Gizmos.color = _currentTarget == null ? TARGET_NULL : TARGET_EXISTS;
            Gizmos.DrawLine(_aggroCheckSource.position, _currentTarget.position);
        }
    }
#endif
}
