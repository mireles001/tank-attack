using UnityEngine;

/// <summary>
/// Visual manipulation component that rotates and scale up/down a targeted transform
/// by its interactor's relative distance 
/// </summary>
public class InteractiveVisualController : MonoBehaviour, IInteractable
{
    [Tooltip("Recomended a child wrapper with Vector3.zero rotation and Vector3.one scale")]
    [SerializeField] private Transform _visuals;
    [Header("Interactive Rotation")]
    [Tooltip("Angle 0 disables rotation manipulation")]
    [SerializeField, Range(0, 90)] private float _rotationAngleLimit;
    [Header("Interactive Scale")]
    [Tooltip("Scale on 0,0,0 disables scale manipulation")]
    [SerializeField] private Vector3 _scaleDelta;

    private Transform _currentInteractionSource;
    private float _maxContactDistance;
    private Vector3 _visualsBaseScale;
    private Vector3 _visualsBaseRotation;

    /// <summary>
    /// Saves its initial/base rotation and scale values
    /// </summary>
    private void Start()
    {
        _visualsBaseRotation = _visuals.eulerAngles;
        _visualsBaseScale = _visuals.localScale;
    }

    private void InteractionAction()
    {
        Vector3 thisPosition = transform.position;
        Vector3 otherPosition = _currentInteractionSource.position;
        float interactionMagnitude = Mathf.Clamp(1 - (Vector3.Distance(thisPosition, otherPosition) / _maxContactDistance), 0f, 1f);

        if (_rotationAngleLimit > 0)
        {
            // Direction
            Vector3 rawDirection = thisPosition - otherPosition;
            float directionAngle = Mathf.Atan2(rawDirection.x, rawDirection.z) * Mathf.Rad2Deg;
            directionAngle += _visualsBaseRotation.y;
            // Rotation
            Vector3 rawRotation = Quaternion.Euler(interactionMagnitude * _rotationAngleLimit, 0, 0) * Vector3.up;
            Vector3 rotateTowards = (Quaternion.Euler(0, directionAngle, 0) * rawRotation) + thisPosition;

            _visuals.up = (rotateTowards - thisPosition).normalized;
            _visuals.eulerAngles += _visualsBaseRotation;
        }

        if (_scaleDelta != Vector3.zero)
        {
            Vector3 scaleTowards = _scaleDelta * interactionMagnitude;
            _visuals.localScale = _visualsBaseScale + scaleTowards;
        }
    }

    #region IINTERACTABLE_INTERFACE_IMPLEMENTATION
    public void StartInteractionHandler(Transform interactionSourceTransform)
    {
        // Stores our interactor transform when interaction begins
        _currentInteractionSource = interactionSourceTransform;
        // Calculates the initial distance when interaction begins (used as max distance)
        _maxContactDistance = Vector3.Distance(transform.position, _currentInteractionSource.position);
    }

    public void InteractionHandler(Transform interactionSourceTransform)
    {
        if (_currentInteractionSource != interactionSourceTransform || _currentInteractionSource == null)
        {
            return;
        }

        InteractionAction();
    }

    public void StopInteractionHandler(Transform interactionSourceTransform)
    {
        if (_currentInteractionSource != interactionSourceTransform)
        {
            return;
        }

        // Reset to default/base values
        _currentInteractionSource = null;
        _visuals.eulerAngles = _visualsBaseRotation;
        _visuals.localScale = _visualsBaseScale;
    }
    #endregion
}
