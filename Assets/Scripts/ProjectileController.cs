using System.Collections;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private Collider _projectileCollider;
    [SerializeField] private int _projectileDamage;
    [SerializeField] private float _projectileSpeed;
    [SerializeField] private float _autoDestroyTimer = 5f;
    [SerializeField] private ParticleSystem _impactFx;

    private Coroutine _autoDestroyCoroutine;

    private void OnDisable()
    {
        KillCoroutine();
    }

    public Transform StartUp(Collider[] sourceColliders)
    {
        foreach (Collider collider in sourceColliders)
        {
            Physics.IgnoreCollision(collider, _projectileCollider);
        }

        _autoDestroyCoroutine = StartCoroutine(AutoDestroyWait());

        return transform;
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * _projectileSpeed * Time.deltaTime);
    }

    public void OnCollisionEnter(Collision collision)
    {
        collision.gameObject.GetComponent<IDestructible>()?.ApplyDamage(_projectileDamage);

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

    private IEnumerator AutoDestroyWait()
    {
        yield return new WaitForSeconds(_autoDestroyTimer);

        Destroy(gameObject);
    }

    private void KillCoroutine()
    {
        if (_autoDestroyCoroutine == null)
        {
            return;
        }

        StopCoroutine(_autoDestroyCoroutine);
        _autoDestroyCoroutine = null;
    }
}
