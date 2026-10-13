using UnityEngine;

namespace BusinessAsUsual.Prototype
{
    // Future jobs, inventory or evidence can add subclasses without changing detection/input.
    public abstract class Interactable : MonoBehaviour
    {
        public virtual Vector3 InteractionPoint => transform.position;
        public abstract string Prompt { get; }
        public virtual bool CanInteract(PlayerInteractor player) => true;
        public abstract bool Interact(PlayerInteractor player);
    }
}
