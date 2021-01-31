using UnityEngine;
using System.Collections.Generic;

public class ScreamDamageable : MonoBehaviour
{
  public event System.Action ScreamDamageTaken;

  private static List<ScreamDamageable> _instances = new List<ScreamDamageable>();

  public static void DoScream(IReadOnlyList<ScreamSoundDefinition> screamSounds, Vector3 fromPos, Vector3 dir)
  {
    for (int i = 0; i < _instances.Count; ++i)
    {
      ScreamDamageable damageable = _instances[i];
      Vector3 toDamageable = damageable.transform.position - fromPos;
      if (toDamageable.magnitude < 5 && Vector3.Angle(dir, toDamageable) < 15)
      {
        damageable.TakeScreamDamage();
      }
    }
  }

  public void TakeScreamDamage()
  {
    ScreamDamageTaken?.Invoke();
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