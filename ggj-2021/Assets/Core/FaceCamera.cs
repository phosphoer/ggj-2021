using UnityEngine;

public class FaceCamera : MonoBehaviour
{
  public bool FlipZ;

  private void OnEnable()
  {
    CameraPreRender.PreRender += OnCameraPreRender;
  }

  private void OnDisable()
  {
    CameraPreRender.PreRender -= OnCameraPreRender;
  }

  private void OnCameraPreRender(Camera cam)
  {
    Vector3 pos = cam.transform.position;
    Vector3 toCamera = pos - transform.position;

    transform.rotation = Quaternion.LookRotation(toCamera.normalized * (FlipZ ? 1.0f : -1.0f), Vector3.up);
  }
}