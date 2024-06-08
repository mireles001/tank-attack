using UnityEngine;
using DG.Tweening;

namespace Shibidubi.TankAttack
{
    public class ExitKeyController : MonoBehaviour
    {
        [SerializeField] private Collider _keyItemCollider;
        [SerializeField] private ParticleSystem _pickUpFx;

        private readonly float DESTROY_TWEEN_DURATION = 0.2f;

        private LevelSettings _settings;

        private void Start()
        {
            if (LevelManager.Instance == null)
            {
                return;
            }

            _settings = LevelManager.Instance.Settings;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (_settings != null && other.gameObject.tag.Equals(_settings.PlayerTag))
            {
                PickUpKey();
            }
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (_settings == null)
            {
                return;
            }

            if (collision.gameObject.tag.Equals(_settings.PlayerTag))
            {
                PickUpKey();
            }
            else
            {
                bool turnOffCollision = false;

                if (collision.gameObject.tag.Equals(_settings.EnemyTag))
                {
                    turnOffCollision = true;
                }
                else
                {
                    ProjectileController otherProjectile = collision.gameObject.GetComponent<ProjectileController>();
                    if (otherProjectile != null)
                    {
                        turnOffCollision = true;
                    }
                }

                if (turnOffCollision)
                {
                    Physics.IgnoreCollision(collision.collider, _keyItemCollider);
                }
            }
        }

        private void PickUpKey()
        {
            if (_pickUpFx != null)
            {
                Instantiate(_pickUpFx).transform.SetPositionAndRotation(transform.position, transform.rotation);
            }

            LevelManager.Instance.PickUpKey();
            _keyItemCollider.enabled = false;
            transform.DOScale(Vector3.zero, DESTROY_TWEEN_DURATION).SetEase(Ease.InBack).OnComplete(() => { Destroy(gameObject); });
        }
    }
}