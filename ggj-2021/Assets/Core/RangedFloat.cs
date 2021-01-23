using UnityEngine;

[System.Serializable]
public struct RangedFloat
{
  public float MinValue;
  public float MaxValue;

  public float RangeSize => MaxValue - MinValue;

  public float RandomValue
  {
    get { return Random.Range(MinValue, MaxValue); }
  }

  public RangedFloat(float min = 0, float max = 0)
  {
    MinValue = min;
    MaxValue = max;
  }

  public bool InRange(float value, bool inclusive = true)
  {
    if (inclusive)
    {
      return value >= MinValue && value <= MaxValue;
    }

    return value > MinValue && value < MaxValue;
  }

  public float Clamp(float value)
  {
    return Mathf.Clamp(value, MinValue, MaxValue);
  }

  public float Lerp(float t)
  {
    return Mathf.Lerp(MinValue, MaxValue, t);
  }

  public float SeededRandom(System.Random rand)
  {
    return MinValue + (float)rand.NextFloat() * RangeSize;
  }
}