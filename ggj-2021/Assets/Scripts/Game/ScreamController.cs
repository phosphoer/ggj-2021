using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ScreamController : MonoBehaviour
{
  public event System.Action ScreamStarted;
  public event System.Action ScreamEnded;
  public event System.Action<ScreamSoundDefinition, float> ScreamSoundPlayed;

  public bool IsScreaming => _screamRoutine != null;
  public bool IsLooping => _loopScream;

  [SerializeField]
  private RangedFloat _screamInterval = new RangedFloat(0.5f, 1.0f);

  private List<ScreamSoundDefinition> _screamSounds = new List<ScreamSoundDefinition>();

  private List<AudioSource> _audioSources = new List<AudioSource>();
  private Coroutine _screamRoutine;
  private bool _loopScream;

  [SerializeField]
  private ScreamSoundDefinition[] _testScream = null;

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

  public void StartScream(IReadOnlyList<ScreamSoundDefinition> screamSounds, bool loopScream, float volumeScale = 1.0f)
  {
    if (IsScreaming)
    {
      StopScream();
    }

    _loopScream = loopScream;
    _screamSounds.Clear();
    _screamSounds.AddRange(screamSounds);

    ScreamStarted?.Invoke();

    _screamRoutine = StartCoroutine(ScreamLoopAsync(volumeScale, loopScream));
  }

  public void StopScream()
  {
    StopCoroutine(_screamRoutine);
    _screamRoutine = null;

    ScreamEnded?.Invoke();
  }

  private IEnumerator ScreamLoopAsync(float volumeScale, bool loop)
  {
    do
    {
      for (int i = 0; i < _screamSounds.Count; ++i)
      {
        ScreamSoundDefinition screamSound = _screamSounds[i];
        AudioSource audioSource = GetAvailableAudioSource();
        AudioManager.ConfigureSourceForSound(audioSource, screamSound.Sound);
        AudioManager.PrepareSourceToPlay(audioSource, screamSound.Sound);
        audioSource.clip = screamSound.Sound.RandomClip;
        audioSource.volume = 0;
        audioSource.Play();

        ScreamSoundPlayed?.Invoke(screamSound, volumeScale);

        // Wait for scream to be done or random interval
        float waitTime = _screamInterval.RandomValue;
        if (!loop)
          waitTime *= 0.5f;
        while (waitTime > 0 && audioSource.isPlaying)
        {
          waitTime -= Time.unscaledDeltaTime;
          audioSource.volume = Mathfx.Damp(audioSource.volume, volumeScale, 0.25f, Time.unscaledDeltaTime * 3);
          yield return null;
        }
      }

      yield return null;
    } while (enabled && loop);

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