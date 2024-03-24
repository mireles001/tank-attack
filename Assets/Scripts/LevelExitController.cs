using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class LevelExitController : MonoBehaviour
{
    [Tooltip("Locked = Non-Trigger, Unlocked = Trigger")]
    [SerializeField] private bool _toggleColliderIsTrigger;
    [Space, Header("Events to be played when exit is unlocked (optional)"), Space]
    [SerializeField] private UnityEvent _onUnlockEvents;

    public bool IsLocked { private set; get; } = true;

    private void Awake()
    {
        if (_toggleColliderIsTrigger)
        {
            UpdateColliderStatus();
        }
    }

    public void Unlock()
    {
        IsLocked = false;
        _onUnlockEvents?.Invoke();
        UpdateColliderStatus();
    }

    private void UpdateColliderStatus()
    {
        Collider[] colliders = gameObject.GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.isTrigger = !IsLocked;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = IsLocked ? Color.red : Color.green;
        Gizmos.DrawSphere(transform.position + Vector3.up * 0.333f, 0.333f);
    }
}
