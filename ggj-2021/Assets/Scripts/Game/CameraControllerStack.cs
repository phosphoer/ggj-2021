using UnityEngine;
using System.Collections.Generic;

public class CameraControllerStack : Singleton<CameraControllerStack>
{
  public Camera Camera
  {
    get { return _camera; }
    set { _camera = value; }
  }

  public CameraControllerBase CurrentCameraController
  {
    get { return _cameraControllers.Count > 0 ? _cameraControllers[_cameraControllers.Count - 1] : null; }
  }

  public float CurrentFovOverride
  {
    get { return _fovStack.Count > 0 ? _fovStack[_fovStack.Count - 1].Value : -1; }
  }

  [SerializeField]
  private Camera _camera = null;

  private List<CameraControllerBase> _cameraControllers = new List<CameraControllerBase>();
  private List<KeyValuePair<string, float>> _fovStack = new List<KeyValuePair<string, float>>();

  public void PushController(CameraControllerBase cameraController)
  {
    if (_cameraControllers.Contains(cameraController))
    {
      return;
    }

    _cameraControllers.Add(cameraController);
    EnsureCameraStack();
  }

  public void PopController(CameraControllerBase cameraController)
  {
    if (_cameraControllers.Remove(cameraController))
    {
      EnsureCameraStack();
    }
  }

  public void PushFovOverride(string key, float fov)
  {
    _fovStack.Add(new KeyValuePair<string, float>(key, fov));
  }

  public void PopFovOverride(string key)
  {
    for (int i = 0; i < _fovStack.Count; ++i)
    {
      if (_fovStack[i].Key == key)
      {
        _fovStack.RemoveAt(i);
        return;
      }
    }
  }

  public void ClearStack()
  {
    _cameraControllers.Clear();
    EnsureCameraStack();
  }

  public void SnapPositionToTarget()
  {
    _camera.transform.localPosition = Vector3.zero;
  }

  public void InterpolateToTarget(Vector3 position, Quaternion rotation)
  {
    _camera.transform.SetParent(null, true);
    if (CurrentCameraController != null)
    {
      CurrentCameraController.transform.SetPositionAndRotation(position, rotation);
    }

    EnsureCameraStack();
  }

  private void Awake()
  {
    Instance = this;
  }

  private void Update()
  {
    if (_cameraControllers.Count > 0)
    {
      // Align camera with current mount point
      _camera.transform.localPosition = Mathfx.Damp(_camera.transform.localPosition, Vector3.zero, 0.5f, Time.unscaledDeltaTime * 3.0f);
      _camera.transform.localRotation = Mathfx.Damp(_camera.transform.localRotation, Quaternion.identity, 0.5f, Time.unscaledDeltaTime * 3.0f);

      float desiredFov = CurrentCameraController.FieldOfView;
      if (_fovStack.Count > 0)
        desiredFov = _fovStack[_fovStack.Count - 1].Value;

      _camera.fieldOfView = Mathfx.Damp(_camera.fieldOfView, desiredFov, 0.5f, Time.unscaledDeltaTime * 2.0f);
    }
  }

  private void EnsureCameraStack()
  {
    if (_cameraControllers.Count > 0)
    {
      CameraControllerBase activeController = _cameraControllers[_cameraControllers.Count - 1];
      activeController.enabled = true;

      _camera.transform.SetParent(activeController.MountPoint, true);
    }
    else
    {
      _camera.transform.SetParent(null, true);
    }
  }
}
