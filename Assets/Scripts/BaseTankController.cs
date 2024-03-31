using UnityEngine;

[DisallowMultipleComponent]
public class BaseTankController : MonoBehaviour
{
    [SerializeField] protected TankHealthController _healthController;
    [SerializeField] protected TankMovementController _movementController;
    [SerializeField] protected TurretAttackController _attackController;

    protected bool _isAlive;

    protected virtual void OnDisable()
    {
        if (_healthController != null)
        {
            _healthController.TankDestroyed -= OnTankDestroyed;
        }
    }

    private void Awake()
    {
        _isAlive = true;
    }

    protected virtual void Start()
    {
        if (_healthController != null)
        {
            _healthController.TankDestroyed += OnTankDestroyed;
        }
    }

    protected virtual void OnTankDestroyed()
    {
        _isAlive = false;
    }
}
