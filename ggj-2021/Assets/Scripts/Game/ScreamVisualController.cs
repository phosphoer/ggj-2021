using UnityEngine;

public class ScreamVisualController : MonoBehaviour
{
  [SerializeField]
  private ScreamController _screamController = null;

  [SerializeField]
  private ScreamVisual _screamVisualPrefab = null;

  private void OnEnable()
  {
    _screamController.ScreamSoundPlayed += OnScreamSoundPlayed;
  }

  private void OnDisable()
  {
    _screamController.ScreamSoundPlayed -= OnScreamSoundPlayed;
  }

  private void OnScreamSoundPlayed(ScreamSoundDefinition screamSound)
  {
    ScreamVisual screamVisual = Instantiate(_screamVisualPrefab);
    screamVisual.transform.position = transform.position;
    screamVisual.Scream = screamSound;
  }
}