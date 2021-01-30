using UnityEngine;

public class InteractionController : MonoBehaviour
{
  public Transform TrackedTransform
  {
    get { return _trackedTransform; }
    set { _trackedTransform = value; }
  }

  public Interactable ClosestInteractable => _closestInteractable;

  [SerializeField]
  private Transform _trackedTransform = null;

  private int _lazyUpdateIndex;
  private Interactable _closestInteractable;

  private void OnDisable()
  {
    if (_closestInteractable != null)
    {
      _closestInteractable.HidePrompt();
      _closestInteractable = null;
    }
  }

  private void Update()
  {
    if (_lazyUpdateIndex < Interactable.InstanceCount)
    {
      // Check if the current interactable is still in range
      float distToClosest = Mathf.Infinity;
      if (_closestInteractable != null)
      {
        distToClosest = Vector3.Distance(_trackedTransform.position, _closestInteractable.transform.position);
        bool isInLightOfSight = IsInLineOfSight(_closestInteractable);
        if (distToClosest >= _closestInteractable.InteractionRadius || !isInLightOfSight || !_closestInteractable.enabled)
        {
          _closestInteractable.HidePrompt();
          _closestInteractable = null;
          distToClosest = Mathf.Infinity;
        }
      }

      // Get the distance to the next potential interactable
      Interactable interactable = Interactable.GetInstance(_lazyUpdateIndex);
      Vector3 toInteractable = interactable.transform.position - _trackedTransform.position;
      float distToInteractable = toInteractable.magnitude;

      // Decide if this interactable is more contextual than the current one
      bool isInteractableMoreContextual = distToInteractable < distToClosest;
      isInteractableMoreContextual &= distToInteractable < interactable.InteractionRadius;
      isInteractableMoreContextual &= interactable != _closestInteractable;

      // If the new interactable is more contextual than the previous, make it the highlighted one
      if (isInteractableMoreContextual)
      {
        // Make this interactable the current one, if it was in line of sight
        if (IsInLineOfSight(interactable))
        {
          if (_closestInteractable != null)
          {
            _closestInteractable.HidePrompt();
          }

          _closestInteractable = interactable;
          _closestInteractable.ShowPrompt();
        }
      }
    }
    else
    {
      if (_closestInteractable != null)
      {
        _closestInteractable.HidePrompt();
      }

      _closestInteractable = null;
    }

    if (Interactable.InstanceCount > 0)
    {
      _lazyUpdateIndex = (_lazyUpdateIndex + 1) % Interactable.InstanceCount;
    }
  }

  private bool IsInLineOfSight(Interactable interactable)
  {
    if (!interactable.RequiresLineOfSight)
      return true;

    // If the interactable requires line of sight to be interacted with, cast a ray and mark it not interactable if we 
    // hit something other than ourselves
    bool inLightOfSight = true;
    RaycastHit hitInfo = default(RaycastHit);
    Vector3 fromPos = _trackedTransform.position.WithY(interactable.transform.position.y);
    Vector3 visibilityRay = interactable.transform.position - fromPos;
    if (Physics.Raycast(fromPos, visibilityRay.normalized, out hitInfo, visibilityRay.magnitude))
    {
      if (!hitInfo.collider.transform.IsChildOf(_trackedTransform) && !hitInfo.collider.transform.IsChildOf(interactable.transform))
        inLightOfSight = false;
    }

    return inLightOfSight;
  }
}