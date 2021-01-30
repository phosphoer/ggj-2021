using UnityEngine;

public class HoldableObject : MonoBehaviour
{
  [SerializeField]
  private Interactable _interactable = null;

  [SerializeField]
  private Rigidbody _rb = null;

  private Transform _originalParent;

  public void StartHold()
  {
    _originalParent = transform.parent;
    _rb.isKinematic = true;
    _interactable.enabled = false;
  }

  public void StopHold()
  {
    transform.parent = _originalParent;
    _rb.isKinematic = false;
    _interactable.enabled = true;
  }

  private void OnEnable()
  {
    _interactable.InteractionTriggered += OnInteractionTriggered;
  }

  private void OnDisable()
  {
    _interactable.InteractionTriggered -= OnInteractionTriggered;
  }

  private void OnInteractionTriggered()
  {
    PlayerCharacterController.Instance.ObjectHolder.HoldObject(this);
  }
}