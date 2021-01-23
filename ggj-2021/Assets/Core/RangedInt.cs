using UnityEngine;

[System.Serializable]
public struct RangedInt
{
  public int MinValue;
  public int MaxValue;

  public int RangeSize => MaxValue - MinValue;

  public int RandomValue
  {
    get { return Random.Range(MinValue, MaxValue); }
  }

  public int RandomValueInclusive
  {
    get { return Random.Range(MinValue, MaxValue + 1); }
  }

  public RangedInt(int min = 0, int max = 0)
  {
    MinValue = min;
    MaxValue = max;
  }

  public bool InRange(int value, bool inclusive = true)
  {
    if (inclusive)
    {
      return value >= MinValue && value <= MaxValue;
    }

    return value > MinValue && value < MaxValue;
  }

  public int SeededRandom(System.Random rand)
  {
    return rand.NextIntRanged(MinValue, MaxValue);
  }

  public int SeededRandomInclusive(System.Random rand)
  {
    return rand.NextIntRanged(MinValue, MaxValue + 1);
  }
}