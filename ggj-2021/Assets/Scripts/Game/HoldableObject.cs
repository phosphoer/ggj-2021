using UnityEngine;

public class HoldableObject : MonoBehaviour
{
  public event System.Action IsHeldChanged;

  public bool IsHeld => _isHeld;

  [SerializeField]
  private Interactable _interactable = null;

  [SerializeField]
  private Rigidbody _rb = null;

  private Transform _originalParent;
  private bool _isHeld;

  public void StartHold()
  {
    _originalParent = transform.parent;
    _rb.isKinematic = true;
    _interactable.enabled = false;
    _isHeld = true;
    IsHeldChanged?.Invoke();
  }

  public void StopHold()
  {
    transform.parent = _originalParent;
    _rb.isKinematic = false;
    _interactable.enabled = true;
    _isHeld = false;
    IsHeldChanged?.Invoke();
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
  }
}