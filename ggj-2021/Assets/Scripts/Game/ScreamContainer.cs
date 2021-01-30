using UnityEngine;

public class ScreamContainer : MonoBehaviour
{
  [SerializeField]
  private ScreamController _screamController = null;

  [SerializeField]
  private string _screamString = "eeh ooh aah";

  [SerializeField]
  private Interactable _mixInteractable = null;

  [SerializeField]
  private HoldableObject _holdable = null;

  public void FillScream(string inputScream)
  {
    _screamString = inputScream;
  }

  public void ReleaseScream()
  {
    if (!string.IsNullOrEmpty(_screamString))
    {
      _screamController.StartScream(_screamString, loopScream: false, volumeScale: 1);
      _screamString = null;
    }
  }

  private void OnEnable()
  {
    _mixInteractable.InteractionTriggered += OnInteractionTriggered;
    _holdable.IsHeldChanged += OnIsHeldChanged;
  }

  private void OnDisable()
  {
    _mixInteractable.InteractionTriggered -= OnInteractionTriggered;
    _holdable.IsHeldChanged -= OnIsHeldChanged;
  }

  private void Start()
  {
    if (!string.IsNullOrEmpty(_screamString))
    {
      _screamController.StartScream(_screamString, loopScream: true, volumeScale: 0.25f);
    }
  }

  private void OnIsHeldChanged()
  {
    _mixInteractable.enabled = !_holdable.IsHeld;
  }

  private void OnInteractionTriggered()
  {

  }
}