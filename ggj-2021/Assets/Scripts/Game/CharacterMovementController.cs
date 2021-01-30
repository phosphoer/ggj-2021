using UnityEngine;

public class CharacterMovementController : MonoBehaviour
{
  public Vector3 CurrentVelocity => _currentVelocity;

  public Vector3 MoveVector = Vector3.zero;
  public float MoveSpeedMultiplier = 1;

  public float Acceleration = 1;
  public float MoveSpeed = 1;
  public float RunLeanAmount = 3;
  public float TurnAnimationSpeed = 5;

  private Vector3 _currentMoveDir = Vector3.zero;
  private Vector3 _currentVelocity = Vector3.zero;

  private void Update()
  {
    float invDeltaTime = 1.0f / Mathf.Max(0.001f, Time.deltaTime);

    // Update move direction
    Vector3 clampedMoveDir = Vector3.ClampMagnitude(MoveVector, 1);
    _currentMoveDir = Mathfx.Damp(_currentMoveDir, clampedMoveDir, 0.25f, Time.deltaTime * Acceleration);

    // Move to new position on nav mesh
    Vector3 newPosition = transform.position + _currentMoveDir * Time.deltaTime * MoveSpeed * MoveSpeedMultiplier;
    Vector3 navPosition;
    if (PathFindManager.Instance.TryGetTraversablePoint(newPosition, out navPosition))
    {
      Vector3 delta = navPosition - transform.position;
      _currentVelocity = Mathfx.Damp(_currentVelocity, delta * invDeltaTime, 0.25f, Time.deltaTime * 3);
      transform.position = navPosition;
    }

    // Rotate and tilt to face direction
    if (_currentMoveDir.sqrMagnitude > 0.01f)
    {
      Vector3 runTiltAxis = Vector3.Cross(_currentMoveDir, Vector3.up);
      Quaternion forwardRot = Quaternion.LookRotation(_currentMoveDir);
      Quaternion runTiltRot = Quaternion.AngleAxis(_currentVelocity.magnitude * RunLeanAmount, runTiltAxis);
      Quaternion finalRot = runTiltRot * forwardRot;
      transform.rotation = Mathfx.Damp(transform.rotation, finalRot, 0.25f, Time.deltaTime * TurnAnimationSpeed);
    }
  }
}