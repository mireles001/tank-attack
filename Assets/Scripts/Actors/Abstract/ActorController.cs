using UnityEngine;

namespace Shibidubi.TankAttack
{
    [DisallowMultipleComponent]
    public abstract class ActorController : MonoBehaviour
    {
        [SerializeField] protected ActorHealthController _healthController;

        protected bool _isAlive;

        protected virtual void OnDisable()
        {
            if (_healthController != null)
            {
                _healthController.HealthDepleted -= OnActorDestroyed;
                _healthController.HealthModified -= OnActorDamaged;
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
                _healthController.HealthModified += OnActorDamaged;
                _healthController.HealthDepleted += OnActorDestroyed;
            }
        }

        protected virtual void OnActorDamaged() {}

        protected virtual void OnActorDestroyed()
        {
            _isAlive = false;
        }

        protected bool DoNotUpdate()
        {
            return !_isAlive || LevelManager.Instance == null || (LevelManager.Instance != null && !LevelManager.Instance.ActiveGameplay);
        }
    }
}