using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AICharacterController : MonoBehaviour
{
  public enum BehaviorState
  {
    Wander = 0,
    Chase,
    Attack,
    Cower,
    Flee,
    Dead
  };

  public CharacterMovementController Character => _characterMovement;
  public AIPerceptionComponent Perception => _perceptionComponent;
  public ScreamController ScreamController => _screamController;

  [SerializeField]
  private AIPerceptionComponent _perceptionComponent = null;

  [SerializeField]
  private CharacterMovementController _characterMovement = null;

  [SerializeField]
  private AIAnimatorController _aiAnimation = null;

  [SerializeField]
  private ScreamController _screamController = null;

  // Behavior State
  private BehaviorState _behaviorState = BehaviorState.Wander;
  private float _timeInBehavior = 0.0f;
  //-- Wander --
  public float WanderRange = 10.0f;
  //-- Chase --
  public float MaxChaseDistance = 50.0f;
  //-- Attack --
  public float AttackRange = 2.0f;
  public float AttackDuration = 2.0f;
  public float AttackTurnSpeed = 5.0f;
  //-- Cower --
  public float CowerDuration = 2.0f;
  [SerializeField]
  private ScreamSoundDefinition _cowerScream;

  // Path Finding State
  public float WaypointTolerance = 1.0f;
  public bool DebugDrawPath = false;

  List<Vector3> _lastPath = new List<Vector3>();
  float _pathRefreshPeriod = -1.0f;
  float _pathRefreshTimer = 0.0f;
  int _pathWaypointIndex = 0;
  Vector3 _spawnLocation = Vector3.zero;
  public bool IsPathStale
  {
    get { return (_pathRefreshPeriod >= 0.0f && _pathRefreshTimer <= 0.0f) || (_pathWaypointIndex >= _lastPath.Count); }
  }

  // Throttle State
  private Vector3 _throttleTarget = Vector3.zero;
  private float _throttleUrgency = 0.5f;
  private bool _hasValidThrottleTarget = false;

  private void Awake()
  {
  }

  private void Start()
  {
    _spawnLocation = this.transform.position;
  }

  private void OnDestroy()
  {

  }

  public void NotifyPlayerScream(IReadOnlyList<ScreamSoundDefinition> screams)
  {
    bool bIsScreamMatch = false;

    foreach (ScreamSoundDefinition scream in screams)
    {
      if (scream == _cowerScream)
      {
        bIsScreamMatch = true;
      }
    }

    if (bIsScreamMatch)
    {
      if (_behaviorState == BehaviorState.Wander)
      {
        SetBehaviorState(BehaviorState.Cower);
      }
    }
    else
    {
      if (_behaviorState == BehaviorState.Wander ||
          _behaviorState == BehaviorState.Flee)
      {
        SetBehaviorState(BehaviorState.Attack);
      }
    }
  }

  private void Update()
  {
    if (_behaviorState == BehaviorState.Dead)
      return;

    UpdateBehavior();
    UpdatePathRefreshTimer();
    UpdatePathFollowing();
    UpdateMoveVector();

    if (DebugDrawPath)
    {
      DrawPath();
    }
  }

  void UpdateBehavior()
  {
    BehaviorState nextBehavior = _behaviorState;

    switch (_behaviorState)
    {
      case BehaviorState.Wander:
        nextBehavior = UpdateBehavior_Wander();
        break;
      case BehaviorState.Chase:
        nextBehavior = UpdateBehavior_Chase();
        break;
      case BehaviorState.Cower:
        nextBehavior = UpdateBehavior_Cower();
        break;
      case BehaviorState.Attack:
        nextBehavior = UpdateBehavior_Attack();
        break;
      case BehaviorState.Flee:
        nextBehavior = UpdateBehavior_Flee();
        break;
      case BehaviorState.Dead:
        break;
    }

    SetBehaviorState(nextBehavior);
  }

  void SetBehaviorState(BehaviorState nextBehavior)
  {
    if (nextBehavior != _behaviorState)
    {
      OnBehaviorStateExited(_behaviorState);
      OnBehaviorStateEntered(nextBehavior);
      _behaviorState = nextBehavior;

      _timeInBehavior = 0.0f;
    }
    else
    {
      _timeInBehavior += Time.deltaTime;
    }
  }

  BehaviorState UpdateBehavior_Wander()
  {
    BehaviorState nextBehavior = BehaviorState.Wander;

    // Spotted the player
    if (_perceptionComponent.CanSeePlayer)
    {
      nextBehavior = BehaviorState.Chase;
    }
    // Have we reached our path destination
    else if (_pathWaypointIndex >= _lastPath.Count)
    {
      Vector2 offset = Random.insideUnitCircle * WanderRange;
      Vector3 wanderTarget = _spawnLocation + Vector3.left * offset.x + Vector3.forward * offset.y;

      if (PathFindManager.Instance.IsPointTraversable(wanderTarget))
      {
        RecomputePathTo(wanderTarget);
      }
    }

    return nextBehavior;
  }

  BehaviorState UpdateBehavior_Chase()
  {
    BehaviorState nextBehavior = BehaviorState.Chase;

    // Too far from spawn point?
    if (!IsWithingDistanceToTarget2D(_spawnLocation, MaxChaseDistance))
    {
      nextBehavior = BehaviorState.Flee;
    }

    // Within attack range?
    if (nextBehavior == BehaviorState.Chase)
    {
      Vector3 attackTarget = GetCurrentPlayerLocation();

      if (IsWithingDistanceToTarget2D(attackTarget, AttackRange))
      {
        nextBehavior = BehaviorState.Attack;
      }
    }

    // Have we lost sight of the player
    if (nextBehavior == BehaviorState.Chase && !_perceptionComponent.IsLastPlayerLocationNewerThan(1.0f))
    {
      nextBehavior = BehaviorState.Flee;
    }

    // Can see and keep pathing to the player?
    if (nextBehavior == BehaviorState.Chase)
    {
      // Assume a path check is going to fail
      nextBehavior = BehaviorState.Flee;

      // Only keep persuit if we haven't lost sight of the player for too long
      if (_perceptionComponent.IsLastPlayerLocationNewerThan(1.0f))
      {
        // Use the current player location rather than stale perception location to prevent oscillation
        Vector3 pathTarget = GetCurrentPlayerLocation();

        if (IsPathStale)
        {
          if (PathFindManager.Instance.IsPointTraversable(pathTarget))
          {
            if (RecomputePathTo(pathTarget))
            {
              nextBehavior = BehaviorState.Chase;
            }
          }
        }
        else
        {
          nextBehavior = BehaviorState.Chase;
        }
      }
    }

    return nextBehavior;
  }

  BehaviorState UpdateBehavior_Cower()
  {
    BehaviorState nextBehavior = BehaviorState.Attack;

    if (_timeInBehavior > CowerDuration)
    {
      nextBehavior = BehaviorState.Dead;
    }

    return nextBehavior;
  }

  BehaviorState UpdateBehavior_Attack()
  {
    BehaviorState nextBehavior = BehaviorState.Attack;

    if (_timeInBehavior > AttackDuration)
    {
      nextBehavior = BehaviorState.Flee;
    }
    else
    {
      FaceTowardTarget(GetCurrentPlayerLocation(), AttackTurnSpeed);
    }

    return nextBehavior;
  }

  BehaviorState UpdateBehavior_Flee()
  {
    BehaviorState nextBehavior = BehaviorState.Flee;

    if (_lastPath.Count == 0)
    {
      // Path failed to spawn point. This is our home now.
      _spawnLocation = transform.position;
      nextBehavior = BehaviorState.Wander;
    }
    // Have we reached our path destination
    else if (_pathWaypointIndex >= _lastPath.Count)
    {
      nextBehavior = BehaviorState.Wander;
    }

    return nextBehavior;
  }

  void OnBehaviorStateExited(BehaviorState oldBehavior)
  {
    switch (oldBehavior)
    {
      case BehaviorState.Wander:
        break;
      case BehaviorState.Chase:
        break;
      case BehaviorState.Cower:
        break;
      case BehaviorState.Attack:
        break;
      case BehaviorState.Flee:
        break;
      case BehaviorState.Dead:
        break;
    }
  }

  void OnBehaviorStateEntered(BehaviorState newBehavior)
  {
    switch (newBehavior)
    {
      case BehaviorState.Wander:
        _throttleUrgency = 0.5f; // half speed
        _pathRefreshPeriod = -1.0f; // manual refresh
        break;
      case BehaviorState.Chase:
        _throttleUrgency = 1.0f; // full speed
        _pathRefreshPeriod = 2.0f; // refresh path every 2 seconds while persuing player
        // Head to the player
        // If this fails we take care of it in attack update
        RecomputePathTo(GetCurrentPlayerLocation());
        break;
      case BehaviorState.Cower:
        _throttleUrgency = 0.0f; // Stop and sh*t yourself
        _pathRefreshPeriod = -1.0f; // manual refresh
        _aiAnimation.PlayEmote(AIAnimatorController.EmoteState.Cower);
        break;
      case BehaviorState.Attack:
        _throttleUrgency = 0.0f; // Stop and attack in place
        _pathRefreshPeriod = -1.0f; // manual refresh
        _aiAnimation.PlayEmote(AIAnimatorController.EmoteState.Attack);
        _screamController.StartScream(new List<ScreamSoundDefinition>() { _cowerScream }, false, 1.0f);
        PlayerCharacterController.Instance.NotifyMonsterScream();
        break;
      case BehaviorState.Flee:
        _throttleUrgency = 1.0f; // full speed
        _pathRefreshPeriod = -1.0f; // manual refresh
        // Head back to spawn location
        // If this fails we take care of it in flee update
        RecomputePathTo(_spawnLocation);
        break;
      case BehaviorState.Dead:
        _aiAnimation.IsDead = true;
        break;
    }
  }

  void UpdatePathRefreshTimer()
  {
    if (_pathRefreshPeriod >= 0)
    {
      _pathRefreshTimer -= Time.deltaTime;
      // Behavior decides where to recompute path too
    }
  }

  bool RecomputePathTo(Vector3 worldTarget)
  {
    _pathRefreshTimer = _pathRefreshPeriod;
    _pathWaypointIndex = 0;
    return PathFindManager.Instance.CalculatePathToPoint(transform.position, worldTarget, _lastPath);
  }

  void UpdatePathFollowing()
  {
    if (_pathWaypointIndex < _lastPath.Count)
    {
      // Always throttle at the next waypoint
      Vector3 waypoint = _lastPath[_pathWaypointIndex];
      Vector3 throttleTarget2d = Vector3.ProjectOnPlane(waypoint, Vector3.up);
      Vector3 position2d = Vector3.ProjectOnPlane(this.transform.position, Vector3.up);

      // Advance to the next waypoint 
      if (IsWithingDistanceToTarget2D(throttleTarget2d, WaypointTolerance))
      {
        ++_pathWaypointIndex;
      }
    }

    if (_pathWaypointIndex < _lastPath.Count)
    {
      Vector3 waypoint = _lastPath[_pathWaypointIndex];

      SetThrottleTarget(waypoint);
    }
    else
    {
      ClearThrottleTarget();
    }
  }

  void DrawPath()
  {
    if (_lastPath.Count <= 0)
      return;

    Vector3 PrevPathPoint = _lastPath[0];
    for (int pathIndex = 1; pathIndex < _lastPath.Count; ++pathIndex)
    {
      Vector3 NextPathPoint = _lastPath[pathIndex];
      Debug.DrawLine(PrevPathPoint, NextPathPoint, Color.red);
      PrevPathPoint = NextPathPoint;
    }
  }

  bool IsWithingDistanceToTarget2D(Vector3 target, float distance)
  {
    Vector3 target2d = Vector3.ProjectOnPlane(target, Vector3.up);
    Vector3 position2d = Vector3.ProjectOnPlane(this.transform.position, Vector3.up);

    return Vector3.Distance(target2d, position2d) <= distance;
  }

  void SetThrottleTarget(Vector3 target)
  {
    _throttleTarget = target;
    _hasValidThrottleTarget = true;
  }

  void ClearThrottleTarget()
  {
    _hasValidThrottleTarget = false;
  }

  void UpdateMoveVector()
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

  void FaceTowardTarget(Vector3 faceTarget, float faceAnimationSpeed)
  {
    Vector3 targetFoward = faceTarget - transform.position;
    Vector3 target2d = Vector3.ProjectOnPlane(targetFoward, Vector3.up);

    Quaternion desiredForwardRot = Quaternion.LookRotation(target2d);
    transform.rotation = Mathfx.Damp(transform.rotation, desiredForwardRot, 0.25f, Time.deltaTime * faceAnimationSpeed);
  }

  Vector3 GetCurrentPlayerLocation()
  {
    return PlayerCharacterController.Instance.transform.position;
  }
}