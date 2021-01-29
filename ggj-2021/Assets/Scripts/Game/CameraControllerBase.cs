using UnityEngine;

public class CameraControllerBase : MonoBehaviour
{
  public Transform MountPoint => _mountPoint != null ? _mountPoint : transform;

  public float FieldOfView = 65.0f;

  [SerializeField]
  private Transform _mountPoint = null;

  public void MatchCameraTransform()
  {
    MountPoint.SetPositionAndRotation(Camera.main.transform.position, Camera.main.transform.rotation);
  }

  public void MoveToFrameObject(Transform target, Vector3 posOffset, Vector3 lookOffset, bool useLocalSpace)
  {
    transform.position = target.position.WithY(posOffset.y);
    transform.position += (useLocalSpace ? target.right : Vector3.right) * posOffset.x;
    transform.position += (useLocalSpace ? target.forward : Vector3.forward) * posOffset.z;
    transform.LookAt(target.position + lookOffset);
  }
}