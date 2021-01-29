using UnityEngine;
using System.Collections.Generic;

public class CameraControllerStack : Singleton<CameraControllerStack>
{
  public Camera Camera
  {
    get { return _camera; }
    set { _camera = value; }
  }

  public IReadOnlyList<CameraControllerBase> Stack => _cameraControllers;

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

  private float _shakeTimer;
  private float _shakeTime;
  private float _shakeMagnitude;

  public void CameraShakeFromPosition(Vector3 fromPos, float radius, float magnitude, float duration)
  {
    float dist = Vector3.Distance(_camera.transform.position, fromPos);
    float shakeScale = 1.0f - Mathf.Clamp01(dist / radius);
    if (shakeScale > 0)
    {
      CameraShake(shakeScale * magnitude, duration);
    }
  }

  public void CameraShake(float magnitude, float duration)
  {
    _shakeTime = duration;
    _shakeTimer = duration;
    _shakeMagnitude = magnitude;
  }

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

  public void SwitchController(CameraControllerBase cameraController)
  {
    PopCurrentController();
    PushController(cameraController);
  }

  public void PopCurrentController()
  {
    PopController(CurrentCameraController);
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

  public void SnapTransformToTarget()
  {
    _camera.transform.localPosition = Vector3.zero;
    _camera.transform.localRotation = Quaternion.identity;
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

      _camera.fieldOfView = Mathfx.Damp(_camera.fieldOfView, desiredFov, 0.25f, Time.unscaledDeltaTime * 2.0f);
    }

    _shakeTimer -= Time.unscaledDeltaTime;
    if (_shakeTimer > 0)
    {
      float shakeT = Mathf.Clamp01(_shakeTimer / _shakeTime);
      _camera.transform.position += Random.onUnitSphere * Random.value * _shakeMagnitude * shakeT;
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