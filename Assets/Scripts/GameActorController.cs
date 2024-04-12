using UnityEngine;

namespace Shibidubi.TankAttack
{
    [DisallowMultipleComponent]
    public abstract class GameActorController : MonoBehaviour
    {
        [SerializeField] protected ActorHealthController _healthController;

        protected bool _isAlive;

        protected virtual void OnDisable()
        {
            if (_healthController != null)
            {
                _healthController.HealthDepleted -= OnActorDestroyed;
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
                _healthController.HealthDepleted += OnActorDestroyed;
            }
        }

        protected virtual void OnActorDestroyed()
        {
            _isAlive = false;
        }
    }
}