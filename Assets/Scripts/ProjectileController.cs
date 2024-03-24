using UnityEngine;

public class ProjectileController : MonoBehaviour
{
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

    private void ProjectileImpact(Vector3 spawnPoint)
    {
        if (_impactFx != null)
        {
            Instantiate(_impactFx).transform.position = spawnPoint;
        }

        Destroy(gameObject);
    }
}
