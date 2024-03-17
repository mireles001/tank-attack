using UnityEngine;

[ExecuteInEditMode]
public class DrawRotatedGizmo : MonoBehaviour
{
    public Vector3 Position { get { return _position; } }
    public Vector3 Rotation { get { return _rotation; } }
    public Vector3 Scale { get { return _scale; } }

    [SerializeField] private Vector3 _position;
    [SerializeField] private Vector3 _rotation;
    [SerializeField] private Vector3 _scale;

    private const float SPHERE_GIZMO_SIZE = 0.1f;

    private void Update()
    {
        Transform target = transform;
        target.localPosition = Position;
        target.localEulerAngles = Rotation;
        target.localScale = Scale;
    }

    private void OnDrawGizmos()
    {
        

        float rotation = Rotation.y - 90;
        Vector3 halvedSize = Scale / 2;
        Vector3 height = Vector3.up * Scale.y;
        Vector3 halvedHeight = height / 2;
        float rawLineAngle = Mathf.Atan2(halvedSize.z, halvedSize.x) * Mathf.Rad2Deg + rotation;
        Quaternion rawLineRotation = Quaternion.AngleAxis(rawLineAngle, Vector3.up);
        float hypotenuse = Mathf.Sqrt(halvedSize.x * halvedSize.x + halvedSize.z * halvedSize.z);
        Vector3 rawLine = Vector3.forward * hypotenuse;
        Vector3 botA = (rawLineRotation * rawLine) + Position - halvedHeight;

        rawLineAngle = Mathf.Atan2(Scale.z, 0) * Mathf.Rad2Deg + rotation;
        rawLineRotation = Quaternion.AngleAxis(rawLineAngle, Vector3.up);
        rawLine = -Vector3.forward * Scale.z;
        Vector3 botB = (rawLineRotation * rawLine) + botA;

        rawLineAngle = Mathf.Atan2(0, Scale.x) * Mathf.Rad2Deg + rotation;
        rawLineRotation = Quaternion.AngleAxis(rawLineAngle, Vector3.up);
        rawLine = -Vector3.forward * Scale.x;

        Vector3 botC = (rawLineRotation * rawLine) + botA;
        Vector3 botD = (rawLineRotation * rawLine) + botB;

        Vector3 topA = botA + height;
        Vector3 topB = botB + height;
        Vector3 topC = botC + height;
        Vector3 topD = botD + height;

        // Draw Corners
        bool validSize = Scale.x > 0 && Scale.y > 0 && Scale.z > 0;
        Color linesColor = validSize ? Color.cyan : Color.red;
        DrawSpaceGizmo(botA, botB, linesColor, true);
        DrawSpaceGizmo(botB, botD, linesColor, true);
        DrawSpaceGizmo(botD, botC, linesColor, true);
        DrawSpaceGizmo(botC, botA, linesColor, true);
        DrawSpaceGizmo(topA, topB, linesColor, true);
        DrawSpaceGizmo(topB, topD, linesColor, true);
        DrawSpaceGizmo(topD, topC, linesColor, true);
        DrawSpaceGizmo(topC, topA, linesColor, true);
        DrawSpaceGizmo(botA, topA, linesColor);
        DrawSpaceGizmo(botB, topB, linesColor);
        DrawSpaceGizmo(botC, topC, linesColor);
        DrawSpaceGizmo(botD, topD, linesColor);
    }

    private static void DrawSpaceGizmo(Vector3 start, Vector3 end, Color gizmoColor, bool drawSphere = false)
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawLine(start, end);

        if (drawSphere && SPHERE_GIZMO_SIZE > 0)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(end, SPHERE_GIZMO_SIZE);
        }
    }
}
