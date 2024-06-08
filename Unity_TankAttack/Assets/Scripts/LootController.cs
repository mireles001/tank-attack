using UnityEngine;
using DG.Tweening;

namespace Shibidubi.TankAttack
{
    public class LootController : MonoBehaviour
    {
        [SerializeField] private GameObject _lootPrefab;
        [SerializeField, Range(0, 1)] private float _dropProbability;
        [Space, SerializeField] private Vector3 _spawnPositionOffset;

        private readonly float SPAWN_TWEEN_DURATION = 0.25f;

        public void SpawnLoot()
        {
            if (_lootPrefab == null || _dropProbability == 0)
            {
                return;
            }

            if (Random.Range(0f, 1f) <= _dropProbability)
            {
                Transform dropInstance = Instantiate(_lootPrefab).transform;
                dropInstance.SetPositionAndRotation(transform.position + _spawnPositionOffset, transform.rotation);
                Vector3 baseScale = dropInstance.localScale;
                dropInstance.localScale = Vector3.zero;
                dropInstance.DOScale(baseScale, SPAWN_TWEEN_DURATION).SetEase(Ease.OutBack);
            }
        }
    }
}