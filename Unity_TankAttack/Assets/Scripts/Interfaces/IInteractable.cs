using UnityEngine;

namespace Shibidubi.TankAttack
{
    interface IInteractable
    {
        void StartInteractionHandler(Transform interactionSourceTransform);
        void InteractionHandler(Transform interactionSourceTransform);
        void StopInteractionHandler(Transform interactionSourceTransform);
    }
}