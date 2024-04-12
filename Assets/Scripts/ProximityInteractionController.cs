using UnityEngine;

namespace Shibidubi.TankAttack
{
    [DisallowMultipleComponent]
    public class ProximityInteractionController : MonoBehaviour
    {
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
    }
}
