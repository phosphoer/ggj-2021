using UnityEngine;

public class AICharacterController : MonoBehaviour
{
  public enum BehaviorState
  {
    Idle = 0,
    Wander,
    Chase,
  }

  public CharacterMovementController Character => _characterMovement;
  public AIPerceptionComponent Perception => _perceptionComponent;

  [SerializeField]
  private AIPerceptionComponent _perceptionComponent = null;

  [SerializeField]
  private CharacterMovementController _characterMovement = null;

  [SerializeField]
  private AIAnimatorController _aiAnimation = null;

  private Vector3 _throttleTarget = Vector3.zero;
  private float _throttleUrgency = 0.5f;
  private bool _hasValidThrottleTarget = false;
  

  private Vector3 _lastKnownPlayerPosition = Vector3.zero;

  private void Awake()
  {
  }

  private void Start()
  {
  }

  private void OnDestroy()
  {

  }

  private void Update()
  {
    UpdateBehavior();
    UpdatePathFinding();
    UpdatePathFollowing();
    UpdateThrottle();
  }

  void UpdateBehavior()
  {

  }

  void UpdatePathFinding()
  {
    //CalculatePathToPoint(Vector3 fromPoint, Vector3 toPoint, List<Vector3> outPath)
  }

  void UpdatePathFollowing()
  {

  }

  void UpdateThrottle()
  {
    Vector3 throttleDirection = Vector3.zero;

    if (_hasValidThrottleTarget)
    {
      throttleDirection = _throttleTarget - this.transform.position;
      throttleDirection.y = 0;
      throttleDirection = Vector3.Normalize(throttleDirection);
    }

    _characterMovement.MoveVector = throttleDirection * _throttleUrgency;

    if (_characterMovement.CurrentVelocity.magnitude > 0.01f)
    {
      _aiAnimation.CurrentLocomotionSpeed = _characterMovement.CurrentVelocity.magnitude;
    }
    else
    {
      _aiAnimation.CurrentLocomotionSpeed = 1;
    }
  }
}