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
        if ((destructibleObject.GetObjectTag().Equals("Player") && !_player) || (destructibleObject.GetObjectTag().Equals("Enemy") && !_enemies))
        {
            return;
        }

        destructibleObject.ApplyDamage(_damage, _isInstaDeath);
    }
}
