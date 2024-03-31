using UnityEngine;

[DisallowMultipleComponent]
public class TankMovementController : MonoBehaviour
{
    public Transform MovementTarget
    {
        get
        {
            return transform;
        }
    }

    [Header("Movement and Rotation")]
    [SerializeField] protected float _movementSpeed;
    [SerializeField] protected float _rotationSpeed;

    protected float _rotationSmoothVelocity;

    #region UNITY_EVENTS

    public void OnTriggerEnter(Collider other)
    {
        var interactable = other.gameObject.GetComponent<IInteractable>();
        interactable?.StartInteractionHandler(transform);
    }

    public void OnTriggerStay(Collider other)
    {
        var interactable = other.gameObject.GetComponent<IInteractable>();
        interactable?.InteractionHandler(transform);
    }

    public void OnTriggerExit(Collider other)
    {
        var interactable = other.gameObject.GetComponent<IInteractable>();
        interactable?.StopInteractionHandler(transform);
    }

    #endregion

    public virtual void Move(float moveInput)
    {
        MovementTarget.position += MovementTarget.forward * moveInput * _movementSpeed;
    }

    public void Rotate(float rotationAngle)
    {
        float smoothRotationAngle = Mathf.SmoothDampAngle(MovementTarget.eulerAngles.y, rotationAngle, ref _rotationSmoothVelocity, _rotationSpeed);

        MovementTarget.rotation = Quaternion.Euler(0, smoothRotationAngle, 0);
    }
}
