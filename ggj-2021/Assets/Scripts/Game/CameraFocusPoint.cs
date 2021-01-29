using UnityEngine;
using System.Collections.Generic;

public class CameraFocusPoint : MonoBehaviour
{
  public static IReadOnlyList<CameraFocusPoint> Instances => _instances;
  private static List<CameraFocusPoint> _instances = new List<CameraFocusPoint>();

  private void OnEnable()
  {
    _instances.Add(this);
  }

  private void OnDisable()
  {
    _instances.Remove(this);
  }
}
