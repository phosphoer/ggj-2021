using UnityEngine;
using System.Collections.Generic;

public class ScreamContainer : MonoBehaviour
{
  public HoldableObject Holdable => _holdable;
  public IReadOnlyList<ScreamSoundDefinition> ScreamSounds => _screamSounds;

  [SerializeField]
  private ScreamController _screamController = null;

  [SerializeField]
  private Interactable _mixInteractable = null;

  [SerializeField]
  private HoldableObject _holdable = null;

  [SerializeField]
  private List<ScreamSoundDefinition> _screamSounds = null;

  public void FillScream(IReadOnlyList<ScreamSoundDefinition> screamSounds)
  {
    _screamSounds.Clear();
    foreach (ScreamSoundDefinition screamSound in screamSounds)
    {
      if (screamSound != null)
      {
        _screamSounds.Add(screamSound);
      }
    }
    _screamController.StartScream(_screamSounds, loopScream: true, volumeScale: 0.25f);
  }

  public void AddScream(ScreamSoundDefinition screamSound)
  {
    _screamSounds.Add(screamSound);
    if (!_screamController.IsScreaming)
      _screamController.StartScream(_screamSounds, loopScream: true, volumeScale: 0.25f);
  }

  public void ReleaseScream()
  {
    if (_screamSounds.Count > 0)
    {
      _screamController.StartScream(_screamSounds, loopScream: false, volumeScale: 1);
      _screamSounds.Clear();
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
    if (_screamSounds.Count > 0)
    {
      _screamController.StartScream(_screamSounds, loopScream: true, volumeScale: 0.25f);
    }
  }

  private void Stop()
  {
    _screamController.StopScream();
  }

  private void OnIsHeldChanged()
  {
    _mixInteractable.enabled = !_holdable.IsHeld;
  }

  private void OnInteractionTriggered()
  {

  }
}