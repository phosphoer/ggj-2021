using UnityEngine;

public class CharacterMovementController : MonoBehaviour
{
  public Vector3 MoveVector = Vector3.zero;
  public float Acceleration = 1;
  public float MoveSpeed = 1;
  public LayerMask TerrainMask;

  private Vector3 _currentMoveDir = Vector3.zero;

  private void Update()
  {
    Vector3 clampedMoveDir = Vector3.ClampMagnitude(MoveVector, 1);
    _currentMoveDir = Mathfx.Damp(_currentMoveDir, clampedMoveDir, 0.25f, Time.deltaTime * Acceleration);

    if (MoveVector.sqrMagnitude > 0)
    {
      Vector3 newPosition = transform.position + _currentMoveDir * Time.deltaTime * MoveSpeed;

      RaycastHit hitInfo;
      if (Physics.Raycast(newPosition + Vector3.up, Vector3.down, out hitInfo, 200, TerrainMask, QueryTriggerInteraction.Ignore))
      {
        newPosition.y = hitInfo.point.y;
      }

      transform.position = newPosition;
    }
  }
}