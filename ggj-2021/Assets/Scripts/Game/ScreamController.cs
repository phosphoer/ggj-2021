using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ScreamController : MonoBehaviour
{
  [SerializeField]
  private ScreamMappingDefinition _screamMapping = null;

  [SerializeField]
  private RangedFloat _screamInterval = new RangedFloat(0.5f, 1.0f);

  private List<string> _screamParts = new List<string>();
  private Coroutine _screamRoutine;

  [ContextMenu("Test Scream")]
  public void DebugTestScream()
  {
    StartScream("aah eeh ooh");
  }

  public void StartScream(string screamString)
  {
    if (_screamRoutine != null)
    {
      StopCoroutine(_screamRoutine);
    }

    _screamParts.Clear();
    _screamParts.AddRange(screamString.Split(' '));
    _screamRoutine = StartCoroutine(ScreamLoopAsync());
  }

  public void StopScream()
  {
    StopCoroutine(_screamRoutine);
    _screamRoutine = null;
  }

  private IEnumerator ScreamLoopAsync()
  {
    while (enabled)
    {
      for (int i = 0; i < _screamParts.Count; ++i)
      {
        string screamPart = _screamParts[i];
        ScreamSoundDefinition screamSound = _screamMapping.GetScream(screamPart);
        var audioInstance = AudioManager.Instance.PlaySound(gameObject, screamSound.Sound);
        // while (audioInstance.AudioSource.isPlaying)
        // yield return null;
        float waitTime = _screamInterval.RandomValue;
        while (waitTime > 0 && audioInstance.AudioSource.isPlaying)
        {
          waitTime -= Time.unscaledDeltaTime;
          yield return null;
        }
      }

      yield return null;
    }
  }
}