using UnityEngine;

public class CameraControllerGame : CameraControllerBase
{
  public RangedFloat ZoomRange = new RangedFloat(10, 30);
  public float ViewportBorder = 0.25f;
  public float ZoomSensitivity = 2;

  private float _zoom;

  private void Start()
  {
    _zoom = ZoomRange.MinValue;
  }

  private void Update()
  {
    Camera cam = CameraControllerStack.Instance.Camera;
    Vector3 focusCenter = Vector3.zero;
    float zoomDelta = 0;
    bool anyOffscreen = false;
    for (int i = 0; i < CameraFocusPoint.Instances.Count; ++i)
    {
      CameraFocusPoint focusPoint = CameraFocusPoint.Instances[i];
      focusCenter += focusPoint.transform.position;

      // Get centered viewport pos
      Vector3 viewportPos = cam.WorldToViewportPoint(focusPoint.transform.position);
      viewportPos -= Vector3.one * 0.5f;
      viewportPos *= 2;

      // Increase amount of desired zoom by how much player is off screen
      // If any players are offscreen we will never zoom in regardless 
      float maxViewPos = Mathf.Max(Mathf.Abs(viewportPos.x), Mathf.Abs(viewportPos.y));
      float delta = Mathf.Clamp((maxViewPos + ViewportBorder) - 1, -1, 1) * ZoomSensitivity;
      if (delta > 0 || anyOffscreen)
      {
        anyOffscreen = true;
        zoomDelta = Mathf.Max(zoomDelta + delta, 0);
      }
      else
      {
        zoomDelta += delta;
      }
    }

    if (CameraFocusPoint.Instances.Count > 0)
    {
      focusCenter /= CameraFocusPoint.Instances.Count;
    }

    Debug.DrawLine(MountPoint.position, focusCenter);

    float desiredZoom = ZoomRange.Clamp(MountPoint.position.y + zoomDelta);
    _zoom = Mathfx.Damp(_zoom, desiredZoom, 0.25f, Time.deltaTime * 2);
    Vector3 desiredPos = focusCenter.WithY(_zoom);
    Vector3 toDesiredPos = MountPoint.position - desiredPos;
    if (toDesiredPos.sqrMagnitude > 0.1f)
    {
      MountPoint.position = Mathfx.Damp(MountPoint.position, desiredPos, 0.5f, Time.deltaTime * 2);
    }
    else
    {
      MountPoint.position = desiredPos;
    }
  }
}
