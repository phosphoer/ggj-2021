using UnityEngine;

public class CameraControllerBase : MonoBehaviour
{
  public Transform MountPoint { get { return _mountPoint != null ? _mountPoint : transform; } }

  public float FieldOfView = 65.0f;

  [SerializeField]
  private Transform _mountPoint = null;

  public void MatchCameraTransform()
  {
    MountPoint.SetPositionAndRotation(Camera.main.transform.position, Camera.main.transform.rotation);
  }
}
