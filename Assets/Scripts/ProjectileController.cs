using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private int _projectileDamage;
    [SerializeField] private float _projectileSpeed;
    [SerializeField] private float _autoDestroyTimer = 5f;
    [SerializeField] private ParticleSystem _impactFx;

    private float _currentTimer;

    private void Update()
    {
        transform.Translate(Vector3.forward * _projectileSpeed * Time.deltaTime);

        _currentTimer += Time.deltaTime;

        if (_currentTimer >= _autoDestroyTimer)
        {
            Destroy(gameObject);
            return;
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        IDestructible destructibleObject = collision.gameObject.GetComponent<IDestructible>();
        if (destructibleObject != null)
        {
            destructibleObject.ApplyDamage(_projectileDamage);
        }

        if (collision.contactCount > 0)
        {
            ProjectileImpact(collision.contacts[0].point);
        }
    }

    private void ProjectileImpact(Vector3 spawnPoint)
    {
        if (_impactFx != null)
        {
            Instantiate(_impactFx).transform.position = spawnPoint;
        }

        Destroy(gameObject);
    }
}
