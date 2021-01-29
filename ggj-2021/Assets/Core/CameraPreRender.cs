using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraPreRender : MonoBehaviour
{
  public static event System.Action<Camera> PreRender;
  public static event System.Action<Camera> PreCull;

  private Camera _camera;

  private void Awake()
  {
    _camera = GetComponent<Camera>();
  }

  private void OnPreRender()
  {
    if (PreRender != null && _camera != null)
      PreRender(_camera);
  }

  private void OnPreCull()
  {
    if (PreCull != null && _camera != null)
      PreCull(_camera);
  }
}