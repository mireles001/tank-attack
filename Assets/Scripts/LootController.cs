using UnityEngine;

namespace Shibidubi.TankAttack
{
    public class LootController : MonoBehaviour
    {
        [SerializeField] private GameObject _lootPrefab;
        [SerializeField, Range(0, 1)] private float _dropProbability;
        [Space, SerializeField] private Vector3 _spawnPositionOffset;

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
            }
        }
    }
}