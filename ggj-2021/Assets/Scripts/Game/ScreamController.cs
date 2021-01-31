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
  private List<AudioSource> _audioSources = new List<AudioSource>();
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

        AudioSource audioSource = GetAvailableAudioSource();
        AudioManager.ConfigureSourceForSound(audioSource, screamSound.Sound);
        AudioManager.PrepareSourceToPlay(audioSource, screamSound.Sound);
        audioSource.clip = screamSound.Sound.RandomClip;
        audioSource.volume = 0;
        audioSource.Play();

        ScreamSoundPlayed?.Invoke(screamSound);

        // Wait for scream to be done or random interval
        float waitTime = _screamInterval.RandomValue;
        while (waitTime > 0 && audioSource.isPlaying)
        {
          waitTime -= Time.unscaledDeltaTime;
          audioSource.volume = Mathfx.Damp(audioSource.volume, _volumeScale, 0.25f, Time.unscaledDeltaTime * 3);
          yield return null;
        }
      }

      yield return null;
    } while (enabled && _loopScream);

    _screamRoutine = null;
  }

  private AudioSource GetAvailableAudioSource()
  {
    for (int i = 0; i < _audioSources.Count; ++i)
    {
      if (!_audioSources[i].isPlaying)
        return _audioSources[i];
    }

    AudioSource audioSource = gameObject.AddComponent<AudioSource>();
    _audioSources.Add(audioSource);
    return audioSource;
  }
}