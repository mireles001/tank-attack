using UnityEngine;

[DisallowMultipleComponent]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private TankHealthController _healthController;
    [SerializeField] private TankMovementController _movementController;
    [SerializeField] private TankAttackController _attackController;

    private void OnDisable()
    {
        if (_healthController != null)
        {
            _healthController.TankDestroyed -= OnTankDestroyed;
        }

        if (LevelManager.Instance == null)
        {
            return;
        }

        LevelManager.Instance.RemoveEnemy();

        LevelManager.Instance.LevelStart -= OnLevelStart;
        LevelManager.Instance.LevelEnd -= OnLevelEnd;
        LevelManager.Instance.PlayerDefeat -= OnPlayerDefeat;
    }

    private void Start()
    {
        if (_healthController != null)
        {
            _healthController.TankDestroyed += OnTankDestroyed;
        }


        if (LevelManager.Instance == null)
        {
            return;
        }

        LevelManager.Instance.AddEnemy();

        LevelManager.Instance.LevelStart += OnLevelStart;
        LevelManager.Instance.LevelEnd += OnLevelEnd;
        LevelManager.Instance.PlayerDefeat += OnPlayerDefeat;
    }

    private void OnLevelStart()
    {

    }

    private void OnLevelEnd()
    {
        OnTankDestroyed();
    }

    private void OnPlayerDefeat()
    {

    }

    private void OnTankDestroyed()
    {
        Destroy(gameObject);
    }
}
