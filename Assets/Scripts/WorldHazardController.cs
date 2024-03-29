using UnityEngine;

[DisallowMultipleComponent]
public class WorldHazardController : MonoBehaviour
{
    [SerializeField] private int _damage;
    [SerializeField] private bool _isInstaDeath;
    [Header("Apply damage to:")]
    [SerializeField] private bool _damagePlayer = true;
    [SerializeField] private bool _damageEnemies = true;

    private LevelSettings _settings;

    private void Start()
    {
        if (LevelManager.Instance == null)
        {
            return;
        }

        _settings = LevelManager.Instance.Settings;
    }

    public void RequestDamage(IDestructible destructibleObject)
    {
        if (_settings == null)
        {
            return;
        }

        if ((destructibleObject.GetObjectTag().Equals(_settings.PlayerTag) && !_damagePlayer) || (destructibleObject.GetObjectTag().Equals(_settings.EnemyTag) && !_damageEnemies))
        {
            return;
        }

        destructibleObject.ApplyDamage(_isInstaDeath ? destructibleObject.GetMaxHealth() : _damage);
    }
}
