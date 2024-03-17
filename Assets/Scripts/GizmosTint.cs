using System;
using UnityEngine;


[ExecuteInEditMode]
public class GizmosTint : MonoBehaviour
{
    private enum ColliderType
    {
        BOX, SPHERE, CAPSULE, OTHER
    }
    private struct ColliderItem
    {
        public ColliderType Type;
        public Collider Collider;

        public ColliderItem(ColliderType type, Collider collider)
        {
            Type = type;
            Collider = collider;
        }
    }

    private ColliderItem[] _colliders;
    private readonly Color GIZMOS_COLOR = Color.red;

    private void Update()
    {
        GetColliders();
    }

    private void GetColliders()
    {
        Collider[] colliders = gameObject.GetComponents<Collider>();
        _colliders = new ColliderItem[colliders.Length];

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider col = colliders[i];
            ColliderType colType = ColliderType.OTHER;

            if (col.GetType() == typeof(BoxCollider))
            {
                colType = ColliderType.BOX;
            }
            else if (col.GetType() == typeof(SphereCollider))
            {
                colType = ColliderType.SPHERE;
            }
            else if (col.GetType() == typeof(CapsuleCollider))
            {
                colType = ColliderType.CAPSULE;
            }

            _colliders[i] = new ColliderItem(colType, col);
        }
    }

    private void OnDrawGizmos()
    {
        if (_colliders == null || _colliders.Length == 0)
        {
            return;
        }

        Gizmos.color = GIZMOS_COLOR;

        foreach (ColliderItem item in _colliders)
        {
            switch (item.Type)
            {
                case ColliderType.BOX:
                    DrawBoxGizmo(item.Collider as BoxCollider);
                    break;
                case ColliderType.SPHERE:
                    DrawSphereGizmo(item.Collider as SphereCollider);
                    break;
                case ColliderType.CAPSULE:
                    DrawCapsuleGizmo(item.Collider as CapsuleCollider);
                    break;
                default:
                    Debug.LogWarning("Unsopported collider type");
                    break;
            }
        }
    }

    private void DrawBoxGizmo(BoxCollider collider)
    {
        
    }

    private void DrawSphereGizmo(SphereCollider collider)
    {
        Bounds bounds = collider.bounds;

        Gizmos.DrawSphere(bounds.center, bounds.size.x / 2);
    }

    private void DrawCapsuleGizmo(CapsuleCollider collider)
    {
        
    }
}
