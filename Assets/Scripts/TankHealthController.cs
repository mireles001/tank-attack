using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class TankHealthController : MonoBehaviour, IDestructible
{
    public event Action TankHealthModified;
    public event Action TankDestroyed;

    [SerializeField] private int _maxHealth;
    [SerializeField] private float _invincibilityDuration;
    [Header("Visual FXs")]
    [SerializeField] private ParticleSystem _tankDestroyedFx;

    private int _health;
    private bool _isInvincible;
    private Coroutine _invicibilityCoroutine;

    private void OnDisable()
    {
        KillInvicibilityCoroutine();
    }

    private void Awake()
    {
        _maxHealth = Mathf.Max(1, _maxHealth);
        _health = _maxHealth;
    }

    public void OnCollisionEnter(Collision collision)
    {
        DamageDealerInteraction(collision.gameObject.GetComponent<WorldHazardController>());
    }

    public void OnTriggerEnter(Collider other)
    {
        DamageDealerInteraction(other.gameObject.GetComponent<WorldHazardController>());
    }

    private void DamageDealerInteraction(WorldHazardController damageDealer)
    {
        if (damageDealer == null)
        {
            return;
        }

        damageDealer.RequestDamage(this);
    }

    private IEnumerator InvincibilityWaitTime(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        _isInvincible = false;
        KillInvicibilityCoroutine();
    }

    private void KillInvicibilityCoroutine()
    {
        if (_invicibilityCoroutine == null)
        {
            return;
        }

        StopCoroutine(_invicibilityCoroutine);
        _invicibilityCoroutine = null;
    }

    #region INTERFACE_DESTRUCTIBLE

    public string GetObjectTag()
    {
        return gameObject.tag;
    }

    public void ApplyDamage(int damage)
    {
        if (_isInvincible)
        {
            return;
        }

        _health = Mathf.Max(0, _health - damage);
        TankHealthModified?.Invoke();

        if (_health > 0)
        {
            if (_invincibilityDuration > 0)
            {
                _isInvincible = true;
                _invicibilityCoroutine = StartCoroutine(InvincibilityWaitTime(_invincibilityDuration));
            }
        }
        else
        {
            if (_tankDestroyedFx != null)
            {
                Instantiate(_tankDestroyedFx).transform.SetPositionAndRotation(transform.position, transform.rotation);
            }

            TankDestroyed?.Invoke();
        }
    }

    public int GetHealth()
    {
        return _health;
    }

    public int GetMaxHealth()
    {
        return _maxHealth;
    }

    #endregion
}
