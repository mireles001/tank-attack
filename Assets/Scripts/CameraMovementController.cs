using UnityEngine;

namespace Shibidubi.TankAttack
{
    public class CameraMovementController : MonoBehaviour
    {
        [SerializeField] private PlayerInputController _playerInput;
        [SerializeField] private Transform _cameraWrapper;
        [Header("Camera Rotation")]
        [SerializeField] private float _rotationSpeed = 1f;
        [SerializeField] private bool _inverted;
        [Space, Header("Input Labels")]
        [SerializeField] private string _rotateLeft = "CameraLeft";
        [SerializeField] private string _rotateRight = "CameraRight";

        private void LateUpdate()
        {
            if (_cameraWrapper == null)
            {
                return;
            }

            if (_playerInput != null)
            {
                _cameraWrapper.position = _playerInput.CameraTarget;
            }

            bool turnLeft = Input.GetButton(_rotateLeft);
            bool turnRight = Input.GetButton(_rotateRight);

            if (_rotationSpeed == 0 || turnLeft == turnRight)
            {
                return;
            }

            float rotationDirection = turnLeft ? 1 : -1;
            rotationDirection = _inverted ? -rotationDirection : rotationDirection;
            _cameraWrapper.Rotate(new Vector3(0, rotationDirection * _rotationSpeed, 0));
        }
    }
}
