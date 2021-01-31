using UnityEngine;
using System.Collections.Generic;

public class PlayerAnimatorController : MonoBehaviour
{
  public enum LocomotionState
  {
    Idle = 0,
    Jog,
    Sneak,
    IdleCarry,
    JogCarry,
    SneakCarry
  }

  public enum EmoteState
  {
    OpenBottle = 0,
    Scared
  }

  public LocomotionState CurrentLocomotionState
  {
    get { return _currentLocomotionState; }
    set
    {
      _currentLocomotionState = value;
    }
  }

  public float CurrentLocomotionSpeed
  {
    get { return _currentLocomotionSpeed; }
    set
    {
      _currentLocomotionSpeed = value;
    }
  }

  public AnimatorCallbacks AnimatorCallbacks => _animatorCallbacks;

  [SerializeField]
  private Animator _animator = null;

  [SerializeField]
  private AnimatorCallbacks _animatorCallbacks = null;

  [SerializeField]
  private SoundBank _footFallSound = null;

  private LocomotionState _currentLocomotionState;
  private EmoteState _currentEmoteState;
  private float _currentLocomotionSpeed;
  private List<AudioSource> _footStepSources = new List<AudioSource>();

  private static readonly int kAnimLocomotionState = Animator.StringToHash("LocomotionState");
  private static readonly int kAnimEmoteState = Animator.StringToHash("EmoteState");
  private static readonly int kAnimLocomotionSpeed = Animator.StringToHash("LocomotionSpeed");
  private static readonly int kAnimEmote = Animator.StringToHash("Emote");

  public void PlayEmote(EmoteState emote)
  {
    _animator.SetFloat(kAnimEmoteState, (float)emote);
    _animator.SetTrigger(kAnimEmote);
  }

  private void OnEnable()
  {
    _animatorCallbacks.AddCallback("OnFootstep", OnFootstep);
  }

  private void OnDisable()
  {
    _animatorCallbacks.RemoveCallback("OnFootstep", OnFootstep);
  }

  private void Update()
  {
    _animator.SetFloat(kAnimLocomotionSpeed, Mathfx.Damp(_animator.GetFloat(kAnimLocomotionSpeed), (float)_currentLocomotionSpeed, 0.25f, Time.deltaTime * 5));
    _animator.SetFloat(kAnimLocomotionState, Mathfx.Damp(_animator.GetFloat(kAnimLocomotionState), (float)_currentLocomotionState, 0.25f, Time.deltaTime * 5));
  }

  private void OnFootstep()
  {
    AudioSource audioSource = GetAvailableAudioSource();
    AudioManager.ConfigureSourceForSound(audioSource, _footFallSound);
    AudioManager.PrepareSourceToPlay(audioSource, _footFallSound);
    audioSource.clip = _footFallSound.RandomClip;
    audioSource.Play();
  }

  private AudioSource GetAvailableAudioSource()
  {
    for (int i = 0; i < _footStepSources.Count; ++i)
    {
      if (!_footStepSources[i].isPlaying)
        return _footStepSources[i];
    }

    AudioSource audioSource = gameObject.AddComponent<AudioSource>();
    _footStepSources.Add(audioSource);
    return audioSource;
  }
}