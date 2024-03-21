using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LevelExitController : MonoBehaviour
{
    public bool IsLocked
    {
        get
        {
            return _isLocked;
        }
    }

    private bool _isLocked = true;

    private void Awake()
    {
        Collider[] colliders = gameObject.GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.isTrigger = true;
        }
    }

    public void SetIsLocked(bool val)
    {
        if (_isLocked == val)
        {
            return;
        }

        _isLocked = val;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _isLocked ? Color.red : Color.green;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}
