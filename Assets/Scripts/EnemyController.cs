using UnityEngine;
using UnityEngine.Events;

public class EnemyController : BaseTankController
{
    [Space, Header("Execute events on destroy"), Space]
    [SerializeField] private UnityEvent _onDestroyEvents;

    protected override void OnDisable()
    {
        base.OnDisable();

        if (LevelManager.Instance == null)
        {
            return;
        }

        LevelManager.Instance.RemoveEnemy();
        LevelManager.Instance.LevelStart -= OnLevelStart;
        LevelManager.Instance.LevelEnd -= OnLevelEnd;
        LevelManager.Instance.PlayerDefeat -= OnPlayerDefeat;
    }

    protected override void Start()
    {
        base.Start();

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

    protected override void OnTankDestroyed()
    {
        base.OnTankDestroyed();

        _onDestroyEvents?.Invoke();
        Destroy(gameObject);
    }
}
