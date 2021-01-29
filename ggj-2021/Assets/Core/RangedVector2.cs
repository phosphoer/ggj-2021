using UnityEngine;

[System.Serializable]
public struct RangedVector2
{
  public Vector2 MinValue;
  public Vector2 MaxValue;

  public Vector2 RandomValue
  {
    get { return new Vector2(Random.Range(MinValue.x, MaxValue.x), Random.Range(MinValue.y, MaxValue.y)); }
  }

  public RangedVector2(Vector2 min = default(Vector2), Vector2 max = default(Vector2))
  {
    MinValue = min;
    MaxValue = max;
  }

  public Vector2 SeededRandom(System.Random rand)
  {
    float x = rand.NextFloatRanged(MinValue.x, MaxValue.x);
    float y = rand.NextFloatRanged(MinValue.y, MaxValue.y);
    return new Vector2(x, y);
  }
}
