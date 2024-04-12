using UnityEditor;
using UnityEngine;

namespace Shibidubi
{
    [ExecuteInEditMode]
    public class CollidersDebugGizmos : MonoBehaviour
    {
#if UNITY_EDITOR
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

        [Header("EXECUTES IN EDITOR ONLY")]
        [SerializeField] private Color _gizmosColor = Color.red;

        private ColliderItem[] _colliders;

        private void LateUpdate()
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

            Gizmos.color = _gizmosColor;
            Handles.color = _gizmosColor;

            foreach (ColliderItem item in _colliders)
            {
                if (!item.Collider.enabled)
                {
                    continue;
                }

                switch (item.Type)
                {
                    case ColliderType.BOX:
                        DrawBoxGizmo(item.Collider as BoxCollider, transform);
                        break;
                    case ColliderType.SPHERE:
                        DrawSphereGizmo(item.Collider as SphereCollider, transform);
                        break;
                    case ColliderType.CAPSULE:
                        DrawCapsuleGizmo(item.Collider as CapsuleCollider, transform);
                        break;
                    default:
                        Debug.LogWarning("Unsopported collider type");
                        break;
                }
            }
        }

        private static void DrawBoxGizmo(BoxCollider collider, Transform t)
        {
            Bounds bounds = collider.bounds;
            Vector3 worldScale = t.lossyScale;

            Vector3 proportionalScale = new Vector3(Mathf.Abs(collider.size.x * worldScale.x), Mathf.Abs(collider.size.y * worldScale.y), Mathf.Abs(collider.size.z * worldScale.z));

            Vector3 xDisplacement = t.right * (proportionalScale.x / 2);
            Vector3 yDisplacement = t.up * (proportionalScale.y / 2);
            Vector3 zDisplacement = t.forward * (proportionalScale.z / 2);

            Vector3 topCenter = bounds.center + yDisplacement;
            Vector3 bottomCenter = bounds.center - yDisplacement;

            Vector3 topA = topCenter + xDisplacement + zDisplacement;
            Vector3 topB = topCenter + xDisplacement - zDisplacement;
            Vector3 topC = topCenter - xDisplacement - zDisplacement;
            Vector3 topD = topCenter - xDisplacement + zDisplacement;

            Vector3 botA = bottomCenter + xDisplacement + zDisplacement;
            Vector3 botB = bottomCenter + xDisplacement - zDisplacement;
            Vector3 botC = bottomCenter - xDisplacement - zDisplacement;
            Vector3 botD = bottomCenter - xDisplacement + zDisplacement;

            Gizmos.DrawLine(topA, topB);
            Gizmos.DrawLine(topB, topC);
            Gizmos.DrawLine(topC, topD);
            Gizmos.DrawLine(topD, topA);

            Gizmos.DrawLine(botA, botB);
            Gizmos.DrawLine(botB, botC);
            Gizmos.DrawLine(botC, botD);
            Gizmos.DrawLine(botD, botA);

            Gizmos.DrawLine(topA, botA);
            Gizmos.DrawLine(topB, botB);
            Gizmos.DrawLine(topC, botC);
            Gizmos.DrawLine(topD, botD);
        }

        private static void DrawSphereGizmo(SphereCollider collider, Transform t)
        {
            Bounds bounds = collider.bounds;
            Gizmos.DrawWireSphere(bounds.center, bounds.size.x / 2);
        }

        private static void DrawCapsuleGizmo(CapsuleCollider collider, Transform t)
        {
            const float arcAngle = 180f;
            Bounds bounds = collider.bounds;
            Vector3 worldScale = t.lossyScale;

            float proportionalRadius;
            float proportionalHeight;
            Vector3 centerA;
            Vector3 centerB;
            Vector3 colliderNormal;
            Vector3 heightDisplacement;
            Vector3 radiusDisplacementA;
            Vector3 radiusDisplacementB;

            if (collider.direction == 0) // 1 = X-Axis
            {
                proportionalRadius = collider.radius * Mathf.Max(Mathf.Abs(worldScale.y), Mathf.Abs(worldScale.z));

                proportionalHeight = GetCapsuleProportionalHeight(proportionalRadius, worldScale.x, collider.height);

                colliderNormal = t.right;
                radiusDisplacementA = t.forward * proportionalRadius;
                radiusDisplacementB = t.up * proportionalRadius;
            }
            else if (collider.direction == 1) // 1 = Y-Axis
            {
                proportionalRadius = collider.radius * Mathf.Max(Mathf.Abs(worldScale.x), Mathf.Abs(worldScale.z));

                proportionalHeight = GetCapsuleProportionalHeight(proportionalRadius, worldScale.y, collider.height);

                colliderNormal = t.up;
                radiusDisplacementA = t.forward * proportionalRadius;
                radiusDisplacementB = t.right * proportionalRadius;
            }
            else // Z-Axis
            {
                proportionalRadius = collider.radius * Mathf.Max(Mathf.Abs(worldScale.x), Mathf.Abs(worldScale.y));

                proportionalHeight = GetCapsuleProportionalHeight(proportionalRadius, worldScale.z, collider.height);

                colliderNormal = t.forward;
                radiusDisplacementA = t.up * proportionalRadius;
                radiusDisplacementB = t.right * proportionalRadius;
            }

            heightDisplacement = colliderNormal * proportionalHeight;
            centerA = bounds.center + heightDisplacement;
            centerB = bounds.center - heightDisplacement;

            Gizmos.DrawLine(centerA + radiusDisplacementA, centerB + radiusDisplacementA);
            Gizmos.DrawLine(centerA - radiusDisplacementA, centerB - radiusDisplacementA);
            Gizmos.DrawLine(centerA + radiusDisplacementB, centerB + radiusDisplacementB);
            Gizmos.DrawLine(centerA - radiusDisplacementB, centerB - radiusDisplacementB);

            float orientationPositive = collider.direction == 1 ? arcAngle : -arcAngle;
            float orientationNegative = collider.direction == 1 ? -arcAngle : arcAngle;
            Handles.DrawWireArc(centerA, radiusDisplacementA, radiusDisplacementB, orientationPositive, proportionalRadius);
            Handles.DrawWireArc(centerA, radiusDisplacementB, radiusDisplacementA, orientationNegative, proportionalRadius);
            Handles.DrawWireArc(centerB, radiusDisplacementA, radiusDisplacementB, orientationNegative, proportionalRadius);
            Handles.DrawWireArc(centerB, radiusDisplacementB, radiusDisplacementA, orientationPositive, proportionalRadius);
            Handles.DrawWireDisc(centerA, colliderNormal, proportionalRadius);
            Handles.DrawWireDisc(centerB, colliderNormal, proportionalRadius);
        }

        private static float GetCapsuleProportionalHeight(float radius, float scale, float colliderHeight)
        {
            float height = Mathf.Abs((colliderHeight / 2) * scale - radius);

            if (scale >= 0)
            {
                if (height < radius)
                {
                    height = (colliderHeight * Mathf.Abs(scale) - radius * 2) / 2;
                }
            }
            else
            {
                height -= radius * 2;
            }

            return Mathf.Max(0, height);
        }
#endif
    }
}