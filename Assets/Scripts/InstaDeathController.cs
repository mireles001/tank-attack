using UnityEngine;

public class InstaDeathController : MonoBehaviour
{
    [SerializeField] private BoxCollider _boxCollider;

    public void OnTriggerEnter(Collider other)
    {
        IDestructible destructibleActor = other.gameObject.GetComponent<IDestructible>();
        if (destructibleActor != null)
        {
            destructibleActor.ApplyDamage(destructibleActor.GetMaxHealth(), true);
        }
    }
}
