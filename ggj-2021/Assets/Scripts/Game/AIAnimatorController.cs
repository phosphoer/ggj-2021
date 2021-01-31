using UnityEngine;

public class AIAnimatorController : MonoBehaviour
{
  public enum LocomotionState
  {
    Idle = 0,
    Move = 1,
  }

  public enum EmoteState
  {
    Attack = 0,
    Cower = 1
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

  [SerializeField]
  private Animator _animator = null;

  private LocomotionState _currentLocomotionState;
  private EmoteState _currentEmoteState;
  private float _currentLocomotionSpeed;

  private bool _isDead;
  public bool IsDead
  {
    get { return _isDead; }
    set { _isDead = value; }
  }

  private static readonly int kAnimLocomotionState = Animator.StringToHash("LocomotionState");
  private static readonly int kAnimEmoteState = Animator.StringToHash("EmoteState");
  private static readonly int kAnimLocomotionSpeed = Animator.StringToHash("LocomotionSpeed");
  private static readonly int kAnimEmote = Animator.StringToHash("Emote");
  private static readonly int kAnimIsDead = Animator.StringToHash("IsDead");

  public void PlayEmote(EmoteState emote)
  {
    _animator.SetFloat(kAnimEmoteState, (float)emote);
    _animator.SetTrigger(kAnimEmote);
  }

  private void Update()
  {
    _animator.SetBool(kAnimIsDead, _isDead);
    _animator.SetFloat(kAnimLocomotionSpeed, Mathfx.Damp(_animator.GetFloat(kAnimLocomotionSpeed), (float)_currentLocomotionSpeed, 0.25f, Time.deltaTime * 5));
    _animator.SetFloat(kAnimLocomotionState, Mathfx.Damp(_animator.GetFloat(kAnimLocomotionState), (float)_currentLocomotionState, 0.25f, Time.deltaTime * 5));
  }
}