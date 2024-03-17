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

    private void OnDrawGizmos()
    {
        if (_boxCollider == null)
        {
            return;
        }

        Bounds colliderBounds = _boxCollider.bounds;

        Vector3 colliderSize = _boxCollider.size;
        Vector3 transformScale = transform.localScale;

        Vector3 size = new Vector3(colliderSize.x * transformScale.x, colliderSize.y * transformScale.y, colliderSize.z * transformScale.z);

        Gizmos.color = Color.red;
        Gizmos.DrawCube(colliderBounds.center, colliderBounds.size);
    }
}
