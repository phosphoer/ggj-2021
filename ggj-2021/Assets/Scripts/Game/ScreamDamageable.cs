using UnityEngine;
using System.Collections.Generic;

public class ScreamDamageable : MonoBehaviour
{
  public event System.Action<IReadOnlyList<ScreamSoundDefinition>> ScreamedAt;

  private static List<ScreamDamageable> _instances = new List<ScreamDamageable>();
  private const float kScreamMinRange = 1;
  private const float kScreamRange = 6;
  private const float kScreamAngle = 90;

  public static void DoScream(IReadOnlyList<ScreamSoundDefinition> screamSounds, Vector3 fromPos, Vector3 dir, ScreamDamageable ignore = null)
  {
    Debug.DrawRay(fromPos, dir * kScreamRange, Color.green, 1);

    for (int i = 0; i < _instances.Count; ++i)
    {
      ScreamDamageable damageable = _instances[i];
      if (damageable == ignore)
        continue;

      Vector3 toDamageable = damageable.transform.position - fromPos;
      float angle = Vector3.Angle(dir.WithY(0), toDamageable.WithY(0));
      float dist = toDamageable.magnitude;
      if ((dist < kScreamRange && angle < kScreamAngle) || dist < kScreamMinRange)
      {
        Debug.Log($"Screaming at {damageable.name}");
        Debug.DrawRay(fromPos, toDamageable, Color.red, 10);
        damageable.NotifyScreamedAt(screamSounds);
      }
    }
  }

  public void NotifyScreamedAt(IReadOnlyList<ScreamSoundDefinition> screamSounds)
  {
    ScreamedAt?.Invoke(screamSounds);
  }

  private void OnEnable()
  {
    _instances.Add(this);
  }

  private void OnDisable()
  {
    _instances.Remove(this);
  }
}