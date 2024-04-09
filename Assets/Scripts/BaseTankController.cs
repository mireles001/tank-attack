using UnityEngine;

[DisallowMultipleComponent]
public class BaseTankController : MonoBehaviour
{
    [SerializeField] protected HealthController _healthController;
    [SerializeField] protected TankMovementController _movementController;
    [SerializeField] protected TurretAttackController _attackController;

    protected bool _isAlive;

    protected virtual void OnDisable()
    {
        if (_healthController != null)
        {
            _healthController.HealthDepleted -= OnTankDestroyed;
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
            _healthController.HealthDepleted += OnTankDestroyed;
        }
    }

    protected virtual void OnTankDestroyed()
    {
        _isAlive = false;
    }
}
