using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPerceptionComponent : MonoBehaviour
{
  public float VisionAngleDegrees = 30;
  public float CanSeeAngleMultiplier = 1.2f;
  public float VisionDistance = 10;
  public float CanSeeDistanceMultiplier = 2.0f;
  public float NearbyDistance = 3;
  public float RefreshInterval = 0.1f;
  public bool DrawDebug = true;

  private float _refreshTimer = 0.0f;

  public GameObject VisionCone => _visionCone;

  [SerializeField]
  private GameObject _visionCone = null;
  private Renderer _visionConeRenderer = null;

  [SerializeField]
  private Material _normalMaterial = null;

  [SerializeField]
  private Material _attackMaterial = null;

  private bool _canSeePlayer = false;
  public bool CanSeePlayer
  {
    get { return _canSeePlayer; }
  }

  private float _awareness = 0;
  public float Awareness
  {
    get { return _awareness; }
  }

  private bool _forceLineOfSightCheck = false;
  public bool ForceLineOfSightCheck
  {
    get { return _forceLineOfSightCheck; }
    set { _forceLineOfSightCheck = value; }
  }

  private bool _performedLineOfSightCheck = false;
  public bool PerformedLineOfSighckCheck
  {
    get { return _performedLineOfSightCheck; }
  }

  private bool _hasLineOfSight = false;
  public bool HasLineOfSight
  {
    get { return _hasLineOfSight; }
  }

  private bool _isPlayerNearbyBehind = false;
  public bool IsPlayerNearbyBehind
  {
    get { return _isPlayerNearbyBehind; }
  }

  private float _currentVisionAngleDegrees = 0;
  public float CurrentVisionAngleDegrees
  {
    get { return _currentVisionAngleDegrees; }
  }

  private float _currentVisionDistance = 0;
  public float CurrentVisionDistance
  {
    get { return _currentVisionDistance; }
  }

  private Vector3 _lastSeenPlayerLocation = Vector3.zero;
  public Vector3 LastSeenPlayerLocation
  {
    get { return _lastSeenPlayerLocation; }
  }

  private float _timeSinceLastSeenPlayer = -1.0f;
  public float TimeSinceLastSeenPlayer
  {
    get { return _timeSinceLastSeenPlayer; }
  }

  private bool _isPlayerInSight = false;
  private float _spotPlayerTimer = 0;

  void Start()
  {
    _refreshTimer = Random.Range(0, RefreshInterval); // Randomly offset that that minimize AI spawned the same frame updating at the same time

    _currentVisionDistance = VisionDistance;
    _currentVisionAngleDegrees = VisionAngleDegrees;
    _awareness = 0.0f;
    UpdateVisionConeVisuals();
  }

  void Update()
  {
    _refreshTimer -= Time.deltaTime;
    if (_refreshTimer <= 0)
    {
      _refreshTimer = RefreshInterval;
      RefreshPlayerSensoryInformation();
    }

    if (_isPlayerInSight)
    {
      _spotPlayerTimer += Time.deltaTime;
      if (_spotPlayerTimer >= 0.5f)
      {
        SetCanSeePlayer(true);
      }
    }
    else
    {
      _spotPlayerTimer = 0;
      SetCanSeePlayer(false);
    }

    if (_canSeePlayer)
    {
      _timeSinceLastSeenPlayer = 0.0f;
    }
    else
    {
      _timeSinceLastSeenPlayer += Time.deltaTime;
    }

    UpdateVisionConeParameters();
    UpdateVisionConeVisuals();
  }

  private void UpdateVisionConeParameters()
  {
    float timeScale = _canSeePlayer ? 4.0f : 1.0f;

    float angleScale = _canSeePlayer ? CanSeeAngleMultiplier : 1.0f;
    float desiredAngle = Mathf.Min(VisionAngleDegrees * angleScale, 170.0f);
    _currentVisionAngleDegrees = Mathfx.Damp(_currentVisionAngleDegrees, desiredAngle, 0.25f, Time.deltaTime * timeScale);

    float distanceScale = _canSeePlayer ? CanSeeDistanceMultiplier : 1.0f;
    float desiredDistance = VisionDistance * distanceScale;
    _currentVisionDistance = Mathfx.Damp(_currentVisionDistance, desiredDistance, 0.25f, Time.deltaTime * timeScale);

    _awareness = Mathfx.Damp(_awareness, _canSeePlayer ? 1.0f : 0.0f, 0.25f, Time.deltaTime * timeScale);
  }

  void UpdateVisionConeVisuals()
  {
    if (_visionCone != null)
    {
      float halfAngleRadians = Mathf.Deg2Rad * CurrentVisionAngleDegrees * 0.5f;
      Vector3 adjustedScale = new Vector3(2.0f * CurrentVisionDistance * Mathf.Tan(halfAngleRadians), 1.0f, -CurrentVisionDistance);

      _visionCone.transform.localScale = adjustedScale;
      _visionConeRenderer = _visionCone.GetComponent<Renderer>();
    }

    if (_visionConeRenderer != null)
    {
      _visionConeRenderer.material = (_awareness > 0.1) ? _attackMaterial : _normalMaterial;
    }
  }

  public bool IsLastPlayerLocationNewerThan(float timeOut)
  {
    return _timeSinceLastSeenPlayer >= 0.0f && _timeSinceLastSeenPlayer < timeOut;
  }

  void RefreshPlayerSensoryInformation()
  {
    Transform playerTransform = PlayerCharacterController.Instance.AIVisibilityTarget;
    Vector3 playerLocation = playerTransform.position;

    bool newCanSeePlayer = false;

    // Nearby player tests
    float actorDistance = Vector3.Distance(transform.position, playerLocation);
    Vector3 targetDir = playerLocation - transform.position;
    float angleToTarget = Vector3.Angle(transform.forward, targetDir);
    bool isPlayerNearby = actorDistance < NearbyDistance;
    bool isPlayerInFront = angleToTarget <= 90.0f;

    // In cone test
    bool isPlayerInCone = false;
    if (actorDistance < CurrentVisionDistance)
    {
      float halfVisionAngle = CurrentVisionAngleDegrees / 2.0f;

      isPlayerInCone = angleToTarget < halfVisionAngle;
    }

    // Raycast test
    Vector3 RayCastStart = Vector3.zero;
    Vector3 RayCastVector = Vector3.zero;
    _hasLineOfSight = false;
    _performedLineOfSightCheck = false;
    if (isPlayerInCone || (isPlayerNearby && isPlayerInFront) || ForceLineOfSightCheck)
    {
      LayerMask mask = LayerMask.GetMask("Terrain", "NotWalkable");
      RaycastHit hit;

      RayCastStart = transform.position;
      RayCastVector = playerLocation - transform.position;

      if (!Physics.Raycast(transform.position, RayCastVector, out hit, CurrentVisionDistance, mask))
      {
        _lastSeenPlayerLocation = playerLocation;
        _hasLineOfSight = true;

        RayCastVector = Vector3.ClampMagnitude(RayCastVector, CurrentVisionDistance);
      }
      else
      {
        RayCastVector = Vector3.ClampMagnitude(RayCastVector, hit.distance);
      }

      _performedLineOfSightCheck = true;
    }

    // Within vision cone OR within front nearby hemisphere
    // AND we have line of sight to the player
    if ((isPlayerInCone || (isPlayerNearby && isPlayerInFront)) && _hasLineOfSight)
    {
      newCanSeePlayer = true;
    }

    if (DrawDebug)
    {
      Vector3 origin = transform.position;
      Vector3 forward = transform.forward;
      Vector3 up = transform.up;
      Vector3 right = transform.right;

      // Draw the cone
      int subdiv = 20;
      float coneHalfAngleRadians = Mathf.Deg2Rad * (CurrentVisionAngleDegrees / 2.0f);
      float circleRadius = CurrentVisionDistance * Mathf.Tan(coneHalfAngleRadians);
      for (int i = 0; i <= subdiv; ++i)
      {
        float radians = Mathf.Deg2Rad * ((float)i * 360.0f / (float)subdiv);

        Debug.DrawLine(
          origin,
          origin + forward * CurrentVisionDistance + right * Mathf.Cos(radians) * circleRadius + up * Mathf.Sin(radians) * circleRadius,
          isPlayerInCone ? Color.red : Color.gray,
          _refreshTimer);
      }

      // Draw the front half of the nearby circle
      Vector3 prevFrontPoint = origin + right * NearbyDistance;
      for (int i = 1; i <= subdiv; ++i)
      {
        float radians = Mathf.Deg2Rad * ((float)i * 180.0f / (float)subdiv);
        Vector3 nextFrontPoint = origin + right * NearbyDistance * Mathf.Cos(radians) + forward * NearbyDistance * Mathf.Sin(radians);

        Debug.DrawLine(
          prevFrontPoint, nextFrontPoint,
          (isPlayerNearby && isPlayerInFront) ? Color.red : Color.gray,
          _refreshTimer);
        prevFrontPoint = nextFrontPoint;
      }
      Debug.DrawLine(
        origin - right * NearbyDistance + forward * 0.01f,
        origin + right * NearbyDistance + forward * 0.01f,
        (isPlayerNearby && isPlayerInFront) ? Color.red : Color.gray,
        _refreshTimer);

      // Draw the back half of the nearby circle
      Vector3 prevBackPoint = origin + right * NearbyDistance;
      for (int i = 1; i <= subdiv; ++i)
      {
        float radians = Mathf.Deg2Rad * ((float)i * 180.0f / (float)subdiv);
        Vector3 nextBackPoint = origin + right * NearbyDistance * Mathf.Cos(radians) - forward * NearbyDistance * Mathf.Sin(radians);

        Debug.DrawLine(
          prevBackPoint, nextBackPoint,
          (isPlayerNearby && !isPlayerInFront) ? Color.red : Color.gray,
          _refreshTimer);
        prevBackPoint = nextBackPoint;
      }
      Debug.DrawLine(
        origin - right * NearbyDistance - forward * 0.01f,
        origin + right * NearbyDistance - forward * 0.01f,
        (isPlayerNearby && !isPlayerInFront) ? Color.red : Color.gray,
        _refreshTimer);

      if (_performedLineOfSightCheck)
      {
        Debug.DrawRay(RayCastStart, RayCastVector, _hasLineOfSight ? Color.magenta : Color.gray, _refreshTimer);
      }
    }

    _isPlayerInSight = newCanSeePlayer;
    SetIsPlayerNearbyBehind(isPlayerNearby && !isPlayerInFront);
  }

  void SetCanSeePlayer(bool newCanSee)
  {
    _canSeePlayer = newCanSee;
  }

  void SetIsPlayerNearbyBehind(bool newValue)
  {
    _isPlayerNearbyBehind = newValue;
  }
}
