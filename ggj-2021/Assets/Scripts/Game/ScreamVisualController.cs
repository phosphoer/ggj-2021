using UnityEngine;

public class ScreamVisualController : MonoBehaviour
{
  public Transform SpawnPoint => _spawnAnchorOverride != null ? _spawnAnchorOverride : transform;

  [SerializeField]
  private ScreamController _screamController = null;

  [SerializeField]
  private ScreamVisual _screamVisualPrefab = null;

  [SerializeField]
  private Transform _spawnAnchorOverride = null;

  private Vector3? _primaryScreamDir;

  private void OnEnable()
  {
    _screamController.ScreamSoundPlayed += OnScreamSoundPlayed;
    _screamController.ScreamStarted += OnScreamStarted;
  }

  private void OnDisable()
  {
    _screamController.ScreamSoundPlayed -= OnScreamSoundPlayed;
    _screamController.ScreamStarted -= OnScreamStarted;
  }

  private void OnScreamStarted()
  {
    if (!_screamController.IsLooping)
    {
      _primaryScreamDir = SpawnPoint.forward.WithY(0).normalized;
    }
    else
    {
      _primaryScreamDir = null;
    }
  }

  private void OnScreamSoundPlayed(ScreamSoundDefinition screamSound, float volumeScale)
  {
    ScreamVisual screamVisual = Instantiate(_screamVisualPrefab);
    screamVisual.transform.position = SpawnPoint.position;
    screamVisual.Scream = screamSound;
    screamVisual.Scale = volumeScale * 2;
    screamVisual.Direction = _primaryScreamDir.HasValue ? _primaryScreamDir.Value : Vector3.zero;
  }
}