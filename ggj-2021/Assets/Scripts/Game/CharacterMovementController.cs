using UnityEngine;

public class CharacterMovementController : MonoBehaviour
{
  public Vector3 MoveVector = Vector3.zero;
  public float Acceleration = 1;
  public float MoveSpeed = 1;
  public LayerMask RaycastMask;
  public LayerMask TerrainMask;

  [SerializeField]
  private Rigidbody _rb = null;

  [SerializeField]
  private Transform _visualRoot = null;

  private Vector3 _currentMoveDir = Vector3.zero;

  private void Update()
  {
    Vector3 clampedMoveDir = Vector3.ClampMagnitude(MoveVector, 1);
    _currentMoveDir = Mathfx.Damp(_currentMoveDir, clampedMoveDir, 0.25f, Time.deltaTime * Acceleration);

    if (_currentMoveDir.sqrMagnitude > 0)
    {
      Vector3 newPosition = transform.position + _currentMoveDir * Time.deltaTime * MoveSpeed;
      Vector3 navPosition;
      if (PathFindManager.Instance.TryGetTraversablePoint(newPosition, out navPosition))
      {
        transform.position = navPosition;
      }
    }
  }
}