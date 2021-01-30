using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ScreamController : MonoBehaviour
{
  public event System.Action<string> ScreamStarted;
  public event System.Action ScreamEnded;
  public event System.Action<ScreamSoundDefinition> ScreamSoundPlayed;

  public bool IsScreaming => _screamRoutine != null;

  [SerializeField]
  private ScreamMappingDefinition _screamMapping = null;

  [SerializeField]
  private RangedFloat _screamInterval = new RangedFloat(0.5f, 1.0f);

  private List<string> _screamParts = new List<string>();
  private Coroutine _screamRoutine;
  private float _volumeScale;
  private bool _loopScream;

  [SerializeField]
  private string _testScream = "aah eeh ooh";

  [ContextMenu("Test Scream")]
  public void DebugTestScream()
  {
    StartScream(_testScream, loopScream: false);
  }

  [ContextMenu("Test Scream Loop")]
  public void DebugTestScreamLoop()
  {
    StartScream(_testScream, loopScream: true);
  }

  public void StartScream(string screamString, bool loopScream, float volumeScale = 1.0f)
  {
    if (IsScreaming)
    {
      StopScream();
    }

    _volumeScale = volumeScale;
    _loopScream = loopScream;

    _screamParts.Clear();
    _screamParts.AddRange(screamString.Split(' '));
    _screamRoutine = StartCoroutine(ScreamLoopAsync());

    ScreamStarted?.Invoke(screamString);
  }

  public void StopScream()
  {
    StopCoroutine(_screamRoutine);
    _screamRoutine = null;

    ScreamEnded?.Invoke();
  }

  private IEnumerator ScreamLoopAsync()
  {
    do
    {
      for (int i = 0; i < _screamParts.Count; ++i)
      {
        // Pick the appropriate scream
        string screamPart = _screamParts[i];
        ScreamSoundDefinition screamSound = _screamMapping.GetScream(screamPart);
        if (screamSound == null)
        {
          Debug.LogWarning($"Failed to get scream for sound: {screamPart}");
          continue;
        }

        var audioInstance = AudioManager.Instance.PlaySound(gameObject, screamSound.Sound);
        audioInstance.AudioSource.volume = 0;

        ScreamSoundPlayed?.Invoke(screamSound);

        // Wait for scream to be done or random interval
        float waitTime = _screamInterval.RandomValue;
        while (waitTime > 0 && audioInstance.AudioSource.isPlaying)
        {
          waitTime -= Time.unscaledDeltaTime;
          audioInstance.AudioSource.volume = Mathfx.Damp(audioInstance.AudioSource.volume, _volumeScale, 0.25f, Time.unscaledDeltaTime * 3);
          yield return null;
        }
      }

      yield return null;
    } while (enabled && _loopScream);

    _screamRoutine = null;
  }
}