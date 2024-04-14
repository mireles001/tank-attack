using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

namespace Shibidubi.TankAttack
{
    public class DestructibleController : MonoBehaviour, IDestructible
    {
        [SerializeField] private int _hitPoints;
        [SerializeField] private ParticleSystem _destroyFx;
        [Space, Header("Execute events on destroy"), Space]
        [SerializeField] private UnityEvent _onDestroyEvents;

        private readonly float DESTROY_TWEEN_DURATION = 0.2f;

        private int _currentHitPoints;

        private void Awake()
        {
            _hitPoints = Mathf.Max(1, _hitPoints);
            _currentHitPoints = _hitPoints;
        }

        private void DestroyObject()
        {
            if (_destroyFx != null)
            {
                Instantiate(_destroyFx).transform.SetPositionAndRotation(transform.position, transform.rotation);
            }

            _onDestroyEvents?.Invoke();
            transform.DOScale(Vector3.zero, DESTROY_TWEEN_DURATION).SetEase(Ease.InBack).OnComplete(() => { Destroy(gameObject); });
        }

        #region IDESTRUCTIBLE_INTERFACE

        public string GetObjectTag()
        {
            return gameObject.tag;
        }

        public int GetHealth()
        {
            return _currentHitPoints;
        }

        public int GetMaxHealth()
        {
            return _hitPoints;
        }

        public void ApplyDamage(int damage, bool isInstaKill = false)
        {
            _currentHitPoints = Mathf.Max(0, _currentHitPoints - damage);

            if (_currentHitPoints == 0)
            {
                DestroyObject();
            }
        }

        #endregion
    }
}
