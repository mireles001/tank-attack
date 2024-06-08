using UnityEngine;

namespace Shibidubi.TankAttack
{
    [CreateAssetMenu(fileName = "LevelSettings", menuName = "TankAttack/LevelSettings")]
    public class LevelSettings : ScriptableObject
    {
        public string PlayerTag
        {
            get
            {
                return _playerTag;
            }
        }
        public string EnemyTag
        {
            get
            {
                return _enemyTag;
            }
        }
        public float StartWaitDuration
        {
            get
            {
                return _startWaitDuration;
            }
        }
        public float EndWaitDuration
        {
            get
            {
                return _endWaitDuration;
            }
        }
        public float RetryWaitDuration
        {
            get
            {
                return _retryWaitDuration;
            }
        }

        [Header("Tags")]
        [SerializeField] private string _playerTag = "Player";
        [SerializeField] private string _enemyTag = "Enemy";
        [Header("Timers")]
        [SerializeField] private float _startWaitDuration;
        [SerializeField] private float _endWaitDuration;
        [SerializeField] private float _retryWaitDuration;
    }
}
