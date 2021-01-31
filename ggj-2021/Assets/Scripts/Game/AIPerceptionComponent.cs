using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPerceptionComponent : MonoBehaviour
{
  public float VisionAngleDegrees = 30;
  public float VisionDistance = 100;
  public float RefreshInterval = 0.1f;
  public bool DrawDebug = true;

  private float _refreshTimer = 0.0f;

  private bool _canSeePlayer = false;
  public bool CanSeePlayer
  {
    get { return _canSeePlayer; }
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

  void Start()
  {
    _refreshTimer = Random.Range(0, RefreshInterval); // Randomly offset that that minimize AI spawned the same frame updating at the same time
  }

  void Update()
  {
    _refreshTimer -= Time.deltaTime;
    if (_refreshTimer <= 0)
    {
      _refreshTimer = RefreshInterval;
      RefreshPlayerVisionInformation();

      if (DrawDebug)
      {
        DrawDebugCone();
      }
    }

    if (_canSeePlayer)
    {
      _timeSinceLastSeenPlayer = 0.0f;
    }
    else
    {
      _timeSinceLastSeenPlayer += Time.deltaTime;
    }
  }

  public bool IsLastPlayerLocationNewerThan(float timeOut)
  {
    return _timeSinceLastSeenPlayer >= 0.0f || _timeSinceLastSeenPlayer < timeOut;
  }

  void RefreshPlayerVisionInformation()
  {
    Transform playerTransform = PlayerCharacterController.Instance.AIVisibilityTarget;
    Vector3 playerLocation = playerTransform.position;

    float actorDistance = Vector3.Distance(transform.position, playerLocation);
    if (actorDistance < VisionDistance)
    {
      Vector3 targetDir = playerLocation - transform.position;
      float angle = Vector3.Angle(targetDir, transform.forward);

      if (angle < VisionAngleDegrees / 2.0f)
      {
        LayerMask mask = LayerMask.GetMask("Terrain");
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, (playerLocation - transform.position), out hit, VisionDistance, mask))
        {
          _canSeePlayer = true;
          _lastSeenPlayerLocation = playerLocation;
          return;
        }
      }
    }

    _canSeePlayer = false;
  }

  void DrawDebugCone()
  {
    Vector3 origin = transform.position;
    Vector3 forward = transform.forward;
    Vector3 up = transform.up;
    Vector3 right = transform.right;

    int subdiv = 20;
    float coneHalfAngleRadians = Mathf.Deg2Rad * (VisionAngleDegrees / 2.0f);
    float circleRadius = VisionDistance * Mathf.Tan(coneHalfAngleRadians);
    for (int i = 0; i <= subdiv; ++i)
    {
      float radians = Mathf.Deg2Rad * ((float)i * 360.0f / (float)subdiv);

      Debug.DrawLine(
        origin,
        origin + forward * VisionDistance + right * Mathf.Cos(radians) * circleRadius + up * Mathf.Sin(radians) * circleRadius,
        _canSeePlayer ? Color.white : Color.yellow,
        _refreshTimer);
    }

    Transform playerTransform = PlayerCharacterController.Instance.AIVisibilityTarget;
    Vector3 playerLocation = playerTransform.position;
    if (_canSeePlayer)
    {
      Debug.DrawLine(origin, _lastSeenPlayerLocation, Color.green, _refreshTimer);
    }
    else
    {
      Vector3 rayDir = Vector3.Normalize(playerLocation - transform.position) * VisionDistance;
      Debug.DrawRay(origin, rayDir, Color.red, _refreshTimer);
    }
  }
}
