using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AICharacterController : MonoBehaviour
{
  public enum BehaviorState
  {
    Wander = 0,
    Idle,
    Chase,
    Attack,
    Cower,
    Flee,
    Dead
  };

  public CharacterMovementController Character => _characterMovement;
  public AIPerceptionComponent Perception => _perceptionComponent;
  public ScreamController ScreamController => _screamController;
  public AIAnimatorController AIAnimator => _aiAnimation;

  [SerializeField]
  private AIPerceptionComponent _perceptionComponent = null;

  [SerializeField]
  private CharacterMovementController _characterMovement = null;

  [SerializeField]
  private AIAnimatorController _aiAnimation = null;

  [SerializeField]
  private ScreamController _screamController = null;

  [SerializeField]
  private ScreamDamageable _screamDamageable = null;

  [SerializeField]
  private GameObject _deathBottleSpawn = null;

  [SerializeField]
  private GameObject _deathFX = null;

  [SerializeField]
  private SoundBank _deathSound = null;

  // Behavior State
  private BehaviorState _behaviorState = BehaviorState.Idle;
  private float _timeInBehavior = 0.0f;
  //-- Idle --
  public float IdleMinDuration = 0.5f;
  public float IdleMaxDuration = 3.0f;
  private float _idleDuration = 0.0f;
  //-- Wander --
  public float WanderRange = 10.0f;
  //-- Chase --
  public float ChaseTimeOut = 4.0f;
  public float MaxChaseDistance = 30.0f;
  //-- Attack --
  public float AttackRange = 2.0f;
  public float AttackDuration = 2.0f;
  public float AttackTurnSpeed = 5.0f;
  public float AttackCooldown = 5.0f;
  public float _timeSinceAttack = -1.0f;
  public bool HasAttackedRecently
  {
    get { return (_timeSinceAttack >= 0 && _timeSinceAttack < AttackCooldown); }
  }
  //-- Cower --
  public float CowerDuration = 2.0f;

  [SerializeField]
  private ScreamSoundDefinition _cowerScream = null;

  [SerializeField]
  private ScreamSoundDefinition _attackScream = null;

  // Path Finding State
  public float WaypointTolerance = 1.0f;
  public bool DebugDrawPath = false;
  List<Vector3> _lastPath = new List<Vector3>();
  float _pathRefreshPeriod = -1.0f;
  float _pathRefreshTimer = 0.0f;
  int _pathWaypointIndex = 0;
  float _pathfollowingStuckTimer = 0.0f;
  public bool IsPathFinished
  {
    // Hit end of the path
    get { return (_pathWaypointIndex >= _lastPath.Count); }
  }
  public bool CantMakePathProgress
  {
    // Got stuck on some geomtry following current path
    get { return _pathfollowingStuckTimer > 1.0f; }
  }
  public bool IsPathStale
  {
    get
    {
      return
      (_pathRefreshPeriod >= 0.0f && _pathRefreshTimer <= 0.0f) || // time for a refresh
      IsPathFinished || // Hit end of the path
      CantMakePathProgress; // Got stuck on some geomtry following current path
    }
  }

  Vector3 _spawnLocation = Vector3.zero;

  // Throttle State
  private Vector3 _throttleTarget = Vector3.zero;
  private float _throttleUrgency = 0.5f;
  private bool _hasValidThrottleTarget = false;

  private void Awake()
  {
  }

  private void OnEnable()
  {
    _screamDamageable.ScreamedAt += OnScreamedAt;
  }

  private void OnDisable()
  {
    _screamDamageable.ScreamedAt -= OnScreamedAt;
  }

  private void Start()
  {
    _spawnLocation = this.transform.position;
  }

  private void OnDestroy()
  {

  }

  private void OnScreamedAt(IReadOnlyList<ScreamSoundDefinition> screamSounds)
  {
    bool bIsScreamMatch = false;

    Debug.Log($"{name} was screamed at");

    foreach (ScreamSoundDefinition scream in screamSounds)
    {
      if (scream == _cowerScream)
      {
        bIsScreamMatch = true;
        Debug.Log($"{name} was screamed at with their least favorite scream: {scream.name}");
      }
    }

    if (bIsScreamMatch)
    {
      Debug.Log($"{name}'s state was {_behaviorState}");
      if (_behaviorState == BehaviorState.Wander || _behaviorState == BehaviorState.Idle)
      {
        Debug.Log($"{name} was set to cower");
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
    UpdateAnimationParameters();

    if (DebugDrawPath)
    {
      DrawPath();
    }
  }

  void UpdateBehavior()
  {
    BehaviorState nextBehavior = _behaviorState;

    // Used for attack cooldown
    if (_behaviorState != BehaviorState.Attack && _timeSinceAttack >= 0)
    {
      _timeSinceAttack += Time.deltaTime;
    }

    switch (_behaviorState)
    {
      case BehaviorState.Idle:
        nextBehavior = UpdateBehavior_Idle();
        break;
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

  BehaviorState UpdateBehavior_Idle()
  {
    BehaviorState nextBehavior = BehaviorState.Idle;

    // Give player some extra time if they are sneaking behind us
    bool isPlayerSneaking = PlayerCharacterController.Instance.IsSneaking;
    float idleTimeout = _perceptionComponent.IsPlayerNearbyBehind ? IdleMaxDuration : _idleDuration;

    // Spotted the player
    if (_perceptionComponent.CanSeePlayer)
    {
      nextBehavior = BehaviorState.Chase;
    }
    // Player was being "too noisy" behind us
    else if (_perceptionComponent.IsPlayerNearbyBehind && !isPlayerSneaking)
    {
      nextBehavior = BehaviorState.Chase;
    }
    // Been in idle too long, go somewhere else
    else if (_timeInBehavior >= idleTimeout)
    {
      nextBehavior = BehaviorState.Wander;
    }

    return nextBehavior;
  }

  BehaviorState UpdateBehavior_Wander()
  {
    BehaviorState nextBehavior = BehaviorState.Wander;

    // Spotted the player
    if (_perceptionComponent.CanSeePlayer)
    {
      nextBehavior = BehaviorState.Chase;
    }
    // Player was being "too noisy" behind us
    else if (_perceptionComponent.IsPlayerNearbyBehind)
    {
      if (PlayerCharacterController.Instance.IsSneaking)
      {
        nextBehavior = BehaviorState.Idle;
      }
      else
      {
        nextBehavior = BehaviorState.Chase;
      }
    }
    // Have we reached our path destination, chill for a bit
    else if (IsPathFinished || CantMakePathProgress)
    {
      nextBehavior = BehaviorState.Idle;
    }

    return nextBehavior;
  }

  BehaviorState UpdateBehavior_Chase()
  {
    BehaviorState nextBehavior = BehaviorState.Chase;

    // Too far from spawn point or chasing for too long?
    if (_timeInBehavior >= ChaseTimeOut || !IsWithingDistanceToTarget2D(_spawnLocation, MaxChaseDistance))
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

    // Have we lost sight of the player (Player specifically blocked line of sight to the enemy)
    if (nextBehavior == BehaviorState.Chase &&
        _perceptionComponent.PerformedLineOfSighckCheck &&
        !_perceptionComponent.HasLineOfSight &&
        !_perceptionComponent.IsLastPlayerLocationNewerThan(1.0f))
    {
      nextBehavior = BehaviorState.Flee;
    }

    // Have we lost sight of the player (been out of the vision cone for a while)
    if (nextBehavior == BehaviorState.Chase && !_perceptionComponent.IsLastPlayerLocationNewerThan(5.0f))
    {
      nextBehavior = BehaviorState.Flee;
    }

    // Can see and keep pathing to the player?
    if (nextBehavior == BehaviorState.Chase)
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
          else
          {
            // Can't path to target, flee
            nextBehavior = BehaviorState.Flee;
          }
        }
        else
        {
          // Target location not traversable, flee
          nextBehavior = BehaviorState.Flee;
        }
      }
      else
      {
        // Path not stale, keep pursuing
        nextBehavior = BehaviorState.Chase;
      }
    }

    return nextBehavior;
  }

  BehaviorState UpdateBehavior_Cower()
  {
    BehaviorState nextBehavior = BehaviorState.Cower;

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

    // Spotted the player and we haven't attacked too recently
    if (_perceptionComponent.CanSeePlayer && !HasAttackedRecently)
    {
      nextBehavior = BehaviorState.Chase;
    }
    // Failed to find path home
    else if (_lastPath.Count == 0)
    {
      // Path failed to spawn point. This is our home now.
      _spawnLocation = transform.position;
      nextBehavior = BehaviorState.Wander;
    }
    // Have we reached our path destination
    else if (IsPathFinished)
    {
      nextBehavior = BehaviorState.Wander;
    }

    return nextBehavior;
  }

  void OnBehaviorStateExited(BehaviorState oldBehavior)
  {
    switch (oldBehavior)
    {
      case BehaviorState.Idle:
        break;
      case BehaviorState.Wander:
        break;
      case BehaviorState.Chase:
        // Stop forcing line of sight checks 
        _perceptionComponent.ForceLineOfSightCheck = false;
        _perceptionComponent.ResetPlayerSpotTimer();
        // Stop dropping player sanity while pursuit active
        GameStateManager.Instance.PlayerSanity.OnPursuitStopped();
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
      case BehaviorState.Idle:
        _throttleUrgency = 0.0f; // stop
        _pathRefreshPeriod = -1.0f; // no refresh
        _idleDuration = Random.Range(IdleMinDuration, IdleMaxDuration);
        break;
      case BehaviorState.Wander:
        _throttleUrgency = 0.5f; // half speed
        _pathRefreshPeriod = -1.0f; // manual refresh
        // Pick a path to a wander target
        {
          Vector2 offset = Random.insideUnitCircle * WanderRange;
          Vector3 wanderTarget = _spawnLocation + Vector3.left * offset.x + Vector3.forward * offset.y;
          RecomputePathTo(wanderTarget);
        }
        break;
      case BehaviorState.Chase:
        _throttleUrgency = 1.0f; // full speed
        _pathRefreshPeriod = 2.0f; // refresh path every 2 seconds while persuing player
        // Force on line of sight checks even when player is out of vision cone
        _perceptionComponent.ForceLineOfSightCheck = true;
        // Start dropping player sanity while pursuit active
        GameStateManager.Instance.PlayerSanity.OnPursuitStarted();
        // Head to the player
        // If this fails we take care of it in attack update
        RecomputePathTo(GetCurrentPlayerLocation());
        break;
      case BehaviorState.Cower:
        _throttleUrgency = 0.0f; // Stop and sh*t yourself
        _pathRefreshPeriod = -1.0f; // manual refresh
        _aiAnimation.PlayEmote(AIAnimatorController.EmoteState.Cower);
        // Set animation dead flag early so that we don't leave emote state
        _aiAnimation.IsDead = true;
        // Hide the vision cone
        _perceptionComponent.gameObject.SetActive(false);
        break;
      case BehaviorState.Attack:
        _throttleUrgency = 0.0f; // Stop and attack in place
        _pathRefreshPeriod = -1.0f; // manual refresh

        _aiAnimation.PlayEmote(AIAnimatorController.EmoteState.Attack);
        _timeSinceAttack = 0.0f; // We just attacked

        var screamSounds = new List<ScreamSoundDefinition>() { _attackScream };
        _screamController.StartScream(screamSounds, false, 1.0f);
        ScreamDamageable.DoScream(screamSounds, _perceptionComponent.transform.position, _perceptionComponent.transform.forward, _screamDamageable);

        break;
      case BehaviorState.Flee:
        _throttleUrgency = 1.0f; // full speed
        _pathRefreshPeriod = -1.0f; // manual refresh
        // Head back to spawn location
        // If this fails we take care of it in flee update
        RecomputePathTo(_spawnLocation);
        break;
      case BehaviorState.Dead:
        // Play death effects to cover the transition
        if (_deathFX != null)
        {
          Instantiate(_deathFX, transform.position, Quaternion.identity);
        }

        AudioManager.Instance.PlaySound(gameObject, _deathSound);

        // Spawn a death bottles in out place
        if (_deathBottleSpawn != null)
        {
          GameObject bottle = Instantiate(_deathBottleSpawn, transform.position, Quaternion.identity);
          ScreamContainer screamContainer = bottle.GetComponent<ScreamContainer>();
          if (screamContainer != null)
          {
            screamContainer.FillScream(new List<ScreamSoundDefinition>() { _attackScream });
          }
        }
        // Clean ourselves up after a moment
        Destroy(this, 0.1f);
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
    _pathfollowingStuckTimer = 0.0f;
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
        _pathfollowingStuckTimer = 0.0f;
        _pathWaypointIndex++;
      }
      else
      {
        // If we aren't making progress toward the waypoint, increment the stuck timer
        if (_characterMovement.CurrentVelocity.magnitude < 0.01f)
        {
          _pathfollowingStuckTimer += Time.deltaTime;
        }
        else
        {
          _pathfollowingStuckTimer = 0.0f;
        }
      }
    }

    // Throttle at next waypoint
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
  }

  void UpdateAnimationParameters()
  {
    if (_characterMovement.CurrentVelocity.magnitude > 0.01f)
    {
      _aiAnimation.CurrentLocomotionSpeed = _characterMovement.CurrentVelocity.magnitude;
      _aiAnimation.CurrentLocomotionState = AIAnimatorController.LocomotionState.Move;
    }
    else
    {
      _aiAnimation.CurrentLocomotionSpeed = 0;
      _aiAnimation.CurrentLocomotionState = AIAnimatorController.LocomotionState.Idle;
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