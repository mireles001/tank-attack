using UnityEngine;

public class DamageDealerController : MonoBehaviour
{
    [SerializeField] private int _damage;
    [SerializeField] private bool _isInstaDeath;
    [Header("Apply damage to:")]
    [SerializeField] private bool _player;
    [SerializeField] private bool _enemies;

    public void RequestDamage(IDestructible destructibleObject)
    {
        if (LevelManager.Instance == null)
        {
            return;
        }

        LevelSettings settings = LevelManager.Instance.Settings;

        if ((destructibleObject.GetObjectTag().Equals(settings.PlayerTag) && !_player) || (destructibleObject.GetObjectTag().Equals(settings.EnemyTag) && !_enemies))
        {
            return;
        }

        destructibleObject.ApplyDamage(_damage, _isInstaDeath);
    }
}
